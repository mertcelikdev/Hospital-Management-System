using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(User user, string password);
        Task<bool> ValidateTokenAsync(string token);
        Task<User?> GetUserByTokenAsync(string token);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task LogoutAsync(string token);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
    }
}
