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
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;
    }
}
