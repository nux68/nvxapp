using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using NetCore.AutoRegisterDi;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ.Listener;
using System.Reflection;

namespace nvxapp.server.Utility
{
    public static class Installers4AttendanceTracking
    {

        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {

            builder.Services.AddScoped<ITimeSheet_EngineService_OnlyCalculate,
                                       TimeSheet_EngineService_OnlyCalculate>();

            // contesto dell'applicativo: tenant_<IdAzienda>_2 in multi-tenant, public in modalita' singola
            builder.Services.AddApplicationDbContext<AttendanceTrackingDbContext>(ApplicationType.AttendanceTracking);





            return builder.Services;
        }
    }
}
