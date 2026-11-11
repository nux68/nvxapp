
namespace nvxapp.server.Utility
{
    /*
     Registrazioni di Risorse aziendali (contesto AssetsDbContext e servizi particolari).
     In Infrastructure e' vuoto: lo valorizza il ramo Assets.
    */
    public static class Installers4Assets
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            return builder.Services;
        }
    }
}
