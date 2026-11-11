using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService.Models
{
    // controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md)

    /// <summary>costo orario di un dipendente impostato nella pagina (non salvato sul server, CDG1)</summary>
    public class CdgCostoDipendente
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public decimal CostoOrario { get; set; }
    }

    /// <summary>ricavo di una sottocommessa impostato nella pagina (non salvato sul server, CDG2)</summary>
    public class CdgRicavo
    {
        public int IdAz_SubCommessa { get; set; }
        public decimal Ricavo { get; set; }
    }

    public class CdgParametri
    {
        /// <summary>periodo: dal mese Year/Month al mese YearAl/MonthAl (compresi, al massimo 12 mesi)</summary>
        public int Year { get; set; }
        public int Month { get; set; }
        public int YearAl { get; set; }
        public int MonthAl { get; set; }

        public decimal? CostoOrarioPredefinito { get; set; }
        public List<CdgCostoDipendente> CostiDipendenti { get; set; } = new List<CdgCostoDipendente>();
        public List<CdgRicavo> Ricavi { get; set; } = new List<CdgRicavo>();

        /// <summary>commesse da mostrare; vuoto = tutte</summary>
        public List<int> IdsCommessa { get; set; } = new List<int>();
    }

    public class Az_CommessaCdg_GetInModel
    {
        public CdgParametri Parametri { get; set; } = new CdgParametri();
    }

    public class Az_CommessaCdg_GetOutModel : ModelResult
    {
        public CdgRisultato Risultato { get; set; } = new CdgRisultato();
    }

    /// <summary>valori di una riga (commessa, sottocommessa, totale)</summary>
    public class CdgValori
    {
        public int Minuti { get; set; }
        public decimal CostoPersonale { get; set; }
        public decimal NoteSpese { get; set; }
        public decimal Costo { get; set; }
        /// <summary>null = ricavo non impostato: margine non calcolato</summary>
        public decimal? Ricavo { get; set; }
        public decimal? Margine { get; set; }
        public decimal? MarginePerc { get; set; }
    }

    public class CdgRigaDipendente
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public int Minuti { get; set; }
        public decimal CostoOrario { get; set; }
        public decimal CostoPersonale { get; set; }
        public decimal NoteSpese { get; set; }
    }

    public class CdgSubCommessaModel : CdgValori
    {
        public int IdAz_SubCommessa { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public List<CdgRigaDipendente> Dipendenti { get; set; } = new List<CdgRigaDipendente>();
    }

    public class CdgCommessaModel : CdgValori
    {
        public int IdAz_Commessa { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public List<CdgSubCommessaModel> SubCommesse { get; set; } = new List<CdgSubCommessaModel>();
    }

    public class CdgDipendenteModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public int Minuti { get; set; }
        public decimal NoteSpese { get; set; }
        /// <summary>costo orario usato; DaPredefinito = non impostato per il dipendente</summary>
        public decimal CostoOrario { get; set; }
        public bool DaPredefinito { get; set; }
    }

    public class CdgRisultato
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public decimal CostoOrarioPredefinito { get; set; }
        public List<CdgCommessaModel> Commesse { get; set; } = new List<CdgCommessaModel>();
        public CdgValori Totale { get; set; } = new CdgValori();
        public List<CdgDipendenteModel> Dipendenti { get; set; } = new List<CdgDipendenteModel>();
        /// <summary>giornate non conteggiate (errori o non calcolate), come in Statistiche attivita'</summary>
        public int GiornateEscluse { get; set; }
        /// <summary>note spese senza sottocommessa a cui attribuirle</summary>
        public decimal NoteSpeseNonAttribuite { get; set; }
        /// <summary>dipendenti senza costo orario (ne' proprio ne' predefinito)</summary>
        public int DipendentiSenzaCosto { get; set; }
    }

    // widget "Ore sulle commesse": mese corrente
    public class Widget_OreCommesseGetInModel
    {
        /// <summary>costo orario predefinito ricordato dal browser (facoltativo)</summary>
        public decimal? CostoOrarioPredefinito { get; set; }
    }

    public class Widget_OreCommesseGetOutModel : ModelResult
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int MinutiTotali { get; set; }
        public decimal? CostoTotale { get; set; }
        public List<CdgCommessaModel> Commesse { get; set; } = new List<CdgCommessaModel>();
    }
}
