namespace nvxapp.server.data.Entities.Public
{
    /// <summary>stato del rendiconto con cui il SuperUser liquida al centro la sua quota (ANALISI_PAGAMENTI_CENTRI.md §3)</summary>
    public enum StatoRendiconto
    {
        /// <summary>generato, modificabile e rigenerabile; il centro non lo vede ancora</summary>
        Bozza = 0,
        /// <summary>confermato dal SuperUser e inviato al centro; i pagamenti sono bloccati sul rendiconto</summary>
        Confermato = 1,
        /// <summary>quota del centro bonificata</summary>
        Liquidato = 2,
        /// <summary>annullato: i pagamenti tornano da liquidare</summary>
        Annullato = 3,
    }

    /// <summary>tipo di email inviata al centro</summary>
    public enum TipoRiepilogoPagamenti
    {
        Mensile = 1,
        Annuale = 2,
        Liquidazione = 3,
    }
}
