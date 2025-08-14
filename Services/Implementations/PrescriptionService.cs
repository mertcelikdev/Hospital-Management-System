using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IMongoCollection<Prescription> _prescriptions;

        public PrescriptionService(IMongoDatabase database)
        {
            _prescriptions = database.GetCollection<Prescription>("Prescriptions");
        }

        public async Task<List<Prescription>> GetAllPrescriptionsAsync()
        {
            return await _prescriptions.Find(_ => true).ToListAsync();
        }

        public async Task<Prescription?> GetPrescriptionByIdAsync(string id)
        {
            return await _prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreatePrescriptionAsync(Prescription prescription)
        {
            prescription.CreatedAt = DateTime.UtcNow;
            prescription.UpdatedAt = DateTime.UtcNow;
            prescription.IsActive = true;
            await _prescriptions.InsertOneAsync(prescription);
        }

        public async Task UpdatePrescriptionAsync(string id, Prescription prescription)
        {
            prescription.UpdatedAt = DateTime.UtcNow;
            await _prescriptions.ReplaceOneAsync(p => p.Id == id, prescription);
        }

        public async Task DeletePrescriptionAsync(string id)
        {
            await _prescriptions.DeleteOneAsync(p => p.Id == id);
        }

        public async Task<List<Prescription>> GetPrescriptionsByPatientIdAsync(string patientId)
        {
            return await _prescriptions.Find(p => p.PatientId == patientId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prescription>> GetPrescriptionsByDoctorIdAsync(string doctorId)
        {
            return await _prescriptions.Find(p => p.DoctorId == doctorId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prescription>> GetPrescriptionsByDateAsync(DateTime date)
        {
            var start = date.Date; var end = start.AddDays(1);
            return await _prescriptions.Find(p => p.PrescriptionDate >= start && p.PrescriptionDate < end)
                .SortByDescending(p => p.PrescriptionDate)
                .ToListAsync();
        }

        public async Task<List<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _prescriptions.Find(p => p.PrescriptionDate >= startDate && p.PrescriptionDate <= endDate)
                .SortByDescending(p => p.PrescriptionDate)
                .ToListAsync();
        }

        public async Task<int> GetPrescriptionsCountByPatientAsync(string patientId)
        {
            return (int)await _prescriptions.CountDocumentsAsync(p => p.PatientId == patientId);
        }

        public async Task<int> GetPrescriptionsCountByDoctorAsync(string doctorId)
        {
            return (int)await _prescriptions.CountDocumentsAsync(p => p.DoctorId == doctorId);
        }

        public async Task<bool> IsPrescriptionValidAsync(string prescriptionId)
        {
            var pr = await _prescriptions.Find(p => p.Id == prescriptionId).FirstOrDefaultAsync();
            if (pr == null) return false;
            if (!pr.IsActive) return false;
            if (pr.ValidUntil.HasValue && pr.ValidUntil.Value < DateTime.UtcNow) return false;
            return true;
        }

        public async Task<List<Prescription>> GetActivePrescriptionsAsync(string patientId)
        {
            var now = DateTime.UtcNow;
            return await _prescriptions.Find(p => p.PatientId == patientId && p.IsActive && (!p.ValidUntil.HasValue || p.ValidUntil >= now))
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
