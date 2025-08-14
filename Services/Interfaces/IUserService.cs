using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<List<User>> GetUsersByDepartmentAsync(string departmentId);
        Task<List<User>> SearchUsersAsync(string searchTerm);
        Task<bool> ChangeUserPasswordAsync(string userId, string newPassword);
        Task<bool> UpdateUserProfileAsync(string userId, User updatedUser);
        Task<int> GetUserCountByRoleAsync(string role);
        Task<int> GetTotalUsersCountAsync();
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
    }
}
