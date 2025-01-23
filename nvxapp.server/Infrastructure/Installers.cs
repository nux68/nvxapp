using nvxapp.server.service.Interfaces;
using System.Reflection;

using NetCore.AutoRegisterDi;
using nvxapp.server.data.Infrastructure;
using Microsoft.EntityFrameworkCore;

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

            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //        options.UseNpgsql(
            //                builder.Configuration["connectionStrings:nvxappDbContext"]
            //                )
            //    );


            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //        options.UseNpgsql(builder.Configuration.GetConnectionString("nvxappDbContext")));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseNpgsql(
                       builder.Configuration.GetConnectionString("nvxappDbContext"),
                       npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data") // Specifica l'assembly per le migrazioni
                   )
               );


            return builder.Services;
        }

    }
}
