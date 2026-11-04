
namespace nvxapp.server.Utility
{
    /*
     Registrazioni del CRM (contesto CrmDbContext e servizi particolari).
     In Infrastructure e' vuoto: lo valorizza il ramo CRM.
    */
    public static class Installers4Crm
    {
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {
            return builder.Services;
        }
    }
}
