using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    // utente dell'azienda (responsabili delle voci)
    public class Hr_UtenteModel
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

    // ------------------------------------------------------------------------------------------------ modelli di checklist
    public class Hr_ModelloVoceModel
    {
        public int Id { get; set; }
        public int Ordine { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? IdAspNetUsersResponsabile { get; set; }
        public int Giorni { get; set; }
        public bool Documento { get; set; }
        public int Azione { get; set; }
    }

    public class Hr_ModelloModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; } = 1;
        public string Descrizione { get; set; } = string.Empty;
        public bool Predefinito { get; set; }
        public bool Attivo { get; set; } = true;
        public int NumeroVoci { get; set; }
        public List<Hr_ModelloVoceModel> Voci { get; set; } = new();
    }

    public class Hr_ModelloListOutModel : ModelResult { public List<Hr_ModelloModel> Modelli { get; set; } = new(); }
    public class Hr_ModelloGetOutModel : ModelResult
    {
        public Hr_ModelloModel Modello { get; set; } = new();
        public List<Hr_UtenteModel> Utenti { get; set; } = new();
    }
    public class Hr_ModelloPutInModel { public Hr_ModelloModel Modello { get; set; } = new(); }
    public class Hr_ModelloPutOutModel : ModelResult { public Hr_ModelloModel Modello { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ checklist dei dipendenti
    public class Hr_ChecklistModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataRiferimento { get; set; }
        public int Stato { get; set; }
        public string? Note { get; set; }
        public int NumeroVoci { get; set; }
        public int NumeroCompletate { get; set; }
        public DateTime? ProssimaScadenza { get; set; }
    }

    public class Hr_ChecklistVoceModel
    {
        public int Id { get; set; }
        public int Ordine { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? IdAspNetUsersResponsabile { get; set; }
        public string? Responsabile { get; set; }
        public DateTime DataPrevista { get; set; }
        public bool Completata { get; set; }
        public DateTime? DataCompletamento { get; set; }
        public bool Documento { get; set; }
        public string? DocumentoNomeFile { get; set; }
        public DateTime? DataRichiestaPresaVisione { get; set; }
        public DateTime? DataConferma { get; set; }
        public int Azione { get; set; }
        public string? Note { get; set; }
    }

    // account del dipendente (HR-ONB.3)
    public class Hr_AccountModel
    {
        public bool Presente { get; set; }
        public string? UserName { get; set; }
        public string? Mail { get; set; }
        public bool Disabilitato { get; set; }
    }

    public class Hr_ChecklistListInModel { public bool SoloAperte { get; set; } = true; }
    public class Hr_ChecklistListOutModel : ModelResult
    {
        public List<Hr_ChecklistModel> Checklist { get; set; } = new();
        public List<Hr_ModelloModel> Modelli { get; set; } = new();
        public List<Hr_DipendenteBreveModel> Dipendenti { get; set; } = new();
    }

    public class Hr_ChecklistGetOutModel : ModelResult
    {
        public Hr_ChecklistModel Checklist { get; set; } = new();
        public List<Hr_ChecklistVoceModel> Voci { get; set; } = new();
        public Hr_AccountModel Account { get; set; } = new();
        public List<Hr_UtenteModel> Utenti { get; set; } = new();
    }

    public class Hr_ChecklistCreaInModel
    {
        public int IdDip_Anagrafica { get; set; }
        public int IdHr_ChecklistModello { get; set; }
        // null = data di assunzione o cessazione del dipendente
        public DateTime? DataRiferimento { get; set; }
    }

    public class Hr_ChecklistPutInModel
    {
        public int Id { get; set; }
        public int Stato { get; set; }
        public string? Note { get; set; }
    }

    public class Hr_ChecklistVocePutInModel { public Hr_ChecklistVoceModel Voce { get; set; } = new(); }
    public class Hr_ChecklistVoceOutModel : ModelResult { public Hr_ChecklistVoceModel Voce { get; set; } = new(); }

    public class Hr_VoceDocUploadModel { public int IdVoce { get; set; } }
}
