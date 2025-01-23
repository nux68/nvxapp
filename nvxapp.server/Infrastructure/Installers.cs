using nvxapp.server.service.Interfaces;
using System.Reflection;

using NetCore.AutoRegisterDi;
using nvxapp.server.data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.Infrastructure
{
    public static class Installers
    {

        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {


            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IServiceBase)))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            return builder.Services;
        }

        public static IServiceCollection InstallEntityContex(this WebApplicationBuilder builder)
        {

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseNpgsql(
                       builder.Configuration.GetConnectionString("nvxappDbContext"),
                       npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data") // Specifica l'assembly per le migrazioni
                   )
               );

            return builder.Services;
        }

        public static IServiceCollection InstallRepositories(this WebApplicationBuilder builder)
        {
            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IRepository<>)))
                            .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //builder.Services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();

            return builder.Services;
        }

        public static IServiceCollection InstallMappers(this WebApplicationBuilder builder)
        {

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return builder.Services;
        }
    }
}
