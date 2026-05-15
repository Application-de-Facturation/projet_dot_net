using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service Produit.
/// Gère le CRUD complet des produits et la liste des catégories.
/// </summary>
public interface IProduitService
{
	// ─── READ ─────────────────────────────────────────────────────────────────

	/// <summary>Retourne tous les produits actifs avec leur catégorie.</summary>
	Task<List<Produit>> GetAllAsync();

	/// <summary>Retourne un produit par son Id avec sa catégorie. Null si inexistant.</summary>
	Task<Produit?> GetByIdAsync(int id);

	/// <summary>Retourne les produits actifs d'une catégorie donnée.</summary>
	Task<List<Produit>> GetByCategorieAsync(int categorieId);

	/// <summary>
	/// Recherche dynamique sur Designation.
	/// Si le terme est vide, retourne tous les produits actifs.
	/// </summary>
	Task<List<Produit>> SearchAsync(string terme);

	/// <summary>
	/// Retourne toutes les catégories.
	/// Utilisé pour alimenter les dropdowns dans les formulaires.
	/// </summary>
	Task<List<Categorie>> GetAllCategoriesAsync();

	// ─── WRITE ────────────────────────────────────────────────────────────────

	/// <summary>Crée un nouveau produit en base.</summary>
	Task<Produit> CreateAsync(Produit produit);

	/// <summary>Met à jour un produit existant.</summary>
	Task UpdateAsync(Produit produit);

	/// <summary>
	/// Suppression logique : IsDeleted = true.
	/// Le produit reste en base pour conserver l'historique des factures.
	/// </summary>
	Task DeleteAsync(int id);
}