namespace nvxapp.server.data.Entities.Public
{
    /*
     Livelli dei ticket di assistenza (vedi nvxapp.server.service/ANALISI_TICKET.md).
     Il numero cresce scendendo nella gerarchia: un livello "sopra" ha un valore piu' basso.
     I valori finiscono nel database: non vanno cambiati.
    */
    public enum TicketLevel
    {
        SuperUser = 0,
        Dev = 1,
        Dealer = 2,
        FinancialAdvisor = 3,
        Company = 4,
        User = 5,
    }

    // stato del ticket
    public enum TicketState
    {
        Aperto = 0,          // in attesa al livello corrente, senza assegnatario
        InCarico = 1,        // assegnato a un utente del livello corrente
        InAttesaInfo = 2,    // rimandato a chi ha aperto o a un livello intermedio per chiedere informazioni
        Risolto = 3,         // risposta data, visibilita' a chi ha aperto
        Chiuso = 4,          // finale
    }

    // passaggio registrato nella cronologia
    public enum TicketAction
    {
        Apertura = 0,
        Assegnazione = 1,
        Inoltro = 2,
        RichiestaInfo = 3,
        RispostaInfo = 4,
        Nota = 5,
        Risoluzione = 6,
        RisoluzioneAI = 7,
        Riapertura = 8,
        Chiusura = 9,
        Riclassificazione = 10,
        Spostamento = 11,    // l'azienda e' passata a un altro studio (CompanyMove)
    }
}
