using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService.Models
{
    // anomalie e giornate da sistemare (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md)
    public class Dip_Anomalie_GetInModel
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        /// <summary>dipendenti selezionati nella pagina; vuoto = tutti quelli visibili</summary>
        public List<string> UsersId { get; set; } = new List<string>();
        /// <summary>true = solo le giornate da sistemare (con almeno un errore)</summary>
        public bool SoloDaSistemare { get; set; }
    }

    public class Dip_Anomalie_GetOutModel : ModelResult
    {
        public AnomalieRisultato Risultato { get; set; } = new AnomalieRisultato();
    }

    public class Dip_Anomalie_SpiegaOutModel : ModelResult
    {
        public string Testo { get; set; } = string.Empty;
        public bool GeneratoConAI { get; set; }
        public string? ProviderAI { get; set; }
    }

    // previsione delle assenze
    public class Dip_PrevisioneAssenze_GetInModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        /// <summary>giorni da prevedere a partire da oggi (1-92, default 30)</summary>
        public int Giorni { get; set; } = 30;
    }

    public class Dip_PrevisioneAssenze_GetOutModel : ModelResult
    {
        public PrevisioneRisultato Risultato { get; set; } = new PrevisioneRisultato();
    }

    // widget "Giornate da sistemare": mese corrente fino a ieri
    public class Widget_GiornateDaSistemareGetInModel { }

    public class Widget_GiornateDaSistemareGetOutModel : ModelResult
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public int DaSistemare { get; set; }
        public int DaVerificare { get; set; }
        public int Dipendenti { get; set; }
        public List<AnomaliaConteggioModel> Conteggi { get; set; } = new List<AnomaliaConteggioModel>();
    }

    // widget "Assenze previste": prossimi 7 giorni
    public class Widget_PrevisioneAssenzeGetInModel { }

    public class Widget_PrevisioneAssenzeGetOutModel : ModelResult
    {
        public decimal MediaPreviste { get; set; }
        public int Pianificate { get; set; }
        public List<PrevisioneGiornoModel> Giorni { get; set; } = new List<PrevisioneGiornoModel>();
    }
}
