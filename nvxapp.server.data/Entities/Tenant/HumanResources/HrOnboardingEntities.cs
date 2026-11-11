using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant.HumanResources
{
    /*
     Onboarding e offboarding (HR-ONB, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.4), schema tenant_<IdAzienda>_5.
     Modelli di checklist (assunzione / cessazione) e checklist del dipendente generate dal modello sulle date
     di assunzione e cessazione di Dip_Anagrafica (Common, D6).
    */

    public enum Hr_TipoChecklist
    {
        Assunzione = 1,
        Cessazione = 2,
    }

    public enum Hr_StatoChecklist
    {
        Aperta = 0,
        Completata = 1,
        Annullata = 2,
    }

    // azione automatica collegata a una voce (HR-ONB.3)
    public enum Hr_AzioneVoce
    {
        Nessuna = 0,
        CreaAccount = 1,
        DisattivaAccount = 2,
    }

    // modello di checklist
    public class Hr_ChecklistModello : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public Hr_TipoChecklist Tipo { get; set; } = Hr_TipoChecklist.Assunzione;
        [Required, MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;
        // modello usato per generare in automatico le checklist del tipo (uno per tipo)
        public bool Predefinito { get; set; }
        public bool Attivo { get; set; } = true;

        public ICollection<Hr_ChecklistModelloVoce>? Hr_ChecklistModelloVoce { get; set; }
    }

    public class Hr_ChecklistModelloVoce : BaseEntity
    {
        [Required]
        public int IdHr_ChecklistModello { get; set; }
        public Hr_ChecklistModello? Hr_ChecklistModelloNavigation { get; set; }

        public int Ordine { get; set; }
        [Required, MaxLength(300)]
        public string Descrizione { get; set; } = string.Empty;
        // utente dell'azienda responsabile della voce (null = amministratori)
        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }
        // giorni rispetto alla data di assunzione o cessazione (negativi = prima)
        public int Giorni { get; set; }
        // documento da consegnare o far firmare al dipendente (presa visione)
        public bool Documento { get; set; }
        public Hr_AzioneVoce Azione { get; set; } = Hr_AzioneVoce.Nessuna;
    }

    // checklist di un dipendente
    public class Hr_Checklist : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public int? IdHr_ChecklistModello { get; set; }
        public Hr_TipoChecklist Tipo { get; set; }
        [Required, MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;
        // data di assunzione o cessazione da cui partono le voci
        public DateTime DataRiferimento { get; set; }
        public Hr_StatoChecklist Stato { get; set; } = Hr_StatoChecklist.Aperta;
        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Hr_ChecklistVoce>? Hr_ChecklistVoce { get; set; }
    }

    public class Hr_ChecklistVoce : BaseEntity
    {
        [Required]
        public int IdHr_Checklist { get; set; }
        public Hr_Checklist? Hr_ChecklistNavigation { get; set; }

        public int Ordine { get; set; }
        [Required, MaxLength(300)]
        public string Descrizione { get; set; } = string.Empty;
        [MaxLength(450)]
        public string? IdAspNetUsersResponsabile { get; set; }
        public int Giorni { get; set; }
        public DateTime DataPrevista { get; set; }
        public bool Completata { get; set; }
        public DateTime? DataCompletamento { get; set; }
        [MaxLength(450)]
        public string? IdAspNetUsersCompletamento { get; set; }

        public bool Documento { get; set; }
        [MaxLength(260)]
        public string? DocumentoNomeFile { get; set; }
        [MaxLength(100)]
        public string? DocumentoContentType { get; set; }
        [MaxLength(100)]
        public string? DocumentoStorageKey { get; set; }
        public long? DocumentoDimensione { get; set; }
        // presa visione chiesta al dipendente
        public DateTime? DataRichiestaPresaVisione { get; set; }

        public Hr_AzioneVoce Azione { get; set; } = Hr_AzioneVoce.Nessuna;
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
