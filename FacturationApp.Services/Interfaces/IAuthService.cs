// FacturationApp.Services/Interfaces/IAuthService.cs

using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service d'authentification.
/// Gère la connexion, l'inscription et la validation des comptes.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Vérifie les identifiants. Accepte un nom d'utilisateur OU un email.
    /// Retourne l'utilisateur si valide, null sinon.
    /// </summary>
    Task<Utilisateur?> ConnecterAsync(string identifiant, string motDePasse);

    /// <summary>
    /// Crée un nouveau compte.
    /// Retourne null si succès, ou le message d'erreur en cas de doublon.
    /// </summary>
    Task<string?> InscrireAsync(string nomUtilisateur, string email, string motDePasse);

    /// <summary>Retourne un utilisateur par son Id.</summary>
    Task<Utilisateur?> GetByIdAsync(int id);
}
