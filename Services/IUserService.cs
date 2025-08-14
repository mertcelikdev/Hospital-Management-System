using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IUserService
    {
        // Existing methods
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);
        Task<IEnumerable<User>> GetDoctorsAsync();
        Task<IEnumerable<User>> GetPatientsAsync();
        Task<IEnumerable<User>> GetNursesAsync();
        Task<IEnumerable<User>> GetStaffAsync();
        Task<User> GetUserByEmailAsync(string email);

        // Additional methods for Dashboard
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUserCountByRoleAsync(string role);
        Task<int> GetActiveUsersCountAsync();
        Task<List<object>> GetRecentUsersAsync(int count);
        Task<int> GetNewUsersCountThisMonthAsync(string role);
        
        // Additional methods used by controllers
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> AuthenticateAsync(string email, string password);
    }
}
