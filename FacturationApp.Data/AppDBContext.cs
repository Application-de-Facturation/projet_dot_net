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

	// M3 ajoutera ici : Utilisateurs (si extension validée)

	// ══════════════════════════════════════════════════════════════════════════
	// CONFIGURATION AVANCÉE
	// ══════════════════════════════════════════════════════════════════════════
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// ── CLIENT (M1 — ne pas modifier) ─────────────────────────────────────
		modelBuilder.Entity<Client>(entity =>
		{
			entity.ToTable("Client");
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Id).ValueGeneratedOnAdd();
			entity.Property(c => c.Nom).IsRequired().HasMaxLength(150);
			entity.Property(c => c.Email).HasMaxLength(200);
			entity.Property(c => c.Telephone).HasMaxLength(20);
			entity.Property(c => c.Adresse).HasMaxLength(500);
			entity.Property(c => c.MatriculeFiscal).HasMaxLength(50);
			entity.Property(c => c.IsDeleted).HasDefaultValue(false);
			entity.HasIndex(c => c.Nom)
				  .HasDatabaseName("IX_Client_Nom")
				  .HasFilter("[IsDeleted] = 0");
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
	}
}