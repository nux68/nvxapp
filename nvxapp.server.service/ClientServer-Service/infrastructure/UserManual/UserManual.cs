using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual
{
    /*
     Manuali utente in PDF, uno per ramo logico (funzioni base, presenze, CRM, documentale), generati con un job
     (UserManualService) e scaricati dalla barra dei job della pagina.

     Il manuale si COMPONE dal menu reale dell'utente (lo manda il client: sezioni e voci che vede in quel momento, cioe'
     ruoli, applicativi attivi e voci nascoste dal "Menu utenti"): un capitolo per sezione del menu, un paragrafo per voce,
     con la schermata della pagina se c'e'. Cosi' il manuale segue le funzioni correnti del programma: una voce nuova
     compare subito (con un testo generico finche' non ha la sua descrizione), una voce tolta sparisce.

     Ogni ramo registra un IUserManualProvider (registrazione automatica) e scrive i testi in
     nvxapp.server.service/Manuals/<codice>.<lingua>.md (risorse incorporate; manca la lingua -> italiano):

        # Titolo del manuale
        sottotitolo                         (riga dopo il titolo)
        ## Introduzione                     capitoli iniziali (prima del primo @@): sempre all'inizio
        ...testo...
        @@ /crmpipeline                     descrizione della funzione all'indirizzo /crmpipeline (fino al @@ o ## successivo)
        ...testo...
        ## Domande frequenti                capitoli dopo i blocchi @@: in fondo al manuale

     Testo: righe consecutive = un paragrafo; "- " elenco puntato; "1. " passaggio numerato; "> " nota; **grassetto**;
     "### " titoletto. Capitoli e titoletti dell'introduzione e dell'appendice per ruolo: ruoli tra graffe in fondo al
     titolo (## Configurazione {CompanyPowerAdmin}); restano solo per chi ha almeno uno di quei ruoli.

     Schermate: nvxapp.server.service/Manuals/img/<indirizzo>.jpg (indirizzo senza "/" iniziale, "/" interni -> "_"),
     incorporate; si rigenerano con lo script nvxapp.server/Tools/ManualScreenshots (vedi il suo README).

     GUIDA (file fisso che completa il manuale): nvxapp.server.service/Manuals/<codice>.guide.<lingua>.md, facoltativa
     (manca la lingua -> italiano), con la stessa sintassi del testo base. Si unisce cosi':
        capitoli prima del primo @@      dopo l'introduzione del testo base (es. "Primi passi", "Concetti")
        @@ /indirizzo                    dopo la descrizione base della funzione (es. "### Campi", "### Esempio",
                                         "### Attenzione"); solo se la funzione e' nel menu dell'utente
        capitoli dopo i blocchi @@       dopo l'appendice del testo base (es. "Glossario", "Risoluzione dei problemi")
     Sintassi in piu' (valida anche nel testo base):
        | Campo | Descrizione |          tabella: righe consecutive che iniziano con "|"; la prima e' l'intestazione,
        |---|---|                        la riga di soli trattini si ignora
        => /crmpipeline /crmofferlist    "Vedi anche": funzioni collegate, mostrate con il loro titolo solo se sono nel
                                         menu dell'utente (le altre si omettono)

     Arricchimenti automatici (dal menu dell'utente, senza testi da scrivere): per ogni capitolo l'elenco delle sue
     funzioni, per ogni funzione dove si trova nel menu, in fondo l'indice alfabetico delle funzioni.
    */
    public interface IUserManualProvider
    {
        /// <summary>Codice del manuale (url /usermanual/&lt;codice&gt;, risorse Manuals/&lt;codice&gt;.&lt;lingua&gt;.md).</summary>
        string Code { get; }

        /// <summary>Applicativo che il manuale descrive: null = funzioni base (sempre disponibile).</summary>
        ApplicationType? Application { get; }

        /// <summary>Manuale nella lingua corrente, composto dal menu dell'utente e filtrato sui suoi ruoli.</summary>
        UserManualDocument Build(IReadOnlyCollection<string> ruoli, IReadOnlyList<UserManualMenuSection> menu);
    }

    // sezione del menu dell'utente (titolo gia' tradotto) con le sue voci
    public class UserManualMenuSection
    {
        public string Titolo { get; set; } = string.Empty;
        public List<UserManualMenuItem> Voci { get; set; } = new();
    }

    public class UserManualMenuItem
    {
        public string Titolo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
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
        Image = 7,
        Subsection = 8,
        Table = 9,      // Rows: la prima riga e' l'intestazione
        Path = 10,      // dove si trova la funzione nel menu (generato)
        SeeAlso = 11,   // funzioni collegate: nel testo gli indirizzi, nel documento composto i titoli
    }

    public class UserManualBlock
    {
        public UserManualBlockType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        // numero del passaggio (Step)
        public int Number { get; set; }
        // immagine (Image)
        public byte[]? Data { get; set; }
        // righe della tabella (Table), la prima e' l'intestazione
        public List<string[]>? Rows { get; set; }
    }

    /// <summary>Provider che legge testi e schermate dalle risorse incorporate dell'assembly (di solito questo).</summary>
    public abstract class UserManualMarkdownProvider : IUserManualProvider
    {
        public abstract string Code { get; }
        public abstract ApplicationType? Application { get; }

        public UserManualDocument Build(IReadOnlyCollection<string> ruoli, IReadOnlyList<UserManualMenuSection> menu)
        {
            var lingua = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var testo = Leggi($".Manuals.{Code}.{lingua}.md") ?? Leggi($".Manuals.{Code}.it.md")
                        ?? throw new InvalidOperationException($"Manuale {Code} mancante (Manuals/{Code}.it.md)");
            var parti = UserManualMarkdown.Parse(testo, ruoli);
            // guida fissa che completa il manuale (facoltativa)
            var testoGuida = Leggi($".Manuals.{Code}.guide.{lingua}.md") ?? Leggi($".Manuals.{Code}.guide.it.md");
            var guida = testoGuida == null ? null : UserManualMarkdown.Parse(testoGuida, ruoli);
            return UserManualMarkdown.Componi(parti, guida, menu, url => Immagine(url));
        }

        // schermata della pagina: Manuals/img/<indirizzo>.jpg (o .png)
        private byte[]? Immagine(string url)
        {
            var nome = UserManualMarkdown.NomeImmagine(url);
            var assembly = GetType().Assembly;
            var risorsa = assembly.GetManifestResourceNames()
                                  .FirstOrDefault(n => n.EndsWith($".Manuals.img.{nome}.jpg", StringComparison.OrdinalIgnoreCase)
                                                    || n.EndsWith($".Manuals.img.{nome}.png", StringComparison.OrdinalIgnoreCase));
            if (risorsa == null)
                return null;
            using var stream = assembly.GetManifestResourceStream(risorsa)!;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private string? Leggi(string suffisso)
        {
            var assembly = GetType().Assembly;
            var nome = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(suffisso, StringComparison.OrdinalIgnoreCase));
            if (nome == null)
                return null;
            using var stream = assembly.GetManifestResourceStream(nome)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    // testo del manuale diviso nelle sue parti (vedi il commento in cima)
    public class UserManualParti
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public List<UserManualBlock> Introduzione { get; set; } = new();
        public Dictionary<string, List<UserManualBlock>> Funzioni { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<UserManualBlock> Appendice { get; set; } = new();
    }

    public static class UserManualMarkdown
    {
        // indirizzi del menu che non sono funzioni da descrivere
        private static readonly string[] Esclusi = { "/usermanual", "/login", "/logout", "/userimpersonate" };

        public static string NomeImmagine(string url) => url.Trim().TrimStart('/').Replace('/', '_').ToLowerInvariant();

        public static UserManualParti Parse(string testo, IReadOnlyCollection<string>? ruoli = null)
        {
            var parti = new UserManualParti();
            List<UserManualBlock> corrente = parti.Introduzione;
            bool funzioniIniziate = false;
            var paragrafo = new List<string>();
            int passo = 0;
            bool capitoloEscluso = false, paragrafoEscluso = false;
            // tabella in costruzione (righe consecutive che iniziano con "|")
            UserManualBlock? tabella = null;

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
                    corrente.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = string.Join(" ", paragrafo) });
                paragrafo.Clear();
            }

            foreach (var grezza in testo.Replace("\r\n", "\n").Split('\n'))
            {
                var riga = grezza.Trim();
                if (riga.Length == 0)
                {
                    Chiudi();
                    passo = 0;
                    tabella = null;
                    continue;
                }
                if (!riga.StartsWith('|'))
                    tabella = null;
                if (riga.StartsWith("@@"))
                {
                    // descrizione di una funzione del menu
                    Chiudi();
                    funzioniIniziate = true;
                    capitoloEscluso = paragrafoEscluso = false;
                    var url = riga[2..].Trim();
                    corrente = new List<UserManualBlock>();
                    parti.Funzioni[url] = corrente;
                    passo = 0;
                    continue;
                }
                if (riga.StartsWith("# "))
                {
                    Chiudi();
                    parti.Title = riga[2..].Trim();
                    continue;
                }
                if (riga.StartsWith("## "))
                {
                    Chiudi();
                    // dopo i blocchi delle funzioni i capitoli vanno in appendice
                    if (funzioniIniziate)
                        corrente = parti.Appendice;
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
                if (riga.StartsWith('|'))
                {
                    // riga di tabella; quella di soli trattini (separatore dell'intestazione) si ignora
                    Chiudi();
                    var celle = riga.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                    if (celle.All(c => c.Length > 0 && c.All(ch => ch == '-' || ch == ':')))
                        continue;
                    if (tabella == null)
                    {
                        tabella = new UserManualBlock { Type = UserManualBlockType.Table, Rows = new List<string[]>() };
                        corrente.Add(tabella);
                    }
                    tabella.Rows!.Add(celle);
                    continue;
                }
                if (riga.StartsWith("=>"))
                {
                    // "Vedi anche": indirizzi delle funzioni collegate, risolti con i titoli del menu in Componi
                    Chiudi();
                    corrente.Add(new UserManualBlock { Type = UserManualBlockType.SeeAlso, Text = riga[2..].Trim() });
                    continue;
                }
                if (parti.Introduzione.Count == 0 && parti.Subtitle == null && parti.Title.Length > 0 && corrente == parti.Introduzione
                    && paragrafo.Count == 0 && !riga.StartsWith('#') && !riga.StartsWith("- ") && !riga.StartsWith('>'))
                {
                    parti.Subtitle = riga;
                    continue;
                }
                UserManualBlockType? tipo = null;
                string contenuto = riga;
                if (riga.StartsWith("### ")) { tipo = corrente == parti.Introduzione || corrente == parti.Appendice ? UserManualBlockType.Section : UserManualBlockType.Subsection; contenuto = riga[4..]; }
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
                corrente.Add(blocco);
            }
            Chiudi();
            return parti;
        }

        /// <summary>Manuale composto: introduzione, un capitolo per sezione del menu (una voce = un paragrafo con schermata e descrizione), appendice.</summary>
        public static UserManualDocument Componi(UserManualParti parti, IReadOnlyList<UserManualMenuSection> menu, Func<string, byte[]?> immagine)
            => Componi(parti, null, menu, immagine);

        /// <summary>
        /// Manuale composto dal testo base e dalla guida fissa (facoltativa): introduzione (base + guida), un capitolo per
        /// sezione del menu con l'elenco delle sue funzioni, per ogni voce dove si trova nel menu, schermata, descrizione base
        /// e approfondimenti della guida, poi appendice (base + guida) e indice alfabetico delle funzioni.
        /// </summary>
        public static UserManualDocument Componi(UserManualParti parti, UserManualParti? guida, IReadOnlyList<UserManualMenuSection> menu, Func<string, byte[]?> immagine)
        {
            var doc = new UserManualDocument { Title = parti.Title, Subtitle = parti.Subtitle };

            // voci del menu da descrivere, per sezione (senza doppioni e senza le pagine di servizio)
            var sezioni = menu.Select(sezione => (sezione.Titolo, Voci: sezione.Voci
                                  .Where(v => !string.IsNullOrWhiteSpace(v.Url) && !Esclusi.Any(e => v.Url.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
                                  .GroupBy(v => v.Url, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList()))
                              .Where(s => s.Voci.Count > 0).ToList();
            // titolo di ogni funzione presente nel menu dell'utente, per i "Vedi anche"
            var titoli = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (titoloSezione, voci) in sezioni)
                foreach (var v in voci)
                    titoli.TryAdd(v.Url, v.Titolo);

            void Aggiungi(IEnumerable<UserManualBlock> blocchi)
            {
                foreach (var b in blocchi)
                {
                    if (b.Type != UserManualBlockType.SeeAlso)
                    {
                        doc.Blocks.Add(b);
                        continue;
                    }
                    // solo le funzioni collegate che l'utente vede, con il loro titolo
                    var collegate = b.Text.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(u => titoli.TryGetValue(u, out var t) ? t : null)
                                          .Where(t => t != null).Distinct().ToList();
                    if (collegate.Count > 0)
                        doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.SeeAlso, Text = InfrastructureMessages.Get("UserManualSeeAlso", string.Join(", ", collegate)) });
                }
            }

            Aggiungi(parti.Introduzione);
            if (guida != null)
                Aggiungi(guida.Introduzione);

            foreach (var (titoloSezione, voci) in sezioni)
            {
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Chapter, Text = titoloSezione });
                if (voci.Count > 1)
                    doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = InfrastructureMessages.Get("UserManualInChapter", string.Join(", ", voci.Select(v => v.Titolo))) });
                foreach (var voce in voci)
                {
                    doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Section, Text = voce.Titolo });
                    doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Path, Text = InfrastructureMessages.Get("UserManualPath", titoloSezione + " › " + voce.Titolo) });
                    var img = immagine(voce.Url);
                    if (img != null)
                        doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Image, Data = img });
                    bool descritta = false;
                    if (parti.Funzioni.TryGetValue(voce.Url, out var testo) && testo.Count > 0)
                    {
                        Aggiungi(testo);
                        descritta = true;
                    }
                    if (guida != null && guida.Funzioni.TryGetValue(voce.Url, out var approfondimento) && approfondimento.Count > 0)
                    {
                        Aggiungi(approfondimento);
                        descritta = true;
                    }
                    if (!descritta)
                        doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Paragraph, Text = InfrastructureMessages.Get("UserManualNoDescription", voce.Titolo, titoloSezione) });
                }
            }

            Aggiungi(parti.Appendice);
            if (guida != null)
                Aggiungi(guida.Appendice);

            // indice alfabetico delle funzioni descritte, con la sezione del menu in cui si trovano
            var indice = sezioni.SelectMany(s => s.Voci.Select(v => new[] { v.Titolo, s.Titolo }))
                                .OrderBy(r => r[0], StringComparer.CurrentCultureIgnoreCase).ToList();
            if (indice.Count > 0)
            {
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Chapter, Text = InfrastructureMessages.Get("UserManualFunctionIndex") });
                var righe = new List<string[]> { new[] { InfrastructureMessages.Get("UserManualIndexFunction"), InfrastructureMessages.Get("UserManualIndexSection") } };
                righe.AddRange(indice);
                doc.Blocks.Add(new UserManualBlock { Type = UserManualBlockType.Table, Rows = righe });
            }
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
