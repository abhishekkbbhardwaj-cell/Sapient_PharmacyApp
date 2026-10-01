using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Repositories;

public interface IMedicineRepository
{
    Task<PagedResult<Medicine>> GetAllAsync(string? query, int pageNumber, int pageSize);
    Task<IEnumerable<Medicine>> SearchAsync(string query);
    Task<Medicine> AddAsync(Medicine medicine);
}
