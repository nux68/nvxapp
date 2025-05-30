using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService.Models
{
    public class Az_AttivitaModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; }  = string.Empty;
    }

    public class Az_Attivita_GetAll_InModel
    {
    }

    public class Az_Attivita_GetAll_OutModel : ModelResult
    {
        public List<Az_AttivitaModel> Az_Attivita { get; set; } = new List<Az_AttivitaModel>();
        public Az_Attivita_GetAll_OutModel() { }
    }
}
