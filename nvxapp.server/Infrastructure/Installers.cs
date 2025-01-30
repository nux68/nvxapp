using nvxapp.server.service.Interfaces;
using System.Reflection;

using NetCore.AutoRegisterDi;
using nvxapp.server.data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Interfaces;
using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities;
using NpgsqlTypes;
using Serilog.Core;
using Serilog.Events;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using nvxapp.server.data.Migrations;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Infrastructure
{
    public static class Installers
    {

        public static IServiceCollection InstallConfiguration(this WebApplicationBuilder builder)
        {

            builder.Services.Configure<JwtParameter>(builder.Configuration.GetSection("JwtParameter"));
            //builder.Services.Configure<JwtParameter>(builder.Configuration.GetSection("JwtParameter"));

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

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseNpgsql(
                       builder.Configuration.GetConnectionString("nvxappDbContext"),
                       npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data") // Specifica l'assembly per le migrazioni
                   )
               );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Personalizza i requisiti della password
                options.Password.RequiredLength = 3;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;

                // Disattiva la validazione delle password
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;

               // options.User.AllowedUserNameCharacters = null;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

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

        //public static IServiceCollection InstallIdentity(this WebApplicationBuilder builder)
        //{

        //    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        //    {
        //        // Personalizza i requisiti della password
        //        options.Password.RequiredLength = 3;
        //        options.Password.RequireUppercase = false;
        //        options.Password.RequireLowercase = false;
        //        options.Password.RequireDigit = false;
        //        options.Password.RequireNonAlphanumeric = false;

        //        // Disattiva la validazione delle password
        //        options.Password.RequireDigit = false;
        //        options.Password.RequiredUniqueChars = 0;

        //        options.User.AllowedUserNameCharacters = null;

        //    }).AddEntityFrameworkStores<ApplicationDbContext>();

        //    builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

        //    return builder.Services;
        //}

    }
}
