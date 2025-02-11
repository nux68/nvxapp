using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Infrastructure.ApplicationDbContext;

namespace nvxapp.server.data.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole,string>
    {

        public DbSet<MyTable> MyTables { get; set; }
        public DbSet<Dealer> Dealer { get; set; }
        public DbSet<UserDealer> UserDealer { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }




        public string _schema { get; set; } = "public";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string schema = "public") : base(options)
        {
            _schema = schema;
            SharedSchema._schema = _schema;


            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            //////////ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            //////////if (UsersEvents != null)
            //////////    Entry(UsersEvents).State = EntityState.Detached;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


            //////////ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            //////////if (UsersEvents != null)
            //////////    Entry(UsersEvents).State = EntityState.Detached;

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(_schema);


            // Configura lo schema per le tabelle di identità
            modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable("AspNetUsers", _schema); });
            modelBuilder.Entity<ApplicationRole>(entity => { entity.ToTable("AspNetRoles", _schema); });

            modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("AspNetUserRoles", _schema); });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims", _schema); });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("AspNetUserLogins", _schema); });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims", _schema); });
            modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("AspNetUserTokens", _schema); });

            // Configura altre entità nello schema appropriato
            modelBuilder.Entity<MyTable>(entity =>
            {
                entity.ToTable("MyTable", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();

                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            // Imposta lo schema per le altre tabelle
            modelBuilder.Entity<MyTable>().ToTable("MyTable", _schema);


            modelBuilder.Entity<Dealer>(entity =>
            {
                entity.ToTable("Dealer", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserDealer>(entity =>
            {
                entity.ToTable("UserDealer", _schema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdDealer, e.IdAspNetUsers }).IsUnique();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descrizione).IsRequired();
                // Crea un indice univoco 
                entity.HasIndex(e => new { e.Descrizione }).IsUnique();
            });

            modelBuilder.Entity<UserCompany>(entity =>
            {
                entity.ToTable("UserCompany", _schema);
                entity.HasKey(e => e.Id);

                // Crea un indice univoco su due colonne
                entity.HasIndex(e => new { e.IdCompany, e.IdAspNetUsers }).IsUnique();
            });




            //#region SET INDEX
            //modelBuilder.Entity<MyTable>().HasIndex(e => e.Descrizione).IsUnique();
            //#endregion

            #region CONFIGURE RELATIONSHIP ONE-TO-MANY 


                #region Dealer-Company
                modelBuilder.Entity<Company>()
                            .HasOne(md => md.DealerNavigation)
                            .WithMany(d => d.Company)
                            .HasForeignKey(md => md.IdDealer);

                modelBuilder.Entity<Dealer>()
                            .HasMany(o => o.Company)
                            .WithOne(i => i.DealerNavigation)
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);


            if (!string.IsNullOrEmpty(_schema))
            {
                //4 SCHEMA
                SharedSchema._schema = _schema;
                optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SchemaAwareMigrationSqlGenerator>();
                optionsBuilder.UseNpgsql(options =>
                        options.MigrationsHistoryTable("__EFMigrationsHistory", _schema)
                    )
                .AddInterceptors(new SchemaInterceptor());

            }
                
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

    }

    

    

}




