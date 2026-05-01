using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models
{
        

    public class ActivityStatisticsModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        //public DateTime Dal { get; set; }
        //public DateTime Al { get; set; }

        /// <summary>Dipendenti selezionati. Lista vuota = tutti i dipendenti.</summary>
        public List<string> SelectedUserId { get; set; } = new List<string>();

        // --- Filtri opzionali (non ancora forniti dall'interfaccia) ---

        /// <summary>Filtro su commesse specifiche. Null o lista vuota = tutte.</summary>
        public List<int>? IdsCommessa { get; set; }

        /// <summary>Filtro su sub-commesse specifiche. Null o lista vuota = tutte.</summary>
        public List<int>? IdsSubCommessa { get; set; }

        /// <summary>Filtro su clienti specifici. Null o lista vuota = tutti.</summary>
        public List<int>? IdsCliente { get; set; }

        /// <summary>Filtro su attività specifiche (IdAz_SubCommessaAttivita). Null o lista vuota = tutte.</summary>
        public List<int>? IdsAttivita { get; set; }
    }

    public class ActivityStatistics_GetInModel
    {
        public ActivityStatisticsModel ActivityStatistics { get; set; } = new ActivityStatisticsModel();
    }

    // ---------------------------------------------------------------
    // Modelli di output
    // ---------------------------------------------------------------

    /// <summary>Riga di statistica ore: dipendente × attività nel periodo.</summary>
    public class ActivityStatistics_RowModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public int IdAz_SubCommessaAttivita { get; set; }
        public string NomeAttivita { get; set; } = string.Empty;
        public int IdAz_SubCommessa { get; set; }
        public string NomeSubCommessa { get; set; } = string.Empty;
        public int IdAz_Commessa { get; set; }
        public string NomeCommessa { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        /// <summary>Totale ore lavorate (in minuti interi per semplicità di serializzazione).</summary>
        public int TotaleMinuti { get; set; }
    }

    /// <summary>Riga di statistica aggregata solo per attività (tutti i dipendenti selezionati).</summary>
    public class ActivityStatistics_TotaleAttivitaModel
    {
        public int IdAz_SubCommessaAttivita { get; set; }
        public string NomeAttivita { get; set; } = string.Empty;
        public int IdAz_SubCommessa { get; set; }
        public string NomeSubCommessa { get; set; } = string.Empty;
        public int IdAz_Commessa { get; set; }
        public string NomeCommessa { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public int TotaleMinuti { get; set; }
    }

    /// <summary>Giornate escluse dal conteggio per stato Init/Err.</summary>
    public class ActivityStatistics_GiornataEsclusaModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public GG_ResultStato Stato { get; set; }
    }

    public class ActivityStatistics_GetOutModel : ModelResult
    {
        /// <summary>Ore per dipendente × attività.</summary>
        public List<ActivityStatistics_RowModel> RighePerDipendente { get; set; } = new List<ActivityStatistics_RowModel>();

        /// <summary>Ore aggregate per attività su tutti i dipendenti selezionati.</summary>
        public List<ActivityStatistics_TotaleAttivitaModel> TotaliPerAttivita { get; set; } = new List<ActivityStatistics_TotaleAttivitaModel>();

        /// <summary>Giornate escluse dal conteggio (stato Init o Err).</summary>
        public List<ActivityStatistics_GiornataEsclusaModel> GiornateEscluse { get; set; } = new List<ActivityStatistics_GiornataEsclusaModel>();
    }
}
