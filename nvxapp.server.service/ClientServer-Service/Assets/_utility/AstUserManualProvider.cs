using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    // manuale utente di Risorse aziendali (testi in nvxapp.server.service/Manuals/assets.<lingua>.md), disponibile con il modulo attivo
    public class AstUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "assets";
        public override ApplicationType? Application => ApplicationType.Assets;
    }
}
