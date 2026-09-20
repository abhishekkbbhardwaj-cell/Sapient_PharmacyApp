using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Api.Contracts.Medicines;
using PharmacyApp.Api.DataModels;
using PharmacyApp.Api.Repositories;

namespace PharmacyApp.Api.Controllers;

[ApiController]
[Route("api/medicine")]
public class MedicineController : ControllerBase
{
    private readonly IMedicineRepository _medicineRepository;

    public MedicineController(IMedicineRepository medicineRepository)
    {
        _medicineRepository = medicineRepository;
    }

    [HttpGet]
    public ActionResult<MedicineListResponse> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("pageNumber must be at least 1 and pageSize must be between 1 and 100.");
        }

        var medicines = _medicineRepository.GetAll(query, pageNumber, pageSize);

        return Ok(new MedicineListResponse
        {
            Items = medicines.Items.Select(ToResponse).ToList(),
            PageNumber = medicines.PageNumber,
            PageSize = medicines.PageSize,
            TotalCount = medicines.TotalCount,
            TotalPages = medicines.TotalPages
        });
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<MedicineResponse>> Search([FromQuery] string query)
    {
        var medicines = _medicineRepository.Search(query).Select(ToResponse);
        return Ok(medicines);
    }

    [HttpPost]
    public ActionResult<MedicineResponse> AddMedicine([FromBody] CreateMedicineRequest request)
    {
        if (request.ExpiryDate <= DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(request.ExpiryDate), "Expiry date must be in the future.");
            return ValidationProblem(ModelState);
        }

        var medicine = new Medicine
        {
            Name = request.Name,
            Notes = request.Notes,
            ExpiryDate = request.ExpiryDate,
            Quantity = request.Quantity,
            Price = request.Price,
            Brand = request.Brand
        };

        var createdMedicine = _medicineRepository.Add(medicine);
        return StatusCode(StatusCodes.Status201Created, ToResponse(createdMedicine));
    }

    private static MedicineResponse ToResponse(Medicine medicine)
    {
        return new MedicineResponse
        {
            Name = medicine.Name,
            Notes = medicine.Notes,
            ExpiryDate = medicine.ExpiryDate,
            Quantity = medicine.Quantity,
            Price = medicine.Price,
            Brand = medicine.Brand
        };
    }
}
