using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Risorse Umane (contesto HumanResourcesDbContext e servizi particolari).
     In Infrastructure e' vuoto: qui (ramo HumanResources) registra il contesto dell'applicativo.
    */
    public static class Installers4HumanResources
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            // contesto dell'applicativo: tenant_<IdAzienda>_5 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<HumanResourcesDbContext>(ApplicationType.HumanResources);

            return builder.Services;
        }
    }
}
