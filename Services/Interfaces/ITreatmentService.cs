using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface ITreatmentService
    {
        Task<List<Treatment>> GetAllTreatmentsAsync();
        Task<Treatment?> GetTreatmentByIdAsync(string id);
        Task<List<Treatment>> GetTreatmentsByPatientIdAsync(string patientId);
        Task<List<Treatment>> GetTreatmentsByDoctorIdAsync(string doctorId);
        Task CreateTreatmentAsync(Treatment treatment);
        Task UpdateTreatmentAsync(string id, Treatment treatment);
        Task DeleteTreatmentAsync(string id);
        Task<List<Treatment>> GetTreatmentsByTypeAsync(string treatmentType);
        Task<List<Treatment>> GetTreatmentsByDateAsync(DateTime date);
        Task<List<Treatment>> GetTreatmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTreatmentsCountByPatientAsync(string patientId);
        Task<int> GetTreatmentsCountByDoctorAsync(string doctorId);
        Task<List<Treatment>> GetActiveTreatmentsAsync(string patientId);
        Task<List<Treatment>> GetCompletedTreatmentsAsync(string patientId);
    }
}
