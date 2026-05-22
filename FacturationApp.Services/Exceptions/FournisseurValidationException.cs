// FacturationApp.Services/Exceptions/FournisseurValidationException.cs
// M4 — Exception métier pour les violations de règles fournisseur.
// Même patron que ClientValidationException (M1).

namespace FacturationApp.Services.Exceptions;

/// <summary>
/// Levée par FournisseurService quand une règle métier est violée :
/// - email ou matricule fiscal déjà enregistré
/// Attrapée dans Form.razor pour afficher une alerte en haut de page.
/// </summary>
public class FournisseurValidationException : Exception
{
    public FournisseurValidationException(string message) : base(message) { }
}
