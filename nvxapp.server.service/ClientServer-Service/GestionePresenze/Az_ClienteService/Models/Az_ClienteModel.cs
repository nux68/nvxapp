using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models
{
    public class Az_ClienteModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_Cliente_GetAll_InModel
    {
    }

    public class Az_Cliente_GetAll_OutModel : ModelResult
    {
        public List<Az_ClienteModel> Az_Cliente { get; set; } = new List<Az_ClienteModel>();
        public Az_Cliente_GetAll_OutModel() { }
    }
}
