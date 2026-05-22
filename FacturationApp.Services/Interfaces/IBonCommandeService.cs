// FacturationApp.Services/Interfaces/IBonCommandeService.cs
// M4 — Contrat du service BonCommande
// Miroir de IFactureService sans timbre fiscal.
// La validation d'un BC ajoute le stock (entrée marchandise fournisseur).

using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service BonCommande.
/// Gère la création, le calcul des totaux, la numérotation et la réception.
/// </summary>
public interface IBonCommandeService
{
    // ─── READ ─────────────────────────────────────────────────────────────────

    /// <summary>Retourne tous les BCs avec leur fournisseur, triés par date décroissante.</summary>
    Task<List<BonCommande>> GetAllAsync();

    /// <summary>Retourne un BC complet : Fournisseur + Lignes + Produits. Null si inexistant.</summary>
    Task<BonCommande?> GetByIdAsync(int id);

    // ─── CALCUL ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcule les montants de chaque ligne et retourne les totaux globaux.
    /// Utilise les champs SNAPSHOT — jamais les valeurs actuelles du produit.
    /// </summary>
    (decimal TotalHT, decimal TotalTVA, decimal TotalTTC) CalculerTotaux(List<LigneBonCommande> lignes);

    // ─── WRITE ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crée un BC en statut "Brouillon" avec ses lignes.
    /// Copie les snapshots depuis Produit pour chaque ligne.
    /// </summary>
    Task<BonCommande> CreateAsync(BonCommande bc, List<LigneBonCommande> lignes);

    /// <summary>
    /// Génère le prochain numéro BC au format BC-2026-0001.
    /// Lit BonCommandeCompteur depuis Parametre, incrémente, sauvegarde.
    /// </summary>
    Task<string> GenererNumeroAsync();

    /// <summary>
    /// Marque un BC comme "Reçu" — opération IRRÉVERSIBLE.
    /// Génère le numéro, incrémente le stock (StockQuantity += quantite),
    /// et enregistre un StockMouvement "ENTREE" pour chaque ligne.
    /// </summary>
    Task ValiderReceptionAsync(int id);

    /// <summary>
    /// Supprime un BC UNIQUEMENT s'il est en statut "Brouillon".
    /// Lève une exception si déjà "Reçu".
    /// </summary>
    Task DeleteAsync(int id);
}
