using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class TreatmentService
    {
        private readonly IMongoCollection<Treatment> _treatments;

        public TreatmentService(IMongoDatabase database)
        {
            _treatments = database.GetCollection<Treatment>("Treatments");
        }

        public async Task<List<Treatment>> GetAllTreatmentsAsync()
        {
            return await _treatments.Find(_ => true).ToListAsync();
        }

        public async Task<Treatment> GetTreatmentByIdAsync(string id)
        {
            return await _treatments.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Treatment>> GetPatientTreatmentsAsync(string patientId)
        {
            return await _treatments.Find(t => t.PatientId == patientId).ToListAsync();
        }

        public async Task<List<Treatment>> GetDoctorTreatmentsAsync(string doctorId)
        {
            return await _treatments.Find(t => t.DoctorId == doctorId).ToListAsync();
        }

        public async Task<List<Treatment>> GetNurseTreatmentsAsync(string nurseId)
        {
            return await _treatments.Find(t => t.NurseId == nurseId).ToListAsync();
        }

        public async Task<List<Treatment>> GetActiveTreatmentsAsync()
        {
            return await _treatments.Find(t => t.Status == TreatmentStatus.InProgress || t.Status == TreatmentStatus.Planned).ToListAsync();
        }

        public async Task<Treatment> CreateTreatmentAsync(Treatment treatment)
        {
            await _treatments.InsertOneAsync(treatment);
            return treatment;
        }

        public async Task UpdateTreatmentAsync(string id, Treatment treatment)
        {
            treatment.UpdatedAt = DateTime.UtcNow;
            await _treatments.ReplaceOneAsync(t => t.Id == id, treatment);
        }

        public async Task DeleteTreatmentAsync(string id)
        {
            await _treatments.DeleteOneAsync(t => t.Id == id);
        }

        public async Task UpdateTreatmentStatusAsync(string id, TreatmentStatus status)
        {
            var update = Builders<Treatment>.Update
                .Set(t => t.Status, status)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            
            await _treatments.UpdateOneAsync(t => t.Id == id, update);
        }

        public async Task AssignNurseAsync(string treatmentId, string nurseId)
        {
            var update = Builders<Treatment>.Update
                .Set(t => t.NurseId, nurseId)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            
            await _treatments.UpdateOneAsync(t => t.Id == treatmentId, update);
        }
    }
}
