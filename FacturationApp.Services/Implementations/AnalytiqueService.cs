using FacturationApp.Data;
using FacturationApp.Services.DTOs;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service analytique.
/// Les méthodes seront complétées quand M2 livrera ses entités
/// (Facture, LigneFacture, Parametre).
/// </summary>
public class AnalytiqueService : IAnalytiqueService
{
    private readonly AppDbContext _context;

    public AnalytiqueService(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<TVAParTauxDto>> GetTVAParTauxAsync()
        => throw new NotImplementedException("En attente des entités M2 (LigneFacture).");

    public Task<decimal> GetTotalTimbreAsync()
        => throw new NotImplementedException("En attente des entités M2 (Facture).");

    public Task<List<CAParMoisDto>> GetCAParPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        => throw new NotImplementedException("En attente des entités M2 (Facture).");

    public Task<List<CAParClientDto>> GetCAParClientAsync(int topN = 5)
        => throw new NotImplementedException("En attente des entités M2 (Facture, Client).");

    public Task<List<CAParProduitDto>> GetCAParProduitAsync(int topN = 5)
        => throw new NotImplementedException("En attente des entités M2 (LigneFacture, Produit).");

    public Task<KpiGlobalDto> GetKpiGlobalAsync()
        => throw new NotImplementedException("En attente des entités M2 (Facture, LigneFacture).");
}