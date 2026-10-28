using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using Serilog;
using System.Collections.Concurrent;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Contratto per lo store delle sessioni conversazionali.
    // Separato da ChatAIService per poter essere registrato come Singleton
    // in modo esplicito, indipendentemente dal ciclo di vita Scoped del service.
    public interface IChatSessionStore
    {
        // Ritorna la sessione solo se esiste, non è scaduta e appartiene a userId.
        ChatSession? TryGet(string sessionId, string userId, int timeoutMinutes = 10);
        ChatSession Create(string userId);
        void Delete(string sessionId);
    }

    // Implementazione in-memoria con ConcurrentDictionary.
    // Registrata come Singleton: una sola istanza per tutta la vita dell'applicazione,
    // condivisa tra tutte le richieste HTTP concorrenti in modo thread-safe.
    public class InMemoryChatSessionStore : IChatSessionStore
    {
        private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

        public ChatSession? TryGet(string sessionId, string userId, int timeoutMinutes = 10)
        {
            if (string.IsNullOrEmpty(sessionId)) return null;

            if (_sessions.TryGetValue(sessionId, out var session))
            {
                // una sessione di un altro utente non si espone (ne' si rimuove)
                if (!string.Equals(session.UserId, userId ?? string.Empty, StringComparison.Ordinal))
                {
                    Log.Warning("[ChatAI] Sessione di un altro utente ignorata. SessionId={SessionId}", sessionId);
                    return null;
                }

                if ((DateTime.UtcNow - session.LastActivity).TotalMinutes < timeoutMinutes)
                    return session;

                _sessions.TryRemove(sessionId, out _);
                Log.Information("[ChatAI] Sessione scaduta rimossa. SessionId={SessionId}", sessionId);
            }

            return null;
        }

        public ChatSession Create(string userId)
        {
            var session = new ChatSession { UserId = userId ?? string.Empty };
            _sessions[session.SessionId] = session;
            return session;
        }

        public void Delete(string sessionId) =>
            _sessions.TryRemove(sessionId, out _);
    }
}
