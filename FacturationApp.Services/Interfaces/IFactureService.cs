using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service Facture — cœur métier de l'application.
/// Gère la création, le calcul des totaux, la numérotation et la validation.
/// </summary>
public interface IFactureService
{
	// ─── READ ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Retourne toutes les factures avec leur client.
	/// Utilisé dans Factures/Index.razor.
	/// </summary>
	Task<List<Facture>> GetAllAsync();

	/// <summary>
	/// Retourne une facture complète : Client + Lignes + Produits.
	/// Utilisé dans Factures/Detail.razor.
	/// Retourne null si inexistante.
	/// </summary>
	Task<Facture?> GetByIdAsync(int id);

	// ─── CALCUL ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Calcule et met à jour MontantHT, MontantTVA, MontantTTC sur chaque ligne.
	/// Retourne un tuple avec les totaux globaux de la facture.
	/// Appelé à chaque modification dans Factures/Create.razor (recalcul temps réel).
	/// 
	/// Formules par ligne :
	///   MontantHT  = Quantite × PrixUnitaireHT (snapshot)
	///   MontantTVA = MontantHT × TauxTVA / 100
	///   MontantTTC = MontantHT + MontantTVA
	/// </summary>
	(decimal TotalHT, decimal TotalTVA, decimal TotalTTC) CalculerTotaux(List<LigneFacture> lignes);

	// ─── WRITE ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Crée une facture en statut "Brouillon" avec ses lignes.
	/// Pour chaque ligne : copie les snapshots depuis Produit (Designation, PrixUnitaireHT, TauxTVA)
	/// puis calcule MontantHT, MontantTVA, MontantTTC.
	/// </summary>
	Task<Facture> CreateAsync(Facture facture, List<LigneFacture> lignes);

	Task UpdateAsync(int factureId, int clientId, List<LigneFacture> lignes);

	/// <summary>
	/// Génère le prochain numéro de facture au format FAC-2025-0001.
	/// Lit FactureCompteur depuis Parametre, incrémente, sauvegarde, retourne le numéro.
	/// </summary>
	Task<string> GenererNumeroAsync();

	/// <summary>
	/// Valide une facture (Brouillon → Validée) — opération IRRÉVERSIBLE.
	/// Étapes :
	///   1. Vérifie que la facture est en Brouillon
	///   2. Génère et assigne le numéro définitif
	///   3. Lit le timbre fiscal depuis Parametre (jamais codé en dur)
	///   4. Calcule NetAPayer = TotalTTC + Timbre
	///   5. Passe Statut à "Validée" + enregistre DateValidation
	///   6. Sauvegarde en base
	/// </summary>
	Task ValiderFactureAsync(int id);

	/// <summary>
	/// Supprime une facture UNIQUEMENT si elle est en statut "Brouillon".
	/// Lève une exception si la facture est déjà validée — elle est immuable.
	/// </summary>
	Task DeleteAsync(int id);
}