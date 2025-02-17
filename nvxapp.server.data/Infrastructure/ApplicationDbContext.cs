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

    


    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        private static string _baseSchema = "public";
        public string? _currentSchema = "public";

        /*
         Questo costruttore viene eseguito dal ApplicationDbContextFactory nei 
         repository che accedono ai tenant (schema varra null e varra estratto dall'header http)
        */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    string? schema) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            
            _configuration = configuration;         // NON SPOSTARE va eseguito 1
            _currentSchema = schema;                // NON SPOSTARE va eseguito 2
            SharedSchema.CurrentSchema = getSchema; // NON SPOSTARE va eseguito 3

            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

        }

        /// Questo costruttore viene eseguito dalla injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IHttpContextAccessor? httpContextAccessor,
                                    IConfiguration configuration) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;          // NON SPOSTARE va eseguito 1
            SharedSchema.CurrentSchema = getSchema;  // NON SPOSTARE va eseguito 2

            //X le date
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;


            
        }


        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SharedSchema.CurrentSchema = getSchema;

            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(_baseSchema);

            Define_Table_DbContext_Infrastructure(modelBuilder);



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
        private string getSchema
        {
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
                        if (_httpContextAccessor?.HttpContext != null)
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




