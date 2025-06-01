using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService.Models
{
    public class Az_AttivitaModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
               
        public string? BackgroundColor { get; set; }

        public string? TextColor { get; set; }
    }

    public class Az_Attivita_GetAll_InModel { }
    public class Az_Attivita_GetAll_OutModel : ModelResult
    {
        public List<Az_AttivitaModel> Az_Attivita { get; set; } = new List<Az_AttivitaModel>();
        public Az_Attivita_GetAll_OutModel() { }
    }
    public class Az_AttivitaGetInModel { public int Id { get; set; } = 0; }
    public class Az_AttivitaGetOutModel : ModelResult
    {
        public Az_AttivitaModel Az_Attivita { get; set; } = new Az_AttivitaModel();
    }
    public class Az_AttivitaPutInModel : ModelResult
    {
        public Az_AttivitaModel Az_Attivita { get; set; } = new Az_AttivitaModel();
    }
    public class Az_AttivitaPutOutModel : ModelResult
    {
        public Az_AttivitaModel Az_Attivita { get; set; } = new Az_AttivitaModel();
    }
}
