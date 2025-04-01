using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;

namespace nvxapp.server.data.Infrastructure
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {

        public virtual DbSet<Az_Anagrafica> Az_Anagrafica { get; set; }
        public virtual DbSet<Az_Sedi> Az_Sedi { get; set; }
        public virtual DbSet<Az_Reparto> Az_Reparto { get; set; }
        public virtual DbSet<Az_RepartoAttivita> Az_RepartoAttivita { get; set; }


        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafiche { get; set; }
        public virtual DbSet<Dip_RapportoLavoro> Dip_RapportiLavoro { get; set; }
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
                .HasOne(t_padre=> t_padre.Dip_AnagraficaNavigation)
                .WithMany(t_figlio=> t_figlio.Dip_RapportoLavoro)
                .HasForeignKey(key_esterna=> key_esterna.IdDip_Anagrafica)
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


            /*Dip_GG_Giustificativi*/
            modelBuilder.Entity<Dip_GG_Giustificativi>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Giustificativi)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dip_GG_Giustificativi>()
                .HasOne(t_padre => t_padre.Par_GiustificativiNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_Giustificativi)
                .HasForeignKey(key_esterna => key_esterna.IdPar_Giustificativi)
                .OnDelete(DeleteBehavior.Cascade);





            /* Dip_GG_NotaSpesa */
            modelBuilder.Entity<Dip_GG_NotaSpesa>()
                .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
                .WithMany(t_figlio => t_figlio.Dip_GG_NotaSpesa)
                .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
                .OnDelete(DeleteBehavior.Cascade);


            /* Dip_GG_Richiesta */
            modelBuilder.Entity<Dip_GG_Richiesta>()
               .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
               .WithMany(t_figlio => t_figlio.Dip_GG_Richiesta)
               .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
               .OnDelete(DeleteBehavior.Cascade);

            /* Dip_GG_Timbratura */
            modelBuilder.Entity<Dip_GG_Timbratura>()
              .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
              .WithMany(t_figlio => t_figlio.Dip_GG_Timbratura)
              .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
              .OnDelete(DeleteBehavior.Cascade);




            /* Az_Anagrafica */
            modelBuilder.Entity<Az_Anagrafica>()
                .HasOne(t_padre => t_padre.CompanyNavigation)
                .WithOne(t_figlio => t_figlio.Az_Anagrafica)
                .HasForeignKey<Az_Anagrafica>(key_esterna => key_esterna.IdCompany)
                .OnDelete(DeleteBehavior.Cascade);

            /* Az_Sedi */
            modelBuilder.Entity<Az_Sedi>()
                .HasOne(t_padre => t_padre.Az_AnagraficaNavigation)
                .WithMany(t_figlio => t_figlio.Az_Sedi)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Anagrafica)
                .OnDelete(DeleteBehavior.Cascade);



            /* Az_Reparto */
            modelBuilder.Entity<Az_Reparto>()
                .HasOne(t_padre => t_padre.Az_SediNavigation)
                .WithMany(t_figlio => t_figlio.Az_Reparto)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Sedi)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazione ricorsiva: un reparto può avere altri reparti come figli
            modelBuilder.Entity<Az_Reparto>()
                .HasOne(t => t.Az_RepartoNavigation)
                .WithMany(t => t.Az_Reparto_Sub)
                .HasForeignKey(t => t.IdAz_Reparto)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminazioni a cascata


            /* Az_RepartoAttivita */
            modelBuilder.Entity<Az_RepartoAttivita>()
                .HasOne(t_padre => t_padre.Az_RepartoNavigation)
                .WithMany(t_figlio => t_figlio.Az_RepartoAttivita)
                .HasForeignKey(key_esterna => key_esterna.IdAz_Reparto)
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





        }



    }
}
