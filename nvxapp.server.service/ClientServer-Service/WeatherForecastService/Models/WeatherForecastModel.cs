using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Service.MyTableService.Models;
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

    public class WeatherForecastOutModel : ModelResult
    {
        // questo verra valorizzato da un altro servizio
        public MyTableOutModel? MyTableModel { get; set; }
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
