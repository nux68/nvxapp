using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService.Models
{
    public class Az_SediAttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_Sedi { get; set; }
        public int IdPar_Attivita { get; set; }
    }

    public class Az_SediAttivita_GetAll_InModel { }

    public class Az_SediAttivita_GetAll_OutModel : ModelResult
    {
        public List<Az_SediAttivitaModel> Az_SediAttivita { get; set; } = new List<Az_SediAttivitaModel>();
    }
}
