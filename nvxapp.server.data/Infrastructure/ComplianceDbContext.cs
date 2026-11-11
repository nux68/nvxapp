using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 6 - Compliance, ANALISI_COMPLIANCE_APPLICATIVO.md.
     multi-tenant: tenant_<IdAzienda>_6     modalita' singola: public
     Migration in Migrations/Compliance.

     Tabelle comuni (schema Common) e di public referenziate in sola lettura, escluse dalle migration:
        - Az_Anagrafica: senza schema (la risolve il search_path)
        - Company, AspNetUsers: schema public
     Le scritture sulle tabelle comuni passano dai repository di Infrastructure (Repositories/Tenant/Common).
    */
    public class ComplianceDbContext : ApplicationDbContextBase
    {
        public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.Compliance;

        public virtual DbSet<Cmp_Cfg> Cmp_Cfg { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreOtherEntities(modelBuilder);
            Gen_References(modelBuilder);
            Gen_Cmp(modelBuilder);
        }

        // come nel CRM: si escludono le entita' degli altri applicativi raggiungibili dalle navigazioni delle comuni
        private static void IgnoreOtherEntities(ModelBuilder modelBuilder)
        {
            var mie = new HashSet<Type>
            {
                typeof(Cmp_Cfg),
                typeof(Az_Anagrafica), typeof(Company), typeof(ApplicationUser),
            };
            foreach (var type in typeof(nvxapp.server.data.Entities.BaseEntity).Assembly.GetTypes()
                         .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested && t.Namespace != null
                                     && t.Namespace.StartsWith("nvxapp.server.data.Entities") && !mie.Contains(t)))
                modelBuilder.Ignore(type);
        }

        private static void Gen_References(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers", TenantSchemaName.Public, t => t.ExcludeFromMigrations());
                entity.Ignore(e => e.UserDealer);
                entity.Ignore(e => e.UserCompany);
                entity.Ignore(e => e.UserFinancialAdvisor);
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company", TenantSchemaName.Public, t => t.ExcludeFromMigrations());
                entity.Ignore(e => e.FinancialAdvisorNavigation);
                entity.Ignore(e => e.UserCompany);
                entity.Ignore(e => e.CompanyApplication);
            });

            modelBuilder.Entity<Az_Anagrafica>(entity =>
            {
                entity.ToTable("Az_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.CompanyNavigation).WithOne().HasForeignKey<Az_Anagrafica>(t => t.IdCompany);
            });
        }

        private static void Gen_Cmp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cmp_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Cmp_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
