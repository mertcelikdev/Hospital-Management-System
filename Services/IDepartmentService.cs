using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface IDepartmentService
    {
        // CRUD Operations
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(string id);
        Task<List<Department>> GetActiveDepartmentsAsync();
        Task CreateDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(string id, Department department);
        Task DeleteDepartmentAsync(string id);

        // Status Operations
        Task ToggleDepartmentStatusAsync(string id);
        Task ActivateDepartmentAsync(string id);
        Task DeactivateDepartmentAsync(string id);

        // User Count Operations
        Task<int> GetDepartmentUserCountAsync(string departmentId, string role);
        Task<List<User>> GetDepartmentUsersAsync(string departmentId, string? role = null);

        // Search and Filter
        Task<List<Department>> SearchDepartmentsAsync(string searchTerm);
        Task<Department?> GetDepartmentByNameAsync(string name);

        // Statistics
        Task<int> GetTotalDepartmentsCountAsync();
        Task<int> GetActiveDepartmentsCountAsync();
        Task<Dictionary<string, int>> GetDepartmentUserStatisticsAsync(string departmentId);

        // Validation
        Task<bool> IsDepartmentNameUniqueAsync(string name, string? excludeId = null);
        Task<bool> CanDeleteDepartmentAsync(string id);

        // Department Head Operations
        Task AssignDepartmentHeadAsync(string departmentId, string userId);
        Task RemoveDepartmentHeadAsync(string departmentId);
        Task<User?> GetDepartmentHeadAsync(string departmentId);

        // Bulk Operations
        Task<List<Department>> GetDepartmentsByIdsAsync(List<string> ids);
        Task BulkUpdateDepartmentStatusAsync(List<string> ids, bool isActive);
    }
}