using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;

// ---------------------------------------------------------------------------
// Prova delle frasi della chat AI (vedi README.md)
//   dotnet run --project tools/ChatAiPhraseTest [-- --pausa 20] [--filtro testo] [--provider AI_groq]
// Ogni frase passa dal motore vero (ChatAIService) con il LLM configurato e i comandi veri; i servizi
// sono finti e i comandi non si eseguono: si registrano intent e valori e si confrontano con l'atteso.
// ---------------------------------------------------------------------------

var opzioni = Opzioni.Leggi(args);
CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo(opzioni.Lingua);

var root = Opzioni.RadiceRepository();
var config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(root, "nvxapp.server", "appsettings.json"), optional: false)
    .AddJsonFile(Path.Combine(root, "nvxapp.server", "appsettings.Development.json"), optional: true)
    .AddEnvironmentVariables("NVXAPP_")
    .AddInMemoryCollection(Opzioni.Sovrascritture(opzioni))
    .Build();

var provider = config["AI:UseLLM"] ?? "";
Console.WriteLine($"Provider: {provider}, modello: {config[$"AI:{provider}:model"]}, pausa tra le frasi: {opzioni.Pausa} s");

var casi = Caso.Carica(Path.Combine(AppContext.BaseDirectory, "frasi"), opzioni.Filtro);
var http = new ServiceCollection().AddHttpClient().BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

int ok = 0, ko = 0, saltati = 0;
foreach (var caso in casi)
{
    var esito = await Prova.Esegui(caso, config, http);
    switch (esito.Stato)
    {
        case "ok": ok++; Console.WriteLine($"✓ {caso.Titolo}  →  {esito.Ottenuto}"); break;
        case "saltato": saltati++; Console.WriteLine($"- {caso.Titolo}  ({esito.Ottenuto})"); break;
        default: ko++; Console.WriteLine($"✗ {caso.Titolo}\n    atteso:   {caso.Atteso}\n    ottenuto: {esito.Ottenuto}"); break;
    }
    if (esito.Stato != "saltato") await Task.Delay(TimeSpan.FromSeconds(opzioni.Pausa));
}

Console.WriteLine($"\nRisultato: {ok} riuscite, {ko} fallite, {saltati} saltate su {casi.Count}.");
return ko == 0 ? 0 : 1;

// ---------------------------------------------------------------------------

record Opzioni(int Pausa, string? Filtro, string? Provider, string Lingua)
{
    public static Opzioni Leggi(string[] args)
    {
        string? Val(string nome) { var i = Array.IndexOf(args, nome); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
        return new Opzioni(int.TryParse(Val("--pausa"), out var p) ? p : 20, Val("--filtro"), Val("--provider"), Val("--lingua") ?? "it-IT");
    }

    // cartella che contiene nvxapp.sln, risalendo da quella del programma
    public static string RadiceRepository()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "nvxapp.sln"))) return dir.FullName;
        throw new InvalidOperationException("nvxapp.sln non trovato risalendo dalla cartella del programma");
    }

    // provider scelto da riga di comando; la chiave, se non e' nell'ambiente del processo,
    // si legge da quello dell'utente o della macchina (NVXAPP_AI__<provider>__ApiKey)
    public static Dictionary<string, string?> Sovrascritture(Opzioni o)
    {
        var valori = new Dictionary<string, string?>();
        if (o.Provider != null) valori["AI:UseLLM"] = o.Provider;
        foreach (var p in new[] { "AI_groq", "AI_openrouter" })
        {
            var nome = $"NVXAPP_AI__{p}__ApiKey";
            var chiave = Environment.GetEnvironmentVariable(nome)
                         ?? Environment.GetEnvironmentVariable(nome, EnvironmentVariableTarget.User)
                         ?? Environment.GetEnvironmentVariable(nome, EnvironmentVariableTarget.Machine);
            if (chiave != null) valori[$"AI:{p}:ApiKey"] = chiave;
        }
        return valori;
    }
}

// Frase di prova (file frasi/*.json):
//   { "frase": "...", "intent": "NomeIntent" | "nessuno", "slots": { "nome": "valore" },
//     "ruolo": "CompanyPowerAdmin", "pagina": "/route", "messaggi": ["turno 1", "turno 2"] }
// I valori attesi che iniziano con "=" sono espressioni di data (es. "=ieri", "=lunedì scorso").
class Caso
{
    public string? Frase { get; set; }
    public List<string>? Messaggi { get; set; }
    public string Intent { get; set; } = "";
    public Dictionary<string, string> Slots { get; set; } = new();
    public string Ruolo { get; set; } = "CompanyPowerAdmin";
    public string Pagina { get; set; } = "";
    public string Origine { get; set; } = "";   // file delle frasi

    public List<string> Turni => Messaggi is { Count: > 0 } ? Messaggi : new() { Frase ?? "" };
    public string Titolo => $"[{Origine}] {string.Join(" / ", Turni)}" + (Ruolo != "CompanyPowerAdmin" ? $" ({Ruolo})" : "");
    public string Atteso => $"{Intent} {Formatta(Slots.ToDictionary(kv => kv.Key, kv => Valore(kv.Value)))}";

    public static string Valore(string v) => v.StartsWith('=') ? SlotNormalizers.Date(v[1..]) ?? v : v;
    public static string Formatta(IDictionary<string, string> slots) =>
        "{ " + string.Join(", ", slots.Select(kv => $"{kv.Key}: {kv.Value}")) + " }";

    public static List<Caso> Carica(string cartella, string? filtro)
    {
        var opzioni = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
        var casi = new List<Caso>();
        foreach (var file in Directory.GetFiles(cartella, "*.json").OrderBy(f => f))
            foreach (var caso in JsonSerializer.Deserialize<List<Caso>>(File.ReadAllText(file), opzioni) ?? new())
            {
                caso.Origine = Path.GetFileNameWithoutExtension(file);
                casi.Add(caso);
            }
        return casi.Where(c => filtro == null || c.Titolo.Contains(filtro, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

static class Prova
{
    public static async Task<(string Stato, string Ottenuto)> Esegui(Caso caso, IConfiguration config, IHttpClientFactory http)
    {
        var ctx = new ChatUserContext { UserId = Finti.UtenteCorrente, Roles = new List<string> { caso.Ruolo }, CurrentRoute = caso.Pagina };
        var registrate = new List<(string Intent, Dictionary<string, string> Slots)>();

        var handlers = Finti.Comandi(ctx);
        var catalog = new IntentCatalog(handlers);
        catalog.Fuilter4UserRoles(new List<string> { caso.Ruolo });
        Finti.Catalogo = catalog;

        if (caso.Intent != "nessuno" && !catalog.Intents.Any(i => i.Name == caso.Intent))
            return ("saltato", $"intent {caso.Intent} non presente per il ruolo {caso.Ruolo} in questo ramo");

        var store = new InMemoryChatSessionStore();
        var servizio = Finti.Crea<ChatAIService>(t =>
            t == typeof(IConfiguration) ? config
            : t == typeof(IHttpContextAccessor) ? new HttpContextAccessor()
            : t == typeof(IWebApiService) ? new WebApiService(http)
            : t == typeof(ICommandRegistry) ? new RegistroCheRegistra(handlers, registrate)
            : t == typeof(IIntentCatalog) ? catalog
            : t == typeof(IChatSessionStore) ? store
            : t == typeof(IChatUserContext) ? ctx
            : t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IOptions<>) ? Finti.Opzioni(t.GetGenericArguments()[0])
            : null);

        ChatAIOutModel? risposta = null;
        var sessionId = "";
        foreach (var turno in caso.Turni)
        {
            registrate.Clear();
            risposta = await servizio.ProcessMessageAsync(new ChatAIInModel { Request = turno, SessionId = sessionId, CurrentRoute = caso.Pagina });

            // limite di richieste del provider: si aspetta e si ripete il turno una volta
            if (risposta.ResponseType == "error" && risposta.Responce.Contains("TooManyRequests"))
            {
                Console.WriteLine("    (limite di richieste del provider: attendo 60 s)");
                await Task.Delay(TimeSpan.FromSeconds(60));
                risposta = await servizio.ProcessMessageAsync(new ChatAIInModel { Request = turno, SessionId = sessionId, CurrentRoute = caso.Pagina });
            }
            sessionId = risposta.SessionId;
        }

        // piano raccolto: in attesa (domanda o conferma) oppure eseguito subito (registrato)
        var sessione = store.TryGet(sessionId, ctx.UserId);
        var piano = sessione?.ActionQueue.Count > 0
            ? sessione.ActionQueue.Select(a => (a.Intent, a.Slots)).ToList()
            : registrate.ToList();

        if (piano.Count == 0)
        {
            var descrizione = $"nessuno ({risposta?.ResponseType}: {risposta?.Responce.Split('\n')[0]})";
            return (caso.Intent == "nessuno" ? "ok" : "ko", descrizione);
        }

        var (intent, slots) = piano[0];
        var ottenuto = $"{intent} {Caso.Formatta(slots)}" + (piano.Count > 1 ? $" (+{piano.Count - 1} azioni)" : "") + $" [{risposta?.ResponseType}]";
        var giusto = intent == caso.Intent &&
                     caso.Slots.All(kv => slots.TryGetValue(kv.Key, out var v) && string.Equals(v, Caso.Valore(kv.Value), StringComparison.OrdinalIgnoreCase));
        return (giusto ? "ok" : "ko", ottenuto);
    }
}

// Registro dei comandi che non esegue: registra intent e valori e risponde "ok"
class RegistroCheRegistra : ICommandRegistry
{
    private readonly Dictionary<string, ICommandHandler> _handlers;
    private readonly List<(string Intent, Dictionary<string, string> Slots)> _registrate;

    public RegistroCheRegistra(IEnumerable<ICommandHandler> handlers, List<(string, Dictionary<string, string>)> registrate)
    {
        _handlers = handlers.ToDictionary(h => h.IntentDefinition.Name, StringComparer.OrdinalIgnoreCase);
        _registrate = registrate;
    }

    public ICommandHandler Resolve(string intentName) => new Registra(_handlers[intentName], _registrate);

    private class Registra : ICommandHandler
    {
        private readonly ICommandHandler _vero;
        private readonly List<(string, Dictionary<string, string>)> _registrate;
        public Registra(ICommandHandler vero, List<(string, Dictionary<string, string>)> registrate) { _vero = vero; _registrate = registrate; }
        public IntentDefinition IntentDefinition => _vero.IntentDefinition;
        public Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            _registrate.Add((IntentDefinition.Name, new Dictionary<string, string>(slots)));
            return Task.FromResult(CommandResult.Ok("ok"));
        }
    }
}
