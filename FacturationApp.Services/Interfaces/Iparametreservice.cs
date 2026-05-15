namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service de configuration.
/// Gère la lecture et la mise à jour des paramètres applicatifs
/// stockés dans la table Parametre (clé/valeur).
/// </summary>
public interface IParametreService
{
	/// <summary>
	/// Retourne la valeur brute d'un paramètre par sa clé.
	/// Retourne null si la clé n'existe pas.
	/// </summary>
	Task<string?> GetValeurAsync(string cle);

	/// <summary>
	/// Retourne le montant du timbre fiscal (clé "TimbreFiscal").
	/// Toujours appelé dynamiquement — jamais codé en dur.
	/// </summary>
	Task<decimal> GetTimbreFiscalAsync();

	/// <summary>
	/// Met à jour la valeur d'un paramètre existant par sa clé.
	/// Utilisé pour incrémenter le compteur de factures.
	/// </summary>
	Task SetValeurAsync(string cle, string valeur);
}