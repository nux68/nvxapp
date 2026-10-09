using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Registrazione dei due contesti e dei servizi multi-tenant.
     Le stesse opzioni Npgsql sono usate dalle factory di design-time (dotnet ef).
    */
    public static class DataLayerInstaller
    {
        public const string MigrationsAssembly = "nvxapp.server.data";

        public static IServiceCollection AddNvxDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PublicDbContext>(options => ConfigurePublic(options, connectionString));
            services.AddDbContext<TenantDbContext>(options => ConfigureTenant(options, connectionString));

            services.AddSingleton<TenancySettings>();
            services.AddScoped<ITenantSchemaAccessor, TenantSchemaAccessor>();
            services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
            services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

            return services;
        }

        public static DbContextOptionsBuilder ConfigurePublic(DbContextOptionsBuilder options, string connectionString)
            => options.UseNpgsql(connectionString, npgsql => npgsql
                          .MigrationsAssembly(MigrationsAssembly)
                          .MigrationsHistoryTable(PublicDbContext.MigrationsHistoryTable, PublicDbContext.Schema));

        public static DbContextOptionsBuilder ConfigureTenant(DbContextOptionsBuilder options, string connectionString)
            => options.UseNpgsql(connectionString, npgsql => npgsql
                          .MigrationsAssembly(MigrationsAssembly)
                          // senza schema: lo storico sta nello schema del search_path (uno per azienda)
                          .MigrationsHistoryTable(TenantDbContext.MigrationsHistoryTable))
                      .AddInterceptors(TenantSearchPathInterceptor.Instance);
    }
}
