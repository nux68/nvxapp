using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    public static class GestionePresenze_JobType
    {
        public const string TimeSheet_Engine_Calculate = "TimeSheet_Engine.Calculate";
        public const string TimeSheet_Engine_Export = "TimeSheet_Engine.Export";
        public const string TimeSheet_Engine_ExportPdf_4User = "TimeSheet_Engine.ExportPdf_4User";
        public const string TimeSheet_Engine_ExportPdf_4Admin = "TimeSheet_Engine.ExportPdf_4Admin";
        public const string Report_Pdf = "Report.Pdf";   // report PDF delle pagine HR (ReportService)
        public const string DipendentiImport = "DipendentiImport.Esegui";   // inserimento dei dipendenti importati da file
        public const string blabla = "blabla";
    }
}
