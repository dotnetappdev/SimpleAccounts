namespace SimpleAccounts.Data.Services;

public interface ITaxService
{
    decimal CalculateTax(decimal amount, string country);
    TaxInfo GetTaxInfo(string country);
}

public class TaxService : ITaxService
{
    public decimal CalculateTax(decimal amount, string country)
    {
        var taxRate = GetTaxRate(country);
        return amount * taxRate;
    }

    public TaxInfo GetTaxInfo(string country)
    {
        return country.ToUpperInvariant() switch
        {
            "UK" => new TaxInfo { Country = "UK", TaxName = "VAT", TaxRate = 0.20m },
            "USA" => new TaxInfo { Country = "USA", TaxName = "Sales Tax", TaxRate = 0.075m }, // Average US sales tax
            _ => new TaxInfo { Country = country, TaxName = "Tax", TaxRate = 0.00m }
        };
    }

    private decimal GetTaxRate(string country)
    {
        return country.ToUpperInvariant() switch
        {
            "UK" => 0.20m,    // 20% VAT
            "USA" => 0.075m,  // 7.5% average sales tax (varies by state)
            _ => 0.00m
        };
    }
}

public class TaxInfo
{
    public string Country { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
}
