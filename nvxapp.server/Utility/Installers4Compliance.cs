
namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Compliance (contesto ComplianceDbContext e servizi particolari).
     In Infrastructure e' vuoto: lo valorizza il ramo Compliance.
    */
    public static class Installers4Compliance
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            return builder.Services;
        }
    }
}
