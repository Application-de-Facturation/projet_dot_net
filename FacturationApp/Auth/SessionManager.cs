// FacturationApp/Auth/SessionManager.cs
//
// ══ POURQUOI CE FICHIER EXISTE ══
// Dans Blazor Server, la connexion est un WebSocket (SignalR).
// Un composant Blazor ne peut PAS écrire un cookie HTTP directement depuis
// le circuit WebSocket — seul le pipeline HTTP (contrôleur) le peut.
//
// Solution : le composant Blazor crée ici un token éphémère (GUID, durée 2 min)
// et redirige l'utilisateur vers /auth/signin/{token}.
// AuthController échange ce token contre un vrai cookie d'authentification.
// Le token est à usage unique (TenterConsommer le supprime immédiatement).

using System.Collections.Concurrent;

namespace FacturationApp.Auth;

/// <summary>
/// Singleton : pont sécurisé entre le circuit Blazor et le contrôleur HTTP.
/// </summary>
public class SessionManager
{
    private record TokenEntry(int UserId, string NomUtilisateur, string Role, DateTime Expires);

    // ConcurrentDictionary : thread-safe pour les accès simultanés
    private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new();

    /// <summary>
    /// Crée un token à usage unique valable 2 minutes.
    /// Retourne le token (GUID sans tirets) à passer dans l'URL.
    /// </summary>
    public string CreerToken(int userId, string nomUtilisateur, string role)
    {
        var token = Guid.NewGuid().ToString("N"); // ex: "a3f2b1c4d5e6..."
        _tokens[token] = new TokenEntry(userId, nomUtilisateur, role, DateTime.UtcNow.AddMinutes(2));
        return token;
    }

    /// <summary>
    /// Consomme et invalide le token en une seule opération atomique.
    /// Retourne false si le token est inconnu, déjà utilisé ou expiré.
    /// </summary>
    public bool TenterConsommer(string token, out (int UserId, string NomUtilisateur, string Role) info)
    {
        info = default;

        // TryRemove est atomique — un token ne peut être consommé qu'une fois
        if (!_tokens.TryRemove(token, out var entry))
            return false;

        if (entry.Expires < DateTime.UtcNow)
            return false;

        info = (entry.UserId, entry.NomUtilisateur, entry.Role);
        return true;
    }
}
