using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;

public class StockMouvement
{
    public int Id { get; set; }

    [Required]
    public int ProduitId { get; set; }

    public Produit Produit { get; set; } = null!;

    // Quantité positive
    public int Quantite { get; set; }

    // Type : "ENTREE" ou "SORTIE"
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Commentaire { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}