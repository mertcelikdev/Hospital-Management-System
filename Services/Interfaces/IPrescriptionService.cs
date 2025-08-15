using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface IPrescriptionService
    {
        Task<List<PrescriptionDto>> GetAllPrescriptionsAsync();
        Task<PrescriptionDto> GetPrescriptionByIdAsync(string id);
        Task<List<PrescriptionDto>> GetPatientPrescriptionsAsync(string patientId);
        Task<List<PrescriptionDto>> GetDoctorPrescriptionsAsync(string doctorId);
        Task<List<PrescriptionDto>> GetNursePrescriptionsAsync(string nurseId);
        Task<List<PrescriptionDto>> GetActivePrescriptionsAsync();
        Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto createPrescriptionDto);
        Task UpdatePrescriptionAsync(string id, UpdatePrescriptionDto updatePrescriptionDto);
        Task DeletePrescriptionAsync(string id);
        Task UpdatePrescriptionStatusAsync(string id, PrescriptionStatus status);
        Task AssignNurseAsync(string prescriptionId, string nurseId);
    }
}
