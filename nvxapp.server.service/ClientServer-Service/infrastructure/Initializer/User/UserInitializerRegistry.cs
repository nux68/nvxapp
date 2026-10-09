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
    // il proprio nome, priorità e applicativo e viene registrato automaticamente.
    public interface IUserInitializerRegistry
    {
        // Esegue gli inizializzatori comuni e quelli degli applicativi ATTIVI per l'azienda
        // dell'utente, in ordine di priorità.
        // Se uno fallisce, logga l'errore e prosegue con i successivi
        // (strategia ContinueOnError per garantire la massima resilienza).
        // Mantiene una cache in-memory per utente e applicativo: cio' che e' gia' stato
        // inizializzato con successo in questa istanza non viene ripetuto.
        Task InitializeAllAsync(UserCompany userCompany);
    }

    public class UserInitializerRegistry : IUserInitializerRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ILogger<UserInitializerRegistry> _logger;

        // chiave: utente + azienda + applicativo (null = comune)
        private static readonly ConcurrentDictionary<(string IdUser, int IdCompany, ApplicationType? Application), byte> _initialized = new();

        // Gli IUserInitializer registrati nella DI vengono risolti in uno scope dedicato
        // all'azienda dell'utente (vedi InitializeAllAsync) e ordinati per Priority crescente.
        public UserInitializerRegistry(
            IServiceScopeFactory scopeFactory,
            ICompanyApplicationRepository companyApplicationRepository,
            ILogger<UserInitializerRegistry> logger)
        {
            _scopeFactory = scopeFactory;
            _companyApplicationRepository = companyApplicationRepository;
            _logger = logger;
        }

        public async Task InitializeAllAsync(UserCompany userCompany)
        {
            var applications = new List<ApplicationType?> { null };
            applications.AddRange(_companyApplicationRepository.ActiveApplications(userCompany.IdCompany).Select(x => (ApplicationType?)x));

            // Cache in-memory: salta cio' che e' gia' stato inizializzato in questa istanza.
            var todo = applications.Where(a => !_initialized.ContainsKey((userCompany.IdAspNetUsers, userCompany.IdCompany, a))).ToList();
            if (todo.Count == 0)
            {
                _logger.LogDebug(
                    "[UserInit] Utente {UserId} già inizializzato in questa istanza. Skip.",
                    userCompany.IdAspNetUsers);
                return;
            }

            // Gli inizializzatori lavorano sui dati dell'azienda DELL'UTENTE, non su quella della
            // richiesta: scope DI dedicato, cosi' i contesti degli applicativi risolvono gli schemi
            // di quell'azienda.
            using var tenantScope = TenantScope.Use(userCompany.IdCompany);
            using var scope = _scopeFactory.CreateScope();
            var initializers = scope.ServiceProvider.GetServices<IUserInitializer>()
                                                    .Where(i => todo.Contains(i.Application))
                                                    .OrderBy(i => i.Priority)
                                                    .ToList();

            _logger.LogInformation(
                "[UserInit] Avvio inizializzazione per utente {UserId}, Company={CompanyId}, Applicativi={Applications}. " +
                "Inizializzatori: {Count}",
                userCompany.IdAspNetUsers,
                userCompany.IdCompany,
                string.Join(", ", todo.Select(a => a?.ToString() ?? "comune")),
                initializers.Count);

            var failed = new HashSet<ApplicationType?>();
            foreach (var initializer in initializers)
            {
                try
                {
                    _logger.LogInformation(
                        "[UserInit] Esecuzione '{InitializerName}' (Priority={Priority}, Applicativo={Application})...",
                        initializer.Name,
                        initializer.Priority,
                        initializer.Application?.ToString() ?? "comune");

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
                    failed.Add(initializer.Application);
                    _logger.LogError(ex,
                        "[UserInit] ERRORE in '{InitializerName}': {Message}. " +
                        "Proseguo con i successivi inizializzatori.",
                        initializer.Name,
                        ex.Message);
                }
            }

            // Marca come inizializzati gli applicativi senza errori in questa istanza
            foreach (var application in todo.Where(a => !failed.Contains(a)))
                _initialized.TryAdd((userCompany.IdAspNetUsers, userCompany.IdCompany, application), 0);

            _logger.LogInformation(
                "[UserInit] Inizializzazione completata per utente {UserId}.",
                userCompany.IdAspNetUsers);
        }
    }
}
