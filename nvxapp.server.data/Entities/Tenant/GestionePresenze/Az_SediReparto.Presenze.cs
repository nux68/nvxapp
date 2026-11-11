using nvxapp.server.data.Entities.Tenant.GestionePresenze;

namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Navigazioni delle presenze sul reparto comune (Az_SediReparto sta nello schema Common, vedi
     Entities/Tenant/Common/Az_SediReparto.cs, AT-COM.1). CommonDbContext le ignora; le usa solo AttendanceTrackingDbContext.
    */
    public partial class Az_SediReparto
    {
        public ICollection<Az_SediRepartoAttivita>? Az_SediRepartoAttivita { get; set; }
        public ICollection<Az_SediRepartoUser>? Az_SediRepartoUser { get; set; }
        public ICollection<Az_SubCommessaSediReparto>? Az_SubCommessaSediReparto { get; set; }
    }
}
