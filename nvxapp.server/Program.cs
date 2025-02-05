using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.Infrastructure;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



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


//Attiva la migrazione
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    var context = services.GetRequiredService<ApplicationDbContext>();
//    context.Database.Migrate();
//}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    var factory = services.GetRequiredService<IApplicationDbContextFactory>();

    // Lista degli schemi dei tenant (puoi sostituirla con una query al DB)
    string[] schemiClienti = { "AAA" };

    // Esegui la migrazione sullo schema di default (public)
    using (var context = factory.CreateDbContext("public"))
    {
        context.Database.Migrate();
    }

    // Esegui la migrazione per ogni schema tenant
    foreach (var schema in schemiClienti)
    {
        using (var context = factory.CreateDbContext(schema))
        {
            context.Database.ExecuteSqlRaw($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";");
            //EnsureSchemaExists(context, schema); // Assicura che lo schema esista
            context.Database.Migrate();
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

    //// Avvia automaticamente la pagina Swagger nel browser predefinito
    //var urlSw = "https://localhost:7146/swagger/index.html";
    //Process.Start(new ProcessStartInfo(urlSw) { UseShellExecute = true });

}

app.UseHttpsRedirection();




app.UseAuthentication();
app.UseAuthorization();

// Middleware per i file statici
app.UseDefaultFiles(); // Serve automaticamente il file "index.html"
app.UseStaticFiles();

app.MapControllers();




app.Run();
