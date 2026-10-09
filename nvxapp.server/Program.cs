using Hangfire;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.Utility;


var builder = WebApplication.CreateBuilder(args);


Boolean useHangFire = false;
string? sUseHangfire = builder.Configuration["Hangfire:UseHangfire"];
bool.TryParse(sUseHangfire, out useHangFire);

Boolean useSignalR = false;
string? sUseSignalR = builder.Configuration["SignalR:UseSignalR"];
bool.TryParse(sUseSignalR, out useSignalR);



Installers.InstallSettings(builder);

Installers.InstallRabbitMq(builder);


////////// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Aggiungi Swagger
builder.Services.AddSwaggerGen();

//Boolean useSignalR

// Aggiungi i servizi CORS
Installers.InstallCors(builder, useSignalR);


Installers.InstallConfiguration(builder);
Installers4AttendanceTracking.InstallServices(builder);
Installers.InstallServices(builder);
Installers.InstallEntityContex(builder);
Installers.InstallRepositories(builder);
Installers.InstallMappers(builder);
Installers.InstallLog(builder);
Installers.InstallAuthentication(builder, useSignalR);

//if(useSignalR)
builder.Services.AddSignalR();


if (useHangFire)
    Installers.InstallHangFire(builder);



Boolean useHttps = false;
string? sUseHttps = builder.Configuration["RunTime:UseHttps"];
bool.TryParse(sUseHttps, out useHttps);

int runTimePort = 0;
string? sRunTimePort = builder.Configuration["RunTime:Port"];
int.TryParse(sRunTimePort, out runTimePort);

//DISABLED HTTPS (aggiunto)
if (useHttps == false)
{
    builder.WebHost.ConfigureKestrel((context, serverOptions) =>
    {
        serverOptions.ListenAnyIP(runTimePort);
    });
}



var app = builder.Build();

// Inizializza la IServiceScopeFactory statica in ServiceBase per RunInBackground
nvxapp.server.Base.ServiceBase.InitScopeFactory(app.Services.GetRequiredService<IServiceScopeFactory>());


// Database: migrazione tabelle condivise, verifica/registrazione della modalita' multi-tenant
// (decisa al primo avvio e non piu' modificabile), migrazione tabelle tenant / schemi azienda.
Boolean configuredMultiTenant = false;
bool.TryParse(builder.Configuration["DbParameter:MultiTenant"], out configuredMultiTenant);
await DatabaseInitializer.InitializeAsync(app.Services, configuredMultiTenant);





if (useSignalR == false)
{
    //Usa la policy CORS globale
    app.UseCors("AllowAllOrigins");
}
else
{
    //SIGNALR (V2)
    app.UseDynamicCors(); // Usa il middleware personalizzato
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Configura l'uso di Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

    //////////////// Avvia automaticamente la pagina Swagger nel browser predefinito
    /////DISABLED HTTPS (disabilitato)
    var urlSw = "";
    if (useHttps)
        urlSw = "https://localhost:" + runTimePort.ToString() + "/swagger/index.html";
    else
        urlSw = "http://localhost:" + runTimePort.ToString() + "/swagger/index.html";

    //Process.Start(new ProcessStartInfo(urlSw) { UseShellExecute = true });

}
else if (app.Environment.IsProduction())
{
    // Configura l'uso di Swagger solo in produzione
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "docs"; // Configura un percorso diverso da "swagger" per maggiore sicurezza
    });
}

//DISABLED HTTPS (disabilitato)
if (useHttps == true)
    app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

// Middleware per i file statici
app.UseDefaultFiles(); // Serve automaticamente il file "index.html"

// Aggiunge il MIME type per il manifest PWA
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
    ,
    OnPrepareResponse = ctx =>
    {
        var fileName = ctx.File.Name;
        var headers = ctx.Context.Response.Headers;

        // index.html e manifest: no-cache ? il browser richiede sempre la versione aggiornata
        if (fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("manifest.webmanifest", StringComparison.OrdinalIgnoreCase))
        {
            headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }
        else
        {
            // Bundle JS/CSS con hash nel nome: cache lunga (1 anno)
            headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
    }
});

app.MapControllers();


if (useSignalR)
    Installers.InstallSignalRHub(app);


if (useHangFire)
{
    ////// Configura la dashboard di Hangfire
    app.UseHangfireDashboard();
    app.MapGet("/", () => "Hangfire è configurato!");
    // https://localhost:[runTimePort]/hangfire
}




app.Run();
