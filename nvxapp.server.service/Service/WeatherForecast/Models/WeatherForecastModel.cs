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

    public class WeatherForecastOutModel
    {
        public List<MyTableModel> MyTableModel { get; set; }
        public List<WeatherForecastModel> WeatherForecastate { get; set; }

        public WeatherForecastOutModel()
        {
            WeatherForecastate = new List<WeatherForecastModel>();
            MyTableModel = new List<MyTableModel>();
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
