namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models
{
    public class Az_CommessaModel
    {
        public  int Id { get; set; }
        public  int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public  int IdAz_Cliente { get; set; }
        public Boolean Default { get; set; }
        public string Data { get; set; } = string.Empty;
        public string DataA { get; set; } = string.Empty;
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

    public class Az_CommessaGetInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Az_CommessaGetOutModel
    {
        public Az_CommessaModel Az_Commessa { get; set; } = new Az_CommessaModel();
    }

    public class Az_CommessaPutInModel
    {
        public Az_CommessaModel Az_Commessa { get; set; } = new Az_CommessaModel();
    }

    public class Az_CommessaPutOutModel
    {
        public Az_CommessaModel Az_Commessa { get; set; } = new Az_CommessaModel();
    }

    public class Az_CommessaDeleteInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Az_CommessaDeleteOutModel
    {
        public Az_CommessaModel Az_Commessa { get; set; } = new Az_CommessaModel();
    }
}
