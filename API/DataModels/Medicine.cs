namespace PharmacyApp.Api.DataModels;

public class Medicine
{
    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string Brand { get; set; } = string.Empty;
}
