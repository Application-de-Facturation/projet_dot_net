using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FacturationApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class M4_AddFournisseurEtBonCommande : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fournisseur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Telephone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Adresse = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MatriculeFiscal = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseur", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BonCommande",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateReception = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Brouillon"),
                    TotalHT = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalTVA = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalTTC = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    FournisseurId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonCommande", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonCommande_Fournisseur_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseur",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LigneBonCommande",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BonCommandeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProduitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TauxTVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: false),
                    MontantHT = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MontantTVA = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MontantTTC = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneBonCommande", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LigneBonCommande_BonCommande_BonCommandeId",
                        column: x => x.BonCommandeId,
                        principalTable: "BonCommande",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LigneBonCommande_Produit_ProduitId",
                        column: x => x.ProduitId,
                        principalTable: "Produit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Parametre",
                columns: new[] { "Id", "Cle", "Valeur" },
                values: new object[,]
                {
                    { 4, "BonCommandePrefixe", "BC" },
                    { 5, "BonCommandeCompteur", "0" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonCommande_DateCreation",
                table: "BonCommande",
                column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommande_FournisseurId",
                table: "BonCommande",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_BonCommande_Statut",
                table: "BonCommande",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseur_Nom",
                table: "Fournisseur",
                column: "Nom",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LigneBonCommande_BonCommandeId",
                table: "LigneBonCommande",
                column: "BonCommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_LigneBonCommande_ProduitId",
                table: "LigneBonCommande",
                column: "ProduitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LigneBonCommande");

            migrationBuilder.DropTable(
                name: "BonCommande");

            migrationBuilder.DropTable(
                name: "Fournisseur");

            migrationBuilder.DeleteData(
                table: "Parametre",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Parametre",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
