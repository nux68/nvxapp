using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models
{
    public class Par_CompetenzaModel
    {
        public  int Id { get; set; }
        public  int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; }  = string.Empty;
    }

    public class Par_Competenza_GetAll_InModel
    {
    }

    public class Par_Competenza_GetAll_OutModel : ModelResult
    {
        public List<Par_CompetenzaModel> Par_Competenza { get; set; } = new List<Par_CompetenzaModel>();
        public Par_Competenza_GetAll_OutModel() { }
    }

    public class Par_CompetenzaGetInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Par_CompetenzaGetOutModel : ModelResult
    {
        public Par_CompetenzaModel Par_Competenza { get; set; } = new Par_CompetenzaModel();
    }

    public class Par_CompetenzaPutInModel : ModelResult
    {
        public Par_CompetenzaModel Par_Competenza { get; set; } = new Par_CompetenzaModel();
    }

    public class Par_CompetenzaPutOutModel : ModelResult
    {
        public Par_CompetenzaModel Par_Competenza { get; set; } = new Par_CompetenzaModel();
    }
}
