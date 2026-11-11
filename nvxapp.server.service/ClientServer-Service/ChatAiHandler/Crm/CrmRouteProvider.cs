using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm
{
    // Pagine del CRM per il comando Navigate. Derivate da route-crm.service.ts / main-menu-crm.service.ts (client).
    // "clienti" e "attività" restano le pagine delle presenze (primo provider vince): qui "clienti crm", "agenda".
    public class CrmRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["crm"]                   = "/crmcustomerlist",
            ["clienti crm"]           = "/crmcustomerlist",
            ["crm clienti"]           = "/crmcustomerlist",
            ["anagrafica clienti crm"] = "/crmcustomerlist",
            ["lead"]                  = "/crmleadlist",
            ["potenziali clienti"]    = "/crmleadlist",
            ["pipeline"]              = "/crmpipeline",
            ["opportunità"]           = "/crmpipeline",
            ["opportunita"]           = "/crmpipeline",
            ["trattative"]            = "/crmpipeline",
            ["agenda"]                = "/crmactivitylist",
            ["agenda crm"]            = "/crmactivitylist",
            ["attività crm"]          = "/crmactivitylist",
            ["attivita crm"]          = "/crmactivitylist",
            ["offerte"]               = "/crmofferlist",
            ["preventivi"]            = "/crmofferlist",
            ["report crm"]            = "/crmreport",
            ["statistiche crm"]       = "/crmreport",
            ["impostazioni crm"]      = "/crmsettings",
            ["configurazione crm"]    = "/crmsettings",
            ["tabelle crm"]           = "/crmparlist",
            ["riassegna"]             = "/crmreassign",
            ["riassegnazione"]        = "/crmreassign",
            ["manuale crm"]           = "/usermanual/crm",

            // nomi inglesi delle stesse pagine
            ["crm customers"]         = "/crmcustomerlist",
            ["leads"]                 = "/crmleadlist",
            ["opportunities"]         = "/crmpipeline",
            ["deals"]                 = "/crmpipeline",
            ["crm agenda"]            = "/crmactivitylist",
            ["crm activities"]        = "/crmactivitylist",
            ["offers"]                = "/crmofferlist",
            ["quotes"]                = "/crmofferlist",
            ["crm report"]            = "/crmreport",
            ["crm settings"]          = "/crmsettings",
            ["crm tables"]            = "/crmparlist",
            ["reassign"]              = "/crmreassign",
            ["crm manual"]            = "/usermanual/crm",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;

        public IReadOnlyList<string> PromptPages { get; } = new[]
        {
            "clienti crm", "lead", "pipeline", "agenda crm", "offerte", "report crm", "impostazioni crm", "tabelle crm", "riassegna", "manuale crm",
            "crm customers", "leads", "crm agenda", "offers", "crm report", "crm settings"
        };
    }
}
