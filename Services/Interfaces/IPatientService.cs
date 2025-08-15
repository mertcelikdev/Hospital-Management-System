using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IPatientService
    {
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientByIdAsync(string id);
        Task<List<PatientDto>> SearchPatientsAsync(string searchTerm);
        Task CreatePatientAsync(CreatePatientDto createPatientDto);
        Task<bool> UpdatePatientAsync(string id, UpdatePatientDto updatePatientDto);
    Task<bool> DeletePatientAsync(string id, string? userId = null);
    Task<bool> RestorePatientAsync(string id);
    Task<bool> HardDeletePatientAsync(string id);
    Task<List<PatientDto>> GetDeletedPatientsAsync();
        Task<bool> PatientExistsAsync(string identityNumber);
        Task<PatientDto?> GetPatientByIdentityNumberAsync(string identityNumber);
        Task<List<PatientDto>> GetRecentPatientsAsync(int count = 10);
        Task<int> GetPatientsCountAsync();
    Task<(List<PatientDto> Items, long Total)> GetPatientsPagedAsync(int page, int pageSize, string? sort, string? dir, string? nameSearch, string? tcSearch, string? letter = null);
    }
}
