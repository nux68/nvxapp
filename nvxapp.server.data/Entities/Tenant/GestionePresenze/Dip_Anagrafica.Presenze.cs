namespace nvxapp.server.data.Entities.Tenant
{
    /*
     Navigazioni delle presenze sul dipendente comune (Dip_Anagrafica sta nello schema Common, vedi
     Entities/Tenant/Common/Dip_Anagrafica.cs). CommonDbContext le ignora; le usa solo AttendanceTrackingDbContext.
     Dip_Competenza e' una tabella comune: la sua collection sta nella classe comune (AT-COM.1).
    */
    public partial class Dip_Anagrafica
    {
        public ICollection<Dip_RapportoLavoro>? Dip_RapportoLavoro { get; set; }
    }
}
