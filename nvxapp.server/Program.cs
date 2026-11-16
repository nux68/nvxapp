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
Installers4DocumentManagement.InstallServices(builder);
Installers4Crm.InstallServices(builder);
Installers4HumanResources.InstallServices(builder);
Installers4Compliance.InstallServices(builder);
Installers4Assets.InstallServices(builder);
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
// licenze: listini iniziali dichiarati dai rami (ILicenseCatalogSeed), creati una sola volta
await LicenseCatalogSeedRunner.RunAsync(app.Services);


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
// File con l'hash della build nel nome, generati da "ng build" con outputHashing=all: main-3CFXPD2I.js,
// polyfills-KX4REJ6Z.js, styles-HORTBXR3.css, chunk--FzSK5Ph.js, chunk-D_40ycs9.js, media/xxx-ABCD1234.png
// (8 caratteri tra lettere, cifre, '_' e '-' prima dell'estensione; i chunk possono avere un '-' come primo carattere)
var hashedBuildFile = new System.Text.RegularExpressions.Regex(@"-[A-Za-z0-9_-]{8}\.[A-Za-z0-9]+$",
    System.Text.RegularExpressions.RegexOptions.Compiled);
var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = provider
    ,
    OnPrepareResponse = ctx =>
    {
        var fileName = ctx.File.Name;
        var request = ctx.Context.Request;
        var headers = ctx.Context.Response.Headers;
        var path = request.Path.Value ?? string.Empty;
        // assets/** e svg/** sono copiati cosi' come sono (nome senza hash), anche se il nome somiglia a uno con hash
        var isCopiedAsset = path.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase) ||
                            path.StartsWith("/svg/", StringComparison.OrdinalIgnoreCase);

        // index.html e manifest: no-store, il browser li richiede sempre (index.html punta ai bundle della build corrente)
        if (fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("manifest.webmanifest", StringComparison.OrdinalIgnoreCase))
        {
            headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }
        // File con l'hash nel nome: il nome cambia a ogni build che ne cambia il contenuto, quindi la copia in cache
        // non diventa mai vecchia -> cache lunga (1 anno, immutable). Idem per un file chiesto con la versione della
        // build nell'URL (?v=<build>, es. le traduzioni: vedi i18n.providers.ts): a ogni pubblicazione cambia l'URL.
        else if ((!isCopiedAsset && hashedBuildFile.IsMatch(fileName)) || request.Query.ContainsKey("v"))
        {
            headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
        // Tutto il resto (assets/**, svg/**, .map, ...) ha lo stesso nome a ogni pubblicazione: no-cache = il browser
        // lo tiene ma a ogni uso chiede al server se e' cambiato (ETag/Last-Modified -> 304 se uguale).
        // Prima avevano anch'essi la cache lunga e il browser usava i file vecchi fino a un Ctrl+F5.
        else
        {
            headers["Cache-Control"] = "no-cache";
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
