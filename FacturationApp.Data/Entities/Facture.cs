using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturationApp.Data.Entities;

public class Facture
{
	//Clé primaire 
	public int Id { get; set; }

	//Numéro de facture
	/// Généré automatiquement par IFactureService.GenererNumero().
	/// Vide tant que la facture est en brouillon — assigné à la validation.

	[MaxLength(20)]
	public string Numero { get; set; } = string.Empty;

	//Dates
	public DateTime DateCreation { get; set; } = DateTime.UtcNow;

	/// <summary>Date à laquelle le statut est passé à "Validée".</summary>
	public DateTime? DateValidation { get; set; }

	//Statut
	/// <summary>
	/// "Brouillon" ou "Validée".
	/// Une fois "Validée" : aucune modification possible — règle métier absolue.
	/// </summary>
	[Required]
	[MaxLength(20)]
	public string Statut { get; set; } = "Brouillon";

	//  Totaux calculés ── decimal(18,3) = norme tunisienne
	[Column(TypeName = "decimal(18,3)")]
	public decimal TotalHT { get; set; }

	[Column(TypeName = "decimal(18,3)")]
	public decimal TotalTVA { get; set; }

	[Column(TypeName = "decimal(18,3)")]
	public decimal TotalTTC { get; set; }

	/// Montant du timbre fiscal — lu depuis Parametre["TimbreFiscal"] au moment
	/// de la validation, puis stocké ici (snapshot du timbre au moment de la facture).
	[Column(TypeName = "decimal(18,3)")]
	public decimal Timbre { get; set; }

	/// <summary>NetAPayer = TotalTTC + Timbre</summary>
	[Column(TypeName = "decimal(18,3)")]
	public decimal NetAPayer { get; set; }

	//Clé étrangère vers Client
	[Required(ErrorMessage = "Le client est obligatoire.")]
	public int ClientId { get; set; }

	/// <summary>Propriété de navigation vers Client.</summary>
	public Client Client { get; set; } = null!;

	//Navigation vers les lignes
	/// Lignes de la facture. Chargées avec Include() dans les requêtes de détail.
	public ICollection<LigneFacture> Lignes { get; set; } = new List<LigneFacture>();
}