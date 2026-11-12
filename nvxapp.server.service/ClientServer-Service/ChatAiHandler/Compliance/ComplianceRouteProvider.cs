using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance
{
    // Pagine di Compliance per il comando Navigate. Derivate da route-compliance.service.ts / main-menu-compliance.service.ts (client).
    // Parole proprie del modulo: sicurezza, gdpr, privacy, data breach, dpi, dvr. "scadenze", "documenti", "formazione" e "corsi"
    // da soli non si usano (scadenzario comune, documentale, Risorse Umane): qui sempre qualificati (ANALISI_COMPLIANCE_VOCALI.md §3.3).
    // Le pagine lato studio (/cmpstudio) non ci sono: la chat dello studio ha solo i comandi comuni.
    public class ComplianceRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["compliance"]                 = "/cmp81",
            ["sicurezza"]                  = "/cmp81",
            ["sicurezza sul lavoro"]       = "/cmp81",
            ["sicurezza lavoro"]           = "/cmp81",
            ["adempimenti sicurezza"]      = "/cmp81",
            ["corsi sicurezza"]            = "/cmp81",
            ["formazione sicurezza"]       = "/cmp81",
            ["visite mediche"]             = "/cmp81",
            ["dpi"]                        = "/cmp81",
            ["dvr"]                        = "/cmp81",
            ["nomine sicurezza"]           = "/cmp81",
            ["registro gdpr"]              = "/cmpgdpr",
            ["gdpr"]                       = "/cmpgdpr",
            ["privacy"]                    = "/cmpgdpr",
            ["registro privacy"]           = "/cmpgdpr",
            ["registro trattamenti"]       = "/cmpgdpr",
            ["registro dei trattamenti"]   = "/cmpgdpr",
            ["data breach"]                = "/cmpgdpr",
            ["violazioni dei dati"]        = "/cmpgdpr",
            ["responsabili esterni"]       = "/cmpgdpr",
            ["i miei adempimenti"]         = "/cmpmine",
            ["miei adempimenti"]           = "/cmpmine",
            ["impostazioni compliance"]    = "/cmpsettings",
            ["configurazione compliance"]  = "/cmpsettings",
            ["manuale compliance"]         = "/usermanual/compliance",

            // nomi inglesi delle stesse pagine
            ["workplace safety"]           = "/cmp81",
            ["health and safety"]          = "/cmp81",
            ["safety"]                     = "/cmp81",
            ["safety training"]            = "/cmp81",
            ["medical examinations"]       = "/cmp81",
            ["ppe"]                        = "/cmp81",
            ["risk assessment"]            = "/cmp81",
            ["gdpr register"]              = "/cmpgdpr",
            ["privacy register"]           = "/cmpgdpr",
            ["processing register"]        = "/cmpgdpr",
            ["data breaches"]              = "/cmpgdpr",
            ["external processors"]        = "/cmpgdpr",
            ["my compliance"]              = "/cmpmine",
            ["my obligations"]             = "/cmpmine",
            ["compliance settings"]        = "/cmpsettings",
            ["compliance manual"]          = "/usermanual/compliance",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;

        public IReadOnlyList<string> PromptPages { get; } = new[]
        {
            "sicurezza sul lavoro", "registro gdpr", "i miei adempimenti", "impostazioni compliance", "manuale compliance",
            "workplace safety", "gdpr register", "my compliance", "compliance settings"
        };
    }
}
