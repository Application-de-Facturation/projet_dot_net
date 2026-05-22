// FacturationApp.Services/Implementations/FournisseurService.cs
// M4 — Implémentation concrète du service Fournisseur
// Miroir exact de ClientService.cs : mêmes patrons, mêmes règles métier.

using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Data.Extensions;
using FacturationApp.Services.Exceptions;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service Fournisseur.
/// Toutes les méthodes utilisent async/await + EF Core.
/// </summary>
public class FournisseurService : IFournisseurService
{
    // DbContext injecté par la DI — jamais instancié manuellement
    private readonly AppDbContext _context;

    public FournisseurService(AppDbContext context)
    {
        _context = context;
    }

    // ─── READ ─────────────────────────────────────────────────────────────────

    /// <summary>Retourne tous les fournisseurs actifs (non supprimés), triés par nom.</summary>
    public async Task<List<Fournisseur>> GetAllAsync()
    {
        return await _context.Fournisseurs
            .WhereActive()           // Filtre IsDeleted = false
            .OrderBy(f => f.Nom)     // Tri alphabétique
            .ToListAsync();
    }

    /// <summary>Retourne un fournisseur par son Id. Null s'il n'existe pas.</summary>
    public async Task<Fournisseur?> GetByIdAsync(int id)
    {
        // FindAsync cherche d'abord dans le cache EF avant d'aller en BDD
        return await _context.Fournisseurs.FindAsync(id);
    }

    /// <summary>
    /// Recherche dynamique sur Nom et MatriculeFiscal.
    /// LINQ traduit Contains() en SQL LIKE '%terme%'.
    /// Si le terme est vide, retourne tous les fournisseurs.
    /// </summary>
    public async Task<List<Fournisseur>> SearchAsync(string terme)
    {
        if (string.IsNullOrWhiteSpace(terme))
            return await GetAllAsync();

        var termeNormalise = terme.Trim().ToLower();

        return await _context.Fournisseurs
            .WhereActive()
            .Where(f =>
                f.Nom.ToLower().Contains(termeNormalise) ||
                (f.MatriculeFiscal != null && f.MatriculeFiscal.ToLower().Contains(termeNormalise)))
            .OrderBy(f => f.Nom)
            .ToListAsync();
    }

    // ─── WRITE ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crée un nouveau fournisseur.
    /// Vérifie l'unicité de l'email et du matricule fiscal avant insertion.
    /// </summary>
    public async Task<Fournisseur> CreateAsync(Fournisseur fournisseur)
    {
        // Vérification unicité email (si renseigné)
        if (!string.IsNullOrWhiteSpace(fournisseur.Email))
        {
            var emailExiste = await _context.Fournisseurs
                .WhereActive()
                .AnyAsync(f => f.Email == fournisseur.Email);

            if (emailExiste)
                throw new FournisseurValidationException(
                    "Un fournisseur avec cet email existe déjà.");
        }

        // Vérification unicité matricule fiscal (si renseigné)
        if (!string.IsNullOrWhiteSpace(fournisseur.MatriculeFiscal))
        {
            var mfExiste = await _context.Fournisseurs
                .WhereActive()
                .AnyAsync(f => f.MatriculeFiscal == fournisseur.MatriculeFiscal);

            if (mfExiste)
                throw new FournisseurValidationException(
                    "Ce matricule fiscal est déjà enregistré.");
        }

        fournisseur.CreatedAt = DateTime.UtcNow;
        fournisseur.IsDeleted = false;

        _context.Fournisseurs.Add(fournisseur);
        await _context.SaveChangesAsync();
        return fournisseur;
    }

    /// <summary>
    /// Met à jour un fournisseur existant.
    /// Vérifie l'unicité en excluant le fournisseur lui-même (c.Id != fournisseur.Id).
    /// </summary>
    public async Task UpdateAsync(Fournisseur fournisseur)
    {
        if (!string.IsNullOrWhiteSpace(fournisseur.Email))
        {
            var emailExiste = await _context.Fournisseurs
                .WhereActive()
                .AnyAsync(f => f.Email == fournisseur.Email && f.Id != fournisseur.Id);

            if (emailExiste)
                throw new FournisseurValidationException(
                    "Un fournisseur avec cet email existe déjà.");
        }

        if (!string.IsNullOrWhiteSpace(fournisseur.MatriculeFiscal))
        {
            var mfExiste = await _context.Fournisseurs
                .WhereActive()
                .AnyAsync(f => f.MatriculeFiscal == fournisseur.MatriculeFiscal && f.Id != fournisseur.Id);

            if (mfExiste)
                throw new FournisseurValidationException(
                    "Ce matricule fiscal est déjà enregistré.");
        }

        _context.Fournisseurs.Update(fournisseur);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Suppression LOGIQUE : on pose IsDeleted = true.
    /// La ligne reste en BDD pour conserver l'historique des bons de commande.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var fournisseur = await _context.Fournisseurs.FindAsync(id);

        if (fournisseur == null)
            return; // Déjà supprimé ou inexistant — on ignore silencieusement

        fournisseur.IsDeleted = true;
        _context.Fournisseurs.Update(fournisseur);
        await _context.SaveChangesAsync();
    }
}
