using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service Facture — cœur métier de l'application.
/// Dépend de IParametreService pour lire le timbre fiscal et le compteur.
/// </summary>
public class FactureService : IFactureService
{
	private readonly AppDbContext _context;
	private readonly IParametreService _parametreService;

	// Double injection : DbContext + ParametreService
	public FactureService(AppDbContext context, IParametreService parametreService)
	{
		_context = context;
		_parametreService = parametreService;
	}

	// ─── READ ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Retourne toutes les factures avec leur client.
	/// Triées par date décroissante (plus récentes en premier).
	/// </summary>
	public async Task<List<Facture>> GetAllAsync()
	{
		return await _context.Factures
			.Include(f => f.Client)
			.OrderByDescending(f => f.DateCreation)
			.ToListAsync();
	}

	/// <summary>
	/// Retourne une facture complète avec Client + toutes ses Lignes + Produits.
	/// Include imbriqué : Facture → Lignes → Produit (pour afficher les détails).
	/// </summary>
	public async Task<Facture?> GetByIdAsync(int id)
	{
		return await _context.Factures
			.Include(f => f.Client)
			.Include(f => f.Lignes)
				.ThenInclude(l => l.Produit)
			.FirstOrDefaultAsync(f => f.Id == id);
	}

	// ─── CALCUL ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Calcule les montants de chaque ligne et retourne les totaux globaux.
	/// 
	/// IMPORTANT : utilise les champs SNAPSHOT de chaque ligne
	/// (PrixUnitaireHT, TauxTVA) — jamais les valeurs actuelles du produit.
	/// 
	/// Formules :
	///   MontantHT  = Quantite × PrixUnitaireHT
	///   MontantTVA = MontantHT × TauxTVA / 100
	///   MontantTTC = MontantHT + MontantTVA
	/// </summary>
	public (decimal TotalHT, decimal TotalTVA, decimal TotalTTC) CalculerTotaux(List<LigneFacture> lignes)
	{
		foreach (var ligne in lignes)
		{
			ligne.MontantHT = ligne.Quantite * ligne.PrixUnitaireHT;
			ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
			ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
		}

		var totalHT = lignes.Sum(l => l.MontantHT);
		var totalTVA = lignes.Sum(l => l.MontantTVA);
		var totalTTC = lignes.Sum(l => l.MontantTTC);

		return (totalHT, totalTVA, totalTTC);
	}

	// ─── WRITE ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Crée une facture en statut "Brouillon" avec ses lignes.
	/// 
	/// Pour chaque ligne, copie les snapshots depuis le Produit en base :
	///   - Designation    → snapshot de la désignation actuelle
	///   - PrixUnitaireHT → snapshot du prix actuel
	///   - TauxTVA        → snapshot du taux actuel
	/// 
	/// Puis calcule MontantHT, MontantTVA, MontantTTC sur chaque ligne.
	/// Calcule et stocke TotalHT, TotalTVA, TotalTTC sur la facture.
	/// </summary>
	public async Task<Facture> CreateAsync(Facture facture, List<LigneFacture> lignes)
	{
		// Initialisation de la facture
		facture.Statut = "Brouillon";
		facture.DateCreation = DateTime.UtcNow;
		facture.Numero = string.Empty; // Le numéro est généré à la validation

		// Pour chaque ligne : copier les snapshots depuis la base
		foreach (var ligne in lignes)
		{
			// Charger le produit depuis la base — on ne fait jamais confiance aux valeurs UI
			var produit = await _context.Produits.FindAsync(ligne.ProduitId)
				?? throw new InvalidOperationException(
					$"Produit Id={ligne.ProduitId} introuvable. Impossible de créer la ligne.");

			// ── SNAPSHOT — copie des valeurs au moment de la facture ──
			ligne.Designation = produit.Designation;
			ligne.PrixUnitaireHT = produit.PrixUnitaireHT;
			ligne.TauxTVA = produit.TauxTVA;

			// Calcul des montants de la ligne
			ligne.MontantHT = ligne.Quantite * ligne.PrixUnitaireHT;
			ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
			ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
		}

		// Calcul des totaux de la facture
		facture.TotalHT = lignes.Sum(l => l.MontantHT);
		facture.TotalTVA = lignes.Sum(l => l.MontantTVA);
		facture.TotalTTC = lignes.Sum(l => l.MontantTTC);

		// Le timbre et NetAPayer sont calculés à la validation — pas à la création
		facture.Timbre = 0;
		facture.NetAPayer = 0;

		// Associer les lignes à la facture
		facture.Lignes = lignes;

		_context.Factures.Add(facture);
		await _context.SaveChangesAsync();

		return facture;
	}

	/// <summary>
	/// Génère le prochain numéro de facture au format FAC-2025-0001.
	/// 
	/// Étapes :
	///   1. Lit "FactureCompteur" depuis Parametre
	///   2. Incrémente le compteur
	///   3. Sauvegarde le nouveau compteur en base
	///   4. Retourne le numéro formaté
	/// </summary>
	public async Task<string> GenererNumeroAsync()
	{
		// Lire le compteur actuel
		var valeurCompteur = await _parametreService.GetValeurAsync("FactureCompteur");

		if (!int.TryParse(valeurCompteur, out var compteur))
			throw new InvalidOperationException(
				"Paramètre 'FactureCompteur' invalide ou absent. Vérifiez le seed data.");

		// Incrémenter
		compteur++;

		// Sauvegarder le nouveau compteur
		await _parametreService.SetValeurAsync("FactureCompteur", compteur.ToString());

		// Retourner le numéro formaté : FAC-2025-0001
		var prefixe = await _parametreService.GetValeurAsync("FacturePrefixe") ?? "FAC";
		var annee = DateTime.Now.Year;

		return $"{prefixe}-{annee}-{compteur:D4}";
	}

	/// <summary>
	/// Valide une facture — opération IRRÉVERSIBLE.
	/// Une facture validée ne peut plus être modifiée ni supprimée.
	/// 
	/// Étapes :
	///   1. Vérifie que la facture existe et est en "Brouillon"
	///   2. Génère et assigne le numéro définitif (FAC-2025-NNNN)
	///   3. Lit le timbre fiscal depuis Parametre (jamais codé en dur)
	///   4. Calcule NetAPayer = TotalTTC + Timbre
	///   5. Passe Statut à "Validée" + enregistre DateValidation
	///   6. Sauvegarde en base
	/// </summary>
	public async Task ValiderFactureAsync(int id)
	{
		var facture = await _context.Factures
			.Include(f => f.Lignes)
			.FirstOrDefaultAsync(f => f.Id == id)
			?? throw new InvalidOperationException($"Facture Id={id} introuvable.");

		// Règle métier : seul un brouillon peut être validé
		if (facture.Statut != "Brouillon")
			throw new InvalidOperationException(
				$"La facture '{facture.Numero}' est déjà validée. Opération impossible.");

		// Étape 1 — Numéro définitif
		facture.Numero = await GenererNumeroAsync();

		// Étape 2 — Timbre fiscal lu dynamiquement (jamais codé en dur)
		facture.Timbre = await _parametreService.GetTimbreFiscalAsync();

		// Étape 3 — Recalcul des totaux (sécurité : on recalcule avant validation)
		var (totalHT, totalTVA, totalTTC) = CalculerTotaux(facture.Lignes.ToList());
		facture.TotalHT = totalHT;
		facture.TotalTVA = totalTVA;
		facture.TotalTTC = totalTTC;

		// Étape 3.5 — Vérifier les stocks pour chaque ligne avant de valider
		var insufficient = new List<string>();
		foreach (var ligne in facture.Lignes)
		{
			var produit = await _context.Produits.FindAsync(ligne.ProduitId)
				?? throw new InvalidOperationException($"Produit Id={ligne.ProduitId} introuvable.");

			if (produit.StockQuantity < ligne.Quantite)
			{
				insufficient.Add($"{produit.Designation} (disponible: {produit.StockQuantity}, demandé: {ligne.Quantite})");
			}
		}

		if (insufficient.Any())
		{
			throw new InvalidOperationException($"Stock insuffisant pour : {string.Join(", ", insufficient)}");
		}

		// Étape 4 — Net à payer
		facture.NetAPayer = facture.TotalTTC + facture.Timbre;

      // Étape 4.5 — Décrémenter le stock et enregistrer les mouvements
		foreach (var ligne in facture.Lignes)
		{
			var produit = await _context.Produits.FindAsync(ligne.ProduitId)
				?? throw new InvalidOperationException($"Produit Id={ligne.ProduitId} introuvable.");

			produit.StockQuantity -= ligne.Quantite;
			_context.Produits.Update(produit);

			// Enregistrer le mouvement de stock
			var mouvement = new StockMouvement
			{
				ProduitId = produit.Id,
				Quantite = ligne.Quantite,
				Type = "SORTIE",
				Commentaire = $"Facture {facture.Numero}",
				Date = DateTime.UtcNow
			};

			_context.StockMouvements.Add(mouvement);
		}

		// Étape 5 — Passage au statut Validée
		facture.Statut = "Validée";
		facture.DateValidation = DateTime.UtcNow;

		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(int factureId, int clientId, List<LigneFacture> lignes)
	{
		var facture = await _context.Factures
			.Include(f => f.Lignes)
			.FirstOrDefaultAsync(f => f.Id == factureId)
			?? throw new InvalidOperationException($"Facture Id={factureId} introuvable.");

		if (facture.Statut != "Brouillon")
			throw new InvalidOperationException(
				"Impossible de modifier une facture validée — elle est immuable.");

		// Mettre à jour le client
		facture.ClientId = clientId;

		// Supprimer toutes les anciennes lignes
		_context.LignesFacture.RemoveRange(facture.Lignes);

		// Recréer les nouvelles lignes avec snapshots depuis la base
		var nouvLignes = new List<LigneFacture>();
		foreach (var ligne in lignes)
		{
			var produit = await _context.Produits.FindAsync(ligne.ProduitId)
				?? throw new InvalidOperationException(
					$"Produit Id={ligne.ProduitId} introuvable.");

			var nouvLigne = new LigneFacture
			{
				FactureId = factureId,
				ProduitId = produit.Id,
				Designation = produit.Designation,
				PrixUnitaireHT = produit.PrixUnitaireHT,
				TauxTVA = produit.TauxTVA,
				Quantite = ligne.Quantite
			};

			nouvLigne.MontantHT = nouvLigne.Quantite * nouvLigne.PrixUnitaireHT;
			nouvLigne.MontantTVA = nouvLigne.MontantHT * nouvLigne.TauxTVA / 100;
			nouvLigne.MontantTTC = nouvLigne.MontantHT + nouvLigne.MontantTVA;

			nouvLignes.Add(nouvLigne);
		}

		// Recalculer les totaux de la facture
		facture.TotalHT = nouvLignes.Sum(l => l.MontantHT);
		facture.TotalTVA = nouvLignes.Sum(l => l.MontantTVA);
		facture.TotalTTC = nouvLignes.Sum(l => l.MontantTTC);

		_context.LignesFacture.AddRange(nouvLignes);
		await _context.SaveChangesAsync();
	}

	/// <summary>
	/// Supprime une facture UNIQUEMENT si elle est en statut "Brouillon".
	/// Lève une exception si la facture est validée — règle métier absolue.
	/// </summary>
	public async Task DeleteAsync(int id)
	{
		var facture = await _context.Factures.FindAsync(id)
			?? throw new InvalidOperationException($"Facture Id={id} introuvable.");

		if (facture.Statut == "Validée")
			throw new InvalidOperationException(
				"Impossible de supprimer une facture validée. Elle est immuable.");

		_context.Factures.Remove(facture);
		await _context.SaveChangesAsync();
	}
}