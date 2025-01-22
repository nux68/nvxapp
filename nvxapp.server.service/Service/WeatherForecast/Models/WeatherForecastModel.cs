using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Service.WeatherForecast.Models
{
    public class WeatherForecastInModel
    {

    }

    public class WeatherForecastOutModel
    {
        public List<WeatherForecastModel> WeatherForecastate { get; set; }

        public WeatherForecastOutModel()
        {
            WeatherForecastate = new List<WeatherForecastModel>();
        }
    }

    public class WeatherForecastModel
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
