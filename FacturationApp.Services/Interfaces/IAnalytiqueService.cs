using FacturationApp.Services.DTOs;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service analytique.
/// Toutes les méthodes ne portent que sur les factures Statut = "Validée".
/// </summary>
public interface IAnalytiqueService
{
    // ── TVA ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Retourne la TVA collectée groupée par taux (7%, 13%, 19%).
    /// </summary>
    Task<List<TVAParTauxDto>> GetTVAParTauxAsync();

    /// <summary>
    /// Retourne le montant total du timbre fiscal sur toutes les factures validées.
    /// </summary>
    Task<decimal> GetTotalTimbreAsync();

    // ── CHIFFRE D'AFFAIRES ────────────────────────────────────────────────

    /// <summary>
    /// Retourne le CA HT et TTC groupé par mois sur une plage de dates.
    /// </summary>
    Task<List<CAParMoisDto>> GetCAParPeriodeAsync(DateTime dateDebut, DateTime dateFin);

    /// <summary>
    /// Retourne le classement des N meilleurs clients par CA.
    /// </summary>
    Task<List<CAParClientDto>> GetCAParClientAsync(int topN = 5);

    /// <summary>
    /// Retourne le classement des N produits les plus vendus par CA HT.
    /// </summary>
    Task<List<CAParProduitDto>> GetCAParProduitAsync(int topN = 5);

    // ── KPI GLOBAUX ───────────────────────────────────────────────────────

    /// <summary>
    /// Retourne les indicateurs clés globaux pour les cards du dashboard.
    /// </summary>
    Task<KpiGlobalDto> GetKpiGlobalAsync();
}