using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using System.Collections.Concurrent;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit
{
    // Registro centrale auto-costruito per l'inizializzazione delle aziende.
    // Non contiene mapping espliciti: ogni ICompanyInitializer dichiara
    // il proprio nome e priorità e viene registrato automaticamente.
    // Per aggiungere un nuovo modulo di inizializzazione basta creare
    // un nuovo ICompanyInitializer e registrarlo nella DI.
    public interface ICompanyInitializerRegistry
    {
        // Esegue TUTTI gli inizializzatori in ordine di priorità.
        // Se uno fallisce, logga l'errore e prosegue con i successivi
        // (strategia ContinueOnError per garantire la massima resilienza).
        // Mantiene una cache in-memory: se l'azienda è già stata inizializzata
        // con successo in questa istanza, esce immediatamente.
        Task InitializeAllAsync(Company company);
    }

    public class CompanyInitializerRegistry : ICompanyInitializerRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CompanyInitializerRegistry> _logger;
        private static readonly ConcurrentDictionary<int, byte> _initializedCompanies = new();

        // Gli ICompanyInitializer registrati nella DI vengono risolti in uno scope dedicato
        // all'azienda (vedi InitializeAllAsync) e ordinati per Priority crescente.
        public CompanyInitializerRegistry(
            IServiceScopeFactory scopeFactory,
            ILogger<CompanyInitializerRegistry> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InitializeAllAsync(Company company)
        {
            // Cache in-memory: se l'azienda è già stata inizializzata con successo
            // in questa istanza del server, evita ogni ulteriore esecuzione.
            if (_initializedCompanies.ContainsKey(company.Id))
            {
                _logger.LogDebug(
                    "[CompanyInit] Azienda {CompanyId} già inizializzata in questa istanza. Skip.",
                    company.Id);
                return;
            }

            // Gli inizializzatori lavorano sui dati DELL'AZIENDA indicata, non su quella della
            // richiesta (es. CompanyGet chiamato da uno studio, senza azienda nel token):
            // scope DI dedicato, cosi' i TenantDbContext risolvono lo schema di questa azienda.
            using var tenantScope = TenantScope.Use(company.Schema);
            using var scope = _scopeFactory.CreateScope();
            var initializers = scope.ServiceProvider.GetServices<ICompanyInitializer>()
                                                    .OrderBy(i => i.Priority)
                                                    .ToList();

            _logger.LogInformation(
                "[CompanyInit] Avvio inizializzazione per azienda {CompanyId}, Descrizione={Descrizione}, Schema={Schema}. " +
                "Inizializzatori disponibili: {Count}",
                company.Id,
                company.Descrizione,
                company.Schema,
                initializers.Count);

            foreach (var initializer in initializers)
            {
                try
                {
                    _logger.LogInformation(
                        "[CompanyInit] Esecuzione '{InitializerName}' (Priority={Priority})...",
                        initializer.Name,
                        initializer.Priority);

                    await initializer.InitializeAsync(company);

                    _logger.LogInformation(
                        "[CompanyInit] '{InitializerName}' completato con successo.",
                        initializer.Name);
                }
                catch (Exception ex)
                {
                    // Strategia ContinueOnError: logghiamo l'errore e proseguiamo
                    // con i successivi inizializzatori per non bloccare
                    // la creazione dell'azienda a causa di un modulo.
                    _logger.LogError(ex,
                        "[CompanyInit] ERRORE in '{InitializerName}': {Message}. " +
                        "Proseguo con i successivi inizializzatori.",
                        initializer.Name,
                        ex.Message);
                }
            }

            // Marca l'azienda come inizializzata in questa istanza
            _initializedCompanies.TryAdd(company.Id, 0);

            _logger.LogInformation(
                "[CompanyInit] Inizializzazione completata per azienda {CompanyId}.",
                company.Id);
        }
    }
}