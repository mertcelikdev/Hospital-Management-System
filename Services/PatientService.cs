using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class PatientService
    {
        private readonly IMongoCollection<Patient> _patients;

        public PatientService(IMongoDatabase database)
        {
            _patients = database.GetCollection<Patient>("patients");
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _patients.Find(_ => true)
                .SortBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(string id)
        {
            return await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
        {
            var filter = Builders<Patient>.Filter.Or(
                Builders<Patient>.Filter.Regex(p => p.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Patient>.Filter.Regex(p => p.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Patient>.Filter.Regex(p => p.IdentityNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Patient>.Filter.Regex(p => p.PhoneNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _patients.Find(filter)
                .SortBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task CreatePatientAsync(Patient patient)
        {
            patient.Id = null; // Let MongoDB generate the ID
            patient.CreatedAt = DateTime.UtcNow;
            patient.UpdatedAt = DateTime.UtcNow;
            await _patients.InsertOneAsync(patient);
        }

        public async Task<bool> UpdatePatientAsync(string id, Patient patient)
        {
            patient.Id = id;
            patient.UpdatedAt = DateTime.UtcNow;
            var result = await _patients.ReplaceOneAsync(p => p.Id == id, patient);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePatientAsync(string id)
        {
            var result = await _patients.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> PatientExistsAsync(string identityNumber)
        {
            var count = await _patients.CountDocumentsAsync(p => p.IdentityNumber == identityNumber);
            return count > 0;
        }

        public async Task<Patient?> GetPatientByIdentityNumberAsync(string identityNumber)
        {
            return await _patients.Find(p => p.IdentityNumber == identityNumber).FirstOrDefaultAsync();
        }

        public async Task<List<Patient>> GetRecentPatientsAsync(int count = 10)
        {
            return await _patients.Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .Limit(count)
                .ToListAsync();
        }

        public async Task<int> GetPatientsCountAsync()
        {
            return (int)await _patients.CountDocumentsAsync(_ => true);
        }
    }
}
