using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService.Models
{
    public class Az_AttivitaCompetenzaModel
    {
        public int Id { get; set; }
        public int IdAz_Attivita { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_AttivitaCompetenza_GetAll_InModel { }

    public class Az_AttivitaCompetenza_GetAll_OutModel : ModelResult
    {
        public List<Az_AttivitaCompetenzaModel> Az_AttivitaCompetenza { get; set; } = new List<Az_AttivitaCompetenzaModel>();
    }
}
