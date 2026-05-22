// FacturationApp/Controllers/AuthController.cs
// Opérations cookie d'authentification — accessibles uniquement via HTTP (pas WebSocket).
// Appelé par NavigationManager.NavigateTo(forceLoad: true) depuis les composants Blazor.

using System.Security.Claims;
using FacturationApp.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FacturationApp.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly SessionManager _sessions;

    public AuthController(SessionManager sessions)
    {
        _sessions = sessions;
    }

    /// <summary>
    /// Échange un token de session (créé par le composant Blazor) contre un cookie d'authentification.
    /// Le token est à usage unique — invalide dès qu'il est consommé ou après 2 minutes.
    /// </summary>
    [HttpGet("signin/{token}")]
    public async Task<IActionResult> SignIn(string token)
    {
        if (!_sessions.TenterConsommer(token, out var info))
        {
            // Token invalide ou expiré → retour login avec message
            return Redirect("/?erreur=1");
        }

        // Construction du ClaimsPrincipal à partir des infos de session
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, info.UserId.ToString()),
            new Claim(ClaimTypes.Name,           info.NomUtilisateur),
            new Claim(ClaimTypes.Role,           info.Role),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Écriture du cookie HTTP-only, persistant 8 heures
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        return Redirect("/clients");
    }

    /// <summary>
    /// Déconnexion : suppression du cookie, redirection vers la page de login.
    /// </summary>
    [HttpGet("signout")]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
