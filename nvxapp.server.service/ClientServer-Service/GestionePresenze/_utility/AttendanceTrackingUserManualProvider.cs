using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // manuale utente della gestione presenze: testi in Manuals/attendance.<lingua>.md (vedi UserManual.cs)
    public class AttendanceTrackingUserManualProvider : UserManualMarkdownProvider
    {
        public override string Code => "attendance";
        public override ApplicationType? Application => ApplicationType.AttendanceTracking;
    }
}
