using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface ITreatmentService
    {
        Task<List<TreatmentDto>> GetAllTreatmentsAsync();
        Task<TreatmentDto> GetTreatmentByIdAsync(string id);
        Task<List<TreatmentDto>> GetPatientTreatmentsAsync(string patientId);
        Task<List<TreatmentDto>> GetDoctorTreatmentsAsync(string doctorId);
        Task<List<TreatmentDto>> GetNurseTreatmentsAsync(string nurseId);
        Task<List<TreatmentDto>> GetActiveTreatmentsAsync();
        Task<TreatmentDto> CreateTreatmentAsync(CreateTreatmentDto createTreatmentDto);
        Task UpdateTreatmentAsync(string id, UpdateTreatmentDto updateTreatmentDto);
        Task DeleteTreatmentAsync(string id);
        Task UpdateTreatmentStatusAsync(string id, TreatmentStatus status);
        Task AssignNurseAsync(string treatmentId, string nurseId);
    }
}
