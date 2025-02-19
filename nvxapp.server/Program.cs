using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.Infrastructure;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);


//

Installers.InstallSettings(builder);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Aggiungi Swagger
builder.Services.AddSwaggerGen();


// Aggiungi i servizi CORS



Installers.InstallCors(builder);


Installers.InstallConfiguration(builder);
Installers.InstallServices(builder);
Installers.InstallEntityContex(builder);
Installers.InstallRepositories(builder);
Installers.InstallMappers(builder);
Installers.InstallLog(builder);
Installers.InstallAuthentication(builder);




var app = builder.Build();





using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var factory = services.GetRequiredService<IApplicationDbContextFactory>();

    // Lista degli schemi dei tenant (puoi sostituirla con una query al DB)
    string[] schemiClienti = { "ScHeMa_A" };

    SharedSchema.MigrazioneRunTime = true;

    var _configuration = app.Services.GetRequiredService<IConfiguration>();
    Boolean MultiTenant = false;
    string? sMultiTenant = _configuration["DbParameter:MultiTenant"];

    bool.TryParse(sMultiTenant, out MultiTenant);
    SharedSchema.MultiTenant = MultiTenant;

    // Esegui la migrazione sullo schema di default (public)
    using (var context = factory.CreateDbContext("public"))
    {
        context.Database.Migrate();
    }

    // Esegui la migrazione per ogni schema tenant
    if (MultiTenant)
    {
        foreach (var schema in schemiClienti)
        {
            using (var context = factory.CreateDbContext(schema.ToLower()))
            {
                //context.Database.ExecuteSqlRaw($"CREATE SCHEMA IF NOT EXISTS \"{schema.ToLower()}\";");
                ////EnsureSchemaExists(context, schema); // Assicura che lo schema esista
                context.Database.Migrate();
            }
        }
    }
   
}





// Usa la policy CORS globale
app.UseCors("AllowAllOrigins");



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Configura l'uso di Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

    //////// Avvia automaticamente la pagina Swagger nel browser predefinito
    //////var urlSw = "https://localhost:7146/swagger/index.html";
    //////Process.Start(new ProcessStartInfo(urlSw) { UseShellExecute = true });

}

app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();

// Middleware per i file statici
app.UseDefaultFiles(); // Serve automaticamente il file "index.html"
app.UseStaticFiles();

app.MapControllers();




app.Run();
