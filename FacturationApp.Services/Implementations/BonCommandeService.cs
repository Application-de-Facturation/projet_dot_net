// FacturationApp.Services/Implementations/BonCommandeService.cs
// M4 — Implémentation concrète du service BonCommande
// Miroir de FactureService.cs sans timbre fiscal.
//
// Différence clé : ValiderReceptionAsync() AJOUTE le stock (entrée marchandise)
// alors que FactureService.ValiderFactureAsync() RETIRE le stock (sortie vente).

using Microsoft.EntityFrameworkCore;
using FacturationApp.Data;
using FacturationApp.Data.Entities;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Implémentation du service BonCommande.
/// Dépend de IParametreService pour lire/incrémenter le compteur de numérotation.
/// </summary>
public class BonCommandeService : IBonCommandeService
{
    private readonly AppDbContext _context;
    private readonly IParametreService _parametreService;

    // Double injection : DbContext + ParametreService (pour la numérotation)
    public BonCommandeService(AppDbContext context, IParametreService parametreService)
    {
        _context = context;
        _parametreService = parametreService;
    }

    // ─── READ ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retourne tous les BCs avec leur fournisseur.
    /// Triés par date décroissante (plus récents en premier).
    /// </summary>
    public async Task<List<BonCommande>> GetAllAsync()
    {
        return await _context.BonsCommande
            .Include(b => b.Fournisseur)
            .OrderByDescending(b => b.DateCreation)
            .ToListAsync();
    }

    /// <summary>
    /// Retourne un BC complet avec Fournisseur + toutes ses Lignes + Produits.
    /// Include imbriqué : BonCommande → Lignes → Produit.
    /// </summary>
    public async Task<BonCommande?> GetByIdAsync(int id)
    {
        return await _context.BonsCommande
            .Include(b => b.Fournisseur)
            .Include(b => b.Lignes)
                .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    // ─── CALCUL ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcule les montants de chaque ligne et retourne les totaux globaux.
    ///
    /// IMPORTANT : utilise les champs SNAPSHOT (PrixUnitaireHT, TauxTVA) —
    /// jamais les valeurs actuelles du produit.
    ///
    /// Formules :
    ///   MontantHT  = Quantite × PrixUnitaireHT
    ///   MontantTVA = MontantHT × TauxTVA / 100
    ///   MontantTTC = MontantHT + MontantTVA
    /// </summary>
    public (decimal TotalHT, decimal TotalTVA, decimal TotalTTC) CalculerTotaux(List<LigneBonCommande> lignes)
    {
        foreach (var ligne in lignes)
        {
            ligne.MontantHT = ligne.Quantite * ligne.PrixUnitaireHT;
            ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
            ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
        }

        return (
            lignes.Sum(l => l.MontantHT),
            lignes.Sum(l => l.MontantTVA),
            lignes.Sum(l => l.MontantTTC)
        );
    }

    // ─── WRITE ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crée un BC en statut "Brouillon" avec ses lignes.
    ///
    /// Pour chaque ligne, copie les snapshots depuis Produit en base :
    ///   - Designation    → snapshot de la désignation actuelle
    ///   - PrixUnitaireHT → snapshot du prix actuel
    ///   - TauxTVA        → snapshot du taux actuel
    ///
    /// Calcule ensuite MontantHT, MontantTVA, MontantTTC et les totaux du BC.
    /// Le numéro est vide à la création — assigné à la réception.
    /// </summary>
    public async Task<BonCommande> CreateAsync(BonCommande bc, List<LigneBonCommande> lignes)
    {
        // Initialisation du BC
        bc.Statut = "Brouillon";
        bc.DateCreation = DateTime.UtcNow;
        bc.Numero = string.Empty; // Généré à la réception

        // Pour chaque ligne : copier les snapshots depuis la base
        foreach (var ligne in lignes)
        {
            var produit = await _context.Produits.FindAsync(ligne.ProduitId)
                ?? throw new InvalidOperationException(
                    $"Produit Id={ligne.ProduitId} introuvable. Impossible de créer la ligne.");

            // ── SNAPSHOT — valeurs figées au moment du BC ──
            ligne.Designation = produit.Designation;
            ligne.PrixUnitaireHT = produit.PrixUnitaireHT;
            ligne.TauxTVA = produit.TauxTVA;

            // Calcul des montants de la ligne
            ligne.MontantHT = ligne.Quantite * ligne.PrixUnitaireHT;
            ligne.MontantTVA = ligne.MontantHT * ligne.TauxTVA / 100;
            ligne.MontantTTC = ligne.MontantHT + ligne.MontantTVA;
        }

        // Totaux du BC
        bc.TotalHT = lignes.Sum(l => l.MontantHT);
        bc.TotalTVA = lignes.Sum(l => l.MontantTVA);
        bc.TotalTTC = lignes.Sum(l => l.MontantTTC);

        bc.Lignes = lignes;

        _context.BonsCommande.Add(bc);
        await _context.SaveChangesAsync();

        return bc;
    }

    /// <summary>
    /// Génère le prochain numéro de BC au format BC-2026-0001.
    ///
    /// Étapes :
    ///   1. Lit "BonCommandeCompteur" depuis Parametre
    ///   2. Incrémente le compteur
    ///   3. Sauvegarde le nouveau compteur en base
    ///   4. Retourne le numéro formaté
    /// </summary>
    public async Task<string> GenererNumeroAsync()
    {
        var valeurCompteur = await _parametreService.GetValeurAsync("BonCommandeCompteur");

        if (!int.TryParse(valeurCompteur, out var compteur))
            throw new InvalidOperationException(
                "Paramètre 'BonCommandeCompteur' invalide ou absent. Vérifiez le seed data.");

        compteur++;

        await _parametreService.SetValeurAsync("BonCommandeCompteur", compteur.ToString());

        var prefixe = await _parametreService.GetValeurAsync("BonCommandePrefixe") ?? "BC";
        var annee = DateTime.Now.Year;

        return $"{prefixe}-{annee}-{compteur:D4}";
    }

    /// <summary>
    /// Marque un BC comme "Reçu" — opération IRRÉVERSIBLE.
    ///
    /// Étapes :
    ///   1. Vérifie que le BC est en "Brouillon"
    ///   2. Génère et assigne le numéro définitif
    ///   3. Recalcule les totaux (sécurité avant validation)
    ///   4. Pour chaque ligne :
    ///      a. Ajoute la quantité au stock (StockQuantity += quantite)
    ///      b. Enregistre un StockMouvement "ENTREE"
    ///   5. Passe Statut à "Reçu" + enregistre DateReception
    ///   6. Sauvegarde en base
    /// </summary>
    public async Task ValiderReceptionAsync(int id)
    {
        var bc = await _context.BonsCommande
            .Include(b => b.Lignes)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new InvalidOperationException($"Bon de commande Id={id} introuvable.");

        // Règle métier : seul un brouillon peut être reçu
        if (bc.Statut != "Brouillon")
            throw new InvalidOperationException(
                $"Le bon de commande '{bc.Numero}' est déjà reçu. Opération impossible.");

        // Étape 1 — Numéro définitif
        bc.Numero = await GenererNumeroAsync();

        // Étape 2 — Recalcul des totaux (sécurité)
        var (totalHT, totalTVA, totalTTC) = CalculerTotaux(bc.Lignes.ToList());
        bc.TotalHT = totalHT;
        bc.TotalTVA = totalTVA;
        bc.TotalTTC = totalTTC;

        // Étape 3 — Entrée en stock pour chaque ligne
        foreach (var ligne in bc.Lignes)
        {
            var produit = await _context.Produits.FindAsync(ligne.ProduitId)
                ?? throw new InvalidOperationException($"Produit Id={ligne.ProduitId} introuvable.");

            // ENTRÉE stock : les marchandises arrivent chez nous → on ajoute
            produit.StockQuantity += ligne.Quantite;
            _context.Produits.Update(produit);

            // Enregistrement du mouvement de stock
            var mouvement = new StockMouvement
            {
                ProduitId = produit.Id,
                Quantite = ligne.Quantite,
                Type = "ENTREE",
                Commentaire = $"Bon de commande {bc.Numero}",
                Date = DateTime.UtcNow
            };

            _context.StockMouvements.Add(mouvement);
        }

        // Étape 4 — Passage au statut Reçu
        bc.Statut = "Reçu";
        bc.DateReception = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Supprime un BC UNIQUEMENT s'il est en statut "Brouillon".
    /// Lève une exception si déjà "Reçu" — le stock a déjà été mis à jour.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var bc = await _context.BonsCommande.FindAsync(id)
            ?? throw new InvalidOperationException($"Bon de commande Id={id} introuvable.");

        if (bc.Statut == "Reçu")
            throw new InvalidOperationException(
                "Impossible de supprimer un bon de commande reçu. Le stock a déjà été mis à jour.");

        _context.BonsCommande.Remove(bc);
        await _context.SaveChangesAsync();
    }
}
