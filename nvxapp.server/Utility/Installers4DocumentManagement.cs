namespace nvxapp.server.Utility
{
    /*
     Registrazioni del documentale (contesto DocumentManagementDbContext e servizi particolari).
     In Infrastructure e' vuoto: lo valorizza il ramo DocumentManagement.
    */
    public static class Installers4DocumentManagement
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            return builder.Services;
        }
    }
}
