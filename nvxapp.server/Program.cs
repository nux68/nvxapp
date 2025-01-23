using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


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



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Aggiungi Swagger
builder.Services.AddSwaggerGen();


// Aggiungi i servizi CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Permette qualsiasi origine
              .AllowAnyMethod() // Permette qualsiasi metodo (GET, POST, PUT, DELETE, ecc.)
              .AllowAnyHeader(); // Permette qualsiasi intestazione
    });
});


Installers.InstallServices(builder);
Installers.InstallEntityContex(builder);
Installers.InstallRepositories(builder);
Installers.InstallMappers(builder);


var app = builder.Build();


//Attiva la migrazione
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
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
