using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Api.Contracts.Medicines;

public sealed class CreateMedicineRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public required string Name { get; init; }

    [MaxLength(1000)]
    public string? Notes { get; init; }

    [Required]
    public DateOnly ExpiryDate { get; init; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; init; }

    [Range(0.01, 9999999.99)]
    public decimal Price { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public required string Brand { get; init; }
}