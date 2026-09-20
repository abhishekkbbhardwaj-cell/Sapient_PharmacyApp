namespace PharmacyApp.Api.Contracts.Medicines;

public sealed class MedicineListResponse
{
    public IReadOnlyList<MedicineResponse> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}