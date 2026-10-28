namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Entità che la chat sa aprire ("apri la causale ferie", "modifica il reparto produzione").
    // Ogni modulo dichiara le proprie implementando questa interfaccia: le registra la DI (scanner
    // dell'assembly) e le usa il comando unico OpenEntityHandler. Per una nuova anagrafica basta
    // una nuova classe, senza un nuovo comando.
    public interface IOpenableEntity
    {
        // valore canonico dello slot "entity" (es. "causale")
        string Key { get; }

        // nome dell'entità nella lingua della richiesta, per messaggi e prompt (es. "causale")
        string DisplayName { get; }

        // parole con cui l'utente la indica, italiano e inglese (es. "causale", "causali", "attendance code")
        IReadOnlyList<string> Words { get; }

        // ruoli che possono aprirla ("*" = tutti)
        IReadOnlyList<string> Roles { get; }

        // pagina di modifica: si apre con history.state = { id }
        string EditRoute { get; }

        // pagine in cui il comando è utile (help contestuale): elenco e modifica
        IReadOnlyList<string> Pages { get; }

        // elementi tra cui cercare quello da aprire
        Task<List<OpenableItem>> LoadAsync();
    }

    public record OpenableItem(int Id, string Descrizione, string? Codice = null);
}
