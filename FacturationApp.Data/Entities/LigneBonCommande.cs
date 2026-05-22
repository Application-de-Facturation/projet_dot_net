// FacturationApp.Data/Entities/LigneBonCommande.cs
// M4 — Ligne d'un bon de commande fournisseur
// Miroir de LigneFacture.cs — applique le même patron SNAPSHOT.
//
// ══ CONCEPT CLÉ — SNAPSHOT ══
// Designation, PrixUnitaireHT et TauxTVA sont COPIÉS depuis Produit
// au moment de la création de la ligne.
// Si le prix d'un produit change après la commande, le BC reste intact.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;

public class LigneBonCommande
{
    // Clé primaire
    public int Id { get; set; }

    // Clé étrangère vers BonCommande — suppression en cascade
    [Required]
    public int BonCommandeId { get; set; }

    // Propriété de navigation vers le BC parent
    public BonCommande BonCommande { get; set; } = null!;

    // Clé étrangère vers Produit — Restrict pour conserver l'historique
    [Required]
    public int ProduitId { get; set; }

    // Propriété de navigation vers le produit
    public Produit Produit { get; set; } = null!;

    // ── SNAPSHOT — copiés depuis Produit à la création, jamais modifiés ──

    /// <summary>Snapshot : désignation du produit au moment du BC.</summary>
    [Required]
    [MaxLength(200)]
    public string Designation { get; set; } = string.Empty;

    /// <summary>Snapshot : prix unitaire HT au moment du BC.</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal PrixUnitaireHT { get; set; }

    /// <summary>Snapshot : taux de TVA au moment du BC.</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal TauxTVA { get; set; }

    // ── Quantité commandée ────────────────────────────────────────────────────
    [Required(ErrorMessage = "La quantité est obligatoire.")]
    [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
    public int Quantite { get; set; }

    // ── Montants calculés — decimal(18,3) = norme tunisienne ─────────────────

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
