using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Base dei contesti degli APPLICATIVI (uno per applicativo, es. MokeDbContext,
     AttendanceTrackingDbContext). Le tabelle comuni stanno invece in PublicDbContext.

     Il modello NON contiene il nome dello schema: le tabelle sono senza qualificatore e
     PostgreSQL le risolve con il search_path della connessione, impostato da
     TenantSearchPathInterceptor all'apertura con lo schema di questo contesto:
        - modalita' singola      -> "public"
        - modalita' multi-tenant -> tenant_<IdAzienda>_<ApplicationType>
     L'accesso e' consentito solo se l'applicativo e' attivo per l'azienda (CompanyApplication).

     Le stesse migration (cartella Migrations/<Applicativo>) valgono per entrambe le modalita'.
     Ogni applicativo ha il proprio storico (__EFMigrationsHistory_<Applicativo>), perche' in
     modalita' singola tutti i contesti stanno nello stesso schema.

     Regole:
        - un nome di tabella appartiene a un solo contesto (verificato all'avvio)
        - riferimenti verso tabelle di public: mappate con schema "public" esplicito ed
          ExcludeFromMigrations(), in sola lettura (le scritture passano da PublicDbContext)
        - nessun riferimento verso tabelle di un altro applicativo o da public verso un applicativo
    */
    public abstract class ApplicationDbContextBase : DbContext
    {
        private readonly ITenantSchemaAccessor _schemaAccessor;
        private string? _schema;

        static ApplicationDbContextBase()
        {
            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected ApplicationDbContextBase(DbContextOptions options, ITenantSchemaAccessor schemaAccessor) : base(options)
        {
            _schemaAccessor = schemaAccessor;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>Applicativo a cui appartengono le tabelle di questo contesto.</summary>
        public abstract ApplicationType Application { get; }

        /// <summary>Nome dello storico migration dell'applicativo.</summary>
        public static string MigrationsHistoryTable(ApplicationType application) => $"__EFMigrationsHistory_{application}";

        /// <summary>
        /// Schema PostgreSQL su cui lavora questo contesto. Risolto al primo utilizzo e poi fisso
        /// per tutta la vita del contesto (un contesto non cambia mai azienda).
        /// </summary>
        public string Schema => _schema ??= TenantSchemaName.Validate(_schemaAccessor.GetSchema(Application));
    }
}
