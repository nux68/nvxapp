namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models
{
    public class Az_CommessaModel
    {
        public required int Id { get; set; }
        public required int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class Az_Commessa_GetAll_InModel
    {
        // Aggiungi qui eventuali parametri di filtro per la ricerca delle commesse
        // Esempio:
        // public int? IdAz_Anagrafica { get; set; }
    }

    public class Az_Commessa_GetAll_OutModel
    {
        public List<Az_CommessaModel> Az_Commessa { get; set; } = new();
    }
}
