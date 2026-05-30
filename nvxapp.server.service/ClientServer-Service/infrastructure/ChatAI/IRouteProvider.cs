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
    }
}
