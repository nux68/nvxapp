using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetCore.AutoRegisterDi;
using NpgsqlTypes;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Interfaces;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.HubAI;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ;
using nvxapp.server.service.RabbitMQ.Listener;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Globalization;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using System.Net;
using System.Reflection;
using System.Text;

namespace nvxapp.server.Utility
{
    public static class Installers
    {
        public static IServiceCollection InstallRabbitMq(this WebApplicationBuilder builder)
        {

            
            builder.Services.AddScoped<iRabbitMqConnection, RabbitMqConnection>();


            //builder.Services.AddHostedService<RabbitMqListenerService>();
            //OK
            var hostedServiceType = typeof(IRabbitMqListenerService);
            var targetAssembly = Assembly.GetAssembly(typeof(IRabbitMqListenerService));

            if (targetAssembly != null)
            {
                var implementations = targetAssembly
               .GetTypes()
               .Where(t => t.IsClass && !t.IsAbstract && hostedServiceType.IsAssignableFrom(t));

                foreach (var implementation in implementations)
                {
                    // Trova il metodo generico corretto per AddHostedService
                    var addHostedServiceMethod = typeof(ServiceCollectionHostedServiceExtensions)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .First(m => m.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService) &&
                                    m.IsGenericMethod);

                    // Costruisci il metodo generico con il tipo specifico
                    var genericMethod = addHostedServiceMethod.MakeGenericMethod(implementation);

                    // Invochi il metodo su builder.Services
                    genericMethod.Invoke(null, new object[] { builder.Services });
                }
            }


            return builder.Services;
        }
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
        public static void InstallSignalRHub(this WebApplication app)
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

            builder.Services.AddHttpClient();

            //builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IServiceBase)))
            //    .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IServiceBase)))
            //        .Where(type => type != typeof(RabbitMqListenerService)) // Escludi RabbitMqListenerService
            //        .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IServiceBase)))
                            .Where(type => !typeof(IRabbitMqListenerService).IsAssignableFrom(type)
                                        && !typeof(nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.IRouteProvider).IsAssignableFrom(type)) // IRouteProvider registrati come Singleton sotto
                            .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            // IChatSessionStore deve essere Singleton: mantiene le sessioni conversazionali
            // per tutta la vita dell'applicazione, condiviso tra richieste HTTP concorrenti.
            builder.Services.AddSingleton<nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.IChatSessionStore,
                                          nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.InMemoryChatSessionStore>();

            // IRouteProvider: auto-scopre tutte le implementazioni nell'assembly e le registra come Singleton.
            // Escluse dallo scanner Scoped sopra per evitare conflitti di lifetime con RouteRegistry Singleton.
            // Per aggiungere un nuovo modulo basta creare una nuova classe IRouteProvider — non serve toccare questo file.
            var routeProviderType = typeof(nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.IRouteProvider);
            var serviceAssembly   = System.Reflection.Assembly.GetAssembly(typeof(IServiceBase));
            if (serviceAssembly != null)
            {
                foreach (var impl in serviceAssembly.GetTypes()
                             .Where(t => t.IsClass && !t.IsAbstract && routeProviderType.IsAssignableFrom(t)))
                {
                    builder.Services.AddSingleton(routeProviderType, impl);
                }
            }
            builder.Services.AddSingleton<nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.IRouteRegistry,
                                          nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.RouteRegistry>();


            return builder.Services;
        }
        public static IServiceCollection InstallEntityContex(this WebApplicationBuilder builder)
        {
            // PublicDbContext (tabelle comuni) + un contesto per applicativo (search_path per azienda/applicativo)
            string connectionString = builder.Configuration.GetConnectionString("nvxappDbContext")
                                      ?? throw new InvalidOperationException("Connection string 'nvxappDbContext' non configurata.");
            builder.Services.AddNvxDataLayer(connectionString);

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
            .AddEntityFrameworkStores<PublicDbContext>()
            .AddDefaultTokenProviders();

            // chiavi DataProtection su DB (public."DataProtectionKeys"): senza, restano in memoria o nel profilo utente
            // e a ogni riavvio/riciclo/pubblicazione/redeploy i link di conferma email e reset password gia' inviati non valgono piu'.
            // ApplicationName fisso: di default dipende dalla cartella dell'app, che cambia tra IIS, Docker e sviluppo
            builder.Services.AddDataProtection()
                .SetApplicationName("nvxapp")
                .PersistKeysToDbContext<PublicDbContext>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<SignInManager<ApplicationUser>>();




            builder.Services.AddScoped<ILongJobNotifier, LongJobNotifier>();
            builder.Services.AddScoped<IJobNotifier, JobNotifier>();



            return builder.Services;
        }
        public static IServiceCollection InstallRepositories(this WebApplicationBuilder builder)
        {
            // solo i repository: i servizi di Infrastructure.Tenancy sono registrati da AddNvxDataLayer
            builder.Services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetAssembly(typeof(IRepository<>)))
                            .Where(type => type.Namespace != null && type.Namespace.StartsWith("nvxapp.server.data.Repositories"))
                            .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //builder.Services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();

            return builder.Services;
        }
     public static IServiceCollection InstallMappers(this WebApplicationBuilder builder)
  {

      builder.Services.AddAutoMapper(cfg =>
      {
          foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
              cfg.AddMaps(assembly);
      });

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
            if (LogOnFile != null)
            {
                bool writeToFile = Boolean.Parse(LogOnFile);

                if (writeToFile)
                {
                    // Path.Combine: il separatore "\" funziona solo su Windows (su Linux/Docker creerebbe un file chiamato "Logs\nvxapp.log");
                    // la cartella parte da ContentRootPath, non dalla cartella corrente del processo
                    string logFile = Path.Combine(builder.Environment.ContentRootPath, "Logs", "nvxapp.log");
                    loggerConfiguration = loggerConfiguration.WriteTo.File(logFile,
                                          rollOnFileSizeLimit: true,
                                          fileSizeLimitBytes: 10000000,
                                          retainedFileCountLimit: 200,
                                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

                }
            }



            string? LogOnDB = builder.Configuration["Logging:LogType:LogOnDB"];

            if (LogOnDB != null)
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


                    // TO DO nuova versione con colonne riorganizzate e su schema differente


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
        public static CultureInfo InstallCultureAndTimeZone(this WebApplicationBuilder builder)
        {
            // Cultura (formato di date e numeri) e fuso orario da configurazione (RunTime:Culture, RunTime:TimeZone)
            // invece che dalla macchina: un container Linux parte con cultura invariante e UTC.
            // Vuoti = impostazioni della macchina, come prima.
            CultureInfo culture = CultureInfo.CurrentCulture;
            string? cultureName = builder.Configuration["RunTime:Culture"];
            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
                // Default*: vale anche per i thread fuori dalle richieste HTTP (job in background, RabbitMQ, export)
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.CurrentCulture = culture;
            }

            string? timeZoneId = builder.Configuration["RunTime:TimeZone"];
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                // DateTime.Now usa il fuso del processo. Su Linux lo decide la variabile TZ (come TZ=Europe/Rome nel container);
                // su Windows e' quello della macchina e non si puo' cambiare: si segnala solo la differenza
                if (!OperatingSystem.IsWindows())
                {
                    Environment.SetEnvironmentVariable("TZ", timeZoneId);
                    TimeZoneInfo.ClearCachedData();
                }

                if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? configuredTimeZone))
                    Log.Warning("RunTime:TimeZone '{TimeZone}' non trovato: si usa il fuso della macchina ({Local})", timeZoneId, TimeZoneInfo.Local.Id);
                else if (!TimeZoneInfo.Local.HasSameRules(configuredTimeZone))
                    Log.Warning("Il fuso della macchina ({Local}) e' diverso da RunTime:TimeZone ({TimeZone}): DateTime.Now usa quello della macchina", TimeZoneInfo.Local.Id, timeZoneId);
            }

            Log.Information("Cultura {Culture}, fuso orario {TimeZone}", culture.Name, TimeZoneInfo.Local.Id);
            return culture;
        }
        public static bool InstallForwardedHeaders(this WebApplicationBuilder builder)
        {
            // Dietro un reverse proxy (nginx, Traefik, Caddy in Docker) che termina HTTPS: X-Forwarded-For e X-Forwarded-Proto
            // danno all'app l'IP reale del client e lo schema (https) originale.
            // Disattivato di default (ReverseProxy:Enabled): in debug e su IIS in-process non serve e non cambia nulla.
            bool enabled = false;
            bool.TryParse(builder.Configuration["ReverseProxy:Enabled"], out enabled);
            if (!enabled)
                return false;

            string[] knownProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
            string[] knownNetworks = builder.Configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>() ?? [];

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                // di default si accettano gli header solo da localhost: il proxy in un'altra macchina/container va dichiarato
                foreach (string proxy in knownProxies)
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                foreach (string network in knownNetworks)
                    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));

                if (knownProxies.Length == 0 && knownNetworks.Length == 0)
                {
                    // nessun proxy dichiarato: ci si fida di chiunque. Va bene solo se la porta dell'app NON e' raggiungibile
                    // se non dal proxy (es. rete interna di docker compose), altrimenti un client puo' falsificare IP e schema
                    options.KnownProxies.Clear();
                    options.KnownIPNetworks.Clear();
                }
            });

            if (knownProxies.Length == 0 && knownNetworks.Length == 0)
                Log.Warning("ReverseProxy attivo senza KnownProxies/KnownNetworks: X-Forwarded-* accettati da qualsiasi indirizzo");
            else
                Log.Information("ReverseProxy attivo, proxy fidati: {Proxies} {Networks}", knownProxies, knownNetworks);

            return true;
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

                                if (useSignalR)
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
        public const string CorsPolicyName = "ConfiguredOrigins";

        public static bool InstallCors(this WebApplicationBuilder builder)
        {
            // Origini a cui il browser permette di chiamare le API e l'hub SignalR da un altro indirizzo (Cors:AllowedOrigins).
            // Vuoto = nessuna (produzione: il client e' servito dal server stesso, stessa origine, CORS non serve).
            // "*" = qualsiasi origine (sviluppo con ng serve su un'altra porta).
            // AllowCredentials: il client SignalR la richiede; con le credenziali non si puo' usare AllowAnyOrigin
            string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (allowedOrigins.Length == 0)
            {
                Log.Information("CORS disattivato: API e SignalR accettano chiamate solo dalla stessa origine");
                return false;
            }

            bool anyOrigin = allowedOrigins.Contains("*");
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    if (anyOrigin)
                        policy.SetIsOriginAllowed(origin => true);
                    else
                        policy.WithOrigins(allowedOrigins);

                    policy.AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            if (anyOrigin)
                Log.Warning("CORS aperto a qualsiasi origine (Cors:AllowedOrigins = \"*\"): solo per lo sviluppo");
            else
                Log.Information("CORS: origini consentite {Origins}", allowedOrigins);

            return true;
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
