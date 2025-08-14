using HospitalManagementSystem.Models;
using MongoDB.Driver;

namespace HospitalManagementSystem.Services
{
    public interface IMedicineService
    {
        Task<List<Medicine>> GetAllMedicinesAsync();
        Task<Medicine?> GetMedicineByIdAsync(string id);
        Task<List<Medicine>> GetMedicinesByCategoryAsync(MedicineCategory category);
        Task<List<Medicine>> GetLowStockMedicinesAsync();
        Task<List<Medicine>> SearchMedicinesAsync(string searchTerm);
        Task<Medicine> CreateMedicineAsync(Medicine medicine);
        Task<bool> UpdateMedicineAsync(string id, Medicine medicine);
        Task<bool> UpdateStockAsync(string id, int newQuantity);
        Task<bool> DeleteMedicineAsync(string id);
        Task<List<MedicineTransaction>> GetMedicineTransactionsAsync(string medicineId);
        Task<MedicineTransaction> CreateTransactionAsync(MedicineTransaction transaction);
    }

    public class MedicineService : IMedicineService
    {
        private readonly IMongoCollection<Medicine> _medicines;
        private readonly IMongoCollection<MedicineTransaction> _transactions;

        public MedicineService(IMongoDbContext context)
        {
            _medicines = context.GetCollection<Medicine>("Medicines");
            _transactions = context.GetCollection<MedicineTransaction>("MedicineTransactions");
        }

        public async Task<List<Medicine>> GetAllMedicinesAsync()
        {
            return await _medicines.Find(x => x.IsActive).ToListAsync();
        }

        public async Task<Medicine?> GetMedicineByIdAsync(string id)
        {
            return await _medicines.Find(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<List<Medicine>> GetMedicinesByCategoryAsync(MedicineCategory category)
        {
            return await _medicines.Find(x => x.Category == category && x.IsActive).ToListAsync();
        }

        public async Task<List<Medicine>> GetLowStockMedicinesAsync()
        {
            return await _medicines.Find(x => x.StockQuantity <= x.MinimumStock && x.IsActive).ToListAsync();
        }

        public async Task<List<Medicine>> SearchMedicinesAsync(string searchTerm)
        {
            var filter = Builders<Medicine>.Filter.And(
                Builders<Medicine>.Filter.Eq(x => x.IsActive, true),
                Builders<Medicine>.Filter.Or(
                    Builders<Medicine>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Medicine>.Filter.Regex(x => x.GenericName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Medicine>.Filter.Regex(x => x.Manufacturer, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            return await _medicines.Find(filter).ToListAsync();
        }

        public async Task<Medicine> CreateMedicineAsync(Medicine medicine)
        {
            medicine.CreatedAt = DateTime.UtcNow;
            medicine.UpdatedAt = DateTime.UtcNow;
            
            await _medicines.InsertOneAsync(medicine);
            return medicine;
        }

        public async Task<bool> UpdateMedicineAsync(string id, Medicine medicine)
        {
            medicine.UpdatedAt = DateTime.UtcNow;
            var result = await _medicines.ReplaceOneAsync(x => x.Id == id, medicine);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStockAsync(string id, int newQuantity)
        {
            var update = Builders<Medicine>.Update
                .Set(x => x.StockQuantity, newQuantity)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            
            var result = await _medicines.UpdateOneAsync(x => x.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMedicineAsync(string id)
        {
            var update = Builders<Medicine>.Update.Set(x => x.IsActive, false);
            var result = await _medicines.UpdateOneAsync(x => x.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<List<MedicineTransaction>> GetMedicineTransactionsAsync(string medicineId)
        {
            return await _transactions.Find(x => x.MedicineId == medicineId)
                .SortByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        public async Task<MedicineTransaction> CreateTransactionAsync(MedicineTransaction transaction)
        {
            // Get current medicine stock
            var medicine = await GetMedicineByIdAsync(transaction.MedicineId);
            if (medicine != null)
            {
                transaction.QuantityBefore = medicine.StockQuantity;
                
                // Calculate new quantity based on transaction type
                int newQuantity = medicine.StockQuantity;
                switch (transaction.Type)
                {
                    case TransactionType.StockIn:
                        newQuantity += transaction.Quantity;
                        break;
                    case TransactionType.StockOut:
                    case TransactionType.Dispensed:
                    case TransactionType.Expired:
                    case TransactionType.Damaged:
                        newQuantity -= transaction.Quantity;
                        break;
                    case TransactionType.Adjustment:
                        newQuantity = transaction.Quantity; // Direct assignment for adjustments
                        break;
                }

                transaction.QuantityAfter = newQuantity;
                transaction.CreatedAt = DateTime.UtcNow;

                // Insert transaction
                await _transactions.InsertOneAsync(transaction);

                // Update medicine stock
                await UpdateStockAsync(transaction.MedicineId, newQuantity);
            }

            return transaction;
        }
    }
}
