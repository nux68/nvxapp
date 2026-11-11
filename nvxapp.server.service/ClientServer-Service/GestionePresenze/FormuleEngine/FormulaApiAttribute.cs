namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /// <summary>
    /// Membro della facciata "x" esposto alle formule: alimenta la "Lista metodi", l'autocompletamento
    /// dell'editor, il controllo dei membri in CtrlSintassi e il prompt dell'AI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FormulaApiAttribute : Attribute
    {
        public string Descrizione { get; }
        public bool InEvidenza { get; set; }
        /// <summary>Firma da mostrare (es. "GetMin(codice)"); se vuota si ricava dai parametri.</summary>
        public string? Firma { get; set; }

        public FormulaApiAttribute(string descrizione)
        {
            Descrizione = descrizione;
        }
    }

    /// <summary>Errore "previsto" della formula (es. codice causale inesistente): il messaggio va nel log com'e'.</summary>
    public class FormulaException : Exception
    {
        public FormulaException(string message) : base(message) { }
    }
}
