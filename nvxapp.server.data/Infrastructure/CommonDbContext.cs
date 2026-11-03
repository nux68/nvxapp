using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 0 - Common: dati dell'azienda condivisi tra gli applicativi.
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Gen_PublicReferences(modelBuilder);
            Gen_Common(modelBuilder);
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
