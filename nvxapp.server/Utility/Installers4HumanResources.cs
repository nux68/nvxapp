
namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Risorse Umane (contesto HumanResourcesDbContext e servizi particolari).
     In Infrastructure e' vuoto: lo valorizza il ramo HumanResources.
    */
    public static class Installers4HumanResources
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            return builder.Services;
        }
    }
}
