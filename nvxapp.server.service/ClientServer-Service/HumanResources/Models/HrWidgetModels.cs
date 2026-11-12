using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    // ------------------------------------------------------------------------------------------------ widget della dashboard (ANALISI_HUMANRESOURCES_WIDGET.md)

    // voce di checklist da fare (onboarding / offboarding)
    public class Hr_WidgetVoceModel
    {
        public int IdHr_Checklist { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataPrevista { get; set; }
    }

    public class Hr_WidgetOnboardingGetOutModel : ModelResult
    {
        // checklist aperte, per tipo
        public int ChecklistAperte { get; set; }
        public int Assunzioni { get; set; }
        public int Cessazioni { get; set; }
        // voci non completate con la data prevista passata
        public int VociScadute { get; set; }
        // voci non completate entro i prossimi GiorniProssime giorni (oggi compreso)
        public int VociProssime { get; set; }
        public int GiorniProssime { get; set; }
        // le prime voci da fare (scadute e prossime), in ordine di data
        public List<Hr_WidgetVoceModel> Voci { get; set; } = new();
    }

    // attestato in scadenza o scaduto (l'ultimo conseguito per dipendente e corso)
    public class Hr_WidgetAttestatoModel
    {
        public int IdHr_Corso { get; set; }
        public string Corso { get; set; } = string.Empty;
        public string Dipendente { get; set; } = string.Empty;
        public DateTime DataScadenza { get; set; }
        public bool Obbligatorio { get; set; }
    }

    public class Hr_WidgetFormazioneGetOutModel : ModelResult
    {
        public int Scaduti { get; set; }
        // in scadenza entro 30 giorni e tra 31 e GiorniFinestra giorni
        public int Entro30 { get; set; }
        public int Entro60 { get; set; }
        public int GiorniFinestra { get; set; }
        // i primi attestati per data di scadenza (scaduti compresi)
        public List<Hr_WidgetAttestatoModel> Attestati { get; set; } = new();
    }
}
