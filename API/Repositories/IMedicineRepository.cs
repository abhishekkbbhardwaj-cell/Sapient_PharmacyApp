using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Repositories;

public interface IMedicineRepository
{
    PagedResult<Medicine> GetAll(string? query, int pageNumber, int pageSize);
    IEnumerable<Medicine> Search(string query);
    Medicine Add(Medicine medicine);
}
