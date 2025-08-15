using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMongoCollection<Medicine> _medicines;

        public MedicineService(IMongoDatabase database)
        {
            _medicines = database.GetCollection<Medicine>("Medicines");
        }

        private static MedicineDto ToDto(Medicine m) => new()
        {
            Id = m.Id ?? string.Empty,
            Name = m.Name,
            Category = m.Category.ToString(),
            Description = m.Description,
            Price = m.Price,
            StockQuantity = m.StockQuantity,
            Manufacturer = m.Manufacturer,
            ExpiryDate = m.ExpiryDate,
            LotNumber = null,
            MinimumStockLevel = m.MinimumStock,
            RequiresPrescription = false,
            UsageInstructions = m.Instructions,
            SideEffects = null,
            // IsActive kaldırıldı
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            IsLowStock = m.StockQuantity <= m.MinimumStock,
            IsExpired = m.ExpiryDate.HasValue && m.ExpiryDate.Value < DateTime.UtcNow,
            IsExpiringSoon = m.ExpiryDate.HasValue && m.ExpiryDate.Value <= DateTime.UtcNow.AddDays(30) && m.ExpiryDate.Value >= DateTime.UtcNow
        };

        public async Task<List<MedicineDto>> GetAllMedicinesAsync()
        {
            var list = await _medicines.Find(_ => true).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<MedicineDto?> GetMedicineByIdAsync(string id)
        {
            var m = await _medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
            return m == null ? null : ToDto(m);
        }

        public async Task<List<MedicineDto>> GetActiveMedicinesAsync()
        {
            var list = await _medicines.Find(_ => true).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task CreateMedicineAsync(CreateMedicineDto dto)
        {
            var entity = new Medicine
            {
                Name = dto.Name,
                Category = Enum.TryParse<MedicineCategory>(dto.Category, true, out var cat) ? cat : MedicineCategory.Other,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Manufacturer = dto.Manufacturer ?? string.Empty,
                ExpiryDate = dto.ExpiryDate,
                MinimumStock = dto.MinimumStockLevel,
                Instructions = dto.UsageInstructions ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // IsActive kaldırıldı
            };
            await _medicines.InsertOneAsync(entity);
        }

        public async Task UpdateMedicineAsync(string id, UpdateMedicineDto dto)
        {
            var update = Builders<Medicine>.Update
                .Set(m => m.Name, dto.Name)
                .Set(m => m.Category, Enum.TryParse<MedicineCategory>(dto.Category, true, out var cat) ? cat : MedicineCategory.Other)
                .Set(m => m.Description, dto.Description ?? string.Empty)
                .Set(m => m.Price, dto.Price)
                .Set(m => m.StockQuantity, dto.StockQuantity)
                .Set(m => m.Manufacturer, dto.Manufacturer ?? string.Empty)
                .Set(m => m.ExpiryDate, dto.ExpiryDate)
                .Set(m => m.MinimumStock, dto.MinimumStockLevel)
                .Set(m => m.Instructions, dto.UsageInstructions ?? string.Empty)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }

        public async Task DeleteMedicineAsync(string id)
        {
            await _medicines.DeleteOneAsync(m => m.Id == id);
        }

        public async Task<List<MedicineDto>> SearchMedicinesAsync(string searchTerm)
        {
            var filter = Builders<Medicine>.Filter.Or(
                Builders<Medicine>.Filter.Regex(m => m.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Medicine>.Filter.Regex(m => m.GenericName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Medicine>.Filter.Regex(m => m.Manufacturer, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            var list = await _medicines.Find(filter).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<MedicineDto?> GetMedicineByNameAsync(string name)
        {
            var m = await _medicines.Find(m => m.Name == name).FirstOrDefaultAsync();
            return m == null ? null : ToDto(m);
        }

        public async Task<List<MedicineDto>> GetMedicinesByManufacturerAsync(string manufacturer)
        {
            var list = await _medicines.Find(m => m.Manufacturer == manufacturer).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<MedicineDto>> GetMedicinesByCategoryAsync(string category)
        {
            var list = await _medicines.Find(m => m.Category.ToString() == category).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task UpdateStockAsync(string id, int newStock)
        {
            var update = Builders<Medicine>.Update
                .Set(m => m.StockQuantity, newStock)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }

        public Task IncreaseStockAsync(string id, int amount) => AdjustStock(id, amount);
        public Task DecreaseStockAsync(string id, int amount) => AdjustStock(id, -amount);

        private async Task AdjustStock(string id, int delta)
        {
            var update = Builders<Medicine>.Update
                .Inc(m => m.StockQuantity, delta)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }

        public async Task<List<MedicineDto>> GetLowStockMedicinesAsync(int threshold = 10)
        {
            var list = await _medicines.Find(m => m.StockQuantity <= threshold).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<MedicineDto>> GetOutOfStockMedicinesAsync()
        {
            var list = await _medicines.Find(m => m.StockQuantity <= 0).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task UpdatePriceAsync(string id, decimal newPrice)
        {
            var update = Builders<Medicine>.Update
                .Set(m => m.Price, newPrice)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }

        public async Task<List<MedicineDto>> GetMedicinesByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var list = await _medicines.Find(m => m.Price >= minPrice && m.Price <= maxPrice).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<MedicineDto>> GetExpiringMedicinesAsync(int daysThreshold = 30)
        {
            var limit = DateTime.UtcNow.AddDays(daysThreshold);
            var list = await _medicines.Find(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value <= limit && m.ExpiryDate.Value >= DateTime.UtcNow).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<MedicineDto>> GetExpiredMedicinesAsync()
        {
            var list = await _medicines.Find(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value < DateTime.UtcNow).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task UpdateExpiryDateAsync(string id, DateTime newExpiryDate)
        {
            var update = Builders<Medicine>.Update.Set(m => m.ExpiryDate, newExpiryDate).Set(m => m.UpdatedAt, DateTime.UtcNow);
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            var cats = await _medicines.Distinct<string>(nameof(Medicine.Category), Builders<Medicine>.Filter.Empty).ToListAsync();
            return cats;
        }

        public async Task<Dictionary<string, int>> GetMedicineCountByCategoryAsync()
        {
            var groups = await _medicines.Aggregate()
                .Group(m => m.Category, g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();
            return groups.ToDictionary(g => g.Category.ToString(), g => g.Count);
        }

        public async Task<int> GetTotalMedicinesCountAsync() => (int)await _medicines.CountDocumentsAsync(_ => true);
    public async Task<int> GetActiveMedicinesCountAsync() => (int)await _medicines.CountDocumentsAsync(_ => true);
        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var list = await _medicines.Find(_ => true).Project(m => m.Price * m.StockQuantity).ToListAsync();
            return list.Sum();
        }

        public async Task<Dictionary<string, object>> GetMedicineStatisticsAsync()
        {
            return new Dictionary<string, object>
            {
                ["Total"] = await GetTotalMedicinesCountAsync(),
                ["Active"] = await GetActiveMedicinesCountAsync(),
                ["LowStock"] = (await GetLowStockMedicinesAsync()).Count,
                ["Expired"] = (await GetExpiredMedicinesAsync()).Count
            };
        }

        public async Task<bool> IsMedicineNameUniqueAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Medicine>.Filter.Eq(m => m.Name, name);
            if (!string.IsNullOrEmpty(excludeId))
                filter &= Builders<Medicine>.Filter.Ne(m => m.Id, excludeId);
            var count = await _medicines.CountDocumentsAsync(filter);
            return count == 0;
        }

        public async Task<bool> IsStockSufficientAsync(string id, int requiredAmount)
        {
            var m = await _medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
            return m != null && m.StockQuantity >= requiredAmount;
        }

        public async Task<bool> IsMedicineExpiredAsync(string id)
        {
            var m = await _medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
            return m != null && m.ExpiryDate.HasValue && m.ExpiryDate.Value < DateTime.UtcNow;
        }

        public async Task<List<MedicineDto>> GetMedicinesByIdsAsync(List<string> ids)
        {
            var list = await _medicines.Find(m => ids.Contains(m.Id!)).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task BulkUpdateStockAsync(Dictionary<string, int> stockUpdates)
        {
            var writes = new List<WriteModel<Medicine>>();
            foreach (var kv in stockUpdates)
            {
                var update = Builders<Medicine>.Update
                    .Set(m => m.StockQuantity, kv.Value)
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);
                writes.Add(new UpdateOneModel<Medicine>(Builders<Medicine>.Filter.Eq(m => m.Id, kv.Key), update));
            }
            if (writes.Any()) await _medicines.BulkWriteAsync(writes);
        }

        public async Task BulkUpdatePricesAsync(Dictionary<string, decimal> priceUpdates)
        {
            var writes = new List<WriteModel<Medicine>>();
            foreach (var kv in priceUpdates)
            {
                var update = Builders<Medicine>.Update
                    .Set(m => m.Price, kv.Value)
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);
                writes.Add(new UpdateOneModel<Medicine>(Builders<Medicine>.Filter.Eq(m => m.Id, kv.Key), update));
            }
            if (writes.Any()) await _medicines.BulkWriteAsync(writes);
        }

        public Task<bool> IsMedicinePrescriptionOnlyAsync(string id)
        {
            // Basit örnek: Category Antibiotic ise reçeteli varsayalım
            return Task.FromResult(true);
        }

        public async Task<List<MedicineDto>> GetPrescriptionMedicinesAsync()
        {
            var list = await _medicines.Find(m => m.Category == MedicineCategory.Antibiotic).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<MedicineDto>> GetOverTheCounterMedicinesAsync()
        {
            var list = await _medicines.Find(m => m.Category != MedicineCategory.Antibiotic).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<int> GetLowStockMedicinesCountAsync() => (await GetLowStockMedicinesAsync()).Count;
        public async Task<int> GetExpiredMedicinesCountAsync() => (await GetExpiredMedicinesAsync()).Count;
        public async Task<int> GetExpiringSoonMedicinesCountAsync() => (await GetExpiringMedicinesAsync()).Count;
    }
}
