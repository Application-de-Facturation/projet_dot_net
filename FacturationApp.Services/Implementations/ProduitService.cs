using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service Produit.
/// Toutes les méthodes utilisent async/await + EF Core.
/// La suppression est toujours logique (IsDeleted = true).
/// </summary>
public class ProduitService : IProduitService
{
	private readonly AppDbContext _context;

	public ProduitService(AppDbContext context)
	{
		_context = context;
	}

	// ─── READ ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Retourne tous les produits actifs avec leur catégorie.
	/// Include(Categorie) évite le problème de lazy loading.
	/// </summary>
	public async Task<List<Produit>> GetAllAsync()
	{
		return await _context.Produits
			.Where(p => !p.IsDeleted)
			.Include(p => p.Categorie)
			.OrderBy(p => p.Designation)
			.ToListAsync();
	}

	/// <summary>
	/// Retourne un produit par son Id avec sa catégorie.
	/// Retourne null si le produit n'existe pas ou est supprimé.
	/// </summary>
	public async Task<Produit?> GetByIdAsync(int id)
	{
		return await _context.Produits
			.Where(p => p.Id == id && !p.IsDeleted)
			.Include(p => p.Categorie)
			.FirstOrDefaultAsync();
	}

	/// <summary>
	/// Retourne les produits actifs d'une catégorie donnée.
	/// Utilisé pour filtrer les produits par catégorie dans l'UI.
	/// </summary>
	public async Task<List<Produit>> GetByCategorieAsync(int categorieId)
	{
		return await _context.Produits
			.Where(p => !p.IsDeleted && p.CategorieId == categorieId)
			.Include(p => p.Categorie)
			.OrderBy(p => p.Designation)
			.ToListAsync();
	}

	/// <summary>
	/// Recherche dynamique sur la Designation.
	/// LINQ traduit Contains() en SQL LIKE '%terme%'.
	/// Si le terme est vide, retourne tous les produits actifs.
	/// </summary>
	public async Task<List<Produit>> SearchAsync(string terme)
	{
		if (string.IsNullOrWhiteSpace(terme))
			return await GetAllAsync();

		var termeNormalise = terme.Trim().ToLower();

		return await _context.Produits
			.Where(p => !p.IsDeleted &&
						p.Designation.ToLower().Contains(termeNormalise))
			.Include(p => p.Categorie)
			.OrderBy(p => p.Designation)
			.ToListAsync();
	}

	/// <summary>
	/// Retourne toutes les catégories triées par nom.
	/// Utilisé pour alimenter les dropdowns dans Produits/Form.razor.
	/// </summary>
	public async Task<List<Categorie>> GetAllCategoriesAsync()
	{
		return await _context.Categories
			.OrderBy(c => c.Nom)
			.ToListAsync();
	}

	// ─── WRITE ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Crée un nouveau produit en base.
	/// IsDeleted et CreatedAt sont forcés ici pour éviter toute manipulation externe.
	/// </summary>
	public async Task<Produit> CreateAsync(Produit produit)
	{
		produit.CreatedAt = DateTime.UtcNow;
		produit.IsDeleted = false;

		_context.Produits.Add(produit);
		await _context.SaveChangesAsync();
		return produit;
	}

	/// <summary>
	/// Met à jour un produit existant.
	/// ATTENTION : modifier le prix d'un produit n'affecte pas les factures existantes
	/// car celles-ci utilisent les snapshots dans LigneFacture.
	/// </summary>
	public async Task UpdateAsync(Produit produit)
	{
		_context.Produits.Update(produit);
		await _context.SaveChangesAsync();
	}

	/// <summary>
	/// Suppression LOGIQUE : IsDeleted = true.
	/// Le produit reste en base pour conserver l'historique des factures
	/// qui référencent ce produit via LigneFacture.ProduitId.
	/// </summary>
	public async Task DeleteAsync(int id)
	{
		var produit = await _context.Produits.FindAsync(id);

		if (produit is null)
			return; // déjà supprimé ou inexistant — on ignore silencieusement

		produit.IsDeleted = true;
		_context.Produits.Update(produit);
		await _context.SaveChangesAsync();
	}
}