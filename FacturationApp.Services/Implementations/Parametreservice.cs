using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service de configuration.
/// Lit et met à jour les paramètres depuis la table Parametre.
/// </summary>
public class ParametreService : IParametreService
{
	private readonly AppDbContext _context;

	// Le DbContext est injecté par la DI — jamais instancié manuellement
	public ParametreService(AppDbContext context)
	{
		_context = context;
	}

	// ─── READ ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Retourne la valeur brute d'un paramètre par sa clé.
	/// Retourne null si la clé n'existe pas en base.
	/// </summary>
	public async Task<string?> GetValeurAsync(string cle)
	{
		var parametre = await _context.Parametres
			.FirstOrDefaultAsync(p => p.Cle == cle);

		return parametre?.Valeur;
	}

	/// <summary>
	/// Retourne le montant du timbre fiscal.
	/// Lit "TimbreFiscal" depuis la base et le convertit en decimal.
	/// Lève une exception si le paramètre est absent ou mal formaté —
	/// cela signifie que le seed data n'a pas été appliqué.
	/// </summary>
	public async Task<decimal> GetTimbreFiscalAsync()
	{
		var valeur = await GetValeurAsync("TimbreFiscal");

		if (valeur is null)
			throw new InvalidOperationException(
				"Paramètre 'TimbreFiscal' introuvable en base. Vérifiez que les migrations et seed data ont été appliqués.");

		if (!decimal.TryParse(valeur, System.Globalization.NumberStyles.Any,
				System.Globalization.CultureInfo.InvariantCulture, out var montant))
			throw new InvalidOperationException(
				$"Paramètre 'TimbreFiscal' invalide : valeur '{valeur}' non convertible en decimal.");

		return montant;
	}

	// ─── WRITE ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Met à jour la valeur d'un paramètre existant.
	/// Utilisé principalement pour incrémenter "FactureCompteur" à chaque nouvelle facture.
	/// </summary>
	public async Task SetValeurAsync(string cle, string valeur)
	{
		var parametre = await _context.Parametres
			.FirstOrDefaultAsync(p => p.Cle == cle);

		if (parametre is null)
			throw new InvalidOperationException(
				$"Paramètre '{cle}' introuvable. Impossible de mettre à jour.");

		parametre.Valeur = valeur;
		await _context.SaveChangesAsync();
	}
}