using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models
{
    public class Dip_CompetenzaModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public int IdAz_Competenza { get; set; }
    }

    public class Dip_Competenza_GetAll_InModel { }

    public class Dip_Competenza_GetAll_OutModel : ModelResult
    {
        public List<Dip_CompetenzaModel> Dip_Competenza { get; set; } = new List<Dip_CompetenzaModel>();
    }
}
