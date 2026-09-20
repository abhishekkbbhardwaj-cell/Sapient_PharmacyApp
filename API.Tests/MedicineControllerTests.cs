using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Api.Contracts.Medicines;
using PharmacyApp.Api.Controllers;
using PharmacyApp.Api.DataModels;
using PharmacyApp.Api.Repositories;
using Xunit;

namespace PharmacyApp.Api.Tests;

public sealed class MedicineControllerTests
{
    [Fact]
    public void GetAll_ReturnsOkWithPagedResponse()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        var result = controller.GetAll();

        var response = Assert.IsType<MedicineListResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(1, response.PageNumber);
        Assert.Single(response.Items);
    }

    [Fact]
    public void GetAll_PassesQueryAndPaginationToRepository()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        controller.GetAll("para", 2, 5);

        Assert.Equal(("para", 2, 5), repository.LastGetAll);
    }

    [Fact]
    public void GetAll_RejectsPageNumberBelowOne()
    {
        var controller = new MedicineController(new FakeMedicineRepository());

        var result = controller.GetAll(pageNumber: 0);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetAll_RejectsPageSizeAboveOneHundred()
    {
        var controller = new MedicineController(new FakeMedicineRepository());

        var result = controller.GetAll(pageSize: 101);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Search_ReturnsMatchingResponseItems()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        var result = controller.Search("para");

        var response = Assert.IsAssignableFrom<IEnumerable<MedicineResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value).ToList();
        Assert.Single(response);
        Assert.Equal("Paracetamol", response[0].Name);
    }

    [Fact]
    public void Search_PassesQueryToRepository()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);

        controller.Search("para");

        Assert.Equal("para", repository.LastSearch);
    }

    [Fact]
    public void AddMedicine_ReturnsCreatedResponse()
    {
        var controller = new MedicineController(new FakeMedicineRepository());
        var request = ValidRequest();

        var result = controller.AddMedicine(request);

        var response = Assert.IsType<MedicineResponse>(Assert.IsType<ObjectResult>(result.Result).Value);
        Assert.Equal(StatusCodes.Status201Created, Assert.IsType<ObjectResult>(result.Result).StatusCode);
        Assert.Equal(request.Name, response.Name);
    }

    [Fact]
    public void AddMedicine_MapsAllRequestFields()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);
        var request = ValidRequest();

        controller.AddMedicine(request);

        Assert.NotNull(repository.AddedMedicine);
        Assert.Equal(request.Name, repository.AddedMedicine.Name);
        Assert.Equal(request.Brand, repository.AddedMedicine.Brand);
        Assert.Equal(request.Notes, repository.AddedMedicine.Notes);
        Assert.Equal(request.Quantity, repository.AddedMedicine.Quantity);
        Assert.Equal(request.Price, repository.AddedMedicine.Price);
        Assert.Equal(request.ExpiryDate, repository.AddedMedicine.ExpiryDate);
    }

    [Fact]
    public void AddMedicine_RejectsPastExpiryDate()
    {
        var controller = new MedicineController(new FakeMedicineRepository());
        var request = InvalidExpiryRequest();

        var result = controller.AddMedicine(request);

        var problem = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result.Result).Value);
        Assert.Contains(nameof(CreateMedicineRequest.ExpiryDate), problem.Errors.Keys);
    }

    [Fact]
    public void AddMedicine_DoesNotWriteWhenExpiryIsInvalid()
    {
        var repository = new FakeMedicineRepository();
        var controller = new MedicineController(repository);
        var request = InvalidExpiryRequest();

        controller.AddMedicine(request);

        Assert.Null(repository.AddedMedicine);
    }

    private static CreateMedicineRequest ValidRequest()
    {
        return new CreateMedicineRequest
        {
            Name = "Paracetamol",
            Brand = "Generic",
            Notes = "Pain relief",
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            Quantity = 20,
            Price = 25.50m
        };
    }

    private static CreateMedicineRequest InvalidExpiryRequest()
    {
        return new CreateMedicineRequest
        {
            Name = "Paracetamol",
            Brand = "Generic",
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today),
            Quantity = 20,
            Price = 25.50m
        };
    }

    private sealed class FakeMedicineRepository : IMedicineRepository
    {
        private readonly Medicine _medicine = new()
        {
            Name = "Paracetamol",
            Brand = "Generic",
            ExpiryDate = new DateOnly(2099, 12, 31),
            Quantity = 20,
            Price = 25
        };

        public (string? Query, int PageNumber, int PageSize)? LastGetAll { get; private set; }
        public string? LastSearch { get; private set; }
        public Medicine? AddedMedicine { get; private set; }

        public PagedResult<Medicine> GetAll(string? query, int pageNumber, int pageSize)
        {
            LastGetAll = (query, pageNumber, pageSize);
            return new PagedResult<Medicine>
            {
                Items = [_medicine],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 1,
                TotalPages = 1
            };
        }

        public IEnumerable<Medicine> Search(string query)
        {
            LastSearch = query;
            return [_medicine];
        }

        public Medicine Add(Medicine medicine)
        {
            AddedMedicine = medicine;
            return medicine;
        }
    }
}
