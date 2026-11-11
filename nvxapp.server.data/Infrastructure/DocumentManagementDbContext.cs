using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 4 - DocumentManagement (documentale), ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md.
     multi-tenant: tenant_<IdAzienda>_4     modalita' singola: public
     Migration in Migrations/DocumentManagement.

     Tabelle comuni (schema Common) e di public referenziate in sola lettura, escluse dalle migration:
        - Az_Anagrafica, Az_Cliente, Dip_Anagrafica: senza schema (le risolve il search_path)
        - Company, AspNetUsers: schema public
    */
    public class DocumentManagementDbContext : ApplicationDbContextBase
    {
        public DocumentManagementDbContext(DbContextOptions<DocumentManagementDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.DocumentManagement;

        public virtual DbSet<Doc_Cfg> Doc_Cfg { get; set; }
        public virtual DbSet<Doc_Par_Tipo> Doc_Par_Tipo { get; set; }
        public virtual DbSet<Doc_Cartella> Doc_Cartella { get; set; }
        public virtual DbSet<Doc_UtenteAbilitazione> Doc_UtenteAbilitazione { get; set; }
        public virtual DbSet<Doc_Documento> Doc_Documento { get; set; }
        public virtual DbSet<Doc_Versione> Doc_Versione { get; set; }
        public virtual DbSet<Doc_Destinatario> Doc_Destinatario { get; set; }
        public virtual DbSet<Doc_Log> Doc_Log { get; set; }

        // tabelle comuni in sola lettura
        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Az_Cliente> Az_Cliente { get; set; }
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            IgnoreOtherEntities(modelBuilder);
            Gen_References(modelBuilder);
            Gen_DocumentManagement(modelBuilder);
        }

        /*
         Le entita' comuni sono partial e negli altri rami (es. presenze) hanno navigazioni verso le loro tabelle
         (Dip_Anagrafica.Dip_RapportoLavoro): nel ramo che li unisce EF le seguirebbe e trascinerebbe qui le tabelle
         delle presenze. Si escludono quindi tutte le entita' che non sono del documentale, comuni o di public referenziate.
        */
        private static void IgnoreOtherEntities(ModelBuilder modelBuilder)
        {
            var mie = new HashSet<Type>
            {
                typeof(Doc_Cfg), typeof(Doc_Par_Tipo), typeof(Doc_Cartella), typeof(Doc_UtenteAbilitazione), typeof(Doc_Documento),
                typeof(Doc_Versione), typeof(Doc_Destinatario), typeof(Doc_Log),
                typeof(Az_Anagrafica), typeof(Az_Cliente), typeof(Dip_Anagrafica), typeof(Company), typeof(ApplicationUser),
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
                entity.Ignore(e => e.Az_ClienteContatto);
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany(t => t.Az_Cliente).HasForeignKey(t => t.IdAz_Anagrafica);
            });

            modelBuilder.Entity<Dip_Anagrafica>(entity =>
            {
                entity.ToTable("Dip_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica);
                entity.HasOne(t => t.AspNetUsersNavigation).WithMany().HasForeignKey(t => t.IdAspNetUsers);
            });
        }

        private static void Gen_DocumentManagement(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doc_Cfg>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithOne().HasForeignKey<Doc_Cfg>(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Doc_Par_Tipo>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.Codice }).IsUnique();
            });

            modelBuilder.Entity<Doc_Cartella>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Doc_CartellaNavigation).WithMany(t => t.Doc_Cartella_Sub).HasForeignKey(t => t.IdDoc_Cartella).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Doc_UtenteAbilitazione>(entity =>
            {
                entity.HasOne(t => t.AspNetUsersNavigation).WithMany().HasForeignKey(t => t.IdAspNetUsers).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.IdAspNetUsers }).IsUnique();
            });

            modelBuilder.Entity<Doc_Documento>(entity =>
            {
                entity.HasOne(t => t.Az_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdAz_Anagrafica).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Doc_Par_TipoNavigation).WithMany().HasForeignKey(t => t.IdDoc_Par_Tipo).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Doc_CartellaNavigation).WithMany().HasForeignKey(t => t.IdDoc_Cartella).OnDelete(DeleteBehavior.SetNull);
                // i documenti del personale restano anche dopo l'uscita del dipendente: il dipendente comune non si cancella se ha documenti
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Az_ClienteNavigation).WithMany().HasForeignKey(t => t.IdAz_Cliente).OnDelete(DeleteBehavior.Restrict);

                // ricerca nel testo: titolo, descrizione e testo della versione corrente (configurazione italiana)
                entity.HasGeneratedTsVectorColumn(t => t.Ricerca!, "italian", t => new { t.Titolo, t.Descrizione, t.Testo })
                      .HasIndex(t => t.Ricerca)
                      .HasMethod("GIN");

                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.NelCestino, t.IdDoc_Cartella });
                entity.HasIndex(t => t.IdDip_Anagrafica);
                entity.HasIndex(t => t.DataScadenza);
            });

            modelBuilder.Entity<Doc_Versione>(entity =>
            {
                entity.HasOne(t => t.Doc_DocumentoNavigation).WithMany(t => t.Doc_Versione).HasForeignKey(t => t.IdDoc_Documento).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(t => new { t.IdDoc_Documento, t.Numero }).IsUnique();
                entity.HasIndex(t => t.Hash);
            });

            modelBuilder.Entity<Doc_Destinatario>(entity =>
            {
                entity.HasOne(t => t.Doc_DocumentoNavigation).WithMany(t => t.Doc_Destinatario).HasForeignKey(t => t.IdDoc_Documento).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation).WithMany().HasForeignKey(t => t.IdDip_Anagrafica).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => t.IdAspNetUsers);
                entity.HasIndex(t => t.IdDip_Anagrafica);
            });

            modelBuilder.Entity<Doc_Log>(entity =>
            {
                entity.HasIndex(t => new { t.IdDoc_Documento, t.Data });
                entity.HasIndex(t => t.Data);
            });
        }
    }
}
