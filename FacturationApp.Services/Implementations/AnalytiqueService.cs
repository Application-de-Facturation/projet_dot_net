using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Services.DTOs;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

public class AnalytiqueService : IAnalytiqueService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AnalytiqueService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ── TVA par taux ──────────────────────────────────────────────────────
    public async Task<List<TVAParTauxDto>> GetTVAParTauxAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.LignesFacture
            .Where(l => l.Facture.Statut == "Validée")
            .GroupBy(l => l.TauxTVA)
            .Select(g => new TVAParTauxDto
            {
                Taux       = g.Key,
                MontantTVA = g.Sum(l => l.MontantTVA)
            })
            .OrderBy(x => x.Taux)
            .ToListAsync();
    }

    // ── Total timbre ──────────────────────────────────────────────────────
    public async Task<decimal> GetTotalTimbreAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.Factures
            .Where(f => f.Statut == "Validée")
            .SumAsync(f => f.Timbre);
    }

    // ── CA par période ────────────────────────────────────────────────────
    public async Task<List<CAParMoisDto>> GetCAParPeriodeAsync(DateTime dateDebut, DateTime dateFin)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var data = await ctx.Factures
            .Where(f => f.Statut == "Validée"
                     && f.DateCreation >= dateDebut
                     && f.DateCreation <= dateFin)
            .GroupBy(f => new { f.DateCreation.Year, f.DateCreation.Month })
            .Select(g => new CAParMoisDto
            {
                Annee      = g.Key.Year,
                Mois       = g.Key.Month,
                MontantHT  = g.Sum(f => f.TotalHT),
                MontantTTC = g.Sum(f => f.TotalTTC)
            })
            .OrderBy(x => x.Annee).ThenBy(x => x.Mois)
            .ToListAsync();

        foreach (var item in data)
        {
            item.LibelleMois = new DateTime(item.Annee, item.Mois, 1)
                .ToString("MMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
        }

        return data;
    }

    // ── Top clients ───────────────────────────────────────────────────────
    public async Task<List<CAParClientDto>> GetCAParClientAsync(int topN = 5)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.Factures
            .Where(f => f.Statut == "Validée")
            .GroupBy(f => new { f.ClientId, f.Client.Nom })
            .Select(g => new CAParClientDto
            {
                ClientId       = g.Key.ClientId,
                NomClient      = g.Key.Nom,
                TotalHT        = g.Sum(f => f.TotalHT),
                NombreFactures = g.Count()
            })
            .OrderByDescending(x => x.TotalHT)
            .Take(topN)
            .ToListAsync();
    }

    // ── Top produits ──────────────────────────────────────────────────────
    public async Task<List<CAParProduitDto>> GetCAParProduitAsync(int topN = 5)
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        return await ctx.LignesFacture
            .Where(l => l.Facture.Statut == "Validée")
            .GroupBy(l => new { l.ProduitId, l.Designation })
            .Select(g => new CAParProduitDto
            {
                ProduitId      = g.Key.ProduitId,
                Designation    = g.Key.Designation,
                TotalHT        = g.Sum(l => l.MontantHT),
                QuantiteTotale = g.Sum(l => l.Quantite)
            })
            .OrderByDescending(x => x.TotalHT)
            .Take(topN)
            .ToListAsync();
    }

    // ── KPI globaux ───────────────────────────────────────────────────────
    public async Task<KpiGlobalDto> GetKpiGlobalAsync()
    {
        await using var ctx = await _contextFactory.CreateDbContextAsync();
        var factures = await ctx.Factures
            .Where(f => f.Statut == "Validée")
            .ToListAsync();

        return new KpiGlobalDto
        {
            CATotalHT              = factures.Sum(f => f.TotalHT),
            CATotalTTC             = factures.Sum(f => f.TotalTTC),
            NombreFacturesValidees = factures.Count,
            TVATotale              = factures.Sum(f => f.TotalTVA),
            TimbreTotal            = factures.Sum(f => f.Timbre)
        };
    }
}