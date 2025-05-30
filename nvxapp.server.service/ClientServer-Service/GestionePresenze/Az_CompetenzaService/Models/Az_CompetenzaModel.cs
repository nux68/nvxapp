using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService.Models
{
    public class Az_CompetenzaModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; }  = string.Empty;
      
    }

    public class Az_Competenza_GetAll_InModel
    {
    }

    public class Az_Competenza_GetAll_OutModel : ModelResult
    {
        public List<Az_CompetenzaModel> Az_Competenza { get; set; } = new List<Az_CompetenzaModel>();
        public Az_Competenza_GetAll_OutModel() { }
    }
}
