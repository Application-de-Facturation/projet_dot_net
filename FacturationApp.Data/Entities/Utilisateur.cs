// FacturationApp.Data/Entities/Utilisateur.cs
// Représente un compte utilisateur de l'application.
// Le mot de passe est toujours stocké haché (PBKDF2-SHA256) — jamais en clair.

using System.ComponentModel.DataAnnotations;

namespace FacturationApp.Data.Entities;

public class Utilisateur
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string NomUtilisateur { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    // PBKDF2-SHA256 : format "base64Salt:base64Hash"
    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    // "Administrateur" ou "Utilisateur"
    [MaxLength(20)]
    public string Role { get; set; } = "Utilisateur";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
