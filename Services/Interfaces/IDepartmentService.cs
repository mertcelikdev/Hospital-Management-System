using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IDepartmentService
    {
        // CRUD Operations
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(string id);
        Task CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);
        Task UpdateDepartmentAsync(string id, UpdateDepartmentDto updateDepartmentDto);
        Task DeleteDepartmentAsync(string id);

        // Status Operations

        // User Count Operations
        Task<int> GetDepartmentUserCountAsync(string departmentId, string role);
        Task<List<UserDto>> GetDepartmentUsersAsync(string departmentId, string? role = null);

        // Search and Filter
        Task<List<DepartmentDto>> SearchDepartmentsAsync(string searchTerm);
        Task<DepartmentDto?> GetDepartmentByNameAsync(string name);

        // Statistics
        Task<int> GetTotalDepartmentsCountAsync();
        Task<Dictionary<string, int>> GetDepartmentUserStatisticsAsync(string departmentId);

        // Validation
        Task<bool> IsDepartmentNameUniqueAsync(string name, string? excludeId = null);
        Task<bool> CanDeleteDepartmentAsync(string id);

        // Department Head Operations
        Task AssignDepartmentHeadAsync(string departmentId, string userId);
        Task RemoveDepartmentHeadAsync(string departmentId);
        Task<UserDto?> GetDepartmentHeadAsync(string departmentId);

        // Bulk Operations
        Task<List<DepartmentDto>> GetDepartmentsByIdsAsync(List<string> ids);
    // IsActive durum yönetimi kaldırıldı
    }
}