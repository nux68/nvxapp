using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User
{
    // Registro centrale auto-costruito.
    // Non contiene mapping espliciti: ogni IUserInitializer dichiara
    // il proprio nome e priorità e viene registrato automaticamente.
    // Per aggiungere un nuovo modulo di inizializzazione basta creare
    // un nuovo IUserInitializer e registrarlo nella DI.
    public interface IUserInitializerRegistry
    {
        // Esegue TUTTI gli inizializzatori in ordine di priorità.
        // Se uno fallisce, logga l'errore e prosegue con i successivi
        // (strategia ContinueOnError per garantire la massima resilienza).
        Task InitializeAllAsync(UserCompany userCompany);
    }

    public class UserInitializerRegistry : IUserInitializerRegistry
    {
        private readonly List<IUserInitializer> _initializers;
        private readonly ILogger<UserInitializerRegistry> _logger;

        // Riceve tutti gli IUserInitializer registrati nella DI.
        // Li ordina per Priority crescente.
        public UserInitializerRegistry(
            IEnumerable<IUserInitializer> initializers,
            ILogger<UserInitializerRegistry> logger)
        {
            _initializers = initializers
                .OrderBy(i => i.Priority)
                .ToList();
            _logger = logger;
        }

        public async Task InitializeAllAsync(UserCompany userCompany)
        {
            _logger.LogInformation(
                "[UserInit] Avvio inizializzazione per utente {UserId}, Company={CompanyId}. " +
                "Inizializzatori disponibili: {Count}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany,
                _initializers.Count);

            foreach (var initializer in _initializers)
            {
                try
                {
                    _logger.LogInformation(
                        "[UserInit] Esecuzione '{InitializerName}' (Priority={Priority})...",
                        initializer.Name,
                        initializer.Priority);

                    await initializer.InitializeAsync(userCompany);

                    _logger.LogInformation(
                        "[UserInit] '{InitializerName}' completato con successo.",
                        initializer.Name);
                }
                catch (Exception ex)
                {
                    // Strategia ContinueOnError: logghiamo l'errore e proseguiamo
                    // con i successivi inizializzatori per non bloccare
                    // la creazione dell'utente a causa di un modulo.
                    _logger.LogError(ex,
                        "[UserInit] ERRORE in '{InitializerName}': {Message}. " +
                        "Proseguo con i successivi inizializzatori.",
                        initializer.Name,
                        ex.Message);
                }
            }

            _logger.LogInformation(
                "[UserInit] Inizializzazione completata per utente {UserId}.",
                userCompany.IdAspNetUsers);
        }
    }
}