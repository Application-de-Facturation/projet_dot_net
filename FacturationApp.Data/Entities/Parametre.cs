using System.ComponentModel.DataAnnotations;

namespace FacturationApp.Data.Entities;


public class Parametre
{
	//Clé primaire
	public int Id { get; set; }

	//Clé de configuration
	[Required(ErrorMessage = "La clé est obligatoire.")]
	[MaxLength(50, ErrorMessage = "La clé ne doit pas dépasser 50 caractères.")]
	public string Cle { get; set; } = string.Empty;

	//Valeur de configuration
	[Required(ErrorMessage = "La valeur est obligatoire.")]
	[MaxLength(200, ErrorMessage = "La valeur ne doit pas dépasser 200 caractères.")]
	public string Valeur { get; set; } = string.Empty;
}