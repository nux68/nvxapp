using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models
{
    public class Az_SediModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_Sedi_GetAll_InModel
    {

    }

    public class Az_Sedi_GetAll_OutModel : ModelResult 
    {
        public List<Az_SediModel> Az_Sedi { get; set; } = new List<Az_SediModel>();

        public Az_Sedi_GetAll_OutModel() 
        {
        
        }
    }

   


}
