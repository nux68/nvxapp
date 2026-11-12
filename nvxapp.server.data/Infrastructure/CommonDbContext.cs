using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 0 - Common: dati dell'azienda condivisi tra gli applicativi
 (Az_Anagrafica, Az_Cliente, Az_ClienteContatto, Dip_Anagrafica, Az_Sedi, Az_SediReparto, Par_Competenza,
 Dip_Competenza, Dip_RapportoAutovetture).
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
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }
        public virtual DbSet<Az_SediReparto> Az_SediReparto { get; set; }
        public virtual DbSet<Par_Competenza> Par_Competenza { get; set; }
        public virtual DbSet<Dip_Competenza> Dip_Competenza { get; set; }
        public virtual DbSet<Dip_RapportoAutovetture> Dip_RapportoAutovetture { get; set; }
        public virtual DbSet<Dip_GG_Richiesta> Dip_GG_Richiesta { get; set; }

        // tipi del modello Common (tabelle comuni e tabelle di public referenziate)
        private static readonly HashSet<Type> _commonTypes = new()
        {
            typeof(Az_Anagrafica), typeof(Az_Cliente), typeof(Az_ClienteContatto), typeof(Dip_Anagrafica),
            typeof(Az_Sedi), typeof(Az_SediReparto), typeof(Par_Competenza), typeof(Dip_Competenza), typeof(Dip_RapportoAutovetture),
            typeof(Dip_GG_Richiesta),
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
         (es. Dip_Anagrafica.Dip_RapportoLavoro delle presenze). Qui si escludono dal modello TUTTE le entita' del
         progetto dati che non sono comuni (o di public referenziate): seguendo quelle navigazioni EF le scoprirebbe
         comunque, trascinando nel modello le tabelle dell'applicativo. Cosi' il modello (e le migration) di Common
         restano uguali in ogni ramo.
        */
        private static void IgnoreApplicationNavigations(ModelBuilder modelBuilder)
        {
            var others = typeof(nvxapp.server.data.Entities.BaseEntity).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested
                            && t.Namespace != null && t.Namespace.StartsWith("nvxapp.server.data.Entities")
                            && !_commonTypes.Contains(t));
            foreach (var type in others)
                modelBuilder.Ignore(type);
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

            /*
             Sedi, reparti, competenze e autovetture (INF-COM): stessa configurazione che avevano nelle presenze.
             Le collection verso le tabelle delle presenze non sono nelle entita' comuni (le aggiunge il ramo
             Presenze in un partial): qui .WithMany() senza navigazione dove serve.
             Percorso di cancellazione: tutte in cascata da Az_Anagrafica (direttamente o tramite Dip_Anagrafica/Az_Sedi).
            */

            /* Az_Sedi */
            modelBuilder.Entity<Az_Sedi>(entity =>
            {
                entity.ToTable("Az_Sedi");
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* Az_SediReparto: reparti della sede, ad albero (il padre non si cancella se ha figli) */
            modelBuilder.Entity<Az_SediReparto>(entity =>
            {
                entity.ToTable("Az_SediReparto");
                entity.HasOne(t => t.Az_SediNavigation)
                      .WithMany(t => t.Az_Reparto)
                      .HasForeignKey(t => t.IdAz_Sedi)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Az_SediRepartoNavigation)
                      .WithMany(t => t.Az_SediReparto_Sub)
                      .HasForeignKey(t => t.IdAz_SediReparto)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* Par_Competenza */
            modelBuilder.Entity<Par_Competenza>(entity =>
            {
                entity.ToTable("Par_Competenza");
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* Dip_Competenza: una riga per dipendente e competenza */
            modelBuilder.Entity<Dip_Competenza>(entity =>
            {
                entity.ToTable("Dip_Competenza");
                entity.HasOne(t => t.Dip_AnagraficaNavigation)
                      .WithMany(t => t.Dip_Competenza)
                      .HasForeignKey(t => t.IdDip_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Par_CompetenzaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdPar_Competenza)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.IdDip_Anagrafica, t.IdPar_Competenza }).IsUnique();
            });

            /* Dip_RapportoAutovetture: legata al dipendente (nelle presenze era legata al rapporto di lavoro) */
            modelBuilder.Entity<Dip_RapportoAutovetture>(entity =>
            {
                entity.ToTable("Dip_RapportoAutovetture");
                entity.HasOne(t => t.Dip_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdDip_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /*
             Dip_GG_Richiesta: richieste da approvare di tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis).
             In cascata dall'azienda e dal dipendente (nelle presenze era in cascata dal rapporto di lavoro, che sta nelle
             presenze: IdDip_RapportoLavoro qui e' senza FK). Le collection verso le tabelle delle presenze le aggiunge il
             ramo Presenze in un partial.
            */
            modelBuilder.Entity<Dip_GG_Richiesta>(entity =>
            {
                entity.ToTable("Dip_GG_Richiesta");
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.Dip_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdDip_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.IdAz_Anagrafica, t.RichiestaStato });
                entity.HasIndex(t => t.IdAspNetUsers);
                entity.HasIndex(t => t.IdDip_RapportoLavoro);
            });
        }
    }
}
