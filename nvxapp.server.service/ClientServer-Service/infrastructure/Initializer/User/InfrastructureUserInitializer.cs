using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User
{
    // Inizializzatore per il dominio Infrastructure.
    // Gestisce le strutture dati infrastrutturali necessarie
    // per un nuovo utente (es. preferenze, notifiche, cfg personali).
    // Priority=10: eseguito prima di GestionePresenze.
    public class InfrastructureUserInitializer : IUserInitializer
    {
        private readonly ILogger<InfrastructureUserInitializer> _logger;

        public string Name => "Infrastructure";
        public int Priority => 10;

        public InfrastructureUserInitializer(ILogger<InfrastructureUserInitializer> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(UserCompany userCompany)
        {
            _logger.LogInformation(
                "[InfrastructureUserInitializer] Inizializzazione infrastructure " +
                "per utente {UserId}, Company={CompanyId}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany);

            // TODO: In futuro qui si potranno inizializzare:
            // - Preferenze utente di default
            // - Sottoscrizioni notifiche
            // - Configurazioni personali
            // - Sessioni o stati infrastrutturali

            await Task.CompletedTask;

            _logger.LogInformation(
                "[InfrastructureUserInitializer] Completato per utente {UserId}.",
                userCompany.IdAspNetUsers);
        }
    }
}