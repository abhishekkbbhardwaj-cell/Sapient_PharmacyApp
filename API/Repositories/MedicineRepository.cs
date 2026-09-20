using System.Text.Json;
using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private const int CacheDurationInSeconds = 10;

    private readonly string _filePath;
    private readonly object _syncRoot = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private List<Medicine>? _cache;
    private readonly Dictionary<string, (IEnumerable<Medicine> Results, DateTimeOffset ExpiresAt)> _searchCache = new(StringComparer.OrdinalIgnoreCase);

    public MedicineRepository(string? databaseDirectory = null)
    {
        databaseDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), "Database");
        Directory.CreateDirectory(databaseDirectory);

        _filePath = Path.Combine(databaseDirectory, "medicines.json");
        EnsureSeedData();
        _cache = LoadFromFile();
    }

    public PagedResult<Medicine> GetAll(string? query, int pageNumber, int pageSize)
    {
        var medicines = GetMedicines(query);
        var totalCount = medicines.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<Medicine>
        {
            Items = medicines
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private List<Medicine> GetMedicines(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return LoadFromFile();
        }

        var normalizedQuery = query.Trim();

        lock (_syncRoot)
        {
            if (_searchCache.TryGetValue(normalizedQuery, out var cachedResult) && cachedResult.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return cachedResult.Results.ToList();
            }

            var results = LoadFromFile()
                .Where(medicine => medicine.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _searchCache[normalizedQuery] = (results, DateTimeOffset.UtcNow.AddSeconds(CacheDurationInSeconds));
            return results;
        }
    }

    public IEnumerable<Medicine> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return LoadFromFile();
        }

        return GetMedicines(query);
    }

    public Medicine Add(Medicine medicine)
    {
        ArgumentNullException.ThrowIfNull(medicine);

        lock (_syncRoot)
        {
            var medicines = LoadFromFile();
            medicines.Add(medicine);
            SaveAll(medicines);
            _searchCache.Clear();
            return medicine;
        }
    }

    private List<Medicine> LoadFromFile()
    {
        lock (_syncRoot)
        {
            if (_cache != null)
            {
                return _cache;
            }

            if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
            {
                _cache = [];
                return _cache;
            }

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _cache = [];
                return _cache;
            }

            var medicines = JsonSerializer.Deserialize<List<Medicine>>(json);
            _cache = medicines ?? [];
            return _cache;
        }
    }

    private void SaveAll(List<Medicine> medicines)
    {
        lock (_syncRoot)
        {
            var json = JsonSerializer.Serialize(medicines, _jsonOptions);
            File.WriteAllText(_filePath, json);
            _cache = medicines;
        }
    }

    private void EnsureSeedData()
    {
        if (File.Exists(_filePath) && new FileInfo(_filePath).Length > 0)
        {
            return;
        }

        var medicines = new List<Medicine>
        {
            new Medicine
            {
                Name = "Paracetamol 500mg",
                Notes = "Used for pain relief and fever",
                ExpiryDate = new DateOnly(2028, 12, 31),
                Quantity = 120,
                Price = 25.50m,
                Brand = "Generic"
            },
            new Medicine
            {
                Name = "Amoxicillin 250mg",
                Notes = "Prescription antibiotic",
                ExpiryDate = new DateOnly(2027, 6, 30),
                Quantity = 80,
                Price = 140.00m,
                Brand = "MediCare"
            },
            new Medicine
            {
                Name = "Vitamin C 1000mg",
                Notes = "Immune support supplement",
                ExpiryDate = new DateOnly(2029, 3, 15),
                Quantity = 200,
                Price = 65.75m,
                Brand = "HealthPlus"
            }
        };

        SaveAll(medicines);
    }
}
