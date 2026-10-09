using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Registrazione del contesto comune, dei contesti degli applicativi e dei servizi multi-tenant.
     Ogni modulo registra il contesto del proprio applicativo con AddApplicationDbContext<T>()
     (es. Installers4AttendanceTracking), prima o dopo AddNvxDataLayer: l'ordine non conta.
     Le stesse opzioni Npgsql sono usate dalle factory di design-time (dotnet ef).
    */
    public static class DataLayerInstaller
    {
        public const string MigrationsAssembly = "nvxapp.server.data";

        public static IServiceCollection AddNvxDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PublicDbContext>(options => ConfigurePublic(options, connectionString));

            services.AddSingleton<TenancySettings>();
            Registry(services).ConnectionString = connectionString;
            services.AddScoped<ITenantSchemaAccessor, TenantSchemaAccessor>();
            services.AddScoped<IApplicationDbContextFactory, ApplicationDbContextFactory>();
            services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

            // applicativi dell'infrastruttura
            services.AddApplicationDbContext<MokeDbContext>(ApplicationType.Moke);

            return services;
        }

        /// <summary>Registra il contesto di un applicativo.</summary>
        public static IServiceCollection AddApplicationDbContext<TContext>(this IServiceCollection services, ApplicationType application)
            where TContext : ApplicationDbContextBase
        {
            var registry = Registry(services);
            registry.Register(application, typeof(TContext));

            // la connection string viene letta alla creazione del contesto (gia' impostata da AddNvxDataLayer)
            services.AddDbContext<TContext>(options => ConfigureApplication(options, registry.ConnectionString, application));
            return services;
        }

        // unico registro dei contesti, creato dalla prima chiamata (AddNvxDataLayer o AddApplicationDbContext)
        private static ApplicationDbContextRegistry Registry(IServiceCollection services)
        {
            var registry = (ApplicationDbContextRegistry?)services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationDbContextRegistry))?.ImplementationInstance;
            if (registry == null)
            {
                registry = new ApplicationDbContextRegistry();
                services.AddSingleton(registry);
            }
            return registry;
        }

        public static DbContextOptionsBuilder ConfigurePublic(DbContextOptionsBuilder options, string connectionString)
            => options.UseNpgsql(connectionString, npgsql => npgsql
                          .MigrationsAssembly(MigrationsAssembly)
                          .MigrationsHistoryTable(PublicDbContext.MigrationsHistoryTable, PublicDbContext.Schema));

        public static DbContextOptionsBuilder ConfigureApplication(DbContextOptionsBuilder options, string connectionString, ApplicationType application)
            => options.UseNpgsql(connectionString, npgsql => npgsql
                          .MigrationsAssembly(MigrationsAssembly)
                          // senza schema: lo storico sta nello schema del search_path (uno per azienda/applicativo)
                          .MigrationsHistoryTable(ApplicationDbContextBase.MigrationsHistoryTable(application)))
                      .AddInterceptors(TenantSearchPathInterceptor.Instance);
    }
}
