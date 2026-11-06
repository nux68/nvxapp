using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    // manuale utente del CRM (testi in nvxapp.server.service/Manuals/crm.<lingua>.md), disponibile con il CRM attivo
    public class CrmUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "crm";
        public override ApplicationType? Application => ApplicationType.Crm;
    }
}
