using AutoMapper;
using nvxapp.server.service.ClientServer.Models;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.Service.MyTableService;
using nvxapp.server.service.Service.MyTableService.Models;
using nvxapp.server.service.Service.WeatherForecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace nvxapp.server.service.Service.WeatherForecast
{
    public class WeatherForecastService : ServiceBase,IWeatherForecastService
    {
        private readonly IMyTableService _myTableService;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastService(IMapper mapper,
                                      IMyTableService myTableService) : base(mapper)
        {
            _myTableService = myTableService;
        }

        public virtual async Task<GenericResult<WeatherForecastOutModel>> GetAll(GenericRequest<WeatherForecastInModel> model)
        {
            return await ExecuteAction(model, async () =>
            {
                WeatherForecastOutModel retVal = new WeatherForecastOutModel();

                GenericRequest<MyTableInModel> requestMyTable = new GenericRequest<MyTableInModel>();
                requestMyTable.Data = new MyTableInModel();
                var resultMyTable = await _myTableService.GetAll(requestMyTable);

                retVal.MyTableModel = resultMyTable.Data.MyTable;

                retVal.WeatherForecastate = Enumerable.Range(1, 5).Select(index => new WeatherForecastModel
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToList();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(1); 

                return retVal;
            });
        }

    }

    public interface IWeatherForecastService : IServiceBase
    {
        public Task<GenericResult<WeatherForecastOutModel>> GetAll(GenericRequest<WeatherForecastInModel> model);
    }

}
