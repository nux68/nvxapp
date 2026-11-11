using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Risorse aziendali (contesto AssetsDbContext e servizi particolari).
     In Infrastructure e' vuoto: qui (ramo Assets) registra il contesto dell'applicativo.
    */
    public static class Installers4Assets
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            // contesto dell'applicativo: tenant_<IdAzienda>_7 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<AssetsDbContext>(ApplicationType.Assets);

            return builder.Services;
        }
    }
}
