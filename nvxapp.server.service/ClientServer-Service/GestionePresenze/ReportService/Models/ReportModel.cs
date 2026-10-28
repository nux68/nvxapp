using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService.Models
{
    // report PDF delle pagine HR (pulsante Report del FAB)
    public enum ReportTipo
    {
        AttivitaTotali = 0,          // Statistiche attivita': totali per attivita'
        AttivitaPerDipendente = 1,   // Statistiche attivita': per dipendente
        PianoFerie = 2,              // Piano ferie
        QuadraturaMensile = 3,       // Quadratura mensile, per reparto
        NotaSpesePerTipo = 4,        // Note spese: per tipo di spesa
        NotaSpesePerDipendente = 5   // Note spese: per dipendente
    }

    public class ReportModel
    {
        public ReportTipo Tipo { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }    // 1-12

        // dipendenti selezionati nella pagina (il server tiene solo quelli che l'utente puo' vedere)
        public List<string> SelectedUserId { get; set; } = new List<string>();

        // quadratura mensile: reparti selezionati (i dipendenti si raggruppano per reparto)
        public List<int> RepartiId { get; set; } = new List<int>();

        // note spese: stessi filtri della pagina (tipo di spesa, null/0 = tutti; stati, null/vuoto = tutti)
        public int? IdPar_TipoSpesa { get; set; }
        public List<StatoRichiesta>? Stati { get; set; }
    }

    public class Report_InModel
    {
        public ReportModel Report { get; set; } = new ReportModel();
    }

    public class Report_OutModel : ModelResult
    {
        public string JobId { get; set; } = string.Empty;
    }
}
