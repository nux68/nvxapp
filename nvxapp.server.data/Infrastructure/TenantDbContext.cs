using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Contesto delle tabelle dei DATI DELLE AZIENDE.

     Il modello NON contiene il nome dello schema: le tabelle sono senza qualificatore e
     PostgreSQL le risolve con il search_path della connessione, impostato da
     TenantSearchPathInterceptor all'apertura con lo schema di questo contesto:
        - modalita' singola      -> "public"   (tabelle tenant accanto a quelle condivise)
        - modalita' multi-tenant -> schema dell'azienda della richiesta (claim "tenant")

     Le stesse migration (cartella Migrations/Tenant) valgono quindi per entrambe le modalita'.
     Lo storico migration ha un nome proprio, per non confondersi con quello di PublicDbContext
     quando i due contesti stanno nello stesso schema (modalita' singola).

     Regole:
        - nessuna tabella con lo stesso nome di una tabella di PublicDbContext
        - riferimenti verso tabelle di public: mappate con schema "public" esplicito ed
          ExcludeFromMigrations(), in sola lettura (le scritture passano da PublicDbContext)
        - nessun riferimento da public verso le tabelle tenant

     Ogni modulo aggiunge le proprie tabelle con un file partial TenantDbContext_<Modulo>.cs.
    */
    public partial class TenantDbContext : DbContext
    {
        public const string MigrationsHistoryTable = "__EFMigrationsHistory_Tenant";

        private readonly ITenantSchemaAccessor _schemaAccessor;
        private string? _schema;

        static TenantDbContext()
        {
            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public TenantDbContext(DbContextOptions<TenantDbContext> options, ITenantSchemaAccessor schemaAccessor) : base(options)
        {
            _schemaAccessor = schemaAccessor;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// Schema PostgreSQL su cui lavora questo contesto. Risolto al primo utilizzo e poi fisso
        /// per tutta la vita del contesto (un contesto non cambia mai azienda).
        /// </summary>
        public string Schema => _schema ??= TenantSchemaName.Validate(_schemaAccessor.GetSchema());


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // nessun HasDefaultSchema: lo schema arriva dal search_path della connessione

            Define_Table_TenantDbContext_Infrastructure(modelBuilder);
        }

    }
}
