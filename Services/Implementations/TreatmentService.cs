using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class TreatmentService : ITreatmentService
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

        public async Task<Treatment?> GetTreatmentByIdAsync(string id)
        {
            return await _treatments.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateTreatmentAsync(Treatment treatment)
        {
            treatment.CreatedAt = DateTime.UtcNow;
            treatment.UpdatedAt = DateTime.UtcNow;
            await _treatments.InsertOneAsync(treatment);
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

        public async Task<List<Treatment>> GetTreatmentsByPatientIdAsync(string patientId)
        {
            return await _treatments.Find(t => t.PatientId == patientId)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Treatment>> GetTreatmentsByDoctorIdAsync(string doctorId)
        {
            return await _treatments.Find(t => t.DoctorId == doctorId)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Treatment>> GetTreatmentsByTypeAsync(string treatmentType)
        {
            if (string.IsNullOrWhiteSpace(treatmentType))
            {
                return await GetAllTreatmentsAsync();
            }

            // Enum parse dene (case-insensitive). Başarısız olursa ad geçenleri filtrele.
            if (Enum.TryParse<TreatmentType>(treatmentType, true, out var parsed))
            {
                return await _treatments.Find(t => t.Type == parsed)
                    .SortByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }

            var lower = treatmentType.ToLowerInvariant();
            return await _treatments.Find(t => t.Type.ToString().ToLower().Contains(lower))
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Treatment>> GetTreatmentsByDateAsync(DateTime date)
        {
            var start = date.Date; var end = start.AddDays(1);
            return await _treatments.Find(t => t.CreatedAt >= start && t.CreatedAt < end)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Treatment>> GetTreatmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _treatments.Find(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTreatmentsCountByPatientAsync(string patientId)
        {
            return (int)await _treatments.CountDocumentsAsync(t => t.PatientId == patientId);
        }

        public async Task<int> GetTreatmentsCountByDoctorAsync(string doctorId)
        {
            return (int)await _treatments.CountDocumentsAsync(t => t.DoctorId == doctorId);
        }

        public async Task<List<Treatment>> GetActiveTreatmentsAsync(string patientId)
        {
            return await _treatments.Find(t => t.PatientId == patientId && t.IsActive)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Treatment>> GetCompletedTreatmentsAsync(string patientId)
        {
            return await _treatments.Find(t => t.PatientId == patientId && !t.IsActive)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
