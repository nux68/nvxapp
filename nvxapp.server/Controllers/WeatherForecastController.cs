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
                                            IHttpContextAccessor httpContextAccessor,
                                            IWeatherForecastService weatherForecastService
            ):base( httpContextAccessor )
        {
            

            _weatherForecastService = weatherForecastService;
        }

        [HttpPost]
        [Route("Get")]
        public async Task<GenericResult<WeatherForecastOutModel>> Get(GenericRequest<WeatherForecastInModel> inModel)
        {
            var res = await  _weatherForecastService.GetAll(inModel);

            return res;
        }
    }
}
