using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.Utility
{
    /*
     Registrazioni del documentale (contesto DocumentManagementDbContext e servizi particolari).
     In Infrastructure e' vuoto: qui (ramo DocumentManagement) registra il contesto dell'applicativo.
    */
    public static class Installers4DocumentManagement
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            // contesto dell'applicativo: tenant_<IdAzienda>_4 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<DocumentManagementDbContext>(ApplicationType.DocumentManagement);

            // svuotamento automatico del cestino (lavoro notturno)
            builder.Services.AddHostedService<DocumentManagementMaintenanceService>();

            return builder.Services;
        }
    }
}
