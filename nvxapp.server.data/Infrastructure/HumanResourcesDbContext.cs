using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 5 - Risorse Umane, ANALISI_HUMANRESOURCES_APPLICATIVO.md.
     multi-tenant: tenant_<IdAzienda>_5     modalita' singola: public
     Migration in Migrations/HumanResources.

     Tabelle comuni (schema Common) e di public referenziate in sola lettura, escluse dalle migration:
        - Az_Anagrafica, Dip_Anagrafica, Par_Competenza, Az_Sedi: senza schema (le risolve il search_path)
        - Company, AspNetUsers: schema public
     Le scritture sulle tabelle comuni passano dai repository di Infrastructure (Repositories/Tenant/Common).
    */
    public class HumanResourcesDbContext : ApplicationDbContextBase
    {
        public HumanResourcesDbContext(DbContextOptions<HumanResourcesDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.HumanResources;

        public virtual DbSet<Hr_Cfg> Hr_Cfg { get; set; }

        // cedolini e CU (HR-CED)
        public virtual DbSet<Hr_DocCaricamento> Hr_DocCaricamento { get; set; }
        public virtual DbSet<Hr_DocDipendente> Hr_DocDipendente { get; set; }
        public virtual DbSet<Hr_DocScarto> Hr_DocScarto { get; set; }

        // formazione (HR-FOR)
        public virtual DbSet<Hr_Corso> Hr_Corso { get; set; }
        public virtual DbSet<Hr_CorsoEdizione> Hr_CorsoEdizione { get; set; }
        public virtual DbSet<Hr_CorsoPartecipante> Hr_CorsoPartecipante { get; set; }
        public virtual DbSet<Hr_PianoFormazione> Hr_PianoFormazione { get; set; }
        public virtual DbSet<Hr_PianoFormazioneRiga> Hr_PianoFormazioneRiga { get; set; }

        // onboarding e offboarding (HR-ONB)
        public virtual DbSet<Hr_ChecklistModello> Hr_ChecklistModello { get; set; }
        public virtual DbSet<Hr_ChecklistModelloVoce> Hr_ChecklistModelloVoce { get; set; }
        public virtual DbSet<Hr_Checklist> Hr_Checklist { get; set; }
        public virtual DbSet<Hr_ChecklistVoce> Hr_ChecklistVoce { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }
        public virtual DbSet<Par_Competenza> Par_Competenza { get; set; }
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreOtherEntities(modelBuilder);
            Gen_References(modelBuilder);
            Gen_Hr(modelBuilder);
        }

        // come nel CRM: si escludono le entita' degli altri applicativi raggiungibili dalle navigazioni delle comuni
        private static void IgnoreOtherEntities(ModelBuilder modelBuilder)
        {
            var mie = new HashSet<Type>
            {
                typeof(Hr_Cfg),
                typeof(Hr_DocCaricamento), typeof(Hr_DocDipendente), typeof(Hr_DocScarto),
                typeof(Hr_Corso), typeof(Hr_CorsoEdizione), typeof(Hr_CorsoPartecipante), typeof(Hr_PianoFormazione), typeof(Hr_PianoFormazioneRiga),
                typeof(Hr_ChecklistModello), typeof(Hr_ChecklistModelloVoce), typeof(Hr_Checklist), typeof(Hr_ChecklistVoce),
                typeof(Az_Anagrafica), typeof(Dip_Anagrafica), typeof(Par_Competenza), typeof(Az_Sedi), typeof(Company), typeof(ApplicationUser),
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

            modelBuilder.Entity<Dip_Anagrafica>(entity =>
            {
                entity.ToTable("Dip_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
                entity.HasOne(t => t.AspNetUsersNavigation).WithMany().HasForeignKey(t => t.IdAspNetUsers);
                entity.Ignore(t => t.Dip_Competenza);
            });

            modelBuilder.Entity<Par_Competenza>(entity =>
            {
                entity.ToTable("Par_Competenza", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
            });

            modelBuilder.Entity<Az_Sedi>(entity =>
            {
                entity.ToTable("Az_Sedi", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
                entity.Ignore(t => t.Az_Reparto);
            });
        }

        private static void Gen_Hr(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hr_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Hr_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });

            // ------------------------------------------------------------------ cedolini e CU
            // i documenti del dipendente restano anche dopo la sua uscita (HR-D4): il dipendente comune non si cancella se ne ha
            modelBuilder.Entity<Hr_DocCaricamento>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Anno, t.Mese });
            });

            modelBuilder.Entity<Hr_DocDipendente>(entity =>
            {
                entity.HasOne(t => t.Hr_DocCaricamentoNavigation).WithMany(t => t.Hr_DocDipendente).HasForeignKey(t => t.IdHr_DocCaricamento).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => new { t.IdHr_DocCaricamento, t.IdDip_Anagrafica }).IsUnique();
                entity.HasIndex(t => new { t.IdDip_Anagrafica, t.DataPubblicazione });
            });

            modelBuilder.Entity<Hr_DocScarto>(entity =>
            {
                entity.HasOne(t => t.Hr_DocCaricamentoNavigation).WithMany(t => t.Hr_DocScarto).HasForeignKey(t => t.IdHr_DocCaricamento).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdHr_DocCaricamento, t.Pagina }).IsUnique();
            });

            // ------------------------------------------------------------------ formazione
            modelBuilder.Entity<Hr_Corso>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Par_CompetenzaNavigation).WithMany().HasForeignKey(t => t.IdPar_Competenza).OnDelete(DeleteBehavior.SetNull);
                entity.Property(t => t.DurataOre).HasPrecision(8, 2);
                entity.HasIndex(t => t.IdAz_Anagrafica);
            });

            modelBuilder.Entity<Hr_CorsoEdizione>(entity =>
            {
                entity.HasOne(t => t.Hr_CorsoNavigation).WithMany(t => t.Hr_CorsoEdizione).HasForeignKey(t => t.IdHr_Corso).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Az_SediNavigation).WithMany().HasForeignKey(t => t.IdAz_Sedi).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Hr_CorsoPartecipante>(entity =>
            {
                entity.HasOne(t => t.Hr_CorsoEdizioneNavigation).WithMany(t => t.Hr_CorsoPartecipante).HasForeignKey(t => t.IdHr_CorsoEdizione).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => new { t.IdHr_CorsoEdizione, t.IdDip_Anagrafica }).IsUnique();
                entity.HasIndex(t => t.IdDip_Anagrafica);
            });

            modelBuilder.Entity<Hr_PianoFormazione>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Anno });
            });

            modelBuilder.Entity<Hr_PianoFormazioneRiga>(entity =>
            {
                entity.HasOne(t => t.Hr_PianoFormazioneNavigation).WithMany(t => t.Hr_PianoFormazioneRiga).HasForeignKey(t => t.IdHr_PianoFormazione).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Hr_CorsoNavigation).WithMany().HasForeignKey(t => t.IdHr_Corso).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
            });

            // ------------------------------------------------------------------ onboarding e offboarding
            modelBuilder.Entity<Hr_ChecklistModello>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Hr_ChecklistModelloVoce>(entity =>
            {
                entity.HasOne(t => t.Hr_ChecklistModelloNavigation).WithMany(t => t.Hr_ChecklistModelloVoce).HasForeignKey(t => t.IdHr_ChecklistModello).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Hr_Checklist>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => new { t.IdDip_Anagrafica, t.Tipo });
            });

            modelBuilder.Entity<Hr_ChecklistVoce>(entity =>
            {
                entity.HasOne(t => t.Hr_ChecklistNavigation).WithMany(t => t.Hr_ChecklistVoce).HasForeignKey(t => t.IdHr_Checklist).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
