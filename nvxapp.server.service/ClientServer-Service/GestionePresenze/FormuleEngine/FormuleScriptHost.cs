using Acornima;
using Acornima.Ast;
using Jint;
using Jint.Runtime;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /// <summary>Errore di sintassi o di membro inesistente, con la riga e la colonna nel testo della formula.</summary>
    public class FormulaErrore
    {
        public int Riga { get; set; }
        public int Colonna { get; set; }
        public string Messaggio { get; set; } = string.Empty;
    }

    /*
     Esecuzione delle formule sul server con Jint (interprete JavaScript scritto in C#, nessun Node e nessun browser).
     - il testo viene avvolto in una funzione, cosi' un "return" al primo livello e' valido (riga 1 di scarto);
     - la formula vede solo "x" (FormulaContext): il CLR non e' esposto, niente file/rete/DB;
     - limiti per ogni esecuzione: tempo, istruzioni, memoria, ricorsione: superati = errore della sola formula;
     - gli script preparati (parse) stanno in cache per impronta del testo.
    */
    public static class FormuleScriptHost
    {
        public const int MaxLunghezzaTesto = 20000;
        private const int RigheScarto = 1;
        private const int MaxCache = 2000;

        private static readonly ConcurrentDictionary<string, Prepared<Script>> _cache = new();

        public static string Impronta(string? testo)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(testo ?? string.Empty)));

        private static string Avvolgi(string? testo) => "(function(){\n" + (testo ?? string.Empty) + "\n})();";

        /// <summary>Engine con i limiti delle formule: uno per calcolo, riusato per tutte le esecuzioni (un thread alla volta).</summary>
        public static Engine NuovoEngine()
            => new Engine(o => o
                .TimeoutInterval(TimeSpan.FromMilliseconds(500))
                .MaxStatements(50_000)
                .LimitMemory(16_000_000)
                .LimitRecursion(32));

        private static Prepared<Script> Prepara(string testo)
        {
            if (_cache.Count > MaxCache)
                _cache.Clear();
            return _cache.GetOrAdd(Impronta(testo), _ => Engine.PrepareScript(Avvolgi(testo)));
        }

        /// <summary>Esegue la formula con "x" = contesto. Le eccezioni (sintassi, runtime, limiti) salgono a chi chiama.</summary>
        public static void Esegui(Engine engine, string testo, FormulaContext x)
        {
            engine.SetValue("x", x);
            engine.Execute(Prepara(testo));
        }

        /// <summary>Messaggio leggibile di un errore di esecuzione, con la riga della formula quando nota.</summary>
        public static string MessaggioErrore(Exception ex)
        {
            switch (ex)
            {
                case FormulaException fe:
                    return fe.Message;
                case ParseErrorException pe:
                    return $"riga {pe.LineNumber - RigheScarto}, col {pe.Column}: {pe.Description}";
                case JavaScriptException je:
                    var riga = je.Location.Start.Line - RigheScarto;
                    return riga > 0 ? $"riga {riga}: {je.Message}" : je.Message;
                case TimeoutException:
                    return "tempo massimo di esecuzione superato (ciclo infinito?)";
                case StatementsCountOverflowException:
                    return "numero massimo di istruzioni superato (ciclo infinito?)";
                case MemoryLimitExceededException:
                    return "memoria massima superata";
                case RecursionDepthOverflowException:
                    return "ricorsione troppo profonda";
                case TargetInvocationException tie when tie.InnerException != null:
                    return MessaggioErrore(tie.InnerException);
                default:
                    return ex.InnerException is FormulaException inner ? inner.Message : ex.Message;
            }
        }

        /// <summary>
        /// Controllo della formula senza eseguirla: sintassi JavaScript e membri di "x" inesistenti
        /// (x.Pippo, x.Causale.Pippo). Con <paramref name="codici"/> controlla anche i codici scritti tra virgolette
        /// nelle chiamate x.Causale.* e x.Giustificativo.* (es. x.Causale.GetMin("ORDI") con ORDI inesistente).
        /// Lista vuota = formula corretta.
        /// </summary>
        public static List<FormulaErrore> CtrlSintassi(string? testo, FormulaCodici? codici = null)
        {
            var errori = new List<FormulaErrore>();
            if (string.IsNullOrWhiteSpace(testo))
            {
                errori.Add(new FormulaErrore { Riga = 1, Colonna = 1, Messaggio = "formula vuota" });
                return errori;
            }
            if (testo.Length > MaxLunghezzaTesto)
            {
                errori.Add(new FormulaErrore { Riga = 1, Colonna = 1, Messaggio = $"formula troppo lunga (massimo {MaxLunghezzaTesto} caratteri)" });
                return errori;
            }

            Script script;
            try
            {
                script = new Parser().ParseScript(Avvolgi(testo));
            }
            catch (ParseErrorException pe)
            {
                var righeTesto = testo.Split('\n');
                var riga = pe.LineNumber - RigheScarto;
                // errore sulla chiusura del wrapper: parentesi, graffa o stringa non chiusa nella formula
                if (riga > righeTesto.Length)
                    errori.Add(new FormulaErrore
                    {
                        Riga = righeTesto.Length,
                        Colonna = righeTesto[^1].Length + 1,
                        Messaggio = "parentesi, graffa o stringa non chiusa (" + (pe.Description ?? pe.Message) + ")"
                    });
                else
                    errori.Add(new FormulaErrore
                    {
                        Riga = Math.Max(1, riga),
                        Colonna = pe.Column + 1,
                        Messaggio = pe.Description ?? pe.Message
                    });
                return errori;
            }

            ControllaMembri(script, errori);
            if (codici != null)
                ControllaCodici(script, codici, errori);
            return errori;
        }

        // x.Causale.Metodo("COD", ...) e x.Giustificativo.Metodo("COD"): i codici scritti come testo devono esistere nell'azienda
        private static void ControllaCodici(Node nodo, FormulaCodici codici, List<FormulaErrore> errori)
        {
            if (nodo is CallExpression call && call.Callee is MemberExpression metodo && !metodo.Computed
                && metodo.Object is MemberExpression oggetto && !oggetto.Computed
                && oggetto.Object is Identifier x && x.Name == "x" && oggetto.Property is Identifier nomeOggetto
                && (nomeOggetto.Name == "Causale" || nomeOggetto.Name == "Giustificativo"))
            {
                bool causale = nomeOggetto.Name == "Causale";
                IEnumerable<string> validi = causale ? codici.Causali.Keys : codici.Giustificativi.Keys;
                foreach (var arg in call.Arguments)
                {
                    if (arg is not Literal lit || lit.Value is not string codice)
                        continue;
                    bool esiste = causale ? codici.Causali.ContainsKey(codice.Trim()) : codici.Giustificativi.ContainsKey(codice.Trim());
                    if (!esiste)
                        errori.Add(new FormulaErrore
                        {
                            Riga = Math.Max(1, arg.Location.Start.Line - RigheScarto),
                            Colonna = arg.Location.Start.Column + 1,
                            Messaggio = $"{(causale ? "Causale" : "Giustificativo")} '{codice}' inesistente. {FormulaCodici.Suggerimenti(codice, validi)}"
                        });
                }
            }

            foreach (var figlio in nodo.ChildNodes)
                ControllaCodici(figlio, codici, errori);
        }

        // x.Membro e x.Oggetto.Membro devono esistere nella facciata (membri [FormulaApi])
        private static void ControllaMembri(Node nodo, List<FormulaErrore> errori)
        {
            if (nodo is MemberExpression me && !me.Computed && me.Property is Identifier prop)
            {
                Type? tipo = null;
                if (me.Object is Identifier id && id.Name == "x")
                    tipo = typeof(FormulaContext);
                else if (me.Object is MemberExpression padre && !padre.Computed && padre.Object is Identifier idp && idp.Name == "x"
                         && padre.Property is Identifier propPadre)
                    tipo = FormuleCatalogo.TipoMembro(typeof(FormulaContext), propPadre.Name);

                if (tipo != null && !FormuleCatalogo.EsisteMembro(tipo, prop.Name))
                {
                    var nome = tipo == typeof(FormulaContext) ? "x" : "x." + ((Identifier)((MemberExpression)me.Object).Property).Name;
                    errori.Add(new FormulaErrore
                    {
                        Riga = Math.Max(1, prop.Location.Start.Line - RigheScarto),
                        Colonna = prop.Location.Start.Column + 1,
                        Messaggio = $"'{nome}.{prop.Name}' non esiste"
                    });
                }
            }

            foreach (var figlio in nodo.ChildNodes)
                ControllaMembri(figlio, errori);
        }
    }
}
