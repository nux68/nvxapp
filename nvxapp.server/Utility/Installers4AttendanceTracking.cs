using NetCore.AutoRegisterDi;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ.Listener;
using System.Reflection;

namespace nvxapp.server.Utility
{
    public static class Installers4AttendanceTracking
    {

        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {







            return builder.Services;
        }
    }
}
