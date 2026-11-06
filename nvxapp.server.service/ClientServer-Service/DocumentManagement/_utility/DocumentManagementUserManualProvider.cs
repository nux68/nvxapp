using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    // manuale utente del documentale (testi in Manuals/documents.<lingua>.md), disponibile con il documentale attivo
    public class DocumentManagementUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "documents";
        public override ApplicationType? Application => ApplicationType.DocumentManagement;
    }
}
