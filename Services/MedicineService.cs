using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class MedicineService
    {
        private readonly IMongoCollection<Medicine> _medicines;

        public MedicineService(IMongoDatabase database)
        {
            _medicines = database.GetCollection<Medicine>("Medicines");
        }

        public async Task<List<Medicine>> GetAllMedicinesAsync()
        {
            return await _medicines.Find(_ => true).ToListAsync();
        }

        public async Task<Medicine> GetMedicineByIdAsync(string id)
        {
            return await _medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Medicine> CreateMedicineAsync(Medicine medicine)
        {
            await _medicines.InsertOneAsync(medicine);
            return medicine;
        }

        public async Task UpdateMedicineAsync(string id, Medicine medicine)
        {
            await _medicines.ReplaceOneAsync(m => m.Id == id, medicine);
        }

        public async Task DeleteMedicineAsync(string id)
        {
            await _medicines.DeleteOneAsync(m => m.Id == id);
        }

        public async Task<List<Medicine>> GetMedicinesByCategoryAsync(MedicineCategory category)
        {
            return await _medicines.Find(m => m.Category == category).ToListAsync();
        }

        public async Task<List<Medicine>> GetLowStockMedicinesAsync()
        {
            return await _medicines.Find(m => m.StockQuantity <= m.MinimumStock).ToListAsync();
        }

        public async Task<List<Medicine>> GetNearExpiryMedicinesAsync()
        {
            var oneMonthFromNow = DateTime.UtcNow.AddMonths(1);
            return await _medicines.Find(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value <= oneMonthFromNow).ToListAsync();
        }

        public async Task UpdateStockAsync(string id, int quantity, string operation)
        {
            var medicine = await GetMedicineByIdAsync(id);
            if (medicine != null)
            {
                int newStock;
                if (operation == "add")
                {
                    newStock = medicine.StockQuantity + quantity;
                }
                else if (operation == "reduce")
                {
                    newStock = Math.Max(0, medicine.StockQuantity - quantity);
                }
                else
                {
                    newStock = quantity;
                }

                var update = Builders<Medicine>.Update
                    .Set(m => m.StockQuantity, newStock)
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);
                    
                await _medicines.UpdateOneAsync(m => m.Id == id, update);
            }
        }

        public async Task UpdatePriceAsync(string id, decimal price)
        {
            var update = Builders<Medicine>.Update
                .Set(m => m.Price, price)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
                
            await _medicines.UpdateOneAsync(m => m.Id == id, update);
        }
    }
}
