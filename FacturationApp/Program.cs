using FacturationApp.Components;
using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Services.Interfaces;
using FacturationApp.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;
using FacturationApp.Auth;

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

// M4 — Fournisseurs et Bons de commande
builder.Services.AddScoped<IFournisseurService, FournisseurService>();
builder.Services.AddScoped<IBonCommandeService, BonCommandeService>();

// M5 — Authentification
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath       = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan  = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();