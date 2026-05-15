using System.ComponentModel.DataAnnotations;

namespace FacturationApp.Data.Entities;


public class Categorie
{
	//Clé primaire
	public int Id { get; set; }

	//Champs
	[Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
	[MaxLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
	public string Nom { get; set; } = string.Empty;

	//Navigation
	// Un-à-plusieurs : une catégorie contient plusieurs produits
	public ICollection<Produit> Produits { get; set; } = new List<Produit>();
}