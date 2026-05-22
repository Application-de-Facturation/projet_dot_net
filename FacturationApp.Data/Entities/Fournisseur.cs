// FacturationApp.Data/Entities/Fournisseur.cs
// M4 — Module Fournisseurs
// Miroir de Client.cs : mêmes validations, même logique de suppression logique.

using System.ComponentModel.DataAnnotations;

namespace FacturationApp.Data.Entities;

public class Fournisseur
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom du fournisseur est obligatoire.")]
    [MaxLength(150, ErrorMessage = "Le nom du fournisseur ne doit pas dépasser 150 caractères.")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [MaxLength(200)]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "L'adresse email n'est pas valide. Exemple : contact@fournisseur.tn")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Le téléphone est obligatoire.")]
    [MaxLength(20)]
    [RegularExpression(
        @"^\+216[\s]?\d{2}[\s]?\d{3}[\s]?\d{3}$",
        ErrorMessage = "Format invalide. Exemples valides : +216 22 333 444 ou +21622333444")]
    public string? Telephone { get; set; }

    [MaxLength(500)]
    public string? Adresse { get; set; }

    // Matricule fiscal tunisien : 7 chiffres + lettre + /[PNMA] + /[CEPBDMA] + /000-/999
    [Required(ErrorMessage = "Le matricule fiscal est obligatoire.")]
    [MaxLength(50)]
    [RegularExpression(
        @"^\d{7}[A-Za-z]/[PNMApnma]/[CEPBDAMcepbdam]/\d{3}$",
        ErrorMessage = "Format invalide. Exemple correct : 1234567A/P/M/000  (7 chiffres + lettre + /P|N|M|A + /C|E|P|B|D|A + /000 à /999)")]
    public string? MatriculeFiscal { get; set; }

    // Suppression logique — jamais supprimé physiquement de la BDD
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
