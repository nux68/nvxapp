namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Applicativi (argomenti) dell'applicazione.
     Le tabelle comuni stanno in public (PublicDbContext); ogni applicativo ha il proprio
     DbContext e, in multi-tenant, uno schema per azienda: tenant_<IdAzienda>_<valore>.
     Ogni azienda attiva solo gli applicativi che usa (tabella CompanyApplication).

     Common (0) non e' un applicativo da attivare: sono i dati dell'azienda condivisi tra gli
     applicativi (anagrafica, clienti, contatti dei clienti). Il suo schema (tenant_<IdAzienda>_0)
     esiste SEMPRE, per ogni azienda, indipendentemente dagli applicativi attivi; non compare in
     CompanyApplication. Vedi nvxapp.server/docs/ANALISI_CRM.md §2.

     Il valore numerico finisce nel nome dello schema e nel database: non va mai cambiato
     per un applicativo esistente.
    */
    public enum ApplicationType
    {
        Common = 0,
        Moke = 1,
        AttendanceTracking = 2,
    }

    public static class ApplicationTypeExtensions
    {
        /// <summary>Applicativo da attivare per azienda (tutti tranne Common, sempre presente).</summary>
        public static bool IsActivatable(this ApplicationType application) => application != ApplicationType.Common;
    }
}
