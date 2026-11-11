using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    // manuale utente di Compliance (testi in nvxapp.server.service/Manuals/compliance.<lingua>.md), disponibile con il modulo attivo
    public class CmpUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "compliance";
        public override ApplicationType? Application => ApplicationType.Compliance;
    }
}
