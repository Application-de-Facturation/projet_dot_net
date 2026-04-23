using Microsoft.EntityFrameworkCore;
using FacturationApp.Data.Entities;

namespace FacturationApp.Data;

/// <summary>
/// AppDbContext est le chef d'orchestre entre le code C# (application)
/// et la base de données (tables SQL).
/// Il expose les tables sous forme de DbSet.
/// </summary>
public class AppDbContext : DbContext
{
    // Le constructeur reçoit les options (connexion SQL Server, SQLite, etc.)
    // Ces options sont injectées depuis Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ----------------- LES TABLES (DbSet = tables SQL) -----------------
    /// <summary>
    /// Table Client.
    /// </summary>
    public DbSet<Client> Clients { get; set; }

    // M2 ajoutera ici : Produits, Categories, Factures, LignesFacture, Parametres
    // M3 ajoutera ici : Utilisateurs (si extension validée)

    // ----------------- CONFIGURATION AVANCÉE -----------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration explicite de l'entité Client
        modelBuilder.Entity<Client>(entity =>
        {
            // Nom de la table
            entity.ToTable("Client");

            // Clé primaire auto-incrémentée
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();

            // Colonnes
            entity.Property(c => c.Nom)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");

            entity.Property(c => c.Email)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            entity.Property(c => c.Telephone)
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            entity.Property(c => c.Adresse)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            entity.Property(c => c.MatriculeFiscal)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            // Valeur par défaut pour IsDeleted
            entity.Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            // Valeur par défaut pour CreatedAt
            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Index pour recherche rapide par nom
            entity.HasIndex(c => c.Nom)
                .HasDatabaseName("IX_Client_Nom")
                .HasFilter("[IsDeleted] = 0");
        });
    }
}