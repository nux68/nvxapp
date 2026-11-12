using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService.Models
{
    // dipendenti presenti e assenti oggi (widget "Presenti oggi", ANALISI_ATTENDANCETRACKING_WIDGET.md)
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

}
