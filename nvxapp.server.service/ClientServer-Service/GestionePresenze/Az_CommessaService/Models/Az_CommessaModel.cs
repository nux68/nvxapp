using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models
{
    public class Az_CommessaModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_Commessa_GetAll_InModel
    {
    }

    public class Az_Commessa_GetAll_OutModel : ModelResult
    {
        public List<Az_CommessaModel> Az_Commessa { get; set; } = new List<Az_CommessaModel>();
        public Az_Commessa_GetAll_OutModel() { }
    }
}
