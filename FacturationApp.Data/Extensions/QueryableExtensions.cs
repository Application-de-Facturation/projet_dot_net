// FacturationApp.Data/Extensions/QueryableExtensions.cs

using FacturationApp.Data.Entities;

namespace FacturationApp.Data.Extensions;

/// <summary>
/// Extensions LINQ partagées par toute l'équipe.
/// WhereActive() filtre automatiquement IsDeleted = false.
/// M1, M2 et M3 utilisent tous cette extension.
/// </summary>
public static class QueryableExtensions
{
    // Filtre les clients non supprimés
    public static IQueryable<Client> WhereActive(this IQueryable<Client> query)
        => query.Where(c => !c.IsDeleted);

    // Filtre les fournisseurs non supprimés (M4)
    public static IQueryable<Fournisseur> WhereActive(this IQueryable<Fournisseur> query)
        => query.Where(f => !f.IsDeleted);
}