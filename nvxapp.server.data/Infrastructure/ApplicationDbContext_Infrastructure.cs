using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.data.Infrastructure
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public DbSet<MyTable> MyTables { get; set; }

        public DbSet<Dealer> Dealer { get; set; }
        public DbSet<UserDealer> UserDealer { get; set; }
        public DbSet<FinancialAdvisor> FinancialAdvisor { get; set; }
        public DbSet<UserFinancialAdvisor> UserFinancialAdvisor { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }


        private void Define_Table_DbContext_Infrastructure(ModelBuilder modelBuilder)
        {
            Gen_InitDB(modelBuilder);
            Gen_MyTable(modelBuilder);
            Gen_DealerAndCompany_Init(modelBuilder);
        }

        private void Gen_InitDB(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable("AspNetUsers", _baseSchema); });
            modelBuilder.Entity<ApplicationRole>(entity => { entity.ToTable("AspNetRoles", _baseSchema); });

            modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("AspNetUserRoles", _baseSchema); });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims", _baseSchema); });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("AspNetUserLogins", _baseSchema); });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims", _baseSchema); });
            modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("AspNetUserTokens", _baseSchema); });
        }
        private void Gen_MyTable(ModelBuilder modelBuilder)
        {
            // Configura altre entità nello schema appropriato
            modelBuilder.Entity<MyTable>(entity =>
            {
                entity.ToTable("MyTable", _currentSchema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();

                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });
        }
        private void Gen_DealerAndCompany_Init(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dealer>(entity =>
            {
                entity.ToTable("Dealer", _baseSchema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserDealer>(entity =>
            {
                entity.ToTable("UserDealer", _baseSchema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdDealer, e.IdAspNetUsers }).IsUnique();
            });


            modelBuilder.Entity<FinancialAdvisor>(entity =>
            {
                entity.ToTable("FinancialAdvisor", _baseSchema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserFinancialAdvisor>(entity =>
            {
                entity.ToTable("UserFinancialAdvisor", _baseSchema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdFinancialAdvisor, e.IdAspNetUsers }).IsUnique();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company", _baseSchema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserCompany>(entity =>
            {
                entity.ToTable("UserCompany", _baseSchema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdCompany, e.IdAspNetUsers }).IsUnique();
            });

            #region CONFIGURE RELATIONS ONE-TO-MANY 

            #region Dealer-FinancialAdvisor

            modelBuilder.Entity<FinancialAdvisor>()
                       .HasOne(md => md.DealerNavigation)
                       .WithMany(d => d.FinancialAdvisor)
                       .HasForeignKey(md => md.IdDealer);
            //questa x specificare la cancellazione
            modelBuilder.Entity<Dealer>()
                        .HasMany(o => o.FinancialAdvisor)
                        .WithOne(i => i.DealerNavigation)
                        .OnDelete(DeleteBehavior.Cascade); //MODIFICARE SENNO RASA VIA TUTTO

            #endregion

            #region FinancialAdvisor-Company
            modelBuilder.Entity<Company>()
                        .HasOne(md => md.FinancialAdvisorNavigation)
                        .WithMany(d => d.Company)
                        .HasForeignKey(md => md.IdFinancialAdvisor);
            //questa x specificare la cancellazione
            modelBuilder.Entity<FinancialAdvisor>()
                        .HasMany(o => o.Company)
                        .WithOne(i => i.FinancialAdvisorNavigation)
                        .OnDelete(DeleteBehavior.Cascade); //MODIFICARE SENNO RASA VIA TUTTO

            #endregion

            #region Dealer-UserDealer

            modelBuilder.Entity<UserDealer>()
                   .HasOne(md => md.DealerNavigation)
                   .WithMany(d => d.UserDealer)
                   .HasForeignKey(md => md.IdDealer);

            modelBuilder.Entity<Dealer>()
                        .HasMany(o => o.UserDealer)
                        .WithOne(i => i.DealerNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserDealer>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserDealer)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserDealer)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region FinancialAdvisor-UserFinancialAdvisor

            modelBuilder.Entity<UserFinancialAdvisor>()
                   .HasOne(md => md.FinancialAdvisorNavigation)
                   .WithMany(d => d.UserFinancialAdvisor)
                   .HasForeignKey(md => md.IdFinancialAdvisor);

            modelBuilder.Entity<FinancialAdvisor>()
                        .HasMany(o => o.UserFinancialAdvisor)
                        .WithOne(i => i.FinancialAdvisorNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserFinancialAdvisor>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserFinancialAdvisor)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserFinancialAdvisor)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Company-UserCompany

            modelBuilder.Entity<UserCompany>()
                        .HasOne(md => md.CompanyNavigation)
                        .WithMany(d => d.UserCompany)
                        .HasForeignKey(md => md.IdCompany);

            modelBuilder.Entity<Company>()
                        .HasMany(o => o.UserCompany)
                        .WithOne(i => i.CompanyNavigation)
                        .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserCompany>()
                        .HasOne(md => md.AspNetUsersNavigation)
                        .WithMany(d => d.UserCompany)
                        .HasForeignKey(md => md.IdAspNetUsers);

            modelBuilder.Entity<ApplicationUser>()
                        .HasMany(o => o.UserCompany)
                        .WithOne(i => i.AspNetUsersNavigation)
                        .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #endregion

        }




    }
}
