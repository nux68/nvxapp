using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models
{
    public class TimeSheet_CalculateModel
    {

        public int Year { get; set; }
        public int Month { get; set; } = 0;
        public List<string> SelectedUserId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        
        public Boolean Approva_Richieste_Timbrature { get; set; }
        public Boolean Approva_Richieste_Giustificativo { get; set; }
        public Boolean Genera_Timbrature_Mancanti { get; set; }

        

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
