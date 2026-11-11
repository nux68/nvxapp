using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    // manuale utente di Risorse Umane (testi in nvxapp.server.service/Manuals/humanresources.<lingua>.md), disponibile con il modulo attivo
    public class HrUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "humanresources";
        public override ApplicationType? Application => ApplicationType.HumanResources;
    }
}
