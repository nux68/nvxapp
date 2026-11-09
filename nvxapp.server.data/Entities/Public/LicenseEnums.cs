namespace nvxapp.server.data.Entities.Public
{
    /*
     Enum delle licenze d'uso (ANALISI_LICENZE.md). Salvati come interi: i valori non vanno rinumerati.
    */

    /// <summary>Chi sottoscrive: azienda o studio (ANALISI_LICENZE.md §5).</summary>
    public enum TipoSoggettoLicenza
    {
        Company = 1,
        FinancialAdvisor = 2,
    }

    /// <summary>
    /// Funzione coperta dal listino. I valori delle singole funzioni coincidono con ApplicationType
    /// (Presenze = 2, CRM = 3, Documentale = 4); Cumulativo copre tutte le funzioni.
    /// </summary>
    public enum AmbitoLicenza
    {
        AttendanceTracking = 2,
        Crm = 3,
        DocumentManagement = 4,
        Cumulativo = 100,
    }

    /// <summary>Livello del piano: stessa scala per tutti i listini, il piu' alto vince (ANALISI_LICENZE.md §3.1, §5).</summary>
    public enum LivelloLicenza
    {
        Free = 1,
        Standard = 2,
        Pro = 3,
    }

    public enum PeriodicitaLicenza
    {
        Mensile = 1,
        Annuale = 2,
    }

    public enum StatoAbbonamento
    {
        /// <summary>in attesa dell'esito del pagamento (gateway reali)</summary>
        InAttesa = 0,
        Attivo = 1,
        Scaduto = 2,
        Sospeso = 3,
        Annullato = 4,
    }

    public enum MetodoPagamento
    {
        Nessuno = 0,
        CartaCredito = 1,
        PayPal = 2,
    }

    public enum StatoPagamento
    {
        Creato = 0,
        InAttesa = 1,
        Completato = 2,
        Fallito = 3,
        Rimborsato = 4,
        /// <summary>giro completo senza transazione reale (ANALISI_LICENZE.md §7.2)</summary>
        Debug = 5,
    }
}
