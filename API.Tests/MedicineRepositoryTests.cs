using PharmacyApp.Api.DataModels;
using PharmacyApp.Api.Repositories;
using Xunit;

namespace PharmacyApp.Api.Tests;

public sealed class MedicineRepositoryTests : IDisposable
{
    private readonly string _databaseDirectory = Path.Combine(Path.GetTempPath(), "pharmacy-tests", Guid.NewGuid().ToString("N"));
    private readonly MedicineRepository _repository;

    public MedicineRepositoryTests()
    {
        _repository = new MedicineRepository(_databaseDirectory);
    }

    [Fact]
    public void ConstructorCreatesSeedData()
    {
        var result = _repository.GetAllAsync(null, 1, 10).GetAwaiter().GetResult();

        Assert.Equal(3, result.TotalCount);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetAllReturnsRequestedPageSize()
    {
        var result = await _repository.GetAllAsync(null, 1, 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetAllReturnsCorrectSecondPage()
    {
        var result = await _repository.GetAllAsync(null, 2, 2);

        Assert.Single(result.Items);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task GetAllCalculatesTotalPages()
    {
        var result = await _repository.GetAllAsync(null, 1, 2);

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllWithQueryFiltersByName()
    {
        var result = await _repository.GetAllAsync("amoxicillin", 1, 10);

        Assert.Single(result.Items);
        Assert.Equal("Amoxicillin 250mg", result.Items[0].Name);
    }

    [Fact]
    public async Task SearchIsCaseInsensitive()
    {
        var result = (await _repository.SearchAsync("PARACETAMOL")).ToList();

        Assert.Single(result);
        Assert.Equal("Paracetamol 500mg", result[0].Name);
    }

    [Fact]
    public async Task SearchMatchesNameOnly()
    {
        var result = (await _repository.SearchAsync("fever")).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchUsesCacheForRepeatedQuery()
    {
        var firstResult = (await _repository.SearchAsync("para")).ToList();
        var secondResult = (await _repository.SearchAsync("PARA")).ToList();

        Assert.Equal(firstResult.Select(medicine => medicine.Name), secondResult.Select(medicine => medicine.Name));
    }

    [Fact]
    public async Task AddPersistsMedicine()
    {
        var medicine = NewMedicine("Test Medicine");

        await _repository.AddAsync(medicine);
        var result = await _repository.GetAllAsync(null, 1, 10);

        Assert.Contains(result.Items, item => item.Name == "Test Medicine");
    }

    [Fact]
    public async Task AddClearsSearchCache()
    {
        var initialResults = (await _repository.SearchAsync("test")).ToList();
        await _repository.AddAsync(NewMedicine("Test Medicine"));

        var updatedResults = (await _repository.SearchAsync("test")).ToList();

        Assert.Empty(initialResults);
        Assert.Single(updatedResults);
    }

    [Fact]
    public async Task AddRejectsNullMedicine()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _repository.AddAsync(null!));
    }

    public void Dispose()
    {
        if (Directory.Exists(_databaseDirectory))
        {
            Directory.Delete(_databaseDirectory, recursive: true);
        }
    }

    private static Medicine NewMedicine(string name)
    {
        return new Medicine
        {
            Name = name,
            Brand = "Test Brand",
            Notes = "Test notes",
            ExpiryDate = new DateOnly(2099, 12, 31),
            Quantity = 10,
            Price = 10
        };
    }
}
