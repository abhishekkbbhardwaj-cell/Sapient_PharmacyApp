using System.Text.Json;
using PharmacyApp.Api.DataModels;

namespace PharmacyApp.Api.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private const int CacheDurationInSeconds = 10;

    private readonly string _filePath;
    private readonly ReaderWriterLockSlim _lock = new();
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
         EnsureSeedDataAsync().ConfigureAwait(true);
        _cache =  LoadFromFileAsync().ConfigureAwait(true).GetAwaiter().GetResult();
    }

    public async Task<PagedResult<Medicine>> GetAllAsync(string? query, int pageNumber, int pageSize)
    {
        var medicines = await GetMedicinesAsync(query);
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

    private async Task<List<Medicine>> GetMedicinesAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await LoadFromFileWithoutLockAsync();
        }

        var normalizedQuery = query.Trim();

        _lock.EnterReadLock();
        try
        {
            if (_searchCache.TryGetValue(normalizedQuery, out var cachedResult) && cachedResult.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return cachedResult.Results.ToList();
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        var results = await LoadFromFileWithoutLockAsync();
        results = results.Where(medicine => medicine.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _lock.EnterWriteLock();
        try
        {
            _searchCache[normalizedQuery] = (results, DateTimeOffset.UtcNow.AddSeconds(CacheDurationInSeconds));
            return results;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<IEnumerable<Medicine>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await LoadFromFileWithoutLockAsync();
        }

        return await GetMedicinesAsync(query);
    }

    public async Task<Medicine> AddAsync(Medicine medicine)
    {
        ArgumentNullException.ThrowIfNull(medicine);

        _lock.EnterWriteLock();
        try
        {
            var medicines = await LoadFromFileWithoutLockAsync();
            medicines.Add(medicine);
            _searchCache.Clear();
            await SaveAllAsync(medicines);
            return medicine;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }


    private async Task<List<Medicine>> LoadFromFileAsync()
    {
        if (_lock.IsWriteLockHeld)
        {
            return await LoadFromFileWithoutLockAsync();
        }

        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_cache != null)
            {
                return _cache.ToList();
            }

            if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
            {
                _cache = [];
                return [];
            }

            var medicines = await LoadFromFileWithoutLockAsync();

            _lock.EnterWriteLock();
            try
            {
                _cache ??= medicines;
                return _cache.ToList();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    private async Task<List<Medicine>> LoadFromFileWithoutLockAsync()
    {
        if (_cache != null)
        {
            return _cache.ToList();
        }

        if (!File.Exists(_filePath) || new FileInfo(_filePath).Length == 0)
        {
            _cache = [];
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        _cache = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<Medicine>>(json) ?? [];
        return _cache.ToList();
    }

    private async Task SaveAllAsync(List<Medicine> medicines)
    {
        var json = JsonSerializer.Serialize(medicines, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
        _cache = medicines;
    }

    private async Task EnsureSeedDataAsync()
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

        await SaveAllAsync(medicines);
    }
}
