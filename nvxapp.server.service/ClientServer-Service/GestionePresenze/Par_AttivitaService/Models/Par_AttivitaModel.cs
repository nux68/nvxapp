using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models
{
    public class Par_AttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
               
        public string? BackgroundColor { get; set; }

        public string? TextColor { get; set; }
    }

    public class Par_Attivita_GetAll_InModel { }
    public class Par_Attivita_GetAll_OutModel : ModelResult
    {
        public List<Par_AttivitaModel> Par_Attivita { get; set; } = new List<Par_AttivitaModel>();
        public Par_Attivita_GetAll_OutModel() { }
    }
    public class Par_AttivitaGetInModel { public int Id { get; set; } = 0; }
    public class Par_AttivitaGetOutModel : ModelResult
    {
        public Par_AttivitaModel Par_Attivita { get; set; } = new Par_AttivitaModel();
    }
    public class Par_AttivitaPutInModel : ModelResult
    {
        public Par_AttivitaModel Par_Attivita { get; set; } = new Par_AttivitaModel();
    }
    public class Par_AttivitaPutOutModel : ModelResult
    {
        public Par_AttivitaModel Par_Attivita { get; set; } = new Par_AttivitaModel();
    }
}
