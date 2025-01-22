using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer.Models;
using nvxapp.server.service.Service.WeatherForecast;
using nvxapp.server.service.Service.WeatherForecast.Models;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : RepositoryNetcoreControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;

        


        public WeatherForecastController(
            //ILogger<WeatherForecastController> logger

            IWeatherForecastService weatherForecastService

            )
        {
            //_logger = logger;

            _weatherForecastService = weatherForecastService;
        }

        [HttpPost]
        [Route("Get")]
        public async Task<GenericResult<WeatherForecastOutModel>> Get(GenericRequest<WeatherForecastInModel> inModel)
        {
            //WeatherForecastOutModel retVal = new WeatherForecastOutModel();

            //retVal.WeatherForecastate = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //    TemperatureC = Random.Shared.Next(-20, 55),
            //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            //})
            //.ToList();

            //return retVal;

            var res = await  _weatherForecastService.GetAll(inModel);

            return res;

            

        }
    }
}
