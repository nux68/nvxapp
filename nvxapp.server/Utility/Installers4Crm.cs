using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.Utility
{
    /*
     Registrazioni del CRM (contesto CrmDbContext e servizi particolari).
     In Infrastructure e' vuoto: qui (ramo CRM) registra il contesto dell'applicativo.
    */
    public static class Installers4Crm
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            // contesto dell'applicativo: tenant_<IdAzienda>_3 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<CrmDbContext>(ApplicationType.Crm);

            // promemoria delle attivita' in scadenza (avvisi SignalR agli assegnatari)
            builder.Services.AddHostedService<CrmReminderService>();

            return builder.Services;
        }
    }
}
