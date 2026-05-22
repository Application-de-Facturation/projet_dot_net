// FacturationApp.Data/Entities/BonCommande.cs
// M4 — Bon de commande fournisseur
// Miroir de Facture.cs sans timbre fiscal (les BCs ne sont pas soumis au timbre).
// Statuts : "Brouillon" → "Reçu" (irréversible, déclenche l'entrée en stock).

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;

public class BonCommande
{
    // Clé primaire
    public int Id { get; set; }

    // Numéro de BC — généré automatiquement à la réception (ex : BC-2026-0001)
    // Vide tant que le statut est "Brouillon"
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    // Date de création du brouillon
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Date de réception : renseignée quand le statut passe à "Reçu"
    public DateTime? DateReception { get; set; }

    // Statut du bon de commande
    // "Brouillon" : en cours de saisie, modifiable et supprimable
    // "Reçu"      : marchandises reçues, IRRÉVERSIBLE — le stock a été mis à jour
    [Required]
    [MaxLength(20)]
    public string Statut { get; set; } = "Brouillon";

    // Totaux calculés — decimal(18,3) = norme tunisienne DT
    [Column(TypeName = "decimal(18,3)")]
    public decimal TotalHT { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal TotalTVA { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal TotalTTC { get; set; }

    // Clé étrangère vers Fournisseur (M4)
    [Required(ErrorMessage = "Le fournisseur est obligatoire.")]
    public int FournisseurId { get; set; }

    // Propriété de navigation — chargée avec Include() dans les requêtes de détail
    public Fournisseur Fournisseur { get; set; } = null!;

    // Lignes du bon de commande — chargées avec Include() pour les détails
    public ICollection<LigneBonCommande> Lignes { get; set; } = new List<LigneBonCommande>();
}
