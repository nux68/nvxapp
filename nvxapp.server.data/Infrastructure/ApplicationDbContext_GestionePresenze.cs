using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.data.Infrastructure
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public virtual DbSet<Dip_Anagrafica> Dip_Anagrafiche { get; set; }
        public virtual DbSet<Dip_RapportoLavoro> Dip_RapportiLavoro { get; set; }
        public virtual DbSet<Dip_GG_Causali> Dip_GG_Causali { get; set; }
        public virtual DbSet<Dip_GG_Giustificativi> Dip_GG_Giustificativi { get; set; }
        public virtual DbSet<Dip_GG_NotaSpesa> Dip_GG_NotaSpese { get; set; }
        public virtual DbSet<Dip_GG_Richiesta> Dip_GG_Richieste { get; set; }
        public virtual DbSet<Dip_GG_Timbratura> Dip_GG_Timbrature { get; set; }

        public virtual DbSet<Par_Causali> Par_Causali { get; set; }
        public virtual DbSet<Par_Giustificativi> Par_Giustificativi { get; set; }




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


            



            /////////////////////////////////////////////////
            /////////////////////////////////////////////////
            /////////////////////////////////////////////////
            //modelBuilder.Entity<Par_Causali>()
            //  .HasOne(t_padre => t_padre.Dip_RapportoLavoroNavigation)
            //  .WithMany(t_figlio => t_figlio.Dip_GG_Timbratura)
            //  .HasForeignKey(key_esterna => key_esterna.IdDip_RapportoLavoro)
            //  .OnDelete(DeleteBehavior.Cascade);



            //modelBuilder.Entity<Dip_GG_Causali>()
            //    .HasOne(ggc => ggc.Dip_RapportoLavoroNavigation)
            //    .WithMany(drl => drl.Dip_GG_Causali)
            //    .HasForeignKey(ggc => ggc.IdDip_RapportoLavoro)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Dip_GG_Giustificativi>()
            //    .HasOne(ggj => ggj.Dip_RapportoLavoroNavigation)
            //    .WithMany(drl => drl.Dip_GG_Giustificativi)
            //    .HasForeignKey(ggj => ggj.IdDip_RapportoLavoro)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Dip_GG_NotaSpesa>()
            //    .HasOne(ggn => ggn.Dip_RapportoLavoroNavigation)
            //    .WithMany(drl => drl.Dip_GG_NotaSpesa)
            //    .HasForeignKey(ggn => ggn.IdDip_RapportoLavoro)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Dip_GG_Richiesta>()
            //    .HasOne(ggr => ggr.Dip_RapportoLavoroNavigation)
            //    .WithMany(drl => drl.Dip_GG_Richiesta)
            //    .HasForeignKey(ggr => ggr.IdDip_RapportoLavoro)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Dip_GG_Timbratura>()
            //    .HasOne(ggt => ggt.Dip_RapportoLavoroNavigation)
            //    .WithMany(drl => drl.Dip_GG_Timbratura)
            //    .HasForeignKey(ggt => ggt.IdDip_RapportoLavoro)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Dip_RapportoLavoro>()
            //    .HasOne(drl => drl.Dip_AnagraficaNavigation)
            //    .WithOne(da => da.Dip_RapportoLavoro)
            //    .HasForeignKey<Dip_RapportoLavoro>(drl => drl.IdDip_Anagrafica)
            //    .OnDelete(DeleteBehavior.Cascade);


            //modelBuilder.Entity<Par_Causali>()
            // .HasMany(pc => pc.Dip_GG_Causali)
            // .WithOne(dgc => dgc.Par_CausaliNavigation)
            // .HasForeignKey(dgc => dgc.IdPar_Causali)
            // .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Par_Giustificativi>()
            //.HasMany(pg => pg.Dip_GG_Giustificativi)
            //.WithOne(dgg => dgg.Par_GiustificativiNavigation)
            //.OnDelete(DeleteBehavior.Cascade);



            ////Configurazione chiave esterna Dip_GG_Giustificativi -> Dip_GG_Richiesta
            //modelBuilder.Entity<Dip_GG_Giustificativi>()
            //.HasOne(dggj => dggj.Dip_RichiestaNavigation)
            //.WithMany(dggr => dggr.Dip_GG_Giustificativi)
            //.HasForeignKey(dggj => dggj.IdDip_Richiesta)
            //.OnDelete(DeleteBehavior.Restrict);  //Oppure, configura a seconda della tua logica di business

            ////Configurazione chiave esterna Dip_GG_Timbratura -> Dip_GG_Richiesta
            //modelBuilder.Entity<Dip_GG_Timbratura>()
            //    .HasOne(dggt => dggt.Dip_RichiestaNavigation)
            //    .WithMany()
            //    .HasForeignKey(dggt => dggt.IdDip_Richiesta)
            //    .OnDelete(DeleteBehavior.Restrict);  //Oppure, configura a seconda della tua logica di business

            ////Configurazione chiave esterna Dip_GG_NotaSpesa -> Dip_GG_Richiesta
            //modelBuilder.Entity<Dip_GG_NotaSpesa>()
            //    .HasOne(dggn => dggn.Dip_GG_RichiestaNavigation)
            //    .WithMany(dggr => dggr.Dip_GG_NotaSpesa)
            //    .HasForeignKey(dggn => dggn.IdDip_GG_Richiesta)
            //    .OnDelete(DeleteBehavior.Restrict);  //Oppure, configura a seconda della tua logica di business
        }



    }
}
