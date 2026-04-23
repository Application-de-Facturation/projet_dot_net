// FacturationApp/Program.cs

using FacturationApp.Components;
using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;                          // Pour AppDbContext
using FacturationApp.Services.Interfaces;           // Pour IClientService ← MANQUAIT
using FacturationApp.Services.Implementations;      // Pour ClientService  ← MANQUAIT

var builder = WebApplication.CreateBuilder(args);

// 
// 1. SERVICES BLAZOR
// 
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 
// 2. BASE DE DONNÉES
// 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 
// 3. SERVICES MÉTIER
// 

// M1 — Clients
builder.Services.AddScoped<IClientService, ClientService>();

// M2 ajoutera ici ses services (Produit, Facture, Parametre...)
// M3 ajoutera ici ses services (Analytique, Auth...)

//
// 4. API REST + SWAGGER  ← DOIT ÊTRE AVANT builder.Build()
//
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();   
builder.Services.AddSwaggerGen();             

// 
// BUILD 
// 
var app = builder.Build();

// 
// 5. SEED BASE DE DONNÉES
// 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// 
// 6. PIPELINE HTTP  =====> ordre important !!!!
// 


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Accessible sur /swagger
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();



// Routes API REST
app.MapControllers();           

// Routes Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();