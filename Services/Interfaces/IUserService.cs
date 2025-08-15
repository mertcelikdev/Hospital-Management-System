using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IUserService
    {
        // CRUD Operations
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task DeleteUserAsync(string id);
        
        // Role-based queries
        Task<IEnumerable<UserDto>> GetDoctorsAsync();
        Task<IEnumerable<UserDto>> GetPatientsAsync();
        Task<IEnumerable<UserDto>> GetNursesAsync();
        Task<IEnumerable<UserDto>> GetStaffAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        
        // Authentication
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<UserDto?> AuthenticateAsync(string email, string password);

        // Dashboard metrics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUserCountByRoleAsync(string role);
        Task<List<UserDto>> GetRecentUsersAsync(int count);
        Task<int> GetNewUsersCountThisMonthAsync(string role);
    }
}
