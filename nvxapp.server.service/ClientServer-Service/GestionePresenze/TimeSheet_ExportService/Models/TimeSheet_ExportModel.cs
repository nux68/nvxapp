using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models
{
    
    public class TimeSheet_ExportModel
    {

        public int Year { get; set; }
        public int Month { get; set; } = 0;
        public List<string> SelectedUserId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public int IdPar_ExportCau { get; set; }
        

    }

    public class TimeSheet_ExportInModel
    {
        public TimeSheet_ExportModel TimeSheet_Export { get; set; } = new TimeSheet_ExportModel();
    }
    public class TimeSheet_ExportOutModel : ModelResult
    {
        public TimeSheet_ExportModel TimeSheet_Export { get; set; } = new TimeSheet_ExportModel();

        public TimeSheet_ExportOutModel()
        {

        }
    }




}
