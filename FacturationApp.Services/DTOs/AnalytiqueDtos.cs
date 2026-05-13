namespace FacturationApp.Services.DTOs;

/// <summary>TVA collectée par taux.</summary>
public class TVAParTauxDto
{
    public decimal Taux { get; set; }
    public decimal MontantTVA { get; set; }
}

/// <summary>Chiffre d'affaires par mois.</summary>
public class CAParMoisDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string LibelleMois { get; set; } = string.Empty;
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>Chiffre d'affaires par client.</summary>
public class CAParClientDto
{
    public int ClientId { get; set; }
    public string NomClient { get; set; } = string.Empty;
    public decimal TotalHT { get; set; }
    public int NombreFactures { get; set; }
}

/// <summary>Chiffre d'affaires par produit.</summary>
public class CAParProduitDto
{
    public int ProduitId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal TotalHT { get; set; }
    public decimal QuantiteTotale { get; set; }
}

/// <summary>KPI globaux pour les cards du dashboard.</summary>
public class KpiGlobalDto
{
    public decimal CATotalHT { get; set; }
    public decimal CATotalTTC { get; set; }
    public int NombreFacturesValidees { get; set; }
    public decimal TVATotale { get; set; }
    public decimal TimbreTotal { get; set; }
}