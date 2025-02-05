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
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using nvxapp.server.data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Infrastructure.ApplicationDbContext;

namespace nvxapp.server.data.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<MyTable> MyTables { get; set; }
        
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
            modelBuilder.Entity<IdentityRole>(entity => { entity.ToTable("AspNetRoles", _schema); });
            modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("AspNetUserRoles", _schema); });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("AspNetUserClaims", _schema); });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("AspNetUserLogins", _schema); });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("AspNetRoleClaims", _schema); });
            modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("AspNetUserTokens", _schema); });

            //// Configura altre entità nello schema appropriato
            //modelBuilder.Entity<MyTable>(entity =>
            //{
            //    entity.ToTable("MyTable", _schema);
            //    entity.HasKey(e => e.Id);
            //    entity.Property(e => e.Descrizione).IsRequired()
            //    ;
            //})/*.HasIndex(e => e.Descrizione).IsUnique()*/;


            // Imposta lo schema per le altre tabelle
            modelBuilder.Entity<MyTable>().ToTable("MyTable", _schema);

            #region SET INDEX
            modelBuilder.Entity<MyTable>().HasIndex(e => e.Descrizione).IsUnique();
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
                    );

            }
                
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }



    }

}




