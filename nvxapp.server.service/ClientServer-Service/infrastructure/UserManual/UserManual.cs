using nvxapp.server.data.Infrastructure.Tenancy;
using System.Globalization;
using System.Reflection;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual
{
    /*
     Manuali utente in PDF, uno per ramo logico (funzioni base, presenze, CRM, documentale), generati con un job
     (UserManualService) e scaricati dalla barra dei job della pagina.
     Ogni ramo registra un IUserManualProvider (registrazione automatica) e scrive il testo in Markdown semplice nei file
     nvxapp.server.service/Manuals/<codice>.<lingua>.md (risorse incorporate; manca la lingua -> italiano):
        # Titolo del manuale        (prima riga; la riga dopo, se non e' un titolo, e' il sottotitolo)
        ## Capitolo
        ### Paragrafo
        testo libero (righe consecutive = un paragrafo)
        - elenco puntato
        1. passaggio numerato
        > nota evidenziata
        **Ruoli:** chi la usa        (riga di testo come le altre)
     Capitoli e paragrafi per ruolo: elenco dei ruoli tra graffe in fondo al titolo, es.
        ## Configurazione {CompanyPowerAdmin}
        ### Approvare le richieste {CompanyPowerAdmin, CompanyAdmin}
     il capitolo (fino al ## successivo) o il paragrafo (fino al ### o ## successivo) resta solo per chi ha almeno uno
     di quei ruoli (quelli dell'utente con cui si sta lavorando, anche se impersonificato); senza graffe vale per tutti.
     Cosi' il manuale descrive solo le funzioni che l'utente puo' usare.
    */
    public interface IUserManualProvider
    {
        /// <summary>Codice del manuale (url /usermanual/&lt;codice&gt;, nome delle risorse Manuals/&lt;codice&gt;.&lt;lingua&gt;.md).</summary>
        string Code { get; }

        /// <summary>Applicativo che il manuale descrive: null = funzioni base (sempre disponibile).</summary>
        ApplicationType? Application { get; }

        /// <summary>Manuale nella lingua corrente, con i soli capitoli e paragrafi dei ruoli indicati.</summary>
        UserManualDocument Build(IReadOnlyCollection<string> ruoli);
    }

    public class UserManualDocument
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public List<UserManualBlock> Blocks { get; set; } = new();
    }

    public enum UserManualBlockType
    {
        Chapter = 1,
        Section = 2,
        Paragraph = 3,
        Bullet = 4,
        Step = 5,
        Note = 6,
    }

    public class UserManualBlock
    {
        public UserManualBlockType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        // numero del passaggio (Step)
        public int Number { get; set; }
    }

    /// <summary>Provider che legge il testo dalle risorse Markdown incorporate di un assembly (di solito questo).</summary>
    public abstract class UserManualMarkdownProvider : IUserManualProvider
    {
        public abstract string Code { get; }
        public abstract ApplicationType? Application { get; }

        public UserManualDocument Build(IReadOnlyCollection<string> ruoli)
        {
            var lingua = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var testo = Leggi(lingua) ?? Leggi("it") ?? throw new InvalidOperationException($"Manuale {Code} mancante (Manuals/{Code}.it.md)");
            return UserManualMarkdown.Parse(testo, ruoli);
        }

        private string? Leggi(string lingua)
        {
            var assembly = GetType().Assembly;
            var suffisso = $".Manuals.{Code}.{lingua}.md";
            var nome = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(suffisso, StringComparison.OrdinalIgnoreCase));
            if (nome == null)
                return null;
            using var stream = assembly.GetManifestResourceStream(nome)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static class UserManualMarkdown
    {
        public static UserManualDocument Parse(string testo, IReadOnlyCollection<string>? ruoli = null)
        {
            var doc = new UserManualDocument();
            var paragrafo = new List<string>();
            int passo = 0;
            // capitolo o paragrafo riservato ad altri ruoli: le righe fino al titolo successivo si saltano
            bool capitoloEscluso = false, paragrafoEscluso = false;

            // "Titolo {RuoloA, RuoloB}" -> titolo senza graffe e se l'utente lo puo' vedere (nessuna graffa = tutti)
            (string Titolo, bool Ammesso) Titolo(string t)
            {
                t = t.Trim();
                int g = t.LastIndexOf('{');
                if (g < 0 || !t.EndsWith('}'))
                    return (t, true);
                var richiesti = t[(g + 1)..^1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                bool ammesso = ruoli == null || richiesti.Length == 0 || richiesti.Any(r => ruoli.Contains(r, StringComparer.OrdinalIgnoreCase));
                return (t[..g].TrimEnd(), ammesso);
            }

            void Chiudi()
            {
                if (paragrafo.Count > 0)
                    doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = string.Join(" ", paragrafo) });
                paragrafo.Clear();
            }

            foreach (var grezza in testo.Replace("\r\n", "\n").Split('\n'))
            {
                var riga = grezza.Trim();
                if (riga.Length == 0)
                {
                    Chiudi();
                    passo = 0;
                    continue;
                }
                if (riga.StartsWith("## "))
                {
                    var (t, ammesso) = Titolo(riga[3..]);
                    capitoloEscluso = !ammesso;
                    paragrafoEscluso = false;
                    riga = "## " + t;
                }
                else if (riga.StartsWith("### "))
                {
                    var (t, ammesso) = Titolo(riga[4..]);
                    paragrafoEscluso = !ammesso;
                    riga = "### " + t;
                }
                if (capitoloEscluso || paragrafoEscluso)
                {
                    Chiudi();
                    continue;
                }
                if (riga.StartsWith("# "))
                {
                    Chiudi();
                    doc.Title = riga[2..].Trim();
                    continue;
                }
                if (doc.Blocks.Count == 0 && doc.Subtitle == null && doc.Title.Length > 0 && !riga.StartsWith('#') && paragrafo.Count == 0 && !riga.StartsWith("- ") && !riga.StartsWith('>'))
                {
                    doc.Subtitle = riga;
                    continue;
                }
                UserManualBlockType? tipo = null;
                string contenuto = riga;
                if (riga.StartsWith("### ")) { tipo = UserManualBlockType.Section; contenuto = riga[4..]; }
                else if (riga.StartsWith("## ")) { tipo = UserManualBlockType.Chapter; contenuto = riga[3..]; passo = 0; }
                else if (riga.StartsWith("- ") || riga.StartsWith("* ")) { tipo = UserManualBlockType.Bullet; contenuto = riga[2..]; }
                else if (riga.StartsWith('>')) { tipo = UserManualBlockType.Note; contenuto = riga.TrimStart('>').Trim(); }
                else
                {
                    int punto = riga.IndexOf(". ", StringComparison.Ordinal);
                    if (punto > 0 && punto <= 3 && riga[..punto].All(char.IsDigit)) { tipo = UserManualBlockType.Step; contenuto = riga[(punto + 2)..]; }
                }

                if (tipo == null)
                {
                    paragrafo.Add(riga);
                    continue;
                }
                Chiudi();
                var blocco = new UserManualBlock { Type = tipo.Value, Text = contenuto.Trim() };
                if (tipo == UserManualBlockType.Step)
                    blocco.Number = ++passo;
                doc.Blocks.Add(blocco);
            }
            Chiudi();
            return doc;
        }
    }

    // manuale delle funzioni base (Infrastructure): accesso, menu, assistenza, utenti e aziende, anagrafiche, applicativi
    public class InfrastructureUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "infrastructure";
        public override ApplicationType? Application => null;
    }
}
