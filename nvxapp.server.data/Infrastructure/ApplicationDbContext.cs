using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;



namespace nvxapp.server.data.Infrastructure
{

    public class MyApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor=null;

        //public MyApplicationDbContextFactory(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpContextAccessor = httpContextAccessor;
        //}

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("nvxappDbContext");

            Boolean MultiTenant = false;
            string? sMultiTenant = configuration["DbParameter:MultiTenant"];

            bool.TryParse(sMultiTenant, out MultiTenant);
            SharedSchema.MultiTenant = MultiTenant;


            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data")
            );

            Console.WriteLine("ciao");

            return new ApplicationDbContext(optionsBuilder.Options, _httpContextAccessor, configuration);
        }
    }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbSet<MyTable> MyTables { get; set; }
        public DbSet<Dealer> Dealer { get; set; }
        public DbSet<UserDealer> UserDealer { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }




        private static string _baseSchema = "public";
        public string? _currentSchema = "public";

        /*
         Questo costruttore viene eseguito dal ApplicationDbContextFactory nei 
         repository che accedono ai tenant
        */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    string? schema) : base(options)
        {
            _currentSchema = schema;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;


            SharedSchema.CurrentSchema = getSchema;

            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            //////////ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            //////////if (UsersEvents != null)
            //////////    Entry(UsersEvents).State = EntityState.Detached;
        }

        /// Questo costruttore viene eseguito dalla injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration) : base(options)
        {
            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            SharedSchema.CurrentSchema = getSchema;

            //////////ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            //////////if (UsersEvents != null)
            //////////    Entry(UsersEvents).State = EntityState.Detached;

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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SharedSchema.CurrentSchema = getSchema;

            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(_baseSchema);

            Gen_InitDB(modelBuilder);

            Gen_MyTable(modelBuilder);

            Gen_DealerAndCompany_Init(modelBuilder);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SharedSchema.CurrentSchema = getSchema;

            base.OnConfiguring(optionsBuilder);


            ////4 SCHEMA DISABLED
            //optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SchemaAwareMigrationSqlGenerator>();
            optionsBuilder.UseNpgsql(options =>
                    options.MigrationsHistoryTable("__EFMigrationsHistory", getSchema)
                )
            .AddInterceptors(new SchemaInterceptor());



            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        /*
         Funzionamento
         se MultiTenant = false (parametri applicazione)
            allora ritorna public
         altrimenti
            se _currentSchema è valorizzato dal costruttore tramite RepositoryXX->ApplicationDbContextFactory
               usa quel valore
            altrimenti
               estrae il valore col HttpContext
         */
        private string getSchema
        {

            get
            {
                string retVal;

                Boolean MultiTenant = false;
                string? sMultiTenant = _configuration["DbParameter:MultiTenant"];

                bool.TryParse(sMultiTenant, out MultiTenant);


                if (MultiTenant == false)
                {
                    _currentSchema = _baseSchema;
                }
                else
                {
                    if (string.IsNullOrEmpty(_currentSchema))
                    {
                        if (_httpContextAccessor.HttpContext != null)
                        {
                            var schema = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant")?.Value;
                            if (!string.IsNullOrEmpty(schema))
                                _currentSchema = schema;
                        }
                    }
                }

                retVal = _currentSchema ?? _baseSchema;

                return retVal;
            }

        }


    }





}




