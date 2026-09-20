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
        var result = _repository.GetAll(null, 1, 10);

        Assert.Equal(3, result.TotalCount);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public void GetAllReturnsRequestedPageSize()
    {
        var result = _repository.GetAll(null, 1, 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public void GetAllReturnsCorrectSecondPage()
    {
        var result = _repository.GetAll(null, 2, 2);

        Assert.Single(result.Items);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public void GetAllCalculatesTotalPages()
    {
        var result = _repository.GetAll(null, 1, 2);

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void GetAllWithQueryFiltersByName()
    {
        var result = _repository.GetAll("amoxicillin", 1, 10);

        Assert.Single(result.Items);
        Assert.Equal("Amoxicillin 250mg", result.Items[0].Name);
    }

    [Fact]
    public void SearchIsCaseInsensitive()
    {
        var result = _repository.Search("PARACETAMOL").ToList();

        Assert.Single(result);
        Assert.Equal("Paracetamol 500mg", result[0].Name);
    }

    [Fact]
    public void SearchMatchesNameOnly()
    {
        var result = _repository.Search("fever").ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SearchUsesCacheForRepeatedQuery()
    {
        var firstResult = _repository.Search("para").ToList();
        var secondResult = _repository.Search("PARA").ToList();

        Assert.Equal(firstResult.Select(medicine => medicine.Name), secondResult.Select(medicine => medicine.Name));
    }

    [Fact]
    public void AddPersistsMedicine()
    {
        var medicine = NewMedicine("Test Medicine");

        _repository.Add(medicine);
        var result = _repository.GetAll(null, 1, 10);

        Assert.Contains(result.Items, item => item.Name == "Test Medicine");
    }

    [Fact]
    public void AddClearsSearchCache()
    {
        var initialResults = _repository.Search("test").ToList();
        _repository.Add(NewMedicine("Test Medicine"));

        var updatedResults = _repository.Search("test").ToList();

        Assert.Empty(initialResults);
        Assert.Single(updatedResults);
    }

    [Fact]
    public void AddRejectsNullMedicine()
    {
        Assert.Throws<ArgumentNullException>(() => _repository.Add(null!));
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
