
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;


public class Produit
{
	// Clé primaire
	public int Id { get; set; }

	// Champs obligatoires
	[Required(ErrorMessage = "La désignation est obligatoire.")]
	[MaxLength(200, ErrorMessage = "La désignation ne doit pas dépasser 200 caractères.")]
	public string Designation { get; set; } = string.Empty;

	
	[Required(ErrorMessage = "Le prix unitaire HT est obligatoire.")]
	[Column(TypeName = "decimal(18,3)")]
	[Range(0, double.MaxValue, ErrorMessage = "Le prix doit être positif ou nul.")]
	public decimal PrixUnitaireHT { get; set; }

	
	[Required(ErrorMessage = "Le taux de TVA est obligatoire.")]
	[Column(TypeName = "decimal(5,2)")]
	public decimal TauxTVA { get; set; }

	// Clé étrangère vers Categorie 
	[Required(ErrorMessage = "La catégorie est obligatoire.")]
	public int CategorieId { get; set; }

	/// <summary>Propriété de navigation — chargée avec Include() si besoin.</summary>
	public Categorie Categorie { get; set; } = null!;

	//Champs techniques
	public bool IsDeleted { get; set; } = false;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	//Navigation inverse
	public ICollection<LigneFacture> LignesFacture { get; set; } = new List<LigneFacture>();
}