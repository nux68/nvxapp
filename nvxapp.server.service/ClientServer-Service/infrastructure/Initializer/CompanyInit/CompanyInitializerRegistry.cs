using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using System.Collections.Concurrent;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit
{
    // Registro centrale auto-costruito per l'inizializzazione delle aziende.
    // Non contiene mapping espliciti: ogni ICompanyInitializer dichiara
    // il proprio nome, priorità e applicativo e viene registrato automaticamente.
    // Per aggiungere un nuovo modulo di inizializzazione basta creare
    // un nuovo ICompanyInitializer e registrarlo nella DI.
    public interface ICompanyInitializerRegistry
    {
        // Esegue gli inizializzatori comuni e quelli degli applicativi ATTIVI per l'azienda,
        // in ordine di priorità.
        // Se uno fallisce, logga l'errore e prosegue con i successivi
        // (strategia ContinueOnError per garantire la massima resilienza).
        // Mantiene una cache in-memory per azienda e applicativo: cio' che e' gia' stato
        // inizializzato con successo in questa istanza non viene ripetuto.
        Task InitializeAllAsync(Company company);

        // Esegue solo gli inizializzatori di un applicativo (usato all'attivazione).
        Task InitializeApplicationAsync(Company company, ApplicationType application);
    }

    public class CompanyInitializerRegistry : ICompanyInitializerRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ILogger<CompanyInitializerRegistry> _logger;

        // chiave: azienda + applicativo (null = comune)
        private static readonly ConcurrentDictionary<(int IdCompany, ApplicationType? Application), byte> _initialized = new();

        // Gli ICompanyInitializer registrati nella DI vengono risolti in uno scope dedicato
        // all'azienda (vedi RunAsync) e ordinati per Priority crescente.
        public CompanyInitializerRegistry(
            IServiceScopeFactory scopeFactory,
            ICompanyApplicationRepository companyApplicationRepository,
            ILogger<CompanyInitializerRegistry> logger)
        {
            _scopeFactory = scopeFactory;
            _companyApplicationRepository = companyApplicationRepository;
            _logger = logger;
        }

        public async Task InitializeAllAsync(Company company)
        {
            var applications = new List<ApplicationType?> { null };
            applications.AddRange(_companyApplicationRepository.ActiveApplications(company.Id).Select(x => (ApplicationType?)x));
            await RunAsync(company, applications);
        }

        public async Task InitializeApplicationAsync(Company company, ApplicationType application)
        {
            await RunAsync(company, new List<ApplicationType?> { application });
        }

        private async Task RunAsync(Company company, List<ApplicationType?> applications)
        {
            // Cache in-memory: salta cio' che e' gia' stato inizializzato in questa istanza.
            var todo = applications.Where(a => !_initialized.ContainsKey((company.Id, a))).ToList();
            if (todo.Count == 0)
            {
                _logger.LogDebug(
                    "[CompanyInit] Azienda {CompanyId} già inizializzata in questa istanza. Skip.",
                    company.Id);
                return;
            }

            // Gli inizializzatori lavorano sui dati DELL'AZIENDA indicata, non su quella della
            // richiesta (es. CompanyGet chiamato da uno studio, senza azienda nel token):
            // scope DI dedicato, cosi' i contesti degli applicativi risolvono gli schemi di questa azienda.
            using var tenantScope = TenantScope.Use(company.Id);
            using var scope = _scopeFactory.CreateScope();
            var initializers = scope.ServiceProvider.GetServices<ICompanyInitializer>()
                                                    .Where(i => todo.Contains(i.Application))
                                                    .OrderBy(i => i.Priority)
                                                    .ToList();

            _logger.LogInformation(
                "[CompanyInit] Avvio inizializzazione per azienda {CompanyId}, Descrizione={Descrizione}, Applicativi={Applications}. " +
                "Inizializzatori: {Count}",
                company.Id,
                company.Descrizione,
                string.Join(", ", todo.Select(a => a?.ToString() ?? "comune")),
                initializers.Count);

            var failed = new HashSet<ApplicationType?>();
            foreach (var initializer in initializers)
            {
                try
                {
                    _logger.LogInformation(
                        "[CompanyInit] Esecuzione '{InitializerName}' (Priority={Priority}, Applicativo={Application})...",
                        initializer.Name,
                        initializer.Priority,
                        initializer.Application?.ToString() ?? "comune");

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
                    failed.Add(initializer.Application);
                    _logger.LogError(ex,
                        "[CompanyInit] ERRORE in '{InitializerName}': {Message}. " +
                        "Proseguo con i successivi inizializzatori.",
                        initializer.Name,
                        ex.Message);
                }
            }

            // Marca come inizializzati gli applicativi senza errori in questa istanza
            foreach (var application in todo.Where(a => !failed.Contains(a)))
                _initialized.TryAdd((company.Id, application), 0);

            _logger.LogInformation(
                "[CompanyInit] Inizializzazione completata per azienda {CompanyId}.",
                company.Id);
        }
    }
}
