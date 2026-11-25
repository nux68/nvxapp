using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.Helpers
{
    /// <summary>
    /// Serializzazione per il LOG con i valori riservati mascherati (password, PIN, token, segreti, chiavi API...).
    /// Non e' la serializzazione delle risposte: quella non cambia.
    ///
    /// Regole:
    ///  - proprieta' con [LogSensitive]: sempre "***" (se non null)
    ///  - nome riservato (anche chiavi di dizionario e JSON dentro una stringa): il nome si divide in parole
    ///    (camelCase, "_", "-", cifre) e si confronta parola per parola, senza distinguere maiuscole: cosi "NewPw", "RefreshToken",
    ///    "api_key" sono riservati, mentre "Mapping", "Pinned", "Keywords", "StorageKey" no (la parola "pin" o "key" non e' intera).
    ///    Parole riservate: password, pw, pwd, pin, token, secret, otp, cvv, credential, badge, authorization, cookie, iban...
    ///    Coppie: api+key, private+key, secret+key, connection+string, verification+code, ...
    ///    Per le parole NON forti (token, badge, iban...) se l'ultima parola e' name/id/type/prefix/... ("TokenName", "IdToken")
    ///    non si maschera; per le forti (password, pw, pin, secret, otp) si maschera comunque ("PasswordType").
    ///  - si mascherano testo e numeri (es. {"pin":1234}) e gli elementi testuali/numerici degli elenchi sotto un nome riservato;
    ///    bool, null, date e oggetti restano visibili (gli oggetti perche' hanno le loro proprieta', mascherate a loro volta).
    ///    I numeri restano visibili se il nome contiene una parola da contatore (MaxTokens, TokenCount, PromptTokens...).
    ///    Stringa vuota e null restano visibili.
    ///  - nel testo libero (anche messaggi di errore) si mascherano le forme "nome=valore" / "nome: valore" con nome riservato
    ///    (anche composto: NewPw, newPassword), valori tra virgolette con spazi, "Bearer xxx"
    ///  - stringhe lunghe (es. contenuti base64) abbreviate PRIMA delle regex (con la lunghezza originale), byte[] scritti come
    ///    "[byte[] N]", output totale limitato
    ///  - mai eccezioni: cicli ignorati, getter che lanciano saltati, in caso di errore resta il solo tipo
    /// </summary>
    public static class LogSanitizer
    {
        public const string Mask = "***";

        // oltre queste soglie il log viene abbreviato
        private const int MaxStringLength = 300;
        private const int MaxJsonInStringLength = 262144;
        private const int MaxOutputLength = 32000;
        private const int MaxDepth = 12;
        // quanto indietro si cerca un delimitatore per tagliare senza spezzare un segreto
        private const int CutWindow = 40;

        // parole intere che indicano un valore riservato (il plurale si riduce togliendo la "s")
        private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwd", "passphrase", "psw", "pw", "pwd",
            "pin", "otp", "cvv", "cvc", "badge",
            "token", "secret", "credential", "apikey", "authorization", "cookie", "bearer", "iban",
            "segreto", "segreta", "credenziale"
        };

        // parole che da sole indicano un segreto vero: la parola neutra in fondo (PasswordData, PasswordType) NON le smaschera
        private static readonly HashSet<string> StrongWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwd", "passphrase", "psw", "pw", "pwd", "pin", "otp", "cvv", "cvc", "secret", "segreto", "segreta"
        };

        // coppie di parole adiacenti riservate (prima parola, seconda parola, in minuscolo)
        private static readonly HashSet<(string, string)> ReservedPairs = new()
        {
            ("api", "key"), ("private", "key"), ("secret", "key"), ("access", "key"), ("signing", "key"), ("encryption", "key"),
            ("client", "secret"), ("connection", "string"),
            ("chiave", "privata"), ("chiave", "segreta"), ("chiave", "api"),
            ("verification", "code"), ("confirmation", "code"), ("security", "code"), ("recovery", "code"), ("reset", "code"),
            ("activation", "code"), ("auth", "code"), ("sms", "code"),
            ("codice", "verifica"), ("codice", "conferma"), ("codice", "sicurezza"), ("codice", "recupero"), ("codice", "attivazione")
        };

        // per le parole NON forti: se l'ultima parola e' una di queste il valore descrive il campo, non e' il segreto (TokenName, IdToken, TokenPrefix)
        private static readonly HashSet<string> NeutralLastWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "name", "id", "type", "prefix", "length", "count", "day", "hour", "min", "max", "date", "expiry", "expire",
            "required", "enabled", "set", "valid", "status", "label", "nome", "tipo", "data", "scadenza", "lunghezza"
        };

        // un numero con una di queste parole nel nome e' un contatore o un limite, non un segreto (MaxTokens, TokenCount, PromptTokens)
        private static readonly HashSet<string> CounterWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "max", "min", "count", "length", "limit", "total", "num", "number", "size", "ttl", "day", "hour", "minute", "second",
            "expiry", "expire", "used", "usage", "remaining", "prompt", "completion", "input", "output", "id", "index", "position",
            "lunghezza", "totale", "giorni", "numero"
        };

        private static readonly Regex WordSplit = new(
            @"[^\p{L}\p{N}]+|(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=\p{L})(?=\d)|(?<=\d)(?=\p{L})",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // "nome=valore", "nome: valore", "\"nome\":\"valore con spazi\"" nel testo libero: il nome e' valutato da Classify
        private static readonly Regex TextPair = new(
            @"(?<k>[A-Za-z_][\w\-]{0,63})(?<kq>[""']?)(?<s>\s*[=:]\s*)(?:(?<q>"")(?<v>[^""]*)""|(?<q>')(?<v>[^']*)'|(?![A-Za-z_][\w\-]*[""']?\s*[=:])(?<v>[^\s&;,""'}\]]+))",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex TextBearer = new(
            @"\b(?<k>Bearer|Basic)\s+(?<v>[A-Za-z0-9\-._~+/]{8,}=*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // decisione per nome, calcolata una volta sola; con un tetto perche' i nomi arrivano anche da chiavi di dizionario
        // e JSON delle richieste (anche anonime)
        private const int MaxCachedNames = 4000;
        private const int MaxCachedNameLength = 64;
        private const int NotSensitive = 0, SensitiveText = 1, SensitiveTextAndNumbers = 2;
        private static readonly ConcurrentDictionary<string, int> NameCache = new(StringComparer.OrdinalIgnoreCase);

        // un solo serializzatore condiviso (thread-safe): la cache dei contratti del resolver vale per tutta la vita del processo
        private static readonly JsonSerializer Serializer = CreateSerializer();

        /// <summary>
        /// true se il NOME indica un valore riservato (regola a parole intere, vedi la descrizione della classe).
        /// </summary>
        public static bool IsSensitiveName(string? name) => Classify(name) != NotSensitive;


        private static int Classify(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return NotSensitive;

            // nomi lunghi: mai in cache, si valuta solo l'inizio
            if (name.Length > MaxCachedNameLength)
                return Evaluate(name.Substring(0, MaxCachedNameLength));

            if (NameCache.TryGetValue(name, out var cached))
                return cached;

            var value = Evaluate(name);
            if (NameCache.Count < MaxCachedNames)
                NameCache.TryAdd(name, value);
            return value;
        }

        /// <summary>
        /// Serializza <paramref name="value"/> in JSON per il log, con i valori riservati mascherati. Non lancia mai eccezioni.
        /// </summary>
        public static string Serialize(object? value)
        {
            if (value == null)
                return "null";

            try
            {
                var token = JToken.FromObject(value, Serializer);
                var text = Walk(token, 0).ToString(Formatting.None);
                if (text.Length > MaxOutputLength)
                    text = text.Substring(0, MaxOutputLength) + "...[troncato, " + text.Length + " caratteri]";
                return text;
            }
            catch (Exception ex)
            {
                // niente messaggio dell'eccezione: potrebbe contenere i dati stessi
                return "[non serializzabile per il log: " + value.GetType().Name + ", " + ex.GetType().Name + "]";
            }
        }

        /// <summary>
        /// Maschera i valori riservati in un testo libero (messaggi di errore, corpi di risposta, JSON gia' scritto).
        /// Un testo JSON viene letto e mascherato per nome di proprieta'. Non lancia mai eccezioni.
        /// </summary>
        public static string? MaskText(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                if (text.Length <= MaxJsonInStringLength && TryParseJson(text, out var json))
                {
                    var t = Walk(json!, 0).ToString(Formatting.None);
                    return t.Length > MaxOutputLength ? t.Substring(0, MaxOutputLength) + "...[troncato, " + t.Length + " caratteri]" : t;
                }

                // prima si taglia, poi si applicano le regex
                if (text.Length > MaxOutputLength)
                    return MaskFreeText(CutSafe(text, MaxOutputLength)) + "...[troncato, " + text.Length + " caratteri]";
                return MaskFreeText(text);
            }
            catch
            {
                return "[testo non mascherabile]";
            }
        }

        // ---- interni ----

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new LogContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                // getter che lancia (o altro errore su una proprieta'): si salta la proprieta' e si va avanti
                Error = (_, e) => e.ErrorContext.Handled = true
            };
            settings.Converters.Add(new ByteArrayLogConverter());
            return JsonSerializer.Create(settings);
        }

        private static int Evaluate(string name)
        {
            var words = WordSplit.Split(name)
                                 .Where(w => w.Length > 0)
                                 .Select(Singular)
                                 .ToList();
            if (words.Count == 0)
                return NotSensitive;

            bool reserved = false, strong = false;
            for (int i = 0; i < words.Count; i++)
            {
                if (ReservedWords.Contains(words[i]))
                {
                    reserved = true;
                    strong |= StrongWords.Contains(words[i]);
                }
                else if (i + 1 < words.Count && ReservedPairs.Contains((words[i].ToLowerInvariant(), words[i + 1].ToLowerInvariant())))
                    reserved = true;
            }
            if (!reserved)
                return NotSensitive;

            // TokenName, IdToken, TokenPrefix...: descrivono il campo (ma PasswordType resta riservato)
            if (!strong && words.Count > 1 && NeutralLastWords.Contains(words[^1]) && !ReservedWords.Contains(words[^1]))
                return NotSensitive;

            return words.Any(CounterWords.Contains) ? SensitiveText : SensitiveTextAndNumbers;
        }

        private static string Singular(string w)
            => w.Length > 3 && w.EndsWith("s", StringComparison.OrdinalIgnoreCase)
               && !w.EndsWith("ss", StringComparison.OrdinalIgnoreCase)
               && !w.EndsWith("us", StringComparison.OrdinalIgnoreCase)
               && !w.EndsWith("is", StringComparison.OrdinalIgnoreCase)
                ? w.Substring(0, w.Length - 1)
                : w;

        private static bool TryParseJson(string text, out JToken? token)
        {
            token = null;
            var t = text.AsSpan().Trim();
            if (t.Length < 2)
                return false;

            // controllo economico prima di Parse (niente eccezioni a ripetizione per testi come "[Admin]"):
            // estremi coerenti e primo carattere utile valido
            char first = t[0], last = t[^1];
            if (!((first == '{' && last == '}') || (first == '[' && last == ']')))
                return false;
            var inner = t.Slice(1).TrimStart();
            if (inner.Length == 0)
                return false;
            char c = inner[0];
            bool ok = first == '{'
                ? c == '"' || c == '}'
                : c == '"' || c == '{' || c == '[' || c == ']' || c == '-' || char.IsDigit(c) || c == 't' || c == 'f' || c == 'n';
            if (!ok)
                return false;

            try
            {
                token = JToken.Parse(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static JToken Walk(JToken token, int depth)
        {
            if (depth > MaxDepth)
                return new JValue("[troppo profondo]");

            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties().ToList())
                    {
                        int kind = Classify(prop.Name);
                        if (kind == NotSensitive)
                        {
                            prop.Value = Walk(prop.Value, depth + 1);
                            continue;
                        }

                        if (prop.Value is JArray list)
                        {
                            // "Tokens": ["a","b"]: si mascherano gli elementi testuali e numerici, gli altri si visitano
                            for (int i = 0; i < list.Count; i++)
                                list[i] = MaskLeaf(list[i], kind) ?? Walk(list[i], depth + 1);
                        }
                        else
                        {
                            prop.Value = MaskLeaf(prop.Value, kind) ?? Walk(prop.Value, depth + 1);
                        }
                    }
                    return obj;

                case JArray arr:
                    for (int i = 0; i < arr.Count; i++)
                        arr[i] = Walk(arr[i], depth + 1);
                    return arr;

                case JValue val when val.Type == JTokenType.String:
                    return new JValue(CleanString(val.Value<string>() ?? string.Empty, depth));

                default:
                    return token;
            }
        }

        // valore sotto un nome riservato: testo non vuoto e numeri (se non contatori) diventano "***"; il resto null (resta com'e')
        private static JToken? MaskLeaf(JToken value, int kind)
        {
            if (value is not JValue v)
                return null;
            if (v.Type == JTokenType.String)
                return string.IsNullOrEmpty(v.Value<string>()) ? null : new JValue(Mask);
            if ((v.Type == JTokenType.Integer || v.Type == JTokenType.Float) && kind == SensitiveTextAndNumbers)
                return new JValue(Mask);
            return null;
        }

        private static string CleanString(string s, int depth)
        {
            // JSON dentro una stringa (es. campi "Dati" o "Payload"): si legge e si maschera per nome
            if (s.Length <= MaxJsonInStringLength && TryParseJson(s, out var inner))
            {
                var text = Walk(inner!, depth + 1).ToString(Formatting.None);
                return text.Length > MaxStringLength * 4 ? Abbreviate(text) : text;
            }

            // stringhe lunghe: si taglia PRIMA delle regex, poi si maschera il prefisso
            if (s.Length > MaxStringLength)
                return Abbreviate(s);

            return MaskFreeText(s);
        }

        // prefisso tagliato su un delimitatore, mascherato, con la lunghezza originale
        private static string Abbreviate(string s)
            => MaskFreeText(CutSafe(s, MaxStringLength / 2)) + "...[" + s.Length + " caratteri]";

        // taglia a max caratteri, ma se possibile su un delimitatore: un segreto a cavallo del taglio non resta in chiaro a meta'
        private static string CutSafe(string s, int max)
        {
            if (s.Length <= max)
                return s;

            int cut = max;
            for (int i = max; i > max - CutWindow && i > 0; i--)
            {
                if (IsDelimiter(s[i - 1]))
                {
                    cut = i;
                    break;
                }
            }
            return s.Substring(0, cut);
        }

        private static bool IsDelimiter(char c)
            => char.IsWhiteSpace(c) || c == '"' || c == '\'' || c == ',' || c == ';' || c == '&' || c == '=' || c == ':'
               || c == '{' || c == '}' || c == '[' || c == ']';

        private static string MaskFreeText(string s)
        {
            if (s.Length < 5)
                return s;

            // prima "Bearer xxx", poi le coppie nome/valore (cosi "Authorization: Bearer xxx" non lascia il token)
            s = TextBearer.Replace(s, m => m.Groups["k"].Value + " " + Mask);
            return TextPair.Replace(s, m =>
            {
                var value = m.Groups["v"].Value;
                if (value.Length == 0 || value == Mask)
                    return m.Value;

                int kind = Classify(m.Groups["k"].Value);
                if (kind == NotSensitive)
                    return m.Value;
                // "MaxTokens=100": un contatore numerico resta visibile
                if (kind == SensitiveText && value.All(char.IsDigit))
                    return m.Value;

                var quote = m.Groups["q"].Value;
                return m.Groups["k"].Value + m.Groups["kq"].Value + m.Groups["s"].Value + quote + Mask + quote;
            });
        }

        /// <summary>
        /// Contract resolver del solo log: le proprieta' con [LogSensitive] restituiscono sempre "***".
        /// Il mascheramento per nome e' fatto dopo, sull'albero JSON (cosi copre anche dizionari e JSON in stringa).
        /// </summary>
        private sealed class LogContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (member.GetCustomAttribute<LogSensitiveAttribute>(true) != null && property.ValueProvider != null)
                    property.ValueProvider = new MaskedValueProvider(property.ValueProvider);

                return property;
            }
        }

        private sealed class MaskedValueProvider : IValueProvider
        {
            private readonly IValueProvider _inner;

            public MaskedValueProvider(IValueProvider inner) => _inner = inner;

            public object? GetValue(object target) => _inner.GetValue(target) == null ? null : Mask;

            // sola lettura: il log non deserializza mai
            public void SetValue(object target, object? value) { }
        }

        /// <summary>byte[] nel log: solo la dimensione (niente base64 enormi).</summary>
        private sealed class ByteArrayLogConverter : JsonConverter
        {
            public override bool CanRead => false;

            public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                => writer.WriteValue(value is byte[] b ? "[byte[] " + b.Length + "]" : null);

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                => throw new NotSupportedException();
        }
    }
}
