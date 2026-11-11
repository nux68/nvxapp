using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 3 - CRM, ANALISI_CRM_APPLICATIVO.md.
     multi-tenant: tenant_<IdAzienda>_3     modalita' singola: public
     Migration in Migrations/Crm.

     Tabelle comuni (schema Common) e di public referenziate in sola lettura, escluse dalle migration:
        - Az_Anagrafica, Az_Cliente, Az_ClienteContatto: senza schema (le risolve il search_path)
        - Company, AspNetUsers: schema public
     Le scritture sulle tabelle comuni passano dai repository di Infrastructure (Repositories/Tenant/Common).
    */
    public class CrmDbContext : ApplicationDbContextBase
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.Crm;

        public virtual DbSet<Crm_Cfg> Crm_Cfg { get; set; }
        public virtual DbSet<Crm_Par_Fase> Crm_Par_Fase { get; set; }
        public virtual DbSet<Crm_Par_TipoAttivita> Crm_Par_TipoAttivita { get; set; }
        public virtual DbSet<Crm_Par_MotivoPerdita> Crm_Par_MotivoPerdita { get; set; }
        public virtual DbSet<Crm_Par_Fonte> Crm_Par_Fonte { get; set; }
        public virtual DbSet<Crm_Par_Prodotto> Crm_Par_Prodotto { get; set; }
        public virtual DbSet<Crm_Par_Tag> Crm_Par_Tag { get; set; }
        public virtual DbSet<Crm_ClienteDati> Crm_ClienteDati { get; set; }
        public virtual DbSet<Crm_ClienteTag> Crm_ClienteTag { get; set; }
        public virtual DbSet<Crm_Lead> Crm_Lead { get; set; }
        public virtual DbSet<Crm_Opportunita> Crm_Opportunita { get; set; }
        public virtual DbSet<Crm_OpportunitaStorico> Crm_OpportunitaStorico { get; set; }
        public virtual DbSet<Crm_Attivita> Crm_Attivita { get; set; }
        public virtual DbSet<Crm_Offerta> Crm_Offerta { get; set; }
        public virtual DbSet<Crm_OffertaRiga> Crm_OffertaRiga { get; set; }
        public virtual DbSet<Crm_Nota> Crm_Nota { get; set; }
        public virtual DbSet<Crm_Allegato> Crm_Allegato { get; set; }
        public virtual DbSet<Crm_Richiesta> Crm_Richiesta { get; set; }
        public virtual DbSet<Crm_RichiestaMessaggio> Crm_RichiestaMessaggio { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Az_Cliente> Az_Cliente { get; set; }
        public virtual DbSet<Az_ClienteContatto> Az_ClienteContatto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreOtherEntities(modelBuilder);
            Gen_References(modelBuilder);
            Gen_Crm(modelBuilder);
        }

        // come nel documentale: si escludono le entita' degli altri applicativi raggiungibili dalle navigazioni delle comuni
        private static void IgnoreOtherEntities(ModelBuilder modelBuilder)
        {
            var mie = new HashSet<Type>
            {
                typeof(Crm_Cfg), typeof(Crm_Par_Fase), typeof(Crm_Par_TipoAttivita), typeof(Crm_Par_MotivoPerdita), typeof(Crm_Par_Fonte),
                typeof(Crm_Par_Prodotto), typeof(Crm_Par_Tag), typeof(Crm_ClienteDati), typeof(Crm_ClienteTag), typeof(Crm_Lead),
                typeof(Crm_Opportunita), typeof(Crm_OpportunitaStorico), typeof(Crm_Attivita), typeof(Crm_Offerta), typeof(Crm_OffertaRiga),
                typeof(Crm_Nota), typeof(Crm_Allegato), typeof(Crm_Richiesta), typeof(Crm_RichiestaMessaggio),
                typeof(Az_Anagrafica), typeof(Az_Cliente), typeof(Az_ClienteContatto), typeof(Company), typeof(ApplicationUser),
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

            modelBuilder.Entity<Az_Cliente>(entity =>
            {
                entity.ToTable("Az_Cliente", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany(t => t.Az_Cliente).HasForeignKey(t => t.IdAz_Anagrafica);
            });

            modelBuilder.Entity<Az_ClienteContatto>(entity =>
            {
                entity.ToTable("Az_ClienteContatto", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany(t => t.Az_ClienteContatto).HasForeignKey(t => t.IdAz_Cliente);
            });
        }

        private static void Anagrafica<T>(ModelBuilder modelBuilder) where T : class
        {
            modelBuilder.Entity<T>().HasOne(typeof(Az_Anagrafica), "Az_AnagraficaNavigation").WithMany().HasForeignKey("IdAz_Anagrafica").OnDelete(DeleteBehavior.Cascade);
        }

        private static void Gen_Crm(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Crm_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Crm_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });

            Anagrafica<Crm_Par_Fase>(modelBuilder);
            Anagrafica<Crm_Par_TipoAttivita>(modelBuilder);
            Anagrafica<Crm_Par_MotivoPerdita>(modelBuilder);
            Anagrafica<Crm_Par_Fonte>(modelBuilder);
            Anagrafica<Crm_Par_Prodotto>(modelBuilder);
            Anagrafica<Crm_Par_Tag>(modelBuilder);
            modelBuilder.Entity<Crm_Par_Fase>().HasIndex(t => new { t.IdAz_Anagrafica, t.Ordine });
            modelBuilder.Entity<Crm_Par_Prodotto>().HasIndex(t => new { t.IdAz_Anagrafica, t.Codice });

            modelBuilder.Entity<Crm_ClienteDati>(entity =>
            {
                entity.HasOne(t => t.Az_ClienteNavigation).WithOne().HasForeignKey<Crm_ClienteDati>(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_Par_FonteNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Fonte).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.IdAspNetUsersResponsabile).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => t.IdAz_Cliente).IsUnique();
                entity.HasIndex(t => t.IdAspNetUsersResponsabile);
            });

            modelBuilder.Entity<Crm_ClienteTag>(entity =>
            {
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_Par_TagNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Tag).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Cliente, t.IdCrm_Par_Tag }).IsUnique();
            });

            modelBuilder.Entity<Crm_Lead>(entity =>
            {
                Anagrafica<Crm_Lead>(modelBuilder);
                entity.HasOne(t => t.Crm_Par_FonteNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Fonte).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.IdAspNetUsersResponsabile).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Stato });
            });

            modelBuilder.Entity<Crm_Opportunita>(entity =>
            {
                Anagrafica<Crm_Opportunita>(modelBuilder);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteContattoNavigation).WithMany().HasForeignKey(t => t.IdAz_ClienteContatto).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Crm_Par_FaseNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Fase).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Crm_Par_MotivoPerditaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_MotivoPerdita).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Crm_Par_FonteNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Fonte).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.IdAspNetUsersResponsabile).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.IdCrm_Par_Fase });
                entity.HasIndex(t => t.IdAz_Cliente);
            });

            modelBuilder.Entity<Crm_OpportunitaStorico>(entity =>
            {
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => t.IdCrm_Opportunita);
            });

            modelBuilder.Entity<Crm_Attivita>(entity =>
            {
                Anagrafica<Crm_Attivita>(modelBuilder);
                entity.HasOne(t => t.Crm_Par_TipoAttivitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_TipoAttivita).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteContattoNavigation).WithMany().HasForeignKey(t => t.IdAz_ClienteContatto).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_LeadNavigation).WithMany().HasForeignKey(t => t.IdCrm_Lead).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.IdAspNetUsersAssegnatario).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.IdAspNetUsersAssegnatario, t.Stato, t.Inizio });
            });

            modelBuilder.Entity<Crm_Offerta>(entity =>
            {
                Anagrafica<Crm_Offerta>(modelBuilder);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteContattoNavigation).WithMany().HasForeignKey(t => t.IdAz_ClienteContatto).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Anno, t.Numero }).IsUnique();
            });

            modelBuilder.Entity<Crm_OffertaRiga>(entity =>
            {
                entity.HasOne(t => t.Crm_OffertaNavigation).WithMany(t => t.Righe).HasForeignKey(t => t.IdCrm_Offerta).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_Par_ProdottoNavigation).WithMany().HasForeignKey(t => t.IdCrm_Par_Prodotto).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Crm_Nota>(entity =>
            {
                Anagrafica<Crm_Nota>(modelBuilder);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Crm_LeadNavigation).WithMany().HasForeignKey(t => t.IdCrm_Lead).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Crm_Allegato>(entity =>
            {
                Anagrafica<Crm_Allegato>(modelBuilder);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Crm_OffertaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Offerta).OnDelete(DeleteBehavior.Restrict);
            });

            // assistenza ai clienti (CRM-ASS)
            modelBuilder.Entity<Crm_Richiesta>(entity =>
            {
                Anagrafica<Crm_Richiesta>(modelBuilder);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteContattoNavigation).WithMany().HasForeignKey(t => t.IdAz_ClienteContatto).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Crm_OpportunitaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Opportunita).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.IdAspNetUsersAssegnatario).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Anno, t.Numero }).IsUnique();
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Stato });
                entity.HasIndex(t => t.IdAz_Cliente);
                entity.HasIndex(t => t.IdAspNetUsersAssegnatario);
            });

            modelBuilder.Entity<Crm_RichiestaMessaggio>(entity =>
            {
                entity.HasOne(t => t.Crm_RichiestaNavigation).WithMany().HasForeignKey(t => t.IdCrm_Richiesta).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => t.IdCrm_Richiesta);
            });
        }
    }
}
