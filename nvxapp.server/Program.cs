using nvxapp.server.Infrastructure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();


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

app.UseAuthorization();

// Middleware per i file statici
app.UseDefaultFiles(); // Serve automaticamente il file "index.html"
app.UseStaticFiles();

app.MapControllers();




app.Run();
