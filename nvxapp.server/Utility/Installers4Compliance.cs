using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Compliance (contesto ComplianceDbContext e servizi particolari).
     In Infrastructure e' vuoto: qui (ramo Compliance) registra il contesto dell'applicativo.
    */
    public static class Installers4Compliance
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            // contesto dell'applicativo: tenant_<IdAzienda>_6 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<ComplianceDbContext>(ApplicationType.Compliance);

            return builder.Services;
        }
    }
}
