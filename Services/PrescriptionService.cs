using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class PrescriptionService
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

        public async Task<Prescription> GetPrescriptionByIdAsync(string id)
        {
            return await _prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Prescription>> GetPatientPrescriptionsAsync(string patientId)
        {
            return await _prescriptions.Find(p => p.PatientId == patientId).ToListAsync();
        }

        public async Task<List<Prescription>> GetDoctorPrescriptionsAsync(string doctorId)
        {
            return await _prescriptions.Find(p => p.DoctorId == doctorId).ToListAsync();
        }

        public async Task<List<Prescription>> GetNursePrescriptionsAsync(string nurseId)
        {
            return await _prescriptions.Find(p => p.NurseId == nurseId).ToListAsync();
        }

        public async Task<List<Prescription>> GetActivePrescriptionsAsync()
        {
            return await _prescriptions.Find(p => p.Status == PrescriptionStatus.Active).ToListAsync();
        }

        public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
        {
            await _prescriptions.InsertOneAsync(prescription);
            return prescription;
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

        public async Task UpdatePrescriptionStatusAsync(string id, PrescriptionStatus status)
        {
            var update = Builders<Prescription>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            await _prescriptions.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task AssignNurseAsync(string prescriptionId, string nurseId)
        {
            var update = Builders<Prescription>.Update
                .Set(p => p.NurseId, nurseId)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            
            await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);
        }
    }
}
