using FacturationApp.Components;
using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Services.Interfaces;
using FacturationApp.Services.Implementations;

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

// Factory pour AnalytiqueService (contexte indépendant par appel)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString), ServiceLifetime.Scoped);

// 
// 3. SERVICES MÉTIER
// 

// M1 — Clients
builder.Services.AddScoped<IClientService, ClientService>();

// M2 — Produits, Factures, Paramètres, PDF
builder.Services.AddScoped<IParametreService, ParametreService>();
builder.Services.AddScoped<IProduitService, ProduitService>();
builder.Services.AddScoped<IFactureService, FactureService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// M3 — Analytique
builder.Services.AddScoped<IAnalytiqueService, AnalytiqueService>();

//
// 4. API REST + SWAGGER
//
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 
// BUILD 
// 
var app = builder.Build();

// 
// 5. MIGRATION BASE DE DONNÉES
// 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// 
// 6. PIPELINE HTTP
// 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();