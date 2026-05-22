// FacturationApp.Data/AppDbContext.cs
// M1 : Client (structure de base)
// M2 : Categorie, Produit, Parametre, Facture, LigneFacture + seed data

using Microsoft.EntityFrameworkCore;
using FacturationApp.Data.Entities;

namespace FacturationApp.Data;

/// <summary>
/// AppDbContext est le chef d'orchestre entre le code C# et la base de données.
/// Il expose toutes les tables sous forme de DbSet.
/// </summary>
public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	// ══════════════════════════════════════════════════════════════════════════
	// TABLES — DbSet = tables SQL
	// ══════════════════════════════════════════════════════════════════════════

	// M1
	public DbSet<Client> Clients { get; set; }

	// M2
	public DbSet<Categorie> Categories { get; set; }
	public DbSet<Produit> Produits { get; set; }
	public DbSet<Parametre> Parametres { get; set; }
	public DbSet<Facture> Factures { get; set; }
	public DbSet<LigneFacture> LignesFacture { get; set; }

	// Stock mouvements
	public DbSet<StockMouvement> StockMouvements { get; set; }

	// M4 — Fournisseurs et Bons de commande
	public DbSet<Fournisseur> Fournisseurs { get; set; }
	public DbSet<BonCommande> BonsCommande { get; set; }
	public DbSet<LigneBonCommande> LignesBonCommande { get; set; }

	// Auth — Comptes utilisateurs
	public DbSet<Utilisateur> Utilisateurs { get; set; }

	// ══════════════════════════════════════════════════════════════════════════
	// CONFIGURATION AVANCÉE
	// ══════════════════════════════════════════════════════════════════════════
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// ── CLIENT (M1) ───────────────────────────────────────────────────────
		modelBuilder.Entity<Client>(entity =>
		{
			entity.ToTable("Client");
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Id).ValueGeneratedOnAdd();
			entity.Property(c => c.Nom).IsRequired().HasMaxLength(150);
			// IsRequired(false) : [Required] sur l'entité sert uniquement à la validation
			// Blazor (DataAnnotationsValidator) — la colonne reste nullable en base pour
			// éviter une migration de rupture sur les données existantes.
			entity.Property(c => c.Email).IsRequired(false).HasMaxLength(200);
			entity.Property(c => c.Telephone).IsRequired(false).HasMaxLength(20);
			entity.Property(c => c.Adresse).HasMaxLength(500);
			entity.Property(c => c.MatriculeFiscal).IsRequired(false).HasMaxLength(50);
			entity.Property(c => c.IsDeleted).HasDefaultValue(false);
			entity.HasIndex(c => c.Nom)
				  .HasDatabaseName("IX_Client_Nom")
				  .HasFilter("[IsDeleted] = 0");
		});

		// ── STOCK MOUVEMENT (nouvelle table pour suivre entrées/sorties) ──────
		modelBuilder.Entity<StockMouvement>(entity =>
		{
			entity.ToTable("StockMouvement");
			entity.HasKey(s => s.Id);
			entity.Property(s => s.Quantite).IsRequired();
			entity.Property(s => s.Type).IsRequired().HasMaxLength(20); // "ENTREE" ou "SORTIE"
			entity.Property(s => s.Commentaire).HasMaxLength(500);

			entity.HasOne(s => s.Produit)
				.WithMany()
				.HasForeignKey(s => s.ProduitId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		// ── CATEGORIE (M2) ─────────────────────────────────────────────────────
		modelBuilder.Entity<Categorie>(entity =>
		{
			entity.ToTable("Categorie");
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);

			// Un-à-plusieurs : Categorie → Produits
			entity.HasMany(c => c.Produits)
				  .WithOne(p => p.Categorie)
				  .HasForeignKey(p => p.CategorieId)
				  .OnDelete(DeleteBehavior.Restrict); // on ne supprime pas une catégorie qui a des produits

			// ── SEED DATA — 4 catégories initiales ──
			entity.HasData(
				new Categorie { Id = 1, Nom = "Informatique" },
				new Categorie { Id = 2, Nom = "Bureautique" },
				new Categorie { Id = 3, Nom = "Services" },
				new Categorie { Id = 4, Nom = "Autres" }
			);
		});

		// ── PRODUIT (M2) ───────────────────────────────────────────────────────
		modelBuilder.Entity<Produit>(entity =>
		{
			entity.ToTable("Produit");
			entity.HasKey(p => p.Id);
			entity.Property(p => p.Designation).IsRequired().HasMaxLength(200);
			entity.Property(p => p.PrixUnitaireHT).HasColumnType("decimal(18,3)");
			entity.Property(p => p.TauxTVA).HasColumnType("decimal(5,2)");
			entity.Property(p => p.IsDeleted).HasDefaultValue(false);

			// Index pour recherche rapide sur produits actifs
			entity.HasIndex(p => p.Designation)
				  .HasDatabaseName("IX_Produit_Designation")
				  .HasFilter("[IsDeleted] = 0");
		});

		// ── PARAMETRE (M2) ─────────────────────────────────────────────────────
		modelBuilder.Entity<Parametre>(entity =>
		{
			entity.ToTable("Parametre");
			entity.HasKey(p => p.Id);
			entity.Property(p => p.Cle).IsRequired().HasMaxLength(50);
			entity.Property(p => p.Valeur).IsRequired().HasMaxLength(200);

			// Index unique sur Cle — chaque clé de config est unique
			entity.HasIndex(p => p.Cle)
				  .IsUnique()
				  .HasDatabaseName("UX_Parametre_Cle");

			// ── SEED DATA — paramètres initiaux ──
			entity.HasData(
				new Parametre { Id = 1, Cle = "TimbreFiscal", Valeur = "1.000" },
				new Parametre { Id = 2, Cle = "FacturePrefixe", Valeur = "FAC" },
				new Parametre { Id = 3, Cle = "FactureCompteur", Valeur = "0" }
			);
		});

		// ── FACTURE (M2) ───────────────────────────────────────────────────────
		modelBuilder.Entity<Facture>(entity =>
		{
			entity.ToTable("Facture");
			entity.HasKey(f => f.Id);
			entity.Property(f => f.Numero).HasMaxLength(20);
			entity.Property(f => f.Statut).IsRequired().HasMaxLength(20).HasDefaultValue("Brouillon");
			entity.Property(f => f.TotalHT).HasColumnType("decimal(18,3)");
			entity.Property(f => f.TotalTVA).HasColumnType("decimal(18,3)");
			entity.Property(f => f.TotalTTC).HasColumnType("decimal(18,3)");
			entity.Property(f => f.Timbre).HasColumnType("decimal(18,3)");
			entity.Property(f => f.NetAPayer).HasColumnType("decimal(18,3)");

			// Clé étrangère vers Client (M1)
			entity.HasOne(f => f.Client)
				  .WithMany()
				  .HasForeignKey(f => f.ClientId)
				  .OnDelete(DeleteBehavior.Restrict);

			// Index pour les requêtes analytiques de M3 (filtrage par statut)
			entity.HasIndex(f => f.Statut).HasDatabaseName("IX_Facture_Statut");
			entity.HasIndex(f => f.DateCreation).HasDatabaseName("IX_Facture_DateCreation");
		});

		// ── LIGNE FACTURE (M2) ─────────────────────────────────────────────────
		modelBuilder.Entity<LigneFacture>(entity =>
		{
			entity.ToTable("LigneFacture");
			entity.HasKey(l => l.Id);

			// Snapshots — decimal(18,3) et decimal(5,2)
			entity.Property(l => l.Designation).IsRequired().HasMaxLength(200);
			entity.Property(l => l.PrixUnitaireHT).HasColumnType("decimal(18,3)");
			entity.Property(l => l.TauxTVA).HasColumnType("decimal(5,2)");

			// Montants calculés
			entity.Property(l => l.MontantHT).HasColumnType("decimal(18,3)");
			entity.Property(l => l.MontantTVA).HasColumnType("decimal(18,3)");
			entity.Property(l => l.MontantTTC).HasColumnType("decimal(18,3)");

			// Clé étrangère vers Facture — suppression en cascade des lignes si la facture est supprimée
			entity.HasOne(l => l.Facture)
				  .WithMany(f => f.Lignes)
				  .HasForeignKey(l => l.FactureId)
				  .OnDelete(DeleteBehavior.Cascade);

			// Clé étrangère vers Produit — Restrict pour conserver l'historique
			entity.HasOne(l => l.Produit)
				  .WithMany(p => p.LignesFacture)
				  .HasForeignKey(l => l.ProduitId)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// ── FOURNISSEUR (M4) ───────────────────────────────────────────────────
		modelBuilder.Entity<Fournisseur>(entity =>
		{
			entity.ToTable("Fournisseur");
			entity.HasKey(f => f.Id);
			entity.Property(f => f.Id).ValueGeneratedOnAdd();
			entity.Property(f => f.Nom).IsRequired().HasMaxLength(150);
			// Même raison que Client : colonnes nullable en base, [Required] pour validation UI
			entity.Property(f => f.Email).IsRequired(false).HasMaxLength(200);
			entity.Property(f => f.Telephone).IsRequired(false).HasMaxLength(20);
			entity.Property(f => f.Adresse).HasMaxLength(500);
			entity.Property(f => f.MatriculeFiscal).IsRequired(false).HasMaxLength(50);
			entity.Property(f => f.IsDeleted).HasDefaultValue(false);

			// Index filtré sur Nom — comme Client
			entity.HasIndex(f => f.Nom)
				  .HasDatabaseName("IX_Fournisseur_Nom")
				  .HasFilter("[IsDeleted] = 0");
		});

		// ── BON DE COMMANDE (M4) ───────────────────────────────────────────────
		modelBuilder.Entity<BonCommande>(entity =>
		{
			entity.ToTable("BonCommande");
			entity.HasKey(b => b.Id);
			entity.Property(b => b.Numero).HasMaxLength(20);
			entity.Property(b => b.Statut).IsRequired().HasMaxLength(20).HasDefaultValue("Brouillon");
			entity.Property(b => b.TotalHT).HasColumnType("decimal(18,3)");
			entity.Property(b => b.TotalTVA).HasColumnType("decimal(18,3)");
			entity.Property(b => b.TotalTTC).HasColumnType("decimal(18,3)");

			// Clé étrangère vers Fournisseur — Restrict (on ne supprime pas un fournisseur qui a des BCs)
			entity.HasOne(b => b.Fournisseur)
				  .WithMany()
				  .HasForeignKey(b => b.FournisseurId)
				  .OnDelete(DeleteBehavior.Restrict);

			// Index pour filtrage par statut et par date
			entity.HasIndex(b => b.Statut).HasDatabaseName("IX_BonCommande_Statut");
			entity.HasIndex(b => b.DateCreation).HasDatabaseName("IX_BonCommande_DateCreation");
		});

		// ── LIGNE BON DE COMMANDE (M4) ─────────────────────────────────────────
		modelBuilder.Entity<LigneBonCommande>(entity =>
		{
			entity.ToTable("LigneBonCommande");
			entity.HasKey(l => l.Id);

			// Snapshots — mêmes types que LigneFacture
			entity.Property(l => l.Designation).IsRequired().HasMaxLength(200);
			entity.Property(l => l.PrixUnitaireHT).HasColumnType("decimal(18,3)");
			entity.Property(l => l.TauxTVA).HasColumnType("decimal(5,2)");

			// Montants calculés
			entity.Property(l => l.MontantHT).HasColumnType("decimal(18,3)");
			entity.Property(l => l.MontantTVA).HasColumnType("decimal(18,3)");
			entity.Property(l => l.MontantTTC).HasColumnType("decimal(18,3)");

			// Clé étrangère vers BonCommande — cascade : supprimer le BC supprime ses lignes
			entity.HasOne(l => l.BonCommande)
				  .WithMany(b => b.Lignes)
				  .HasForeignKey(l => l.BonCommandeId)
				  .OnDelete(DeleteBehavior.Cascade);

			// Clé étrangère vers Produit — Restrict pour conserver l'historique
			entity.HasOne(l => l.Produit)
				  .WithMany()
				  .HasForeignKey(l => l.ProduitId)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// ── UTILISATEUR (Auth) ────────────────────────────────────────────────
		modelBuilder.Entity<Utilisateur>(entity =>
		{
			entity.ToTable("Utilisateur");
			entity.HasKey(u => u.Id);
			entity.Property(u => u.NomUtilisateur).IsRequired().HasMaxLength(50);
			entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
			entity.Property(u => u.MotDePasseHash).IsRequired();
			entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Utilisateur");
			entity.Property(u => u.IsDeleted).HasDefaultValue(false);

			// Un nom d'utilisateur et un email doivent être uniques parmi les comptes actifs
			entity.HasIndex(u => u.NomUtilisateur)
				  .IsUnique()
				  .HasDatabaseName("UX_Utilisateur_NomUtilisateur");
			entity.HasIndex(u => u.Email)
				  .IsUnique()
				  .HasDatabaseName("UX_Utilisateur_Email");
		});

		// ── PARAMETRE — seed data M4 (IDs 4 et 5, les 1-3 sont pris par M2) ───
		// Ajoute les paramètres de numérotation des bons de commande
		modelBuilder.Entity<Parametre>().HasData(
			new Parametre { Id = 4, Cle = "BonCommandePrefixe", Valeur = "BC" },
			new Parametre { Id = 5, Cle = "BonCommandeCompteur", Valeur = "0" }
		);
	}
}