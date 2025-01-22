using nvxapp.server.service.Interfaces;
using System.Reflection;

using NetCore.AutoRegisterDi;

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

    }
}
