namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Contratto per i provider di route di navigazione.
    // Ogni modulo (Infrastructure, AttendanceTracking, …) implementa questa
    // interfaccia per esporre la propria mappa  chiave ? route Angular.
    // RouteRegistry si auto-costruisce a runtime dall'IEnumerable<IRouteProvider>
    // iniettato dalla DI — non serve toccare RouteRegistry per aggiungere nuovi moduli.
    public interface IRouteProvider
    {
        // Dizionario  nome/alias (case-insensitive) ? route Angular (es. "/timeclockuser")
        IReadOnlyDictionary<string, string> Routes { get; }

        // Nomi che per alcuni ruoli portano a un'altra pagina: la stessa voce di menu ("Richieste",
        // "Calendario") e' una pagina diversa per l'utente e per l'amministratore.
        // Valgono prima di Routes, nell'ordine dichiarato; per gli utenti con piu' ruoli vince la prima
        // regola di un loro ruolo (si dichiarano prima le pagine dell'amministrazione).
        IReadOnlyList<RoleRoute> RoleRoutes => [];
    }

    // nome della pagina -> route, solo per gli utenti con uno dei ruoli indicati
    public record RoleRoute(string Name, string Route, params string[] Roles);
}
