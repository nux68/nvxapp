using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources
{
    // Pagine di Risorse Umane per il comando Navigate.
    // Derivate da route-human-resources.service.ts / main-menu-human-resources.service.ts (client).
    // "dipendenti", "impostazioni", "calendario hr" restano delle presenze (primo provider vince): qui "dipendenti hr",
    // "impostazioni hr". "formazione" e "corsi" sono di Risorse Umane (ANALISI_HUMANRESOURCES_VOCALI.md §3.3).
    public class HumanResourcesRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["risorse umane"]               = "/hremployeelist",
            ["dipendenti hr"]               = "/hremployeelist",
            ["anagrafica hr"]               = "/hremployeelist",
            ["dipendenti risorse umane"]    = "/hremployeelist",
            ["cedolini"]                    = "/hrpayslips",
            ["cedolini e cu"]               = "/hrpayslips",
            ["buste paga"]                  = "/hrpayslips",
            ["carica cedolini"]             = "/hrpayslipupload",
            ["caricamento cedolini"]        = "/hrpayslipupload",
            ["i miei cedolini"]             = "/hrmypayslips",
            ["miei cedolini"]               = "/hrmypayslips",
            ["le mie buste paga"]           = "/hrmypayslips",
            ["corsi"]                       = "/hrcourselist",
            ["formazione"]                  = "/hrcourselist",
            ["catalogo corsi"]              = "/hrcourselist",
            ["piani di formazione"]         = "/hrplanlist",
            ["piano di formazione"]         = "/hrplanlist",
            ["piano formazione"]            = "/hrplanlist",
            ["checklist hr"]                = "/hrchecklistlist",
            ["onboarding"]                  = "/hrchecklistlist",
            ["offboarding"]                 = "/hrchecklistlist",
            ["checklist onboarding"]        = "/hrchecklistlist",
            ["modelli checklist"]           = "/hrchecklisttemplatelist",
            ["modelli di checklist"]        = "/hrchecklisttemplatelist",
            ["impostazioni hr"]             = "/hrsettings",
            ["configurazione hr"]           = "/hrsettings",
            ["impostazioni risorse umane"]  = "/hrsettings",
            ["cedolini studio"]             = "/hrstudiopayslips",
            ["manuale risorse umane"]       = "/usermanual/humanresources",
            ["manuale hr"]                  = "/usermanual/humanresources",

            // nomi inglesi delle stesse pagine
            ["human resources"]             = "/hremployeelist",
            ["hr employees"]                = "/hremployeelist",
            ["payslips"]                    = "/hrpayslips",
            ["pay slips"]                   = "/hrpayslips",
            ["upload payslips"]             = "/hrpayslipupload",
            ["my payslips"]                 = "/hrmypayslips",
            ["courses"]                     = "/hrcourselist",
            ["training"]                    = "/hrcourselist",
            ["training plans"]              = "/hrplanlist",
            ["training plan"]               = "/hrplanlist",
            ["hr checklists"]               = "/hrchecklistlist",
            ["onboarding checklists"]       = "/hrchecklistlist",
            ["checklist templates"]         = "/hrchecklisttemplatelist",
            ["hr settings"]                 = "/hrsettings",
            ["hr manual"]                   = "/usermanual/humanresources",
            ["human resources manual"]      = "/usermanual/humanresources",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;

        // "cedolini": l'elenco dei caricamenti per l'ufficio del personale, quello dello studio per lo studio,
        // i propri per il dipendente (si dichiarano prima le pagine dell'amministrazione)
        public IReadOnlyList<RoleRoute> RoleRoutes { get; } = new[]
        {
            new RoleRoute("cedolini", "/hrpayslips", "CompanyPowerAdmin", "CompanyAdmin"),
            new RoleRoute("payslips", "/hrpayslips", "CompanyPowerAdmin", "CompanyAdmin"),
            new RoleRoute("buste paga", "/hrpayslips", "CompanyPowerAdmin", "CompanyAdmin"),
            new RoleRoute("cedolini", "/hrstudiopayslips", "FinancialAdvisorPowerAdmin", "FinancialAdvisorAdmin"),
            new RoleRoute("payslips", "/hrstudiopayslips", "FinancialAdvisorPowerAdmin", "FinancialAdvisorAdmin"),
            new RoleRoute("cedolini", "/hrmypayslips", "User"),
            new RoleRoute("payslips", "/hrmypayslips", "User"),
            new RoleRoute("buste paga", "/hrmypayslips", "User"),
        };

        public IReadOnlyList<string> PromptPages { get; } = new[]
        {
            "risorse umane", "dipendenti hr", "cedolini", "carica cedolini", "i miei cedolini", "corsi", "formazione", "piani di formazione",
            "checklist hr", "onboarding", "modelli checklist", "impostazioni hr", "manuale risorse umane",
            "human resources", "hr employees", "payslips", "my payslips", "courses", "training plans", "hr checklists", "hr settings"
        };
    }
}
