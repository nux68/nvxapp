using Microsoft.Extensions.DependencyInjection;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Elenco dei contesti degli applicativi registrati (ApplicationType -> tipo del contesto).
     Ogni modulo registra il proprio con AddApplicationDbContext<T>(); in un ramo possono
     esistere applicativi dell'enum senza contesto (es. AttendanceTracking su Infrastructure).
    */
    public sealed class ApplicationDbContextRegistry
    {
        private readonly Dictionary<ApplicationType, Type> _contexts = new();

        public ApplicationDbContextRegistry(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>Connection string comune a tutti i contesti (stesso database, schemi diversi).</summary>
        public string ConnectionString { get; }

        public IReadOnlyDictionary<ApplicationType, Type> Contexts => _contexts;

        public bool IsRegistered(ApplicationType application) => _contexts.ContainsKey(application);

        internal void Register(ApplicationType application, Type contextType)
        {
            if (_contexts.TryGetValue(application, out var existing) && existing != contextType)
                throw new InvalidOperationException($"Applicativo {application} gia' registrato con il contesto {existing.Name}.");
            _contexts[application] = contextType;
        }
    }


    /*
     Crea il contesto di un applicativo su uno schema ESPLICITO, senza controllo di attivazione
     e indipendente dalla richiesta corrente. Da usare per provisioning/migration e controlli
     all'avvio. Il contesto restituito va chiuso dal chiamante (using).

     Per il normale accesso ai dati si inietta direttamente il contesto dell'applicativo.
    */
    public interface IApplicationDbContextFactory
    {
        ApplicationDbContextBase Create(ApplicationType application, string schema);
    }

    public sealed class ApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContextRegistry _registry;

        public ApplicationDbContextFactory(IServiceProvider serviceProvider, ApplicationDbContextRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public ApplicationDbContextBase Create(ApplicationType application, string schema)
        {
            if (!_registry.Contexts.TryGetValue(application, out var contextType))
                throw new InvalidOperationException($"Nessun contesto registrato per l'applicativo {application}.");

            // stesse opzioni del contesto registrato, schema fisso
            return (ApplicationDbContextBase)ActivatorUtilities.CreateInstance(
                _serviceProvider, contextType, new FixedTenantSchemaAccessor(schema));
        }
    }
}
