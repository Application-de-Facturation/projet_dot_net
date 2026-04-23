// FacturationApp.Services/Implementations/ClientService.cs

using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Data.Extensions;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation concrète du service Client.
/// Toutes les méthodes utilisent async/await + EF Core.
/// </summary>
public class ClientService : IClientService
{
    // Le DbContext est injecté par la DI (jamais instancié manuellement)
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    // ─── READ ─────────────────────────────────────────────────────────────────

    /// <summary>Retourne tous les clients actifs (non supprimés).</summary>
    public async Task<List<Client>> GetAllAsync()
    {
        return await _context.Clients
            .WhereActive()           // Filtre IsDeleted = false
            .OrderBy(c => c.Nom)     // Tri alphabétique
            .ToListAsync();          // Déclenche la requête SQL et retourne la liste
    }

    /// <summary>Retourne un client par son Id. Retourne null s'il n'existe pas.</summary>
    public async Task<Client?> GetByIdAsync(int id)
    {
        // FindAsync est optimisé pour chercher par clé primaire
        // Il cherche d'abord dans le cache EF avant d'aller en BDD
        return await _context.Clients.FindAsync(id);
    }

    /// <summary>
    /// Recherche dynamique sur Nom et MatriculeFiscal.
    /// LINQ traduit Contains() en SQL LIKE '%terme%'.
    /// Si le terme est vide, retourne tous les clients.
    /// </summary>
    public async Task<List<Client>> SearchAsync(string terme)
    {
        // Si pas de terme de recherche, on retourne tout
        if (string.IsNullOrWhiteSpace(terme))
            return await GetAllAsync();

        // Normalisation : lowercase pour une recherche insensible à la casse
        var termeNormalise = terme.Trim().ToLower();

        return await _context.Clients
            .WhereActive()
            .Where(c =>
                c.Nom.ToLower().Contains(termeNormalise) ||
                (c.MatriculeFiscal != null && c.MatriculeFiscal.ToLower().Contains(termeNormalise)))
            .OrderBy(c => c.Nom)
            .ToListAsync();
    }

    // ─── WRITE ────────────────────────────────────────────────────────────────

    /// <summary>Crée un nouveau client en base.</summary>
    public async Task<Client> CreateAsync(Client client)
    {
        // On s'assure que la date de création est bien en UTC
        client.CreatedAt = DateTime.UtcNow;
        client.IsDeleted = false;

        _context.Clients.Add(client);          // Prépare l'INSERT en mémoire
        await _context.SaveChangesAsync();      // Exécute l'INSERT en BDD
        return client;                          // L'objet a maintenant un Id généré
    }

    /// <summary>Met à jour un client existant.</summary>
    public async Task UpdateAsync(Client client)
    {
        // Update() marque toutes les propriétés comme "modifiées"
        // EF Core générera un UPDATE SQL pour toutes les colonnes
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Suppression LOGIQUE : on ne supprime pas la ligne de la BDD.
    /// On met IsDeleted = true, ce qui la rend invisible dans toutes les requêtes.
    /// Conformément aux exigences du schema.md.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
            return; // Client déjà supprimé ou inexistant — on ignore silencieusement

        client.IsDeleted = true;               // Suppression logique
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }
}