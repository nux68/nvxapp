using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 7 - Risorse aziendali, ANALISI_ASSETS_APPLICATIVO.md.
     multi-tenant: tenant_<IdAzienda>_7     modalita' singola: public
     Migration in Migrations/Assets.

     Tabelle comuni (schema Common) e di public referenziate in sola lettura, escluse dalle migration:
        - Az_Anagrafica: senza schema (la risolve il search_path)
        - Company, AspNetUsers: schema public
     Le scritture sulle tabelle comuni passano dai repository di Infrastructure (Repositories/Tenant/Common).
    */
    public class AssetsDbContext : ApplicationDbContextBase
    {
        public AssetsDbContext(DbContextOptions<AssetsDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.Assets;

        public virtual DbSet<Ast_Cfg> Ast_Cfg { get; set; }

        // flotta (AST-FLO)
        public virtual DbSet<Ast_Veicolo> Ast_Veicolo { get; set; }
        public virtual DbSet<Ast_VeicoloAssegnazione> Ast_VeicoloAssegnazione { get; set; }
        public virtual DbSet<Ast_VeicoloScadenza> Ast_VeicoloScadenza { get; set; }
        public virtual DbSet<Ast_VeicoloCosto> Ast_VeicoloCosto { get; set; }
        public virtual DbSet<Ast_Multa> Ast_Multa { get; set; }

        // beni assegnati (AST-BENI)
        public virtual DbSet<Ast_Categoria> Ast_Categoria { get; set; }
        public virtual DbSet<Ast_Bene> Ast_Bene { get; set; }
        public virtual DbSet<Ast_BeneAssegnazione> Ast_BeneAssegnazione { get; set; }

        // prenotazioni (AST-PREN)
        public virtual DbSet<Ast_Risorsa> Ast_Risorsa { get; set; }
        public virtual DbSet<Ast_Prenotazione> Ast_Prenotazione { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }
        public virtual DbSet<Dip_RapportoAutovetture> Dip_RapportoAutovetture { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreOtherEntities(modelBuilder);
            Gen_References(modelBuilder);
            Gen_Ast(modelBuilder);
            Gen_AstFlotta(modelBuilder);
            Gen_AstBeni(modelBuilder);
            Gen_AstPrenotazioni(modelBuilder);
        }

        // come nel CRM: si escludono le entita' degli altri applicativi raggiungibili dalle navigazioni delle comuni
        private static void IgnoreOtherEntities(ModelBuilder modelBuilder)
        {
            var mie = new HashSet<Type>
            {
                typeof(Ast_Cfg),
                typeof(Ast_Veicolo), typeof(Ast_VeicoloAssegnazione), typeof(Ast_VeicoloScadenza), typeof(Ast_VeicoloCosto), typeof(Ast_Multa),
                typeof(Ast_Categoria), typeof(Ast_Bene), typeof(Ast_BeneAssegnazione),
                typeof(Ast_Risorsa), typeof(Ast_Prenotazione),
                typeof(Az_Anagrafica), typeof(Dip_Anagrafica), typeof(Az_Sedi), typeof(Dip_RapportoAutovetture),
                typeof(Company), typeof(ApplicationUser),
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

            // dipendenti, sedi e autovetture dei dipendenti (Common): sola lettura, le scritture con i repository di Common
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
                entity.Ignore(t => t.Az_Reparto);
            });

            modelBuilder.Entity<Dip_RapportoAutovetture>(entity =>
            {
                entity.ToTable("Dip_RapportoAutovetture", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica);
            });
        }

        private static void Gen_Ast(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ast_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Ast_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });
        }

        /*
         Flotta (AST-FLO). Cancellazione: tutto in cascata dal veicolo (e dall'azienda); il dipendente cancellato da Common
         porta via le sue assegnazioni, sulle multe resta la multa senza conducente; sede e autovettura del dipendente
         cancellate lasciano il riferimento vuoto.
        */
        private static void Gen_AstFlotta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ast_Veicolo>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Az_SediNavigation).WithMany().HasForeignKey(t => t.IdAz_Sedi).OnDelete(DeleteBehavior.SetNull);
                // la targa e' unica tra i veicoli dell'azienda
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Targa }).IsUnique();
            });

            modelBuilder.Entity<Ast_VeicoloAssegnazione>(entity =>
            {
                entity.HasOne(t => t.Ast_VeicoloNavigation).WithMany(t => t.Ast_VeicoloAssegnazione).HasForeignKey(t => t.IdAst_Veicolo).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_RapportoAutovettureNavigation).WithMany().HasForeignKey(t => t.IdDip_RapportoAutovetture).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAst_Veicolo, t.DataInizio });
                entity.HasIndex(t => t.IdDip_Anagrafica);
            });

            modelBuilder.Entity<Ast_VeicoloScadenza>(entity =>
            {
                entity.HasOne(t => t.Ast_VeicoloNavigation).WithMany(t => t.Ast_VeicoloScadenza).HasForeignKey(t => t.IdAst_Veicolo).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAst_Veicolo, t.DataScadenza });
            });

            modelBuilder.Entity<Ast_VeicoloCosto>(entity =>
            {
                entity.HasOne(t => t.Ast_VeicoloNavigation).WithMany(t => t.Ast_VeicoloCosto).HasForeignKey(t => t.IdAst_Veicolo).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAst_Veicolo, t.Data });
            });

            modelBuilder.Entity<Ast_Multa>(entity =>
            {
                entity.HasOne(t => t.Ast_VeicoloNavigation).WithMany(t => t.Ast_Multa).HasForeignKey(t => t.IdAst_Veicolo).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAst_Veicolo, t.DataInfrazione });
                entity.HasIndex(t => t.IdDip_Anagrafica);
            });
        }

        /*
         Beni assegnati (AST-BENI). Cancellazione: tutto in cascata dall'azienda; la categoria usata da un bene non si cancella;
         il dipendente cancellato da Common porta via le sue assegnazioni.
        */
        private static void Gen_AstBeni(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ast_Categoria>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<Ast_Bene>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Ast_CategoriaNavigation).WithMany().HasForeignKey(t => t.IdAst_Categoria).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(t => t.Az_SediNavigation).WithMany().HasForeignKey(t => t.IdAz_Sedi).OnDelete(DeleteBehavior.SetNull);
                // il codice di inventario e' unico tra i beni dell'azienda
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.CodiceInventario }).IsUnique();
            });

            modelBuilder.Entity<Ast_BeneAssegnazione>(entity =>
            {
                entity.HasOne(t => t.Ast_BeneNavigation).WithMany(t => t.Ast_BeneAssegnazione).HasForeignKey(t => t.IdAst_Bene).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAst_Bene, t.DataConsegna });
                entity.HasIndex(t => t.IdDip_Anagrafica);
                // un bene e' in mano a un solo dipendente per volta: una sola assegnazione aperta per bene
                entity.HasIndex(t => t.IdAst_Bene).IsUnique().HasFilter("\"DataRestituzione\" IS NULL").HasDatabaseName("IX_Ast_BeneAssegnazione_Aperta");
            });
        }

        /*
         Prenotazioni (AST-PREN). Cancellazione: in cascata dall'azienda e dalla risorsa; l'utente cancellato porta via le sue
         prenotazioni; il veicolo della flotta cancellato lascia la risorsa senza veicolo.
        */
        private static void Gen_AstPrenotazioni(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ast_Risorsa>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Az_SediNavigation).WithMany().HasForeignKey(t => t.IdAz_Sedi).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(t => t.Ast_VeicoloNavigation).WithMany().HasForeignKey(t => t.IdAst_Veicolo).OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Tipo, t.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<Ast_Prenotazione>(entity =>
            {
                entity.HasOne(t => t.Ast_RisorsaNavigation).WithMany().HasForeignKey(t => t.IdAst_Risorsa).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.AspNetUsersNavigation).WithMany().HasForeignKey(t => t.IdAspNetUsers).OnDelete(DeleteBehavior.Cascade);
                // ricerca dei conflitti (sovrapposizione sulla stessa risorsa) e del calendario
                entity.HasIndex(t => new { t.IdAst_Risorsa, t.Inizio, t.Fine });
                entity.HasIndex(t => new { t.IdAspNetUsers, t.Inizio });
            });
        }
    }
}
