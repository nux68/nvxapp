using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 0 - Common: dati dell'azienda condivisi tra gli applicativi
 (Az_Anagrafica, Az_Cliente, Az_ClienteContatto, Dip_Anagrafica).
     multi-tenant: tenant_<IdAzienda>_0     modalita' singola: public
     Migration in Migrations/Common.

     Lo schema esiste SEMPRE per ogni azienda (non si attiva in CompanyApplication) e viene
     migrato prima degli schemi degli applicativi, che hanno chiavi esterne verso le sue tabelle.

     Gli applicativi leggono queste tabelle mappandole nel proprio contesto con
     ExcludeFromMigrations() e SENZA schema (le risolve il search_path, che per i contesti degli
     applicativi contiene anche lo schema Common: vedi TenantSearchPathInterceptor).
     Le scritture passano solo da questo contesto (repository in Repositories/Tenant/Common).
    */
    public class CommonDbContext : ApplicationDbContextBase
    {
        public CommonDbContext(DbContextOptions<CommonDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.Common;

        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Az_Cliente> Az_Cliente { get; set; }
        public virtual DbSet<Az_ClienteContatto> Az_ClienteContatto { get; set; }
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }

        // tipi del modello Common (tabelle comuni e tabelle di public referenziate)
        private static readonly HashSet<Type> _commonTypes = new()
        {
            typeof(Az_Anagrafica), typeof(Az_Cliente), typeof(Az_ClienteContatto), typeof(Dip_Anagrafica),
            typeof(Company), typeof(ApplicationUser),
        };

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreApplicationNavigations(modelBuilder);
            Gen_PublicReferences(modelBuilder);
            Gen_Common(modelBuilder);
        }

        /*
         Le entita' comuni sono partial: i rami degli applicativi aggiungono navigazioni verso le proprie tabelle
         (es. Dip_Anagrafica.Dip_RapportoLavoro delle presenze). Qui si ignorano tutte le navigazioni che non
         portano a un tipo del modello Common, cosi' il modello (e le migration) di Common restano uguali in ogni ramo.
        */
        private static void IgnoreApplicationNavigations(ModelBuilder modelBuilder)
        {
            // si ignorano i TIPI di destinazione (non solo le proprieta'): altrimenti le convenzioni di EF li
            // scoprirebbero comunque seguendo le navigazioni, trascinando nel modello tutte le tabelle dell'applicativo
            var foreign = new HashSet<Type>();
            foreach (var type in new[] { typeof(Az_Anagrafica), typeof(Az_Cliente), typeof(Az_ClienteContatto), typeof(Dip_Anagrafica) })
            {
                foreach (var property in type.GetProperties())
                {
                    var target = property.PropertyType;
                    if (target.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(target))
                        target = target.GetGenericArguments()[0];

                    bool isEntity = target.IsClass && target != typeof(string) && target.Namespace != null
                                    && target.Namespace.StartsWith("nvxapp.server.data.Entities");
                    if (isEntity && !_commonTypes.Contains(target))
                        foreign.Add(target);
                }
            }

            foreach (var target in foreign)
                modelBuilder.Ignore(target);
        }

        /*
         Tabelle di public referenziate: schema "public" esplicito, escluse dalle migration,
         in sola lettura.
        */
        private static void Gen_PublicReferences(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company", TenantSchemaName.Public, t => t.ExcludeFromMigrations());
                entity.Ignore(e => e.FinancialAdvisorNavigation);
                entity.Ignore(e => e.UserCompany);
                entity.Ignore(e => e.CompanyApplication);
            });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers", TenantSchemaName.Public, t => t.ExcludeFromMigrations());
                entity.Ignore(e => e.UserDealer);
                entity.Ignore(e => e.UserCompany);
                entity.Ignore(e => e.UserFinancialAdvisor);
            });
        }

        private static void Gen_Common(ModelBuilder modelBuilder)
        {
            /* Az_Anagrafica: una sola riga per azienda */
            modelBuilder.Entity<Az_Anagrafica>(entity =>
            {
                entity.ToTable("Az_Anagrafica");
                entity.HasOne(t => t.CompanyNavigation)
                      .WithOne()
                      .HasForeignKey<Az_Anagrafica>(t => t.IdCompany)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* Az_Cliente */
            modelBuilder.Entity<Az_Cliente>(entity =>
            {
                entity.ToTable("Az_Cliente");
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany(t => t.Az_Cliente)
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* Dip_Anagrafica: l'utente e' facoltativo; eliminandolo il dipendente resta (SetNull) */
            modelBuilder.Entity<Dip_Anagrafica>(entity =>
            {
                entity.ToTable("Dip_Anagrafica");
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.AspNetUsersNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAspNetUsers)
                      .OnDelete(DeleteBehavior.SetNull);

                // un utente e' al piu' un dipendente per azienda (i NULL non si scontrano: dipendenti senza account)
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.IdAspNetUsers }).IsUnique();
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.CodiceFiscale });
            });

            /* Az_ClienteContatto */
            modelBuilder.Entity<Az_ClienteContatto>(entity =>
            {
                entity.ToTable("Az_ClienteContatto");
                entity.HasOne(t => t.Az_ClienteNavigation)
                      .WithMany(t => t.Az_ClienteContatto)
                      .HasForeignKey(t => t.IdAz_Cliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
