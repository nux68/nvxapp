using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Payments.Models
{
    /*
     Modelli della ripartizione degli incassi ai centri (ANALISI_PAGAMENTI_CENTRI.md §4). Gli enum viaggiano come interi:
        StatoRendiconto         0 Bozza, 1 Confermato, 2 Liquidato, 3 Annullato
        TipoRiepilogoPagamenti  1 Mensile, 2 Annuale, 3 Liquidazione
        Stato del pagamento, Ambito, Livello, Periodicita: come nelle licenze (LicenseModel.cs)
     Mese null = tutto l'anno. IdDealer null nei riepiloghi = pagamenti di clienti senza centro (restano alla piattaforma).
    */

    public class PaymentDealerSummaryModel
    {
        public int? IdDealer { get; set; }
        public string? Dealer { get; set; }
        /// <summary>pagamenti incassati nel periodo (Completato e Debug)</summary>
        public int NumeroPagamenti { get; set; }
        public decimal Incassato { get; set; }
        /// <summary>pagamenti del periodo poi rimborsati (informativo)</summary>
        public decimal Rimborsato { get; set; }
        /// <summary>di cui in modalita' debug (gia' compresi in NumeroPagamenti e Incassato)</summary>
        public int NumeroDebug { get; set; }
        public decimal ImportoDebug { get; set; }
        /// <summary>trattenuta attuale del centro</summary>
        public decimal PercentualePiattaforma { get; set; }
        /// <summary>stime sull'incassato del periodo con la trattenuta attuale; i definitivi sono nei rendiconti</summary>
        public decimal QuotaPiattaforma { get; set; }
        public decimal QuotaCentro { get; set; }
        /// <summary>quote dei rendiconti del periodo gia' bonificate / confermate e da bonificare</summary>
        public decimal Liquidato { get; set; }
        public decimal DaLiquidare { get; set; }
        /// <summary>solo con il mese: il rendiconto del mese (null = nessuno)</summary>
        public PaymentSettlementModel? Rendiconto { get; set; }
    }

    public class PaymentTotalsModel
    {
        public int NumeroPagamenti { get; set; }
        public decimal Incassato { get; set; }
        public decimal QuotaPiattaforma { get; set; }
        public decimal QuotaCentri { get; set; }
        public decimal Liquidato { get; set; }
        public decimal DaLiquidare { get; set; }
    }

    public class PaymentRowModel
    {
        public int Id { get; set; }
        public DateTime? DataPagamento { get; set; }
        public int? IdDealer { get; set; }
        public string? Dealer { get; set; }
        public string? Soggetto { get; set; }
        /// <summary>1 azienda, 2 studio</summary>
        public int TipoSoggetto { get; set; }
        public int Ambito { get; set; }
        public int Livello { get; set; }
        public int Periodicita { get; set; }
        public DateTime DataInizioCoperta { get; set; }
        public DateTime DataFineCoperta { get; set; }
        public decimal Importo { get; set; }
        public string Valuta { get; set; } = "EUR";
        public int Stato { get; set; }
        public bool Debug { get; set; }
        public int? IdRendiconto { get; set; }
        public int? StatoRendiconto { get; set; }
        public int? IdRendicontoRettifica { get; set; }
    }

    public class PaymentSettlementModel
    {
        public int Id { get; set; }
        public int IdDealer { get; set; }
        public string? Dealer { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public int NumeroPagamenti { get; set; }
        public decimal TotaleIncassato { get; set; }
        public decimal TotaleRettifiche { get; set; }
        public decimal PercentualePiattaforma { get; set; }
        public decimal QuotaPiattaforma { get; set; }
        public decimal QuotaCentro { get; set; }
        public string Valuta { get; set; } = "EUR";
        public int Stato { get; set; }
        public DateTime? DataConferma { get; set; }
        public DateTime? DataLiquidazione { get; set; }
        public string? RiferimentoBonifico { get; set; }
        public string? NumeroFatturaCentro { get; set; }
        public DateTime? DataFatturaCentro { get; set; }
        public string? Note { get; set; }
        public DateTime? UltimoInvio { get; set; }
    }

    public class PaymentDealerItemModel
    {
        public int IdDealer { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class PaymentDealerConfigModel
    {
        public int IdDealer { get; set; }
        public string? Dealer { get; set; }
        /// <summary>null = trattenuta generale</summary>
        public decimal? PercentualePiattaforma { get; set; }
        public decimal PercentualeGenerale { get; set; }
        public string? Iban { get; set; }
        public string? Intestatario { get; set; }
        public string? EmailAmministrazione { get; set; }
    }

    public class PaymentSettingsModel
    {
        public decimal PercentualePiattaforma { get; set; }
        public bool RiepilogoAutomatico { get; set; }
        public int GiornoInvioMensile { get; set; }
        public int MeseInvioAnnuale { get; set; }
    }

    public class PaymentEmailLogModel
    {
        public DateTime? Data { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
        public int Tipo { get; set; }
        public bool Automatico { get; set; }
        public string? Destinatari { get; set; }
        public bool Esito { get; set; }
        public string? Errore { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Riepilogo per centro (SuperUser) e del centro

    public class PaymentReportGetInModel
    {
        public int Anno { get; set; }
        public int? Mese { get; set; }
    }

    public class PaymentReportGetOutModel : ModelResult
    {
        public bool IsSuperUser { get; set; }
        public List<PaymentDealerSummaryModel> Centro { get; set; } = new();
        public PaymentTotalsModel Totali { get; set; } = new();
        public bool DebugPagamenti { get; set; }
    }

    public class PaymentDealerGetInModel
    {
        /// <summary>ignorato per gli utenti del centro (vale quello del token)</summary>
        public int? IdDealer { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
    }

    public class PaymentDealerGetOutModel : ModelResult
    {
        public bool IsSuperUser { get; set; }
        /// <summary>DealerPowerAdmin del centro o SuperUser: dati di liquidazione e fattura</summary>
        public bool CanEditDealerData { get; set; }
        public bool EmailConfigurata { get; set; }
        public PaymentDealerSummaryModel Riepilogo { get; set; } = new();
        public List<PaymentRowModel> Pagamento { get; set; } = new();
        /// <summary>rendiconti dell'anno (il centro non vede le bozze)</summary>
        public List<PaymentSettlementModel> Rendiconto { get; set; } = new();
        public List<PaymentEmailLogModel> Invio { get; set; } = new();
        public List<PaymentDealerItemModel> Dealer { get; set; } = new();
    }

    // ---------------------------------------------------------------------------
    // Rendiconti (SuperUser; la fattura anche il DealerPowerAdmin)

    public class PaymentSettlementGenerateInModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        /// <summary>null = tutti i centri con qualcosa da liquidare</summary>
        public int? IdDealer { get; set; }
    }

    public class PaymentSettlementGenerateOutModel : ModelResult
    {
        public int Generati { get; set; }
    }

    public class PaymentSettlementConfirmInModel
    {
        public int Id { get; set; }
    }

    public class PaymentSettlementConfirmOutModel : ModelResult
    {
        public PaymentSettlementModel Rendiconto { get; set; } = new();
        public bool EmailInviata { get; set; }
    }

    public class PaymentSettlementCancelInModel
    {
        public int Id { get; set; }
    }

    public class PaymentSettlementCancelOutModel : ModelResult { }

    public class PaymentSettlementPayInModel
    {
        public int Id { get; set; }
        public DateTime DataLiquidazione { get; set; }
        public string? RiferimentoBonifico { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentSettlementPayOutModel : ModelResult
    {
        public PaymentSettlementModel Rendiconto { get; set; } = new();
        public bool EmailInviata { get; set; }
    }

    public class PaymentSettlementInvoicePutInModel
    {
        public int Id { get; set; }
        public string? NumeroFatturaCentro { get; set; }
        public DateTime? DataFatturaCentro { get; set; }
    }

    public class PaymentSettlementInvoicePutOutModel : ModelResult
    {
        public PaymentSettlementModel Rendiconto { get; set; } = new();
    }

    // ---------------------------------------------------------------------------
    // Email e documenti

    public class PaymentSummarySendInModel
    {
        public int? IdDealer { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
    }

    /// <summary>l'email parte con un job: l'esito arriva nella barra dei job</summary>
    public class PaymentSummarySendOutModel : ModelResult
    {
        public string JobId { get; set; } = string.Empty;
        public string? Destinatari { get; set; }
    }

    public class PaymentExportInModel
    {
        /// <summary>SuperUser: null = tutti i centri (solo Excel)</summary>
        public int? IdDealer { get; set; }
        public int Anno { get; set; }
        public int? Mese { get; set; }
        /// <summary>"pdf" o "xlsx"</summary>
        public string Formato { get; set; } = "pdf";
    }

    /// <summary>il file si prepara con un job: arriva nella barra dei job con il link di download</summary>
    public class PaymentExportOutModel : ModelResult
    {
        public string JobId { get; set; } = string.Empty;
    }

    // ---------------------------------------------------------------------------
    // Dati di liquidazione del centro e impostazioni

    public class PaymentDealerConfigGetInModel
    {
        public int? IdDealer { get; set; }
    }

    public class PaymentDealerConfigGetOutModel : ModelResult
    {
        public PaymentDealerConfigModel Config { get; set; } = new();
        public bool CanEditPercentuale { get; set; }
    }

    public class PaymentDealerConfigPutInModel
    {
        public PaymentDealerConfigModel Config { get; set; } = new();
    }

    public class PaymentDealerConfigPutOutModel : ModelResult
    {
        public PaymentDealerConfigModel Config { get; set; } = new();
    }

    public class PaymentSettingsGetInModel { }

    public class PaymentSettingsGetOutModel : ModelResult
    {
        public PaymentSettingsModel Settings { get; set; } = new();
        public bool EmailConfigurata { get; set; }
        public bool DebugPagamenti { get; set; }
    }

    public class PaymentSettingsPutInModel
    {
        public PaymentSettingsModel Settings { get; set; } = new();
    }

    public class PaymentSettingsPutOutModel : ModelResult
    {
        public PaymentSettingsModel Settings { get; set; } = new();
    }
}
