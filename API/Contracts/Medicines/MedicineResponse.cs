namespace PharmacyApp.Api.Contracts.Medicines;

public sealed class MedicineResponse
{
    public string Name { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateOnly ExpiryDate { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string Brand { get; init; } = string.Empty;
}