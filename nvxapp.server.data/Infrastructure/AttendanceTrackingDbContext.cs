using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Applicativo 2 - AttendanceTracking (gestione presenze).
     multi-tenant: tenant_<IdAzienda>_2     modalita' singola: public
     Migration in Migrations/AttendanceTracking.
    */
    public class AttendanceTrackingDbContext : ApplicationDbContextBase
    {
        public AttendanceTrackingDbContext(DbContextOptions<AttendanceTrackingDbContext> options, ITenantSchemaAccessor schemaAccessor)
            : base(options, schemaAccessor)
        {
        }

        public override ApplicationType Application => ApplicationType.AttendanceTracking;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Define_Table_AttendanceTracking(modelBuilder);
        }

        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }
        public virtual DbSet<Az_SediReparto> Az_SediReparto { get; set; }
        public virtual DbSet<Az_SediRepartoAttivita> Az_SediRepartoAttivita { get; set; }
        public virtual DbSet<Az_SediRepartoUser> Az_SediRepartoUser { get; set; }
        public virtual DbSet<Az_Cfg> Az_Cfg { get; set; }
        public virtual DbSet<Par_Attivita> Par_Attivita { get; set; }
        public virtual DbSet<Az_Commessa> Az_Commessa { get; set; }
        public virtual DbSet<Az_Cliente> Az_Cliente { get; set; }
        public virtual DbSet<Par_AttivitaCompetenza> Par_AttivitaCompetenza { get; set; }
        public virtual DbSet<Az_SubCommessa> Az_SubCommessa { get; set; }
        public virtual DbSet<Az_SubCommessaAttivita> Az_SubCommessaAttivita { get; set; }
        public virtual DbSet<Az_SediAttivita> Az_SediAttivita { get; set; }

        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafica { get; set; }
        public virtual DbSet<Dip_RapportoLavoro> Dip_RapportoLavoro { get; set; }
        public virtual DbSet<Dip_ProfiloOrario> Dip_ProfiloOrario { get; set; }
        public virtual DbSet<Dip_Competenza> Dip_Competenza { get; set; }
        public virtual DbSet<Dip_RapportoAutovetture> Dip_RapportoAutovetture { get; set; }

        public virtual DbSet<Dip_GG_Giustificativi> Dip_GG_Giustificativi { get; set; }
        public virtual DbSet<Dip_GG_Timbratura> Dip_GG_Timbrature { get; set; }
        public virtual DbSet<Dip_GG_NotaSpesa> Dip_GG_NotaSpese { get; set; }
        public virtual DbSet<Dip_GG_NotaSpesa_Allegato> Dip_GG_NotaSpesa_Allegato { get; set; }
        public virtual DbSet<Par_TipoSpesa> Par_TipoSpesa { get; set; }
        public virtual DbSet<Dip_GG_Richiesta> Dip_GG_Richieste { get; set; }
        public virtual DbSet<Dip_GG_Causali> Dip_GG_Causali { get; set; }




        public virtual DbSet<Par_Causali> Par_Causali { get; set; }
        public virtual DbSet<Par_Giustificativi> Par_Giustificativi { get; set; }
        public virtual DbSet<Par_Arrotondamenti> Par_Arrotondamenti { get; set; }
        public virtual DbSet<Par_Orario> Par_Orario { get; set; }
        public virtual DbSet<Par_ProfiloOrario> Par_ProfiloOrario { get; set; }
        public virtual DbSet<Par_ProfiloOrarioGG> Par_ProfiloOrarioGG { get; set; }
        public virtual DbSet<Par_OrarioIntervalloHH> Par_OrarioIntervalloHH { get; set; }
        public virtual DbSet<Par_OrarioPausa> Par_OrarioPausa { get; set; }

        public virtual DbSet<My_Template1> My_template1 { get; set; }
        public virtual DbSet<Az_SubCommessaSediReparto> Az_SubCommessaSediReparto { get; set; }

        public virtual DbSet<Par_ExportCau> Par_ExportCau { get; set; }
        public virtual DbSet<Par_ExportCau_Causali> Par_ExportCau_Causali { get; set; }
        public virtual DbSet<Par_ExportCau_Docs> Par_ExportCau_Docs { get; set; }

        public virtual DbSet<Dip_Rapporto_Giustificativi_Maturazione> Dip_Rapporto_Giustificativi_Maturazione { get; set; }
        public virtual DbSet<Dip_Contatori_Riporto> Dip_Contatori_Riporto { get; set; }
        public virtual DbSet<Par_Festivita> Par_Festivita { get; set; }
        public virtual DbSet<Dip_Periodo> Dip_Periodo { get; set; }
        public virtual DbSet<Par_Formule> Par_Formule { get; set; }
        public virtual DbSet<Par_ProfiloFormule> Par_ProfiloFormule { get; set; }
        public virtual DbSet<Par_ProfiloFormuleDett> Par_ProfiloFormuleDett { get; set; }
        public virtual DbSet<Dip_ProfiloFormule> Dip_ProfiloFormule { get; set; }
        public virtual DbSet<Dip_GG_FormuleLog> Dip_GG_FormuleLog { get; set; }
        
        

        private void Define_Table_AttendanceTracking(ModelBuilder modelBuilder)
        {
            Gen_PublicReferences_GestionePresenze(modelBuilder);
            Gen_Init_GestionePresenze(modelBuilder);
        }

        /*
         Tabelle di public (e comuni, vedi sotto) referenziate dalle tabelle presenze (chiavi esterne verso utenti e aziende).
         Mappate con lo schema "public" esplicito e ESCLUSE dalle migration tenant: le crea e le
         aggiorna solo PublicDbContext. Da qui si usano in sola lettura (Include, join).
         Le navigazioni verso altre tabelle di public non servono al modulo e vengono ignorate.
        */
        private void Gen_PublicReferences_GestionePresenze(ModelBuilder modelBuilder)
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

            /*
             Tabelle comuni dell'azienda (schema Common, tenant_<IdAzienda>_0 o public): le crea e le aggiorna
             CommonDbContext. Qui SENZA schema (le risolve il search_path, che contiene anche lo schema Common)
             e in sola lettura: le scritture passano dai repository di Repositories/Tenant/Common.
            */
            // tabella comune non usata dalle presenze: fuori dal modello
            modelBuilder.Ignore<Az_ClienteContatto>();

            modelBuilder.Entity<Az_Anagrafica>(entity =>
            {
                entity.ToTable("Az_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.CompanyNavigation)
                      .WithOne()
                      .HasForeignKey<Az_Anagrafica>(t => t.IdCompany);
            });

            modelBuilder.Entity<Az_Cliente>(entity =>
            {
                entity.ToTable("Az_Cliente", t => t.ExcludeFromMigrations());
                entity.Ignore(e => e.Az_ClienteContatto);
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany(t => t.Az_Cliente)
                      .HasForeignKey(t => t.IdAz_Anagrafica);
            });

            // dipendente comune (Common): le navigazioni verso le presenze sono nella partial Dip_Anagrafica.Presenze.cs
            modelBuilder.Entity<Dip_Anagrafica>(entity =>
            {
                entity.ToTable("Dip_Anagrafica", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica);
                entity.HasOne(t => t.AspNetUsersNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAspNetUsers);
            });

            /*
             Sedi, reparti, competenze e autovetture (AT-COM.1): tabelle comuni create da CommonDbContext (INF-COM),
             qui con la stessa configurazione e in sola lettura (scritture con i repository *CommonRepository).
             Le relazioni dalle tabelle delle presenze (Az_SediAttivita, Az_SediRepartoAttivita, Az_SediRepartoUser,
             Az_SubCommessaSediReparto, Par_AttivitaCompetenza, Dip_GG_NotaSpesa) restano configurate sotto: le loro
             chiavi esterne puntano alle tabelle comuni (migration AttendanceTracking_CommonTables).
            */
            modelBuilder.Entity<Az_Sedi>(entity =>
            {
                entity.ToTable("Az_Sedi", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Az_SediReparto>(entity =>
            {
                entity.ToTable("Az_SediReparto", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_SediNavigation)
                      .WithMany(t => t.Az_Reparto)
                      .HasForeignKey(t => t.IdAz_Sedi)
                      .OnDelete(DeleteBehavior.Cascade);
                // Relazione ricorsiva: un reparto può avere altri reparti come figli
                entity.HasOne(t => t.Az_SediRepartoNavigation)
                      .WithMany(t => t.Az_SediReparto_Sub)
                      .HasForeignKey(t => t.IdAz_SediReparto)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Par_Competenza>(entity =>
            {
                entity.ToTable("Par_Competenza", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Az_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdAz_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Dip_Competenza>(entity =>
            {
                entity.ToTable("Dip_Competenza", t => t.ExcludeFromMigrations());
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

            // autovettura del dipendente (prima era legata al rapporto di lavoro)
            modelBuilder.Entity<Dip_RapportoAutovetture>(entity =>
            {
                entity.ToTable("Dip_RapportoAutovetture", t => t.ExcludeFromMigrations());
                entity.HasOne(t => t.Dip_AnagraficaNavigation)
                      .WithMany()
                      .HasForeignKey(t => t.IdDip_Anagrafica)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void Gen_Init_GestionePresenze(ModelBuilder modelBuilder)
        {

            /* Dip_RapportoLavoro */
            modelBuilder.Entity<Dip_RapportoLavoro>()
                .HasOne(t_padre => t_padre.Dip_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_RapportoLavoro)
                .HasForeignKey(key_esterna => key_esterna.IdDip_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_RapportoLavoro>()
              .HasOne(t_padre => t_padre.Az_SubCommessaAttivitaNavigation)
              .WithMany(t_figlio => t_figlio.Dip_RapportoLavoro)
              .HasForeignKey(key_esterna => key_esterna.IdAz_SubCommessaAttivita)
              .OnDelete(DeleteBehavior.NoAction);



            /* Dip_ProfiloOrario */
            modelBuilder.Entity<Dip_ProfiloOrario>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany(t_figlio => t_figlio.Dip_ProfiloOrario)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_ProfiloOrario>()
                .HasOne(t_padre => t_padre.Par_ProfiloOrarioNavigation)
                .WithMany(t_figlio => t_figlio.Dip_ProfiloOrario)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloOrario)
                .OnDelete(DeleteBehavior.Cascade);

            
            /*Dip_GG_Result*/
            modelBuilder.Entity<Dip_GG_Result>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Result)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);



            /*Dip_GG_Causali*/
            modelBuilder.Entity<Dip_GG_Causali>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Causali)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_GG_Causali>()
                .HasOne(t_padre => t_padre.Par_CausaliNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Causali)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Causali)
                .OnDelete(DeleteBehavior.Cascade);


      



            /* Dip_GG_Richiesta */
            modelBuilder.Entity<Dip_GG_Richiesta>().HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
               .WithMany(t_figlio => t_figlio.Dip_GG_Richiesta)
               .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
               .OnDelete(DeleteBehavior.Cascade);

            EntityTypeBuilder<Dip_GG_Richiesta> richiestaEntity = modelBuilder.Entity<Dip_GG_Richiesta>();

            // Configurazione per le liste di oggetti JSON (Npgsql le mappa a jsonb)
            richiestaEntity.Property(e => e.RichiestaApprovazioneData)
                .HasColumnType("jsonb");

            richiestaEntity.Property(e => e.RevocaApprovazioneData)
                .HasColumnType("jsonb");


            /*Dip_GG_Giustificativi*/
            modelBuilder.Entity<Dip_GG_Giustificativi>()
                 .HasOne(t_padre => t_padre.Dip_RichiestaNavigation)
                 .WithMany(t_figlio => t_figlio.Dip_GG_Giustificativi)
                 .HasForeignKey(key_esterna => key_esterna.IdDip_GG_Richiesta)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_GG_Giustificativi>()
                .HasOne(t_padre => t_padre.Par_GiustificativiNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Giustificativi)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Giustificativi)
                .OnDelete(DeleteBehavior.Cascade);

            /* Dip_GG_NotaSpesa */
            modelBuilder.Entity<Dip_GG_NotaSpesa>()
                .HasOne(t_padre => t_padre.Dip_GG_RichiestaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_NotaSpesa)
                .HasForeignKey(key_esterna => key_esterna.IdDip_GG_Richiesta)
                .OnDelete(DeleteBehavior.Cascade);

            // un tipo di spesa usato da qualche spesa non si puo' cancellare
            modelBuilder.Entity<Dip_GG_NotaSpesa>()
                .HasOne(t_padre => t_padre.Par_TipoSpesaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_NotaSpesa)
                .HasForeignKey(key_esterna => key_esterna.IdPar_TipoSpesa)
                .OnDelete(DeleteBehavior.Restrict);

            // un'autovettura usata da qualche spesa non si puo' cancellare
            modelBuilder.Entity<Dip_GG_NotaSpesa>()
                .HasOne(t_padre => t_padre.Dip_RapportoAutovettureNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_NotaSpesa)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoAutovetture)
                .OnDelete(DeleteBehavior.Restrict);

            /* Dip_GG_NotaSpesa_Allegato */
            modelBuilder.Entity<Dip_GG_NotaSpesa_Allegato>()
                .HasOne(t_padre => t_padre.Dip_GG_NotaSpesaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_NotaSpesa_Allegato)
                .HasForeignKey(key_esterna => key_esterna.IdDip_GG_NotaSpesa)
                .OnDelete(DeleteBehavior.Cascade);

            /* Formule del calcolo giornata (ANALISI_ATTENDANCETRACKING_FORMULE.md) */
            modelBuilder.Entity<Par_Formule>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Par_Formule>()
                .HasIndex(x => new { x.IdAz_Anagrafica, x.Codice })
                .IsUnique();

            modelBuilder.Entity<Par_ProfiloFormule>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Par_ProfiloFormule>()
                .HasIndex(x => new { x.IdAz_Anagrafica, x.Codice })
                .IsUnique();

            modelBuilder.Entity<Par_ProfiloFormuleDett>()
                .HasOne(t_padre => t_padre.Par_ProfiloFormuleNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloFormuleDett)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloFormule)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Par_ProfiloFormuleDett>()
                .HasOne(t_padre => t_padre.Par_FormuleNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloFormuleDett)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Formule)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Dip_ProfiloFormule>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Dip_ProfiloFormule>()
                .HasOne(t_padre => t_padre.Par_ProfiloFormuleNavigation)
                .WithMany(t_figlio => t_figlio.Dip_ProfiloFormule)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloFormule)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Dip_GG_FormuleLog>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Dip_GG_FormuleLog>()
                .HasOne(t_padre => t_padre.Par_FormuleNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdPar_Formule)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Dip_GG_FormuleLog>()
                .HasIndex(x => new { x.IdDip_RapportoLavoro, x.Giorno });

            /* Par_TipoSpesa */
            modelBuilder.Entity<Par_TipoSpesa>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);


            /* Dip_GG_Timbratura */
            modelBuilder.Entity<Dip_GG_Timbratura>()
              .HasOne(t_padre => t_padre.Dip_RichiestaNavigation)
              .WithMany(t_figlio => t_figlio.Dip_GG_Timbratura)
              .HasForeignKey(key_esterna => key_esterna.IdDip_GG_Richiesta)
              .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Dip_GG_Timbratura>()
              .HasOne(t_padre => t_padre.Az_SubCommessaAttivitaNavigation)
              .WithMany(t_figlio => t_figlio.Dip_GG_Timbratura)
              .HasForeignKey(key_esterna => key_esterna.IdAz_SubCommessaAttivita)
              .OnDelete(DeleteBehavior.Cascade);

            /* Az_Cfg */
            modelBuilder.Entity<Az_Cfg>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithOne()
                .HasForeignKey<Az_Cfg>(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            // cliente predefinito delle presenze (tabella comune Az_Cliente)
            modelBuilder.Entity<Az_Cfg>()
                .HasOne(t => t.Az_ClienteDefaultNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAz_ClienteDefault)
                .OnDelete(DeleteBehavior.SetNull);


            /* Az_Sedi, Az_SediReparto: tabelle comuni (AT-COM.1), mappate in Gen_PublicReferences_GestionePresenze */

            /* Az_RepartoAttivita */
            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasOne(t => t.Az_SediRepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediRepartoAttivita)
                .HasForeignKey(t => t.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasOne(t => t.Par_AttivitaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdPar_Attivita)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasIndex(t => new { t.IdAz_SediReparto, t.IdPar_Attivita })
                .IsUnique();

            /* Az_SediRepartoUser */
            modelBuilder.Entity<Az_SediRepartoUser>()
                .HasOne(t_padre => t_padre.Az_SediRepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediRepartoUser)
                .HasForeignKey(key_esterna => key_esterna.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediRepartoUser>()
                .HasOne(t => t.AspNetUsersNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAspNetUsers)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Az_SediRepartoUser>()
                .HasIndex(t => new { t.IdAz_SediReparto, t.IdAspNetUsers })
                .IsUnique();


            /* Az_Attivita */
            modelBuilder.Entity<Par_Attivita>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_AttivitaCompetenza */
            modelBuilder.Entity<Par_AttivitaCompetenza>()
                .HasOne(t_padre => t_padre.Par_AttivitaNavigation)
                .WithMany(t_figlio => t_figlio.Par_AttivitaCompetenza)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Attivita)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Par_AttivitaCompetenza>()
                .HasOne(t => t.Par_CompetenzaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdPar_Competenza)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_Commessa */
            modelBuilder.Entity<Az_Commessa>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazione 1-1/1-N con Az_Cliente
            modelBuilder.Entity<Az_Commessa>()
                .HasOne(t => t.Az_ClienteNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAz_Cliente)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            /* Az_SubCommessa */
            modelBuilder.Entity<Az_SubCommessa>()
                .HasOne(t_padre => t_padre.Az_CommessaNavigation)
                .WithMany(t_figlio => t_figlio.Az_SubCommessa)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Commessa)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_SubCommessaAttivita */
            modelBuilder.Entity<Az_SubCommessaAttivita>()
                .HasOne(t => t.Az_SubCommessaNavigation)
                .WithMany(t_figlio => t_figlio.Az_SubCommessaAttivita)
                .HasForeignKey(t => t.IdAz_SubCommessa)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SubCommessaAttivita>()
                .HasOne(t => t.Par_AttivitaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdPar_Attivita)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_SediAttivita */
            modelBuilder.Entity<Az_SediAttivita>()
                .HasOne(t_padre => t_padre.Az_SediNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediAttivita)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Sedi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediAttivita>()
                .HasOne(t => t.Par_AttivitaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdPar_Attivita)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Causali */
            modelBuilder.Entity<Par_Causali>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Giustificativi */
            modelBuilder.Entity<Par_Giustificativi>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Par_Giustificativi>()
                .HasOne(p => p.Causale_Navigation)
                .WithMany() 
                .HasForeignKey(p => p.IdCausale)
                .OnDelete(DeleteBehavior.Restrict); 

            
            /* Par_ExportCau */
            modelBuilder.Entity<Par_ExportCau>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);


            /* Par_ExportCau_Docs */
            modelBuilder.Entity<Par_ExportCau_Docs>()
                .HasOne(t_padre => t_padre.Par_ExportCauNavigation)
                .WithMany(t_figlio => t_figlio.Par_ExportCau_Docs)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ExportCau)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_ExportCau_Causali */
            modelBuilder.Entity<Par_ExportCau_Causali>()
                .HasOne(t_padre => t_padre.Par_ExportCauNavigation)
                .WithMany(t_figlio => t_figlio.Par_ExportCau_Causali)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ExportCau)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Par_ExportCau_Causali>()
                .HasOne(t_padre => t_padre.CausaleNavigation)
                .WithMany(t_figlio => t_figlio.Par_ExportCau_Causali)
                .HasForeignKey(key_esterna => key_esterna.IdCausale)
                .OnDelete(DeleteBehavior.Cascade);




            /* Par_Arrotondamenti */
            modelBuilder.Entity<Par_Arrotondamenti>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Orario */
            modelBuilder.Entity<Par_Orario>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Par_Orario>()
            //  .HasOne(t_padre => t_padre.Az_SubCommessaAttivita_MonteOreNavigation)
            //  .WithMany(t_figlio => t_figlio.Par_Orario)
            //  .HasForeignKey(key_esterna => key_esterna.IdAz_SubCommessaAttivita_MonteOre)
            //  .OnDelete(DeleteBehavior.Cascade);



            /* Par_ProfiloOrario */
            modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);
               
             modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(p => p.Par_Orario_FestivoNavigation)
                .WithMany() 
                .HasForeignKey(p => p.IdPar_Orario_Festivo)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(p => p.Causale_Lavoro_StraoNavigation)
                .WithMany() 
                .HasForeignKey(p => p.IdCausale_Lavoro_Strao)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(p => p.Causale_Lavoro_SupplNavigation)
                .WithMany() 
                .HasForeignKey(p => p.IdCausale_Lavoro_Suppl)
                .OnDelete(DeleteBehavior.Restrict); 

            foreach (var nav in new[] { "Causale_Lavoro_StraoNottNavigation",
                                        "Causale_Lavoro_StraoFestNavigation",
                                        "Causale_Lavoro_StraoFestNottNavigation",
                                        "Causale_Lavoro_SupplEccNavigation",
                                        "Causale_Lavoro_EccOrdNavigation",
                                        "Causale_BancaOreNavigation" })
            {
                modelBuilder.Entity<Par_ProfiloOrario>()
                    .HasOne(typeof(Par_Causali), nav)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            }

            modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(p => p.Giustificativo_Assenza_IngiustNavigation)
                .WithMany() 
                .HasForeignKey(p => p.IdGiustificativo_Assenza_Ingiust)
                .OnDelete(DeleteBehavior.Restrict); 


            /* Par_ProfiloOrarioGG */
            modelBuilder.Entity<Par_ProfiloOrarioGG>()
                .HasOne(t_padre => t_padre.Par_ProfiloOrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloOrarioGG)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloOrario)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Par_ProfiloOrarioGG>()
            //  .HasOne(t_padre => t_padre.Az_SubCommessaAttivitaNavigation)
            //  .WithMany(t_figlio => t_figlio.Par_ProfiloOrarioGG)
            //  .HasForeignKey(key_esterna => key_esterna.IdAz_SubCommessaAttivita)
            //  .OnDelete(DeleteBehavior.Cascade);




            // Relazione 1-1/1-N con Par_Orario
            modelBuilder.Entity<Par_ProfiloOrarioGG>()
                  .HasOne(t => t.Par_OrarioNavigation)
                  .WithMany()
                  .HasForeignKey(t => t.IdPar_Orario)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();




            /* Par_OrarioPausa (ANALISI_ATTENDANCETRACKING_PAUSE.md) */
            modelBuilder.Entity<Par_OrarioPausa>()
                .HasOne(t_padre => t_padre.Par_OrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_OrarioPausa)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Orario)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_ProfiloOrarioIntervalloHH */
            modelBuilder.Entity<Par_OrarioIntervalloHH>()
                .HasOne(t_padre => t_padre.Par_OrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_OrarioIntervalloHH)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Orario)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Par_OrarioIntervalloHH>()
            //  .HasOne(t_padre => t_padre.Az_SubCommessaAttivitaNavigation)
            //  .WithMany(t_figlio => t_figlio.Par_OrarioIntervalloHH)
            //  .HasForeignKey(key_esterna => key_esterna.IdAz_SubCommessaAttivita)
            //  .OnDelete(DeleteBehavior.Cascade);



            /* Par_ProfiloOrarioGG */
            modelBuilder.Entity<Par_ProfiloOrarioGG>()
                .HasOne(t_padre => t_padre.Par_ProfiloOrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloOrarioGG)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloOrario)
                .OnDelete(DeleteBehavior.Cascade);


            /* Az_SubCommessaSediReparto */
            modelBuilder.Entity<Az_SubCommessaSediReparto>()
                .HasOne(t => t.Az_SubCommessaNavigation)
                .WithMany(t => t.Az_SubCommessaSediReparto)
                .HasForeignKey(t => t.IdAz_SubCommessa)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SubCommessaSediReparto>()
                .HasOne(t => t.Az_SediRepartoNavigation)
                .WithMany(t => t.Az_SubCommessaSediReparto)
                .HasForeignKey(t => t.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SubCommessaSediReparto>()
                .HasIndex(t => new { t.IdAz_SubCommessa, t.IdAz_SediReparto })
                .IsUnique();

            
            /* Az_SubCommessaUser */
            modelBuilder.Entity<Az_SubCommessaUser>()
                .HasOne(t => t.Az_SubCommessaNavigation)
                .WithMany(t => t.Az_SubCommessaUser)
                .HasForeignKey(t => t.IdAz_SubCommessa)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SubCommessaUser>()
                .HasOne(t => t.AspNetUsersNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAspNetUsers)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SubCommessaUser>()
                .HasIndex(t => new { t.IdAz_SubCommessa, t.IdAspNetUsers })
                .IsUnique();

            /* Dip_Rapporto_Giustificativi_Maturazione */
            modelBuilder.Entity<Dip_Rapporto_Giustificativi_Maturazione>()
                .HasOne(t => t.Dip_RapportoLavoroNavigation)
                .WithMany(t => t.Dip_Rapporto_Giustificativi_Maturazione)
                .HasForeignKey(t => t.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_Rapporto_Giustificativi_Maturazione>()
                .HasOne(t => t.Par_GiustificativiNavigation)
                .WithMany(t => t.Dip_Rapporto_Giustificativi_Maturazione)
                .HasForeignKey(t => t.IdPar_Giustificativi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_Rapporto_Giustificativi_Maturazione>()
                .HasIndex(t => new { t.IdDip_RapportoLavoro, t.IdPar_Giustificativi })
                .IsUnique();

            /* Dip_Contatori_Riporto */
            modelBuilder.Entity<Dip_Contatori_Riporto>()
                .HasOne(t => t.Dip_RapportoLavoroNavigation)
                .WithMany(t => t.Dip_Contatori_Riporto)
                .HasForeignKey(t => t.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_Contatori_Riporto>()
                .HasOne(t => t.Par_GiustificativiNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdPar_Giustificativi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_Contatori_Riporto>()
                .HasIndex(t => new { t.IdDip_RapportoLavoro, t.IdPar_Giustificativi, t.Anno })
                .IsUnique();

            /* Par_Festivita */
            modelBuilder.Entity<Par_Festivita>()
                .HasOne(t => t.Az_AnagraficaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Dip_Periodo */
            modelBuilder.Entity<Dip_Periodo>()
                .HasOne(t => t.Dip_RapportoLavoroNavigation)
                .WithMany(t => t.Dip_Periodo)
                .HasForeignKey(t => t.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_Periodo>()
                .HasIndex(t => new { t.IdDip_RapportoLavoro, t.Anno, t.Mese })
                .IsUnique();

        }



    }
}
