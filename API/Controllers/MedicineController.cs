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
    public async Task<ActionResult<MedicineListResponse>> GetAllAsync(
        [FromQuery] string? query = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("pageNumber must be at least 1 and pageSize must be between 1 and 100.");
        }

        var medicines = await _medicineRepository.GetAllAsync(query, pageNumber, pageSize);

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
    public async Task<ActionResult<IEnumerable<MedicineResponse>>> SearchAsync([FromQuery] string query)
    {
        var medicines = (await _medicineRepository.SearchAsync(query)).Select(ToResponse);
        return Ok(medicines);
    }

    [HttpPost("add")]
    public async Task<ActionResult<MedicineResponse>> AddMedicineAsync([FromBody] CreateMedicineRequest request)
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

        var createdMedicine = await _medicineRepository.AddAsync(medicine);
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
