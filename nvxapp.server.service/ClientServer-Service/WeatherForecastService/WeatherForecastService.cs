using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.service.Service.MyTableService;
using nvxapp.server.service.Service.MyTableService.Models;
using nvxapp.server.service.Service.WeatherForecast.Models;




namespace nvxapp.server.service.Service.WeatherForecast
{
    public class WeatherForecastService : ServiceBase, IWeatherForecastService
    {
        private readonly IMyTableService _myTableService;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,

                                      IMyTableService myTableService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, httpContextAccessor)
        {
            _myTableService = myTableService;
        }

        public virtual async Task<GenericResult<WeatherForecastOutModel>> GetAll(GenericRequest<WeatherForecastInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                WeatherForecastOutModel retVal = new WeatherForecastOutModel();

                GenericRequest<MyTableInModel> requestMyTable = new GenericRequest<MyTableInModel>();

                try
                {
                    retVal.MyTableModel = _myTableService.GetAll(requestMyTable, true).Result.Data;
                }
                catch (Exception ex)
                {
                    retVal.AddMessage(ex.Message, MessageType.Exception);
                }


                retVal.WeatherForecastate = Enumerable.Range(1, 5).Select(index => new WeatherForecastModel
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToList();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IWeatherForecastService : IServiceBase
    {
        public Task<GenericResult<WeatherForecastOutModel>> GetAll(GenericRequest<WeatherForecastInModel> model, Boolean isSubProcess);
    }

}
