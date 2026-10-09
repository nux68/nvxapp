using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using System.Collections.Concurrent;

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
        // Mantiene una cache in-memory: se l'utente è già stato inizializzato
        // con successo in questa istanza, esce immediatamente.
        Task InitializeAllAsync(UserCompany userCompany);
    }

    public class UserInitializerRegistry : IUserInitializerRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<UserInitializerRegistry> _logger;
        private static readonly ConcurrentDictionary<string, byte> _initializedUsers = new();

        // Gli IUserInitializer registrati nella DI vengono risolti in uno scope dedicato
        // all'azienda dell'utente (vedi InitializeAllAsync) e ordinati per Priority crescente.
        public UserInitializerRegistry(
            IServiceScopeFactory scopeFactory,
            ICompanyRepository companyRepository,
            ILogger<UserInitializerRegistry> logger)
        {
            _scopeFactory = scopeFactory;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task InitializeAllAsync(UserCompany userCompany)
        {
            // Cache in-memory: se l'utente è già stato inizializzato con successo
            // in questa istanza del server, evita ogni ulteriore esecuzione.
            if (_initializedUsers.ContainsKey(userCompany.IdAspNetUsers))
            {
                _logger.LogDebug(
                    "[UserInit] Utente {UserId} già inizializzato in questa istanza. Skip.",
                    userCompany.IdAspNetUsers);
                return;
            }

            // Gli inizializzatori lavorano sui dati dell'azienda DELL'UTENTE, non su quella della
            // richiesta: scope DI dedicato, cosi' i TenantDbContext risolvono lo schema di quell'azienda.
            var company = await _companyRepository.FindByIdAsync(userCompany.IdCompany);
            using var tenantScope = TenantScope.Use(company?.Schema);
            using var scope = _scopeFactory.CreateScope();
            var initializers = scope.ServiceProvider.GetServices<IUserInitializer>()
                                                    .OrderBy(i => i.Priority)
                                                    .ToList();

            _logger.LogInformation(
                "[UserInit] Avvio inizializzazione per utente {UserId}, Company={CompanyId}, Schema={Schema}. " +
                "Inizializzatori disponibili: {Count}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany,
                company?.Schema,
                initializers.Count);

            foreach (var initializer in initializers)
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

            // Marca l'utente come inizializzato in questa istanza
            _initializedUsers.TryAdd(userCompany.IdAspNetUsers, 0);

            _logger.LogInformation(
                "[UserInit] Inizializzazione completata per utente {UserId}.",
                userCompany.IdAspNetUsers);
        }
    }
}