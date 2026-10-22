namespace nvxapp.server.data.Localization
{
    /*
     Errore previsto (validazione, dato non trovato, operazione non permessa...): il messaggio e'
     destinato all'utente, gia' nella sua lingua (vedi ServerText), e arriva al client cosi' com'e'.
     Ogni altra eccezione e' un errore tecnico: il client riceve un messaggio generico con un codice
     di riferimento e il dettaglio resta solo nel log (ServiceBase.ExecuteAction).
    */
    public class UserMessageException : Exception
    {
        public UserMessageException(string message) : base(message) { }

        public UserMessageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
