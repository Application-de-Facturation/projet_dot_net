using System.ComponentModel.DataAnnotations;

namespace FacturationApp.Data.Entities;

/// <summary>
/// Représente un client de l'application de facturation.
/// Cette entité sera transformée en table SQL par EF Core.
/// </summary>
public class Client
{
    // ---------------- Clé primaire ----------------
    public int Id { get; set; }

    // ---------------- Champs obligatoires ----------------
    [Required(ErrorMessage = "Le nom du client est obligatoire.")]
    [MaxLength(150, ErrorMessage = "Le nom du client ne doit pas dépasser 150 caractères.")]
    public string Nom { get; set; } = string.Empty;

    // ---------------- Champs optionnels ----------------
    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telephone { get; set; }

    [MaxLength(500)]
    public string? Adresse { get; set; }

    // Matricule fiscal tunisien
    [MaxLength(50)]
    public string? MatriculeFiscal { get; set; }

    // ---------------- Champs techniques ----------------
    // Suppression logique : false = actif, true = supprimé
    public bool IsDeleted { get; set; } = false;

    // Date de création
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}