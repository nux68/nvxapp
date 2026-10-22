using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Route del modulo AttendanceTracking.
    // Derivate da MainMenuAttendanceTrackingService (client Angular).
    public class AttendanceTrackingRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- User ---
            ["timbratura"]                = "/timeclockuser",
            ["timeclockuser"]             = "/timeclockuser",
            ["presenze"]                  = "/timeclockuser",
            ["richieste"]                 = "/requestlistuser",
            ["requestlistuser"]           = "/requestlistuser",
            ["calendario"]                = "/usertimesheet",
            ["usertimesheet"]             = "/usertimesheet",
            ["richiesta giustificativo"]  = "/requestjustificationuser",
            ["giustificativo"]            = "/requestjustificationuser",
            ["richiesta timbratura"]      = "/requestclockinguser",

            // --- CompanyAdmin / PowerAdmin ---
            ["richieste admin"]           = "/requestlistadmin",
            ["requestlistadmin"]          = "/requestlistadmin",
            ["calendario hr"]             = "/poweradmintimesheet",
            ["poweradmintimesheet"]       = "/poweradmintimesheet",
            ["calendario admin"]          = "/admintimesheet",
            ["admintimesheet"]            = "/admintimesheet",
            ["utenti reparto"]            = "/userdepartmentlist",
            ["export causali"]            = "/exportcausali",
            ["statistiche attività"]      = "/activitystatistics",
            ["statistiche"]               = "/activitystatistics",
            ["personale presente"]        = "/presentstaff",
            ["piano ferie"]               = "/vacationplan",
            ["giustificativi"]            = "/justificationlist",
            ["causali"]                   = "/causalilist",
            ["configurazione"]            = "/companycfgedit",
            ["impostazioni"]              = "/companycfgedit",
            ["settings"]                  = "/companycfgedit",
            ["orari"]                     = "/orarilist",
            ["profili orari"]             = "/profiliorarilist",
            ["modelli export"]            = "/exportcaulist",
            ["commesse"]                  = "/commessalist",
            ["clienti"]                   = "/customerlist",
            ["reparti"]                   = "/departmentlist",
            ["sedi"]                      = "/azsedilist",
            ["attività"]                  = "/activitylist",
            ["competenze"]                = "/competencelist",

            // nomi inglesi delle stesse pagine (l'utente puo' usare entrambe le lingue)
            // --- User ---
            ["clocking"]                  = "/timeclockuser",
            ["time clock"]                = "/timeclockuser",
            ["clock in"]                  = "/timeclockuser",
            ["attendance"]                = "/timeclockuser",
            ["requests"]                  = "/requestlistuser",
            ["my requests"]               = "/requestlistuser",
            ["calendar"]                  = "/usertimesheet",
            ["timesheet"]                 = "/usertimesheet",
            ["my timesheet"]              = "/usertimesheet",
            ["leave request"]             = "/requestjustificationuser",
            ["justification request"]     = "/requestjustificationuser",
            ["clocking request"]          = "/requestclockinguser",

            // --- CompanyAdmin / PowerAdmin ---
            ["admin requests"]            = "/requestlistadmin",
            ["hr calendar"]               = "/poweradmintimesheet",
            ["hr timesheet"]              = "/poweradmintimesheet",
            ["admin calendar"]            = "/admintimesheet",
            ["admin timesheet"]           = "/admintimesheet",
            ["department users"]          = "/userdepartmentlist",
            ["attendance code export"]    = "/exportcausali",
            ["export attendance codes"]   = "/exportcausali",
            ["activity statistics"]       = "/activitystatistics",
            ["statistics"]                = "/activitystatistics",
            ["present staff"]             = "/presentstaff",
            ["staff present"]             = "/presentstaff",
            ["vacation plan"]             = "/vacationplan",
            ["holiday plan"]              = "/vacationplan",
            ["leave types"]               = "/justificationlist",
            ["justifications"]            = "/justificationlist",
            ["attendance codes"]          = "/causalilist",
            ["configuration"]             = "/companycfgedit",
            ["company settings"]          = "/companycfgedit",
            ["work schedules"]            = "/orarilist",
            ["schedules"]                 = "/orarilist",
            ["schedule profiles"]         = "/profiliorarilist",
            ["export templates"]          = "/exportcaulist",
            ["projects"]                  = "/commessalist",
            ["job orders"]                = "/commessalist",
            ["customers"]                 = "/customerlist",
            ["clients"]                   = "/customerlist",
            ["departments"]               = "/departmentlist",
            ["sites"]                     = "/azsedilist",
            ["locations"]                 = "/azsedilist",
            ["activities"]                = "/activitylist",
            ["skills"]                    = "/competencelist",
            ["competences"]               = "/competencelist",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;
    }
}
