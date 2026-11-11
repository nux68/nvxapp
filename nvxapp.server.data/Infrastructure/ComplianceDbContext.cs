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
        - Az_Anagrafica, Dip_Anagrafica, Az_Sedi: senza schema (le risolve il search_path)
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

        // sicurezza sul lavoro (CMP-81)
        public virtual DbSet<Cmp_Mansione> Cmp_Mansione { get; set; }
        public virtual DbSet<Cmp_MansioneCorso> Cmp_MansioneCorso { get; set; }
        public virtual DbSet<Cmp_DipendenteMansione> Cmp_DipendenteMansione { get; set; }
        public virtual DbSet<Cmp_Formazione> Cmp_Formazione { get; set; }
        public virtual DbSet<Cmp_Visita> Cmp_Visita { get; set; }
        public virtual DbSet<Cmp_Dpi> Cmp_Dpi { get; set; }
        public virtual DbSet<Cmp_DpiConsegna> Cmp_DpiConsegna { get; set; }
        public virtual DbSet<Cmp_Nomina> Cmp_Nomina { get; set; }
        public virtual DbSet<Cmp_Documento> Cmp_Documento { get; set; }

        // registro GDPR (CMP-GDPR)
        public virtual DbSet<Cmp_Trattamento> Cmp_Trattamento { get; set; }
        public virtual DbSet<Cmp_Responsabile> Cmp_Responsabile { get; set; }
        public virtual DbSet<Cmp_Violazione> Cmp_Violazione { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }

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
                typeof(Cmp_Mansione), typeof(Cmp_MansioneCorso), typeof(Cmp_DipendenteMansione), typeof(Cmp_Formazione), typeof(Cmp_Visita),
                typeof(Cmp_Dpi), typeof(Cmp_DpiConsegna), typeof(Cmp_Nomina), typeof(Cmp_Documento),
                typeof(Cmp_Trattamento), typeof(Cmp_Responsabile), typeof(Cmp_Violazione),
                typeof(Az_Anagrafica), typeof(Dip_Anagrafica), typeof(Az_Sedi), typeof(Company), typeof(ApplicationUser),
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

            // dipendenti e sedi (Common) in sola lettura, per le query che li uniscono alle tabelle del modulo
            modelBuilder.Entity<Dip_Anagrafica>(entity =>
            {
                entity.ToTable("Dip_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
                entity.HasOne(t => t.AspNetUsersNavigation).WithMany().HasForeignKey(t => t.IdAspNetUsers);
            });

            modelBuilder.Entity<Az_Sedi>(entity =>
            {
                entity.ToTable("Az_Sedi", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
            });
        }

        // FK verso Az_Anagrafica (Common) di ogni tabella del modulo: si cancellano con l'azienda
        private static void AzCascade<T>(ModelBuilder modelBuilder) where T : class
            => modelBuilder.Entity<T>().HasOne(typeof(Az_Anagrafica), "Az_AnagraficaNavigation").WithMany().HasForeignKey("IdAz_Anagrafica").OnDelete(DeleteBehavior.Cascade);

        private static void Gen_Cmp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cmp_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Cmp_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });

            /*
             CMP-81. I dati del dipendente si cancellano con il dipendente (Common, in cascata come le altre tabelle comuni);
             le scadenze registrate nello scadenzario (public) restano finche' il modulo non le aggiorna.
            */
            AzCascade<Cmp_Mansione>(modelBuilder);
            AzCascade<Cmp_MansioneCorso>(modelBuilder);
            modelBuilder.Entity<Cmp_MansioneCorso>(entity =>
            {
                entity.HasOne(t => t.Cmp_MansioneNavigation).WithMany().HasForeignKey(t => t.IdCmp_Mansione).OnDelete(DeleteBehavior.Cascade);
            });
            AzCascade<Cmp_DipendenteMansione>(modelBuilder);
            modelBuilder.Entity<Cmp_DipendenteMansione>(entity =>
            {
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Cmp_MansioneNavigation).WithMany().HasForeignKey(t => t.IdCmp_Mansione).OnDelete(DeleteBehavior.Cascade);
            });
            AzCascade<Cmp_Formazione>(modelBuilder);
            modelBuilder.Entity<Cmp_Formazione>(entity =>
            {
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.Property(t => t.Ore).HasPrecision(7, 2);
                entity.HasIndex(t => new { t.IdDip_Anagrafica, t.TipoCorso });
            });
            AzCascade<Cmp_Visita>(modelBuilder);
            modelBuilder.Entity<Cmp_Visita>(entity =>
            {
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => t.IdDip_Anagrafica);
            });
            AzCascade<Cmp_Dpi>(modelBuilder);
            AzCascade<Cmp_DpiConsegna>(modelBuilder);
            modelBuilder.Entity<Cmp_DpiConsegna>(entity =>
            {
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                // un DPI gia' consegnato non si cancella dal catalogo (si disattiva)
                entity.HasOne(t => t.Cmp_DpiNavigation).WithMany().HasForeignKey(t => t.IdCmp_Dpi).OnDelete(DeleteBehavior.Restrict);
            });
            AzCascade<Cmp_Nomina>(modelBuilder);
            modelBuilder.Entity<Cmp_Nomina>(entity =>
            {
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Az_SediNavigation).WithMany().HasForeignKey(t => t.IdAz_Sedi).OnDelete(DeleteBehavior.SetNull);
            });
            AzCascade<Cmp_Documento>(modelBuilder);

            // CMP-GDPR
            AzCascade<Cmp_Trattamento>(modelBuilder);
            AzCascade<Cmp_Responsabile>(modelBuilder);
            AzCascade<Cmp_Violazione>(modelBuilder);
        }
    }
}
