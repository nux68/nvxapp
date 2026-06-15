using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
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
        private readonly List<ICompanyInitializer> _initializers;
        private readonly ILogger<CompanyInitializerRegistry> _logger;
        private static readonly ConcurrentDictionary<int, byte> _initializedCompanies = new();

        // Riceve tutti gli ICompanyInitializer registrati nella DI.
        // Li ordina per Priority crescente.
        public CompanyInitializerRegistry(
            IEnumerable<ICompanyInitializer> initializers,
            ILogger<CompanyInitializerRegistry> logger)
        {
            _initializers = initializers
                .OrderBy(i => i.Priority)
                .ToList();
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

            _logger.LogInformation(
                "[CompanyInit] Avvio inizializzazione per azienda {CompanyId}, Descrizione={Descrizione}. " +
                "Inizializzatori disponibili: {Count}",
                company.Id,
                company.Descrizione,
                _initializers.Count);

            foreach (var initializer in _initializers)
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