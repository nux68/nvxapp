using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Route del modulo Infrastructure.
    // Derivate da MainMenuInfrastructureService (client Angular).
    public class InfrastructureRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["home"]                 = "/home",
            ["dashboard"]            = "/home",
            ["impersonate"]          = "/userimpersonate",
            ["login"]                = "/login",
            ["logout"]               = "/logout",
            ["utente"]               = "/user",
            ["user"]                 = "/user",
            ["superuser"]            = "/superuser",
            ["admin"]                = "/admin",
            ["poweradmin"]           = "/poweradmin",
            ["utenti"]               = "/userlist",
            ["userlist"]             = "/userlist",
            ["centri"]               = "/dealerlist",
            ["dealerlist"]           = "/dealerlist",
            ["centro power admin"]   = "/dealerpoweradmin",
            ["centro admin"]         = "/dealeradmin",
            ["utenti centro"]        = "/userdealerlist",
            ["studi"]                = "/financialadvisorlist",
            ["studio power admin"]   = "/financialadvisorpoweradmin",
            ["studio admin"]         = "/financialadvisoradmin",
            ["utenti studio"]        = "/userfinancialadvisorlist",
            ["aziende"]              = "/companylist",
            ["azienda power admin"]  = "/companypoweradmin",
            ["azienda admin"]        = "/companyadmin",
            ["utenti azienda"]       = "/usercompanylist",

            // nomi inglesi delle stesse pagine (l'utente puo' usare entrambe le lingue)
            ["users"]                        = "/userlist",
            ["user list"]                    = "/userlist",
            ["dealers"]                      = "/dealerlist",
            ["dealer list"]                  = "/dealerlist",
            ["centers"]                      = "/dealerlist",
            ["centres"]                      = "/dealerlist",
            ["dealer power admin"]           = "/dealerpoweradmin",
            ["dealer admin"]                 = "/dealeradmin",
            ["dealer users"]                 = "/userdealerlist",
            ["financial advisors"]           = "/financialadvisorlist",
            ["financial advisor list"]       = "/financialadvisorlist",
            ["financial advisor power admin"] = "/financialadvisorpoweradmin",
            ["financial advisor admin"]      = "/financialadvisoradmin",
            ["financial advisor users"]      = "/userfinancialadvisorlist",
            ["companies"]                    = "/companylist",
            ["company list"]                 = "/companylist",
            ["company power admin"]          = "/companypoweradmin",
            ["company admin"]                = "/companyadmin",
            ["company users"]                = "/usercompanylist",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;
    }
}
