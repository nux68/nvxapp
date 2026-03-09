using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models
{
    public class TimeSheet_CalculateModel
    {

        public int Id { get; set; }
        public int NumCoppia { get; set; } = 0;

    }

    public class TimeSheet_CalculateInModel
    {
        public TimeSheet_CalculateModel TimeSheet_Calculate { get; set; } = new TimeSheet_CalculateModel();
    }

    public class TimeSheet_CalculateOutModel : ModelResult
    {
        public TimeSheet_CalculateModel TimeSheet_Calculate { get; set; } = new TimeSheet_CalculateModel();

        public TimeSheet_CalculateOutModel()
        {

        }
    }


}
