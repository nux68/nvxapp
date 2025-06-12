using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models
{
    public class Par_AttivitaCompetenzaModel
    {
        public int Id { get; set; }
        public int IdPar_Attivita { get; set; }
        public int IdPar_Competenza { get; set; }
    }

    public class Par_AttivitaCompetenza_GetAll_InModel { }

    public class Par_AttivitaCompetenza_GetAll_OutModel : ModelResult
    {
        public List<Par_AttivitaCompetenzaModel> Par_AttivitaCompetenza { get; set; } = new List<Par_AttivitaCompetenzaModel>();
    }



    public class Par_AttivitaCompetenza_Selected_GetInModel
    {
        public int IdPar_Attivita { get; set; }
    }
    public class Par_AttivitaCompetenza_Selected_GetOutModel: ModelResult
    {
        public List<Par_AttivitaCompetenzaModel> Par_AttivitaCompetenza { get; set; } = new List<Par_AttivitaCompetenzaModel>();
    }

    public class Par_AttivitaCompetenza_Selected_PutInModel
    {
        public int IdPar_Attivita { get; set; }
        public List<Par_AttivitaCompetenzaModel> Par_AttivitaCompetenza { get; set; } = new List<Par_AttivitaCompetenzaModel>();
    }
    public class Par_AttivitaCompetenza_Selected_PutOutModel: ModelResult
    {
        public List<Par_AttivitaCompetenzaModel> Par_AttivitaCompetenza { get; set; } = new List<Par_AttivitaCompetenzaModel>();
    }



}
