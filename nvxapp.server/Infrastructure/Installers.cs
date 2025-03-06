using nvxapp.server.service.Interfaces;
using System.Reflection;
using NetCore.AutoRegisterDi;
using nvxapp.server.data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Interfaces;
using Microsoft.AspNetCore.Identity;
using NpgsqlTypes;
using Serilog.Core;
using Serilog.Events;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using nvxapp.server.service.ServerModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.HubAI;
using System.Net;
using Hangfire;
using Hangfire.PostgreSql;

namespace nvxapp.server.Infrastructure
{
    public static class Installers
    {

        public static IServiceCollection InstallHangFire(this WebApplicationBuilder builder)
        {
            var connStr = builder.Configuration.GetConnectionString("nvxappDbContext");

            connStr = connStr + ";SearchPath=hangfire";

            // Aggiungi Hangfire ai servizi
            builder.Services.AddHangfire(config => config
                   .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                   .UseSimpleAssemblyNameTypeSerializer()
                   .UseRecommendedSerializerSettings()
                   .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connStr)));


            builder.Services.AddHangfireServer();

            ////////ESEMPIO UTILIZZO
            //////BackgroundJob.Enqueue(() => Console.WriteLine("Job eseguito!"));
            ////////ricorrente
            //////RecurringJob.AddOrUpdate("job-id", () => Console.WriteLine("Job ricorrente!"), Cron.Daily);

            return builder.Services;
        }


        public static  void  InstallSignalRHub(this WebApplication app)
        {
            app.MapHub<SignalRHub>("/chathub");
        }


        public static IServiceCollection InstallConfiguration(this WebApplicationBuilder builder)
        {

            builder.Services.Configure<JwtParameter>(builder.Configuration.GetSection("JwtParameter"));
            //builder.Services.Configure<JwtParameter>(builder.Configuration.GetSection("ALTRASEZIONE"));

            return builder.Services;
        }
        public static IServiceCollection InstallServices(this WebApplicationBuilder builder)
        {

            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IServiceBase)))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);


            return builder.Services;
        }
        public static IServiceCollection InstallEntityContex(this WebApplicationBuilder builder)
        {
            var serviceProvider = builder.Services.AddEntityFrameworkNpgsql()
                                                  .BuildServiceProvider();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            
                options.UseNpgsql(
                       builder.Configuration.GetConnectionString("nvxappDbContext"),
                       npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data") // Specifica l'assembly per le migrazioni
                       ).UseInternalServiceProvider(serviceProvider)
            );

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddHttpContextAccessor();  
            builder.Services.AddScoped<SignInManager<ApplicationUser>>();


            //4 SCHEMA
            builder.Services.AddScoped<IApplicationDbContextFactory, ApplicationDbContextFactory>();
            builder.Services.AddEntityFrameworkNpgsql();
            //builder.Services.AddMvc();
            //builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            ////4 SCHEMA DISABLED
            //builder.Services.AddScoped<IMigrationsSqlGenerator, SchemaAwareMigrationSqlGenerator>();







            return builder.Services;
        }
        public static IServiceCollection InstallRepositories(this WebApplicationBuilder builder)
        {
            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IRepository<>)))
                            .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //builder.Services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();

            return builder.Services;
        }
        public static IServiceCollection InstallMappers(this WebApplicationBuilder builder)
        {

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return builder.Services;
        }
        public static IServiceCollection InstallLog(this WebApplicationBuilder builder)
        {

            //Log.Logger = new LoggerConfiguration().WriteTo.RollingFile("c:\\Logs\\{Date}.log").CreateLogger();

            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Information;

            var logMinimumLevel = builder.Configuration["Logging:LogLevel:Serilog"];

            if (logMinimumLevel != null)
            {
                logMinimumLevel = logMinimumLevel.ToLower();

                if (logMinimumLevel.StartsWith("info"))
                {
                    levelSwitch.MinimumLevel = LogEventLevel.Information;
                }
                else if (logMinimumLevel.StartsWith("debug"))
                {
                    levelSwitch.MinimumLevel = LogEventLevel.Debug;
                }
                else if (logMinimumLevel.StartsWith("trace"))
                {
                    levelSwitch.MinimumLevel = LogEventLevel.Verbose;
                }
                else if (logMinimumLevel.StartsWith("warn"))
                {
                    levelSwitch.MinimumLevel = LogEventLevel.Warning;
                }
            }






            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Console();

            string? LogOnFile = builder.Configuration["Logging:LogType:LogOnFile"];
            if(LogOnFile!=null)
            {
                bool writeToFile = Boolean.Parse(LogOnFile);

                if (writeToFile)
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.File($"Logs\\nvxapp.log",
                                          rollOnFileSizeLimit: true,
                                          fileSizeLimitBytes: 10000000,
                                          retainedFileCountLimit: 200,
                                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

                }
            }
            


            string? LogOnDB = builder.Configuration["Logging:LogType:LogOnDB"];

            if(LogOnDB!=null)
            {
                bool writeToDatabase = Boolean.Parse(LogOnDB);
                if (writeToDatabase)
                {
                    //IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
                    //{
                    //    {"Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                    //    //{"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
                    //    {"Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                    //    {"Date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                    //    {"Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                    //    //{"Properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
                    //    //{"PropsTest", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
                    //    //{"MachineName", new SinglePropertyColumnWriter("machine_name", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "1") },

                    //    {"CallState", new Call_state_ColumnWriter(PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
                    //    {"MethodName", new MethodName_ColumnWriter("a1",PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
                    //    {"GuidCall", new SimpleString_ColumnWriter("a2",PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
                    //    {"DataCall", new SimpleString_ColumnWriter("a3",PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
                    //};

                    //loggerConfiguration = loggerConfiguration.WriteTo.PostgreSQL(
                    //                      connectionString: builder.Configuration["connectionStrings:nvxappDbContext"],
                    //                      tableName: "Logs",
                    //                      columnOptions: columnWriters,
                    //                      needAutoCreateTable: true,
                    //                      restrictedToMinimumLevel: levelSwitch.MinimumLevel);


                    // TODO nuova versione con colonne riorganizzate e su schema differente


                    IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
                    {
                        {"Date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                        {"Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                        {"CallState", new Call_state_ColumnWriter(PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
                        {"MethodName", new MethodName_ColumnWriter("a1",PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
                        {"Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                        {"Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                        {"GuidCall", new SimpleString_ColumnWriter("a2",PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
                    };

                    loggerConfiguration = loggerConfiguration.WriteTo.PostgreSQL(
                                          connectionString: builder.Configuration["connectionStrings:nvxappDbContext"],
                                          tableName: "Logs",
                                          columnOptions: columnWriters,
                                          needAutoCreateTable: true,
                                          restrictedToMinimumLevel: levelSwitch.MinimumLevel,
                                          schemaName: "log");
                }
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            return builder.Services;
        }


        public static IServiceCollection InstallAuthentication(this WebApplicationBuilder builder, Boolean useSignalR)
        {
            string JwtParameterKey = builder.Configuration["JwtParameter:Key"] ?? "";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = builder.Configuration["JwtParameter:Issuer"],
                                    ValidAudience = builder.Configuration["JwtParameter:Audience"],
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtParameterKey)),

                                    ClockSkew = TimeSpan.Zero // Disattiva il ritardo predefinito di 5 minuti
                                };

                                if(useSignalR)
                                {
                                    // 🔹 Gestisce il token anche per SignalR (quando inviato nella query string)
                                    options.Events = new JwtBearerEvents
                                    {
                                        OnMessageReceived = context =>
                                        {
                                            var accessToken = context.Request.Query["access_token"];
                                            var path = context.HttpContext.Request.Path;

                                            // ✅ Se è una richiesta SignalR e il token è presente nella query string, usalo
                                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                                            {
                                                context.Token = accessToken;
                                            }

                                            return Task.CompletedTask;
                                        }
                                    };
                                }
                                
                            });

            return builder.Services;
        }



        public static IServiceCollection InstallCors(this WebApplicationBuilder builder, Boolean useSignalR)
        {
            builder.Services.AddCors(options =>
            {

                if(useSignalR==false)
                {
                    options.AddPolicy("AllowAllOrigins", policy =>
                    {
                        policy.AllowAnyOrigin() // Permette qualsiasi origine
                              .AllowAnyMethod() // Permette qualsiasi metodo (GET, POST, PUT, DELETE, ecc.)
                              .AllowAnyHeader(); // Permette qualsiasi intestazione
                    });
                }
                else
                {
                    //SIGNALR (V2)
                    builder.Services.AddCors(options =>
                    {
                        options.AddPolicy("DynamicOrigins", policy =>
                        {
                            policy.AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials()
                                  .SetIsOriginAllowed(origin => true); // Consente tutte le origini
                        });
                    });
                }



                

            });

            return builder.Services;
        }

        //SIGNALR (V2)
        public static void UseDynamicCors(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                var origin = context.Request.Headers["Origin"];
                if (!string.IsNullOrEmpty(origin))
                {
                    // Aggiungi l'intestazione Access-Control-Allow-Origin
                    context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"); // Metodi consentiti
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, x-requested-with, x-signalr-user-agent"); // Intestazioni consentite
                }

                if (context.Request.Method == "OPTIONS")
                {
                    // Intestazioni preflight
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, x-requested-with, x-signalr-user-agent");
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.CompleteAsync();
                    return;
                }

                await next();
            });
        }




        public static IServiceCollection InstallSettings(this WebApplicationBuilder builder)
        {
            /*
                     ATTENZIONE IMPOSTARE IN :
    
                    Pannello di controllo -> Sistema -> Impostazioni si sistema avanzate -> Variabili ambiente
                        mettere
                            ASPNETCORE_ENVIRONMENT
                                Svi            
                                Test
                                Staging
                                Siges
                
                     */


            if (builder.Environment.EnvironmentName.Contains("Development"))
            {
                // Configurazione personalizzata sviluppatori
                builder.Configuration
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables();
            }
            else
            {
                builder.Configuration
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables();
            }

            return builder.Services;
        }

    }
}
