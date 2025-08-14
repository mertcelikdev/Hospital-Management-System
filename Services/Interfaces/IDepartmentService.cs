using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(string id);
        Task CreateDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(string id, Department department);
        Task DeleteDepartmentAsync(string id);
        Task<List<User>> GetDepartmentStaffAsync(string departmentId);
        Task<int> GetDepartmentStaffCountAsync(string departmentId);
        Task<bool> AssignStaffToDepartmentAsync(string departmentId, string staffId);
        Task<bool> RemoveStaffFromDepartmentAsync(string departmentId, string staffId);
    }
}
