namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Navigazioni delle presenze sull'autovettura comune (Dip_RapportoAutovetture sta nello schema Common, legata al
     dipendente, vedi Entities/Tenant/Common/Dip_RapportoAutovetture.cs, AT-COM.1). CommonDbContext le ignora;
     le usa solo AttendanceTrackingDbContext (spese di rimborso chilometrico).
    */
    public partial class Dip_RapportoAutovetture
    {
        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }
}
