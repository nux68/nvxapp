using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService.Models
{
    // dipendenti presenti e assenti oggi (widget "Presenti oggi", ANALISI_WIDGET_PRESENZE.md)
    public class Widget_PresenzeOggiGetInModel
    {
    }

    public class Widget_PresenzeOggiGetOutModel : ModelResult
    {
        public DateTime Data { get; set; }
        // dipendenti considerati (in servizio oggi, nei reparti visibili a chi chiede)
        public int Totale { get; set; }
        public int Presenti { get; set; }
        public int Assenti { get; set; }
        // assenti con un giustificativo di giornata approvato (ferie, malattia, ...): parte degli assenti
        public int AssentiGiustificati { get; set; }
    }

    // richieste approvate e da approvare (widget "Richieste", ANALISI_WIDGET_PRESENZE.md)
    public class Widget_RichiesteGetInModel
    {
    }

    public class Widget_RichiesteGetOutModel : ModelResult
    {
        // true = conteggi da amministratore (richieste dei dipendenti), false = proprie richieste
        public bool Admin { get; set; }
        // richieste che l'utente puo' approvare adesso (solo amministratori)
        public int DaApprovare { get; set; }
        // proprie richieste ancora da decidere
        public int InAttesa { get; set; }
        // richieste approvate del mese corrente (dei dipendenti per l'amministratore, proprie per l'utente)
        public int ApprovateMese { get; set; }
    }
}
