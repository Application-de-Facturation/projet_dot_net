using FacturationApp.Data;
using FacturationApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FacturationApp.Services.Implementations;

/// <summary>
/// Génère un PDF conforme au modèle tunisien avec QuestPDF.
/// Structure : En-tête → Infos client → Lignes → Totaux → Timbre → Net à payer
/// </summary>
public class PdfService : IPdfService
{
	private readonly AppDbContext _context;

	public PdfService(AppDbContext context)
	{
		_context = context;
		// Licence communautaire QuestPDF (gratuite pour projets open source)
		QuestPDF.Settings.License = LicenseType.Community;
	}

	public async Task<byte[]> GenererPdfAsync(int factureId)
	{
		// Charger la facture complète avec toutes ses relations
		var facture = await _context.Factures
			.Include(f => f.Client)
			.Include(f => f.Lignes)
				.ThenInclude(l => l.Produit)
			.FirstOrDefaultAsync(f => f.Id == factureId)
			?? throw new InvalidOperationException($"Facture Id={factureId} introuvable.");

		if (facture.Statut != "Validée")
			throw new InvalidOperationException("Seules les factures validées peuvent être exportées en PDF.");

		// Groupes TVA pour le récapitulatif
		var groupesTVA = facture.Lignes
			.GroupBy(l => l.TauxTVA)
			.Select(g => new { Taux = g.Key, MontantTVA = g.Sum(l => l.MontantTVA) })
			.OrderBy(g => g.Taux)
			.ToList();

        // Couleurs
		var couleurPrimaire = "#1b6ec2";
		var couleurTexte = "#212529";
		var couleurGris = "#6c757d";
		var couleurLigneAlt = "#f8f9fa";
		var couleurBlanc = "#ffffff";

		var pdf = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4);
				page.Margin(1.5f, Unit.Centimetre);
				page.DefaultTextStyle(x => x.FontSize(10).FontColor(couleurTexte));

				// ══ EN-TÊTE ═══════════════════════════════════════════════════
				page.Header().Column(col =>
				{
					col.Item().Row(row =>
					{
						// Titre FACTURE
						row.RelativeItem().Column(c =>
						{
							c.Item().Text("FACTURE")
								.FontSize(28).Bold().FontColor(couleurPrimaire);
							c.Item().Text(facture.Numero)
								.FontSize(14).Bold();
						});

						// Infos société (à adapter selon le projet)
						row.RelativeItem().AlignRight().Column(c =>
						{
							c.Item().Text("FacturationApp").Bold().FontSize(12);
							c.Item().Text("Tunis, Tunisie").FontColor(couleurGris);
						});
					});

					col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(couleurPrimaire);

					// Numéro + dates
					col.Item().Row(row =>
					{
						row.RelativeItem().Column(c =>
						{
							c.Item().Text(t =>
							{
								t.Span("Date d'émission : ").SemiBold();
								t.Span(facture.DateCreation.ToString("dd/MM/yyyy"));
							});
							if (facture.DateValidation.HasValue)
							{
								c.Item().Text(t =>
								{
									t.Span("Date de validation : ").SemiBold();
									t.Span(facture.DateValidation.Value.ToString("dd/MM/yyyy"));
								});
							}
						});

						// Statut
						row.ConstantItem(100).AlignRight().Background("#28a745")
							.Padding(6).Text("VALIDÉE")
							.FontColor(Colors.White).Bold().FontSize(10).AlignCenter();
					});

					col.Item().Height(12);
				});

				// ══ CORPS ═════════════════════════════════════════════════════
				page.Content().Column(col =>
				{
					// ── Infos client ──────────────────────────────────────────
					col.Item().Border(1).BorderColor("#dee2e6").Padding(10).Column(c =>
					{
						c.Item().Text("FACTURÉ À").Bold().FontColor(couleurPrimaire).FontSize(9);
						c.Item().Height(4);
						c.Item().Text(facture.Client?.Nom ?? "").Bold().FontSize(12);

						if (!string.IsNullOrEmpty(facture.Client?.MatriculeFiscal))
							c.Item().Text(t =>
							{
								t.Span("Matricule fiscal : ").SemiBold();
								t.Span(facture.Client.MatriculeFiscal);
							});

						if (!string.IsNullOrEmpty(facture.Client?.Email))
							c.Item().Text(t =>
							{
								t.Span("Email : ").SemiBold();
								t.Span(facture.Client.Email);
							});

						if (!string.IsNullOrEmpty(facture.Client?.Telephone))
							c.Item().Text(t =>
							{
								t.Span("Tél : ").SemiBold();
								t.Span(facture.Client.Telephone);
							});

						if (!string.IsNullOrEmpty(facture.Client?.Adresse))
							c.Item().Text(t =>
							{
								t.Span("Adresse : ").SemiBold();
								t.Span(facture.Client.Adresse);
							});
					});

					col.Item().Height(16);

					// ── Tableau des lignes ────────────────────────────────────
					col.Item().Table(table =>
					{
						// Colonnes
						table.ColumnsDefinition(columns =>
						{
							columns.RelativeColumn(4);   // Désignation
							columns.RelativeColumn(1.5f); // Quantité
							columns.RelativeColumn(2);   // PU HT
							columns.RelativeColumn(1.5f); // TVA %
							columns.RelativeColumn(2);   // Montant HT
							columns.RelativeColumn(2);   // Montant TTC
						});

						// En-tête tableau
						static IContainer EnteteStyle(IContainer c) =>
							c.Background("#1b6ec2").Padding(6);

						table.Header(header =>
						{
                            header.Cell().Element(EnteteStyle)
								.Text("Désignation").Bold().FontColor(couleurBlanc);
                            header.Cell().Element(EnteteStyle).AlignCenter()
								.Text("Qté").Bold().FontColor(couleurBlanc);
                            header.Cell().Element(EnteteStyle).AlignRight()
								.Text("PU HT (DT)").Bold().FontColor(couleurBlanc);
                            header.Cell().Element(EnteteStyle).AlignCenter()
								.Text("TVA %").Bold().FontColor(couleurBlanc);
                            header.Cell().Element(EnteteStyle).AlignRight()
								.Text("Montant HT").Bold().FontColor(couleurBlanc);
                            header.Cell().Element(EnteteStyle).AlignRight()
								.Text("Montant TTC").Bold().FontColor(couleurBlanc);
						});

						// Lignes
                        var index = 0;
						foreach (var ligne in facture.Lignes)
						{
							var bg = index % 2 == 0 ? couleurBlanc : couleurLigneAlt;
							index++;

							IContainer CellStyle(IContainer c) =>
								c.Background(bg).BorderBottom(1).BorderColor("#dee2e6").Padding(6);

							table.Cell().Element(CellStyle)
								.Text(ligne.Designation).SemiBold();
							table.Cell().Element(CellStyle).AlignCenter()
								.Text(ligne.Quantite.ToString());
							table.Cell().Element(CellStyle).AlignRight()
								.Text(ligne.PrixUnitaireHT.ToString("F3"));
							table.Cell().Element(CellStyle).AlignCenter()
								.Text($"{ligne.TauxTVA} %");
							table.Cell().Element(CellStyle).AlignRight()
								.Text(ligne.MontantHT.ToString("F3"));
							table.Cell().Element(CellStyle).AlignRight()
								.Text(ligne.MontantTTC.ToString("F3")).SemiBold();
						}
					});

					col.Item().Height(20);

					// ── Récapitulatif totaux ──────────────────────────────────
					col.Item().AlignRight().Width(280).Column(recap =>
					{
						// TVA par taux
						foreach (var g in groupesTVA)
						{
							recap.Item().Row(row =>
							{
								row.RelativeItem().Text($"TVA {g.Taux} % :").FontColor(couleurGris);
								row.ConstantItem(100).AlignRight()
									.Text($"{g.MontantTVA:F3} DT").FontColor(couleurGris);
							});
						}

						recap.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#dee2e6");

						recap.Item().Row(row =>
						{
							row.RelativeItem().Text("Total HT :").SemiBold();
							row.ConstantItem(100).AlignRight()
								.Text($"{facture.TotalHT:F3} DT").SemiBold();
						});

						recap.Item().Row(row =>
						{
							row.RelativeItem().Text("Total TVA :").SemiBold();
							row.ConstantItem(100).AlignRight()
								.Text($"{facture.TotalTVA:F3} DT").SemiBold();
						});

						recap.Item().Row(row =>
						{
							row.RelativeItem().Text("Total TTC :").SemiBold();
							row.ConstantItem(100).AlignRight()
								.Text($"{facture.TotalTTC:F3} DT").SemiBold();
						});

						recap.Item().Row(row =>
						{
							row.RelativeItem().Text("Timbre fiscal :").SemiBold();
							row.ConstantItem(100).AlignRight()
								.Text($"{facture.Timbre:F3} DT").SemiBold();
						});

						recap.Item().PaddingVertical(4).LineHorizontal(2).LineColor(couleurPrimaire);

						// Net à payer — mis en valeur
						recap.Item().Background(couleurPrimaire).Padding(8).Row(row =>
						{
							row.RelativeItem().Text("NET À PAYER :").Bold()
								.FontColor(Colors.White).FontSize(12);
							row.ConstantItem(120).AlignRight()
								.Text($"{facture.NetAPayer:F3} DT").Bold()
								.FontColor(Colors.White).FontSize(12);
						});
					});
				});

				// ══ PIED DE PAGE ══════════════════════════════════════════════
				page.Footer().Column(col =>
				{
					col.Item().LineHorizontal(1).LineColor("#dee2e6");
					col.Item().PaddingTop(6).Row(row =>
					{
						row.RelativeItem().Text(t =>
						{
							t.Span("Facture N° ").FontColor(couleurGris);
							t.Span(facture.Numero).SemiBold().FontColor(couleurGris);
						});
						row.RelativeItem().AlignCenter()
							.Text(t =>
							{
								t.Span("Page ").FontColor(couleurGris);
								t.CurrentPageNumber().FontColor(couleurGris);
								t.Span(" / ").FontColor(couleurGris);
								t.TotalPages().FontColor(couleurGris);
							});
						row.RelativeItem().AlignRight()
							.Text("Document généré par FacturationApp").FontColor(couleurGris);
					});
				});
			});
		});

		return pdf.GeneratePdf();
	}
}