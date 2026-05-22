// FacturationApp.Services/Interfaces/IFournisseurService.cs
// M4 — Contrat du service Fournisseur
// Miroir exact de IClientService : même signature, même logique.

using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Définit CE QUE le service peut réaliser — pas COMMENT il le fait.
/// Toutes les méthodes sont async (opérations BDD = I/O asynchrone).
/// </summary>
public interface IFournisseurService
{
    // ─── READ ─────────────────────────────────────────────────────────────────

    /// <summary>Retourne tous les fournisseurs actifs (IsDeleted = false).</summary>
    Task<List<Fournisseur>> GetAllAsync();

    /// <summary>Retourne un fournisseur par son Id. Null s'il n'existe pas.</summary>
    Task<Fournisseur?> GetByIdAsync(int id);

    /// <summary>Recherche dynamique sur Nom et MatriculeFiscal.</summary>
    Task<List<Fournisseur>> SearchAsync(string terme);

    // ─── WRITE ────────────────────────────────────────────────────────────────

    /// <summary>Crée un nouveau fournisseur. Vérifie l'unicité email et MF.</summary>
    Task<Fournisseur> CreateAsync(Fournisseur fournisseur);

    /// <summary>Met à jour un fournisseur existant. Vérifie l'unicité email et MF.</summary>
    Task UpdateAsync(Fournisseur fournisseur);

    /// <summary>Suppression LOGIQUE : IsDeleted = true.</summary>
    Task DeleteAsync(int id);
}
