using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class MedicationService : IMedicationService
    {
        private readonly IMongoCollection<Medication> _medications;

        public MedicationService(IMongoDatabase database)
        {
            _medications = database.GetCollection<Medication>("Medications");
        }

        public async Task<List<Medication>> GetAllMedicationsAsync()
        {
            return await _medications.Find(_ => true).ToListAsync();
        }

        public async Task<Medication?> GetMedicationByIdAsync(string id)
        {
            return await _medications.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Medication?> GetMedicationByNameAsync(string name)
        {
            return await _medications.Find(m => m.Name == name).FirstOrDefaultAsync();
        }

        public async Task CreateMedicationAsync(Medication medication)
        {
            medication.CreatedAt = DateTime.UtcNow;
            medication.UpdatedAt = DateTime.UtcNow;
            await _medications.InsertOneAsync(medication);
        }

        public async Task UpdateMedicationAsync(string id, Medication medication)
        {
            medication.UpdatedAt = DateTime.UtcNow;
            await _medications.ReplaceOneAsync(m => m.Id == id, medication);
        }

        public async Task DeleteMedicationAsync(string id)
        {
            await _medications.DeleteOneAsync(m => m.Id == id);
        }

        public async Task<List<Medication>> SearchMedicationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllMedicationsAsync();

            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filter = Builders<Medication>.Filter.Regex(m => m.Name, regex);
            return await _medications.Find(filter).ToListAsync();
        }

        public async Task<List<Medication>> GetMedicationsByTypeAsync(string type)
        {
            // Type alanı örnekte yok; varsayımsal olarak Name veya Description içinde arama yapılabilir.
            var regex = new MongoDB.Bson.BsonRegularExpression(type, "i");
            var filter = Builders<Medication>.Filter.Or(
                Builders<Medication>.Filter.Regex(m => m.Description, regex),
                Builders<Medication>.Filter.Regex(m => m.Name, regex)
            );
            return await _medications.Find(filter).ToListAsync();
        }

        public async Task<List<Medication>> GetMedicationsByManufacturerAsync(string manufacturer)
        {
            return await _medications.Find(m => m.Manufacturer == manufacturer).ToListAsync();
        }

        public async Task<int> GetTotalMedicationsCountAsync()
        {
            return (int)await _medications.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> IsMedicationAvailableAsync(string medicationId, int requiredQuantity)
        {
            var med = await _medications.Find(m => m.Id == medicationId).FirstOrDefaultAsync();
            return med != null && med.StockQuantity >= requiredQuantity && med.IsActive;
        }

        public async Task<bool> UpdateMedicationStockAsync(string medicationId, int quantity)
        {
            var update = Builders<Medication>.Update
                .Inc(m => m.StockQuantity, quantity)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);
            var result = await _medications.UpdateOneAsync(m => m.Id == medicationId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Medication>> GetLowStockMedicationsAsync()
        {
            return await _medications.Find(m => m.StockQuantity <= m.MinimumStockLevel).ToListAsync();
        }

        public async Task<List<Medication>> GetExpiredMedicationsAsync()
        {
            var now = DateTime.UtcNow;
            return await _medications.Find(m => m.ExpiryDate != null && m.ExpiryDate <= now).ToListAsync();
        }

        public async Task<List<Medication>> GetExpiringMedicationsAsync(int daysFromNow)
        {
            var target = DateTime.UtcNow.AddDays(daysFromNow);
            var now = DateTime.UtcNow;
            return await _medications.Find(m => m.ExpiryDate != null && m.ExpiryDate > now && m.ExpiryDate <= target).ToListAsync();
        }
    }
}
