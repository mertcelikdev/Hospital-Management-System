using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<List<Prescription>> GetAllPrescriptionsAsync();
        Task<Prescription?> GetPrescriptionByIdAsync(string id);
        Task<List<Prescription>> GetPrescriptionsByPatientIdAsync(string patientId);
        Task<List<Prescription>> GetPrescriptionsByDoctorIdAsync(string doctorId);
        Task CreatePrescriptionAsync(Prescription prescription);
        Task UpdatePrescriptionAsync(string id, Prescription prescription);
        Task DeletePrescriptionAsync(string id);
        Task<List<Prescription>> GetPrescriptionsByDateAsync(DateTime date);
        Task<List<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetPrescriptionsCountByPatientAsync(string patientId);
        Task<int> GetPrescriptionsCountByDoctorAsync(string doctorId);
        Task<bool> IsPrescriptionValidAsync(string prescriptionId);
        Task<List<Prescription>> GetActivePrescriptionsAsync(string patientId);
    }
}
