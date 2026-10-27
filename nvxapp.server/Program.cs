using Hangfire;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
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



Installers.InstallConfiguration(builder);
Installers4AttendanceTracking.InstallServices(builder);
Installers.InstallServices(builder);
Installers.InstallEntityContex(builder);
Installers.InstallRepositories(builder);
Installers.InstallMappers(builder);
Installers.InstallLog(builder);
// cultura e fuso orario da configurazione (RunTime:Culture, RunTime:TimeZone), non dalla macchina
CultureInfo serverCulture = Installers.InstallCultureAndTimeZone(builder);
// CORS: origini consentite da configurazione (Cors:AllowedOrigins); vuoto = solo stessa origine
Boolean useCors = Installers.InstallCors(builder);
Installers.InstallAuthentication(builder, useSignalR);
// dietro un reverse proxy (Docker): IP e schema originali da X-Forwarded-*; disattivato di default (ReverseProxy:Enabled)
Boolean useReverseProxy = Installers.InstallForwardedHeaders(builder);

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


// primo middleware: tutti quelli successivi (CORS, https, autenticazione, log) vedono IP e schema originali
if (useReverseProxy)
    app.UseForwardedHeaders();





// CORS (API e hub SignalR): solo per le origini di Cors:AllowedOrigins
if (useCors)
    app.UseCors(Installers.CorsPolicyName);




// Swagger / OpenAPI: solo con Swagger:Enabled (sviluppo). In produzione le API non vanno documentate
// pubblicamente; se serve su un server di test: Swagger__Enabled=true (e Swagger__RoutePrefix)
Boolean useSwagger = false;
bool.TryParse(builder.Configuration["Swagger:Enabled"], out useSwagger);
if (useSwagger)
{
    app.MapOpenApi();
    app.UseSwagger();
    string swaggerRoutePrefix = builder.Configuration["Swagger:RoutePrefix"] ?? "swagger";
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = swaggerRoutePrefix;
    });
}

//DISABLED HTTPS (disabilitato)
if (useHttps == true)
    app.UseHttpsRedirection();


// lingua dei messaggi per l'utente (UICulture) dall'header Accept-Language mandato dal client
// (o ?ui-culture= per SignalR); la formattazione di date e numeri (Culture) e' quella di RunTime:Culture
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(serverCulture, new CultureInfo("it")),
    SupportedCultures = new[] { serverCulture },
    SupportedUICultures = new[] { new CultureInfo("it"), new CultureInfo("en") },
    FallBackToParentUICultures = true,
});

app.UseAuthentication();
app.UseAuthorization();

// Middleware per i file statici
app.UseDefaultFiles(); // Serve automaticamente il file "index.html"

// Aggiunge il MIME type per il manifest PWA
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";
var staticFileOptions = new StaticFileOptions
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
};
app.UseStaticFiles(staticFileOptions);

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


// Fallback del client Angular: le sue rotte (es. /confirm-email e /reset-password dei link nelle email,
// o una pagina ricaricata con F5) non sono file e senza fallback il server risponde 404. Si risponde con
// index.html e la rotta la gestisce Angular. Esclusi le API, l'hub SignalR e i file (es. un .js mancante resta 404).
app.MapFallbackToFile("{*path:nonfile:regex(^(?!api/|chathub).*$)}", "index.html", staticFileOptions);

app.Run();
