// FacturationApp.Data/Entities/LigneFacture.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;

/// <summary>
/// Représente une ligne d'une facture (un produit + quantité + montants calculés).
/// 
/// ══ CONCEPT CLÉ — SNAPSHOT ══
/// Les champs Designation, PrixUnitaireHT et TauxTVA sont COPIÉS depuis Produit
/// au moment de la création de la ligne. Ils ne sont PAS des clés étrangères vers
/// les colonnes de Produit — ce sont des valeurs figées dans le temps.
/// 
/// Pourquoi ? Si le prix d'un produit est modifié après la création de la facture,
/// la facture doit rester intacte avec les valeurs de l'époque.
/// 
/// Les calculs utilisent TOUJOURS les champs snapshot, jamais Produit.PrixUnitaireHT.
/// </summary>
public class LigneFacture
{
	//Clé primaire
	public int Id { get; set; }

	//Clé étrangère vers Facture
	[Required]
	public int FactureId { get; set; }

	/// <summary>Propriété de navigation vers la facture parente.</summary>
	public Facture Facture { get; set; } = null!;

	//  Clé étrangère vers Produit
	/// <summary>
	/// Référence vers le produit d'origine — utile pour les requêtes analytiques
	/// de M3 (classement des produits les plus vendus).
	/// </summary>
	[Required]
	public int ProduitId { get; set; }

	/// <summary>Propriété de navigation vers le produit.</summary>
	public Produit Produit { get; set; } = null!;

	// copiés depuis Produit à la création, jamais modifiés

	/// <summary>Snapshot : désignation du produit au moment de la facture.</summary>
	[Required]
	[MaxLength(200)]
	public string Designation { get; set; } = string.Empty;

	/// <summary>Snapshot : prix unitaire HT au moment de la facture.</summary>
	[Column(TypeName = "decimal(18,3)")]
	public decimal PrixUnitaireHT { get; set; }

	/// <summary>Snapshot : taux de TVA au moment de la facture (0, 7, 13 ou 19).</summary>
	[Column(TypeName = "decimal(5,2)")]
	public decimal TauxTVA { get; set; }

	// ── Quantité ──────────────────────────────────────────────────────────────
	[Required(ErrorMessage = "La quantité est obligatoire.")]
	[Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
	public int Quantite { get; set; }

	// ── Montants calculés ── decimal(18,3) = norme tunisienne ─────────────────

	/// <summary>MontantHT = Quantite × PrixUnitaireHT (snapshot)</summary>
	[Column(TypeName = "decimal(18,3)")]
	public decimal MontantHT { get; set; }

	/// <summary>MontantTVA = MontantHT × TauxTVA / 100</summary>
	[Column(TypeName = "decimal(18,3)")]
	public decimal MontantTVA { get; set; }

	/// <summary>MontantTTC = MontantHT + MontantTVA</summary>
	[Column(TypeName = "decimal(18,3)")]
	public decimal MontantTTC { get; set; }
}