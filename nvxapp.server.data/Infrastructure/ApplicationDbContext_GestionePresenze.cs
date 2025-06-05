using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;

namespace nvxapp.server.data.Infrastructure
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {

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

        public virtual DbSet<Dip_GG_Giustificativi> Dip_GG_Giustificativi { get; set; }
        public virtual DbSet<Dip_GG_Timbratura> Dip_GG_Timbrature { get; set; }
        public virtual DbSet<Dip_GG_NotaSpesa> Dip_GG_NotaSpese { get; set; }
        public virtual DbSet<Dip_GG_Richiesta> Dip_GG_Richieste { get; set; }
        public virtual DbSet<Dip_GG_Causali> Dip_GG_Causali { get; set; }




        public virtual DbSet<Par_Causali> Par_Causali { get; set; }
        public virtual DbSet<Par_Giustificativi> Par_Giustificativi { get; set; }
        public virtual DbSet<Par_Arrotondamenti> Par_Arrotondamenti { get; set; }
        public virtual DbSet<Par_Orario> Par_Orario { get; set; }
        public virtual DbSet<Par_ProfiloOrario> Par_ProfiloOrario { get; set; }
        public virtual DbSet<Par_ProfiloOrarioGG> Par_ProfiloOrarioGG { get; set; }
        public virtual DbSet<Par_OrarioIntervalloHH> Par_ProfiloOrarioIntervalloHH { get; set; }

        public virtual DbSet<My_Template1> My_template1 { get; set; }
        public virtual DbSet<Az_SubCommessaSediReparto> Az_SubCommessaSediReparto { get; set; }


        private void Define_Table_DbContext_GestionePresenze(ModelBuilder modelBuilder)
        {
            Gen_Init_GestionePresenze(modelBuilder);
        }

        private void Gen_Init_GestionePresenze(ModelBuilder modelBuilder)
        {

            /* Dip_Anagrafica */
            modelBuilder.Entity<Dip_Anagrafica>()
                .HasOne(da => da.AspNetUsersNavigation)
                .WithOne(au => au.Dip_Anagrafica)
                .HasForeignKey<Dip_Anagrafica>(da => da.IdAspNetUsers)
                .OnDelete(DeleteBehavior.Cascade);

            /* Dip_RapportoLavoro */
            modelBuilder.Entity<Dip_RapportoLavoro>()
                .HasOne(t_padre => t_padre.Dip_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_RapportoLavoro)
                .HasForeignKey(key_esterna => key_esterna.IdDip_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

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


            /* Dip_GG_Timbratura */
            modelBuilder.Entity<Dip_GG_Timbratura>()
              .HasOne(t_padre => t_padre.Dip_RichiestaNavigation)
              .WithMany(t_figlio => t_figlio.Dip_GG_Timbratura)
              .HasForeignKey(key_esterna => key_esterna.IdDip_GG_Richiesta)
              .OnDelete(DeleteBehavior.Cascade);




            /* Az_Anagrafica */
            modelBuilder.Entity<Az_Anagrafica>()
                .HasOne(t_padre => t_padre.CompanyNavigation)
                .WithOne(t_figlio => t_figlio.Az_Anagrafica)
                .HasForeignKey<Az_Anagrafica>(key_esterna => key_esterna.IdCompany)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_Cfg */
            modelBuilder.Entity<Az_Cfg>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithOne(t_figlio => t_figlio.Az_Cfg)
                .HasForeignKey<Az_Cfg>(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);


            /* Az_Sedi */
            modelBuilder.Entity<Az_Sedi>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Az_Sedi)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);



            /* Az_Reparto */
            modelBuilder.Entity<Az_SediReparto>()
                .HasOne(t_padre => t_padre.Az_SediNavigation)
                .WithMany(t_figlio => t_figlio.Az_Reparto)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Sedi)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazione ricorsiva: un reparto può avere altri reparti come figli
            modelBuilder.Entity<Az_SediReparto>()
                .HasOne(t => t.Az_SediRepartoNavigation)
                .WithMany(t => t.Az_SediReparto_Sub)
                .HasForeignKey(t => t.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminazioni a cascata


            /* Az_RepartoAttivita */
            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasOne(t => t.Az_SediRepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediRepartoAttivita)
                .HasForeignKey(t => t.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasOne(t => t.Az_SediAttivitaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAz_SediAttivita)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Az_SediRepartoAttivita>()
                .HasIndex(t => new { t.IdAz_SediReparto, t.IdAz_SediAttivita })
                .IsUnique();

            /* Az_SediRepartoUser */
            modelBuilder.Entity<Az_SediRepartoUser>()
                .HasOne(t_padre => t_padre.Az_SediRepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediRepartoUser)
                .HasForeignKey(key_esterna => key_esterna.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);




            /* Az_RepartoUser */
            modelBuilder.Entity<Az_SediRepartoUser>()
                .HasOne(t_padre => t_padre.Az_SediRepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_SediRepartoUser)
                .HasForeignKey(key_esterna => key_esterna.IdAz_SediReparto)
                .OnDelete(DeleteBehavior.Cascade);


            /* Az_Attivita */
            modelBuilder.Entity<Par_Attivita>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Par_Attivita)
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
                .WithMany(t_figlio => t_figlio.Az_Commessa)
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
                .HasOne(t => t.Az_SediAttivitaNavigation)
                .WithMany()
                .HasForeignKey(t => t.IdAz_SediAttivita)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_Cliente */
            modelBuilder.Entity<Az_Cliente>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Az_Cliente)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
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
                .WithMany(t_figlio => t_figlio.Par_Causali)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Giustificativi */
            modelBuilder.Entity<Par_Giustificativi>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Par_Giustificativi)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Arrotondamenti */
            modelBuilder.Entity<Par_Arrotondamenti>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Par_Arrotondamenti)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_Orario */
            modelBuilder.Entity<Par_Orario>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Par_Orario)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_ProfiloOrario */
            modelBuilder.Entity<Par_ProfiloOrario>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloOrario)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);

            /* Par_ProfiloOrarioGG */
            modelBuilder.Entity<Par_ProfiloOrarioGG>()
                .HasOne(t_padre => t_padre.Par_ProfiloOrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloOrarioGG)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloOrario)
                .OnDelete(DeleteBehavior.Cascade);


            /* Par_ProfiloOrarioIntervalloHH */
            modelBuilder.Entity<Par_OrarioIntervalloHH>()
                .HasOne(t_padre => t_padre.Par_OrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_OrarioIntervalloHH)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Orario)
                .OnDelete(DeleteBehavior.Cascade);



            /* Par_ProfiloOrarioGG */
            modelBuilder.Entity<Par_ProfiloOrarioGG>()
                .HasOne(t_padre => t_padre.Par_ProfiloOrarioNavigation)
                .WithMany(t_figlio => t_figlio.Par_ProfiloOrarioGG)
                .HasForeignKey(key_esterna => key_esterna.IdPar_ProfiloOrario)
                .OnDelete(DeleteBehavior.Cascade);


            /* Dip_Competenza */
            modelBuilder.Entity<Dip_Competenza>()
                .HasOne(t_padre => t_padre.Dip_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Dip_Competenza)
                .HasForeignKey(key_esterna => key_esterna.IdDip_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Dip_Competenza>()
                .HasOne(t_padre => t_padre.Par_CompetenzaNavigation)
                .WithMany()
                .HasForeignKey(key_esterna => key_esterna.IdPar_Competenza)
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

        }



    }
}
