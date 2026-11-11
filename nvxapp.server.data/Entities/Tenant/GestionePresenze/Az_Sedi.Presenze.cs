namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Navigazioni delle presenze sulla sede comune (Az_Sedi sta nello schema Common, vedi
     Entities/Tenant/Common/Az_Sedi.cs, AT-COM.1). CommonDbContext le ignora; le usa solo AttendanceTrackingDbContext.
    */
    public partial class Az_Sedi
    {
        public ICollection<Az_SediAttivita>? Az_SediAttivita { get; set; }
    }
}
