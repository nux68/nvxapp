using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Models
{
    /*
     Modelli delle licenze (ANALISI_LICENZE.md). Gli enum viaggiano come interi:
        TipoSoggetto 1 Company, 2 FinancialAdvisor
        Ambito       2 Presenze, 3 CRM, 4 Documentale, 100 Cumulativo
        Livello      1 Free, 2 Standard, 3 Pro
        Periodicita  1 Mensile, 2 Annuale
        Metodo       0 Nessuno, 1 Carta di credito, 2 PayPal
        StatoAbbonamento 0 In attesa, 1 Attivo, 2 Scaduto, 3 Sospeso, 4 Annullato
        StatoPagamento   0 Creato, 1 In attesa, 2 Completato, 3 Fallito, 4 Rimborsato, 5 Debug
        StatoLicenza     0 Non sottoscritta, 1 Attiva, 2 In grace, 3 Scaduta
        Fonte            0 Nessuna, 1 Propria, 2 Studio
    */

    // ---------------------------------------------------------------------------
    // Impostazioni (SuperUser)

    public class LicenseSettingsModel
    {
        public bool Attive { get; set; }
        public int? MaxUtentiFree { get; set; }
        public int? MaxUtentiStandard { get; set; }
        public int? MaxUtentiPro { get; set; }
        public int? MaxClientiFree { get; set; }
        public int? MaxClientiStandard { get; set; }
        public int? MaxClientiPro { get; set; }
        public int? MaxSpazioMBFree { get; set; }
        public int? MaxSpazioMBStandard { get; set; }
        public int? MaxSpazioMBPro { get; set; }
        public int GraceGiorni { get; set; }
        public int AvvisoGiorni { get; set; }
    }

    public class LicenseDealerModel
    {
        public int IdDealer { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool PuoGestireListini { get; set; }
    }

    public class LicenseSettingsGetInModel
    {
    }
    public class LicenseSettingsGetOutModel : ModelResult
    {
        public LicenseSettingsModel Settings { get; set; } = new LicenseSettingsModel();
        public List<LicenseDealerModel> Dealer { get; set; } = new List<LicenseDealerModel>();
        public bool DebugPagamenti { get; set; }
    }

    public class LicenseSettingsPutInModel
    {
        public LicenseSettingsModel Settings { get; set; } = new LicenseSettingsModel();
        public List<LicenseDealerModel> Dealer { get; set; } = new List<LicenseDealerModel>();
    }
    public class LicenseSettingsPutOutModel : ModelResult
    {
        public LicenseSettingsModel Settings { get; set; } = new LicenseSettingsModel();
        public List<LicenseDealerModel> Dealer { get; set; } = new List<LicenseDealerModel>();
        public bool DebugPagamenti { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Listini (SuperUser, centro abilitato)

    public class LicensePlanModel
    {
        public int Id { get; set; }
        public int Livello { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public int Ordine { get; set; }
        public decimal PrezzoMensile { get; set; }
        public decimal PrezzoAnnuale { get; set; }
        // limiti (null = illimitati): utenti (Presenze), clienti (CRM), spazio in MB (Documentale); il Cumulativo li usa tutti
        public int? MaxUtenti { get; set; }
        public int? MaxClienti { get; set; }
        public int? MaxSpazioMB { get; set; }
        public bool Attivo { get; set; } = true;
    }

    public class LicenseCatalogModel
    {
        public int Id { get; set; }
        public int? IdDealer { get; set; }
        public string? Dealer { get; set; }
        public int TipoSoggetto { get; set; }
        public int Ambito { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string Valuta { get; set; } = "EUR";
        public bool DisponibileSuCompany { get; set; }
        public bool Attivo { get; set; } = true;
        public List<LicensePlanModel> Piano { get; set; } = new List<LicensePlanModel>();
        // abbonamenti nati da questo listino (validi)
        public int Abbonamenti { get; set; }
        public bool CanEdit { get; set; }
    }

    public class LicenseCatalogListInModel
    {
    }
    public class LicenseCatalogListOutModel : ModelResult
    {
        public List<LicenseCatalogModel> Listino { get; set; } = new List<LicenseCatalogModel>();
        // puo' creare listini (SuperUser: globali; centro abilitato: propri)
        public bool CanCreate { get; set; }
        public bool IsSuperUser { get; set; }
    }

    public class LicenseCatalogGetInModel
    {
        public int Id { get; set; }
    }
    public class LicenseCatalogGetOutModel : ModelResult
    {
        public LicenseCatalogModel Listino { get; set; } = new LicenseCatalogModel();
        public bool IsSuperUser { get; set; }
        public List<LicenseDealerModel> Dealer { get; set; } = new List<LicenseDealerModel>();
    }

    public class LicenseCatalogPutInModel
    {
        public LicenseCatalogModel Listino { get; set; } = new LicenseCatalogModel();
    }
    public class LicenseCatalogPutOutModel : ModelResult
    {
        public LicenseCatalogModel Listino { get; set; } = new LicenseCatalogModel();
    }

    public class LicenseCatalogDeleteInModel
    {
        public int Id { get; set; }
    }
    public class LicenseCatalogDeleteOutModel : ModelResult
    {
        // false = usato da abbonamenti: disattivato invece che eliminato
        public bool Deleted { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Gestione licenze del soggetto (azienda o studio)

    public class LicenseSubscriptionModel
    {
        public int Id { get; set; }
        public int Ambito { get; set; }
        public int Livello { get; set; }
        public string? DescrizionePiano { get; set; }
        public int Periodicita { get; set; }
        public decimal Prezzo { get; set; }
        public string Valuta { get; set; } = "EUR";
        public int? MaxUtenti { get; set; }
        public int? MaxClienti { get; set; }
        public int? MaxSpazioMB { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataScadenza { get; set; }
        public int Stato { get; set; }
        public bool RinnovoAutomatico { get; set; }
        public int Metodo { get; set; }
        public bool DisponibileSuCompany { get; set; }
        // nella grace (scaduta da poco: vale ancora)
        public bool InGrace { get; set; }
        public bool InScadenza { get; set; }
    }

    public class LicenseFunctionStatusModel
    {
        public int Ambito { get; set; }
        public int Stato { get; set; }
        public int? Livello { get; set; }
        public string? DescrizionePiano { get; set; }
        public int Fonte { get; set; }
        public string? Studio { get; set; }
        public DateTime? DataScadenza { get; set; }
        // limite della funzione (null = illimitati o non applicabile): utenti (Presenze), clienti (CRM), spazio (Documentale)
        public int? MaxUtenti { get; set; }
        public int? MaxClienti { get; set; }
        public int? MaxSpazioMB { get; set; }
        // uso attuale: clienti attivi (CRM) e spazio occupato in MB (Documentale); gli utenti sono in UtentiAttivi
        public int? ClientiAttivi { get; set; }
        public long? SpazioUsatoMB { get; set; }
        // uso oltre il limite della funzione
        public bool OltreLimite { get; set; }
    }

    public class LicenseOfferModel
    {
        public int Ambito { get; set; }
        public int IdListino { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string Valuta { get; set; } = "EUR";
        public bool DisponibileSuCompany { get; set; }
        // piani acquistabili (per l'azienda coperta dallo studio solo quelli di livello superiore)
        public List<LicensePlanModel> Piano { get; set; } = new List<LicensePlanModel>();
    }

    public class LicensePaymentModel
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public int Ambito { get; set; }
        public int Livello { get; set; }
        public decimal Importo { get; set; }
        public string Valuta { get; set; } = "EUR";
        public int Periodicita { get; set; }
        public DateTime DataInizioCoperta { get; set; }
        public DateTime DataFineCoperta { get; set; }
        public int Metodo { get; set; }
        public int Stato { get; set; }
        public bool Debug { get; set; }
    }

    public class LicenseCoveredCompanyModel
    {
        public int IdCompany { get; set; }
        public string Azienda { get; set; } = string.Empty;
        // funzioni per cui l'azienda ha un piano proprio superiore a quello dello studio
        public List<int> AmbitoSuperiore { get; set; } = new List<int>();
    }

    public class LicenseStatusGetInModel
    {
    }
    public class LicenseStatusGetOutModel : ModelResult
    {
        public int TipoSoggetto { get; set; }
        public string Soggetto { get; set; } = string.Empty;
        public string? Studio { get; set; }
        // sistema licenze attivo (altrimenti tutto e' consentito, come Pro)
        public bool Attive { get; set; }
        // puo' sottoscrivere e rinnovare (PowerAdmin del soggetto)
        public bool CanManage { get; set; }
        public bool DebugPagamenti { get; set; }
        public List<int> Metodo { get; set; } = new List<int>();
        public int GraceGiorni { get; set; }
        public int UtentiAttivi { get; set; }
        public int? LimiteUtenti { get; set; }
        public List<LicenseFunctionStatusModel> Funzione { get; set; } = new List<LicenseFunctionStatusModel>();
        // stato della licenza Cumulativo (propria o dello studio), come per le singole funzioni
        public LicenseFunctionStatusModel Cumulativo { get; set; } = new LicenseFunctionStatusModel { Ambito = 100 };
        public List<LicenseSubscriptionModel> Abbonamento { get; set; } = new List<LicenseSubscriptionModel>();
        public List<LicenseOfferModel> Offerta { get; set; } = new List<LicenseOfferModel>();
        public List<LicensePaymentModel> Pagamento { get; set; } = new List<LicensePaymentModel>();
        // studio: aziende coperte dalle sue licenze (sola lettura)
        public List<LicenseCoveredCompanyModel> AziendaCoperta { get; set; } = new List<LicenseCoveredCompanyModel>();
    }

    public class LicenseSubscribeInModel
    {
        public int Ambito { get; set; }
        public int Livello { get; set; }
        public int Periodicita { get; set; } = 1;
        public int Metodo { get; set; }
        public bool RinnovoAutomatico { get; set; }
        public string? ReturnUrl { get; set; }
    }
    public class LicenseSubscribeOutModel : ModelResult
    {
        public LicenseSubscriptionModel Abbonamento { get; set; } = new LicenseSubscriptionModel();
        // pagina del provider su cui portare l'utente; null = sottoscrizione conclusa
        public string? RedirectUrl { get; set; }
        public bool Debug { get; set; }
    }

    public class LicenseRenewInModel
    {
        public int IdAbbonamento { get; set; }
        public string? ReturnUrl { get; set; }
    }
    public class LicenseRenewOutModel : ModelResult
    {
        public LicenseSubscriptionModel Abbonamento { get; set; } = new LicenseSubscriptionModel();
        public string? RedirectUrl { get; set; }
        public bool Debug { get; set; }
    }

    public class LicenseAutoRenewPutInModel
    {
        public int IdAbbonamento { get; set; }
        public bool RinnovoAutomatico { get; set; }
    }
    public class LicenseAutoRenewPutOutModel : ModelResult
    {
        public LicenseSubscriptionModel Abbonamento { get; set; } = new LicenseSubscriptionModel();
    }
}
