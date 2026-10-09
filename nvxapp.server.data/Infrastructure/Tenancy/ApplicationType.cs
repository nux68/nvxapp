namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Applicativi (argomenti) dell'applicazione.
     Le tabelle comuni stanno in public (PublicDbContext); ogni applicativo ha il proprio
     DbContext e, in multi-tenant, uno schema per azienda: tenant_<IdAzienda>_<valore>.
     Ogni azienda attiva solo gli applicativi che usa (tabella CompanyApplication).

     Il valore numerico finisce nel nome dello schema e nel database: non va mai cambiato
     per un applicativo esistente.
    */
    public enum ApplicationType
    {
        Moke = 1,
        AttendanceTracking = 2,
    }
}
