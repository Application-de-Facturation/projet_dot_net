// FacturationApp.Services/Implementations/AuthService.cs
// Authentification avec hachage PBKDF2-SHA256 (100 000 itérations).
// Aucune dépendance externe : System.Security.Cryptography suffit.

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    // ─── CONNEXION ────────────────────────────────────────────────────────────

    /// <summary>
    /// Accepte un nom d'utilisateur OU un email (insensible à la casse).
    /// Compare le mot de passe fourni contre le hash stocké via PBKDF2-SHA256.
    /// Retourne null si les identifiants sont incorrects.
    /// </summary>
    public async Task<Utilisateur?> ConnecterAsync(string identifiant, string motDePasse)
    {
        var id = identifiant.Trim().ToLower();

        // Recherche par nom d'utilisateur OU email
        var utilisateur = await _context.Utilisateurs
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u =>
                u.NomUtilisateur.ToLower() == id ||
                u.Email.ToLower() == id);

        if (utilisateur == null)
            return null;

        // Vérification du mot de passe — comparaison à temps constant (anti timing-attack)
        return VerifierMotDePasse(motDePasse, utilisateur.MotDePasseHash)
            ? utilisateur
            : null;
    }

    // ─── INSCRIPTION ──────────────────────────────────────────────────────────

    /// <summary>
    /// Crée un compte après avoir vérifié l'unicité du nom et de l'email.
    /// Le mot de passe est haché avant insertion — jamais stocké en clair.
    /// Retourne null si tout va bien, ou un message d'erreur lisible.
    /// </summary>
    public async Task<string?> InscrireAsync(string nomUtilisateur, string email, string motDePasse)
    {
        var nom = nomUtilisateur.Trim();
        var mail = email.Trim().ToLower();

        // Vérification unicité du nom d'utilisateur
        var nomExiste = await _context.Utilisateurs
            .AnyAsync(u => !u.IsDeleted && u.NomUtilisateur.ToLower() == nom.ToLower());
        if (nomExiste)
            return "Ce nom d'utilisateur est déjà pris.";

        // Vérification unicité de l'email
        var emailExiste = await _context.Utilisateurs
            .AnyAsync(u => !u.IsDeleted && u.Email.ToLower() == mail);
        if (emailExiste)
            return "Un compte existe déjà avec cet email.";

        var utilisateur = new Utilisateur
        {
            NomUtilisateur = nom,
            Email = mail,
            MotDePasseHash = HacherMotDePasse(motDePasse),
            Role = "Utilisateur",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return null; // null = succès
    }

    // ─── LECTURE ──────────────────────────────────────────────────────────────

    public async Task<Utilisateur?> GetByIdAsync(int id)
        => await _context.Utilisateurs.FindAsync(id);

    // ─── HACHAGE PBKDF2-SHA256 ────────────────────────────────────────────────

    /// <summary>
    /// Hache le mot de passe avec un sel aléatoire de 16 octets et 100 000 itérations.
    /// Format de sortie : "base64(sel):base64(hash)"
    /// </summary>
    private static string HacherMotDePasse(string motDePasse)
    {
        var sel = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse, sel,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return $"{Convert.ToBase64String(sel)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Vérifie le mot de passe fourni contre un hash stocké.
    /// Utilise CryptographicOperations.FixedTimeEquals pour résister aux attaques temporelles.
    /// </summary>
    private static bool VerifierMotDePasse(string motDePasse, string hashStocke)
    {
        var parties = hashStocke.Split(':');
        if (parties.Length != 2) return false;

        var sel = Convert.FromBase64String(parties[0]);
        var hashAttendu = Convert.FromBase64String(parties[1]);

        var hashCalcule = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse, sel,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return CryptographicOperations.FixedTimeEquals(hashCalcule, hashAttendu);
    }
}
