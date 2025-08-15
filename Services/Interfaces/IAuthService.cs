using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IAuthService
    {
        // Authentication
        Task<UserDto?> AuthenticateAsync(string email, string password);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto);
        Task LogoutAsync(string userId);

        // Password Management
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
        Task<string> HashPasswordAsync(string password);

        // Token Management
        Task<string> GenerateTokenAsync(UserDto user);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserFromTokenAsync(string token);
        Task RevokeTokenAsync(string token);
    Task<RefreshTokenRecord> CreateRefreshTokenAsync(string userId, TimeSpan? lifetime = null);
    Task<RefreshTokenRecord?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllRefreshTokensForUserAsync(string userId);

    // (Deprecated) Session Management kaldırıldı – JWT kullanılıyor

        // Security
        Task<bool> IsAccountLockedAsync(string email);
        Task LockAccountAsync(string email, int lockoutMinutes = 30);
        Task UnlockAccountAsync(string email);
        Task IncrementFailedLoginAttemptsAsync(string email);
        Task ResetFailedLoginAttemptsAsync(string email);

        // User Validation
        Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null);
        Task<bool> IsPhoneUniqueAsync(string phone, string? excludeUserId = null);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> GetUserByPhoneAsync(string phone);

        // Role Management
        Task<bool> HasRoleAsync(string userId, string role);
        Task<bool> HasAnyRoleAsync(string userId, params string[] roles);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task AssignRoleAsync(string userId, string role);
        Task RemoveRoleAsync(string userId, string role);

        // Permission Management
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<bool> CanAccessResourceAsync(string userId, string resource, string action);

        // Activity Tracking
        Task LogLoginActivityAsync(string userId, string ipAddress, string userAgent);
        Task LogLogoutActivityAsync(string userId);
        Task<List<LoginActivity>> GetUserLoginHistoryAsync(string userId, int limit = 10);
        Task<List<LoginActivity>> GetRecentLoginActivitiesAsync(int limit = 50);

        // Password Policy
        Task<bool> ValidatePasswordPolicyAsync(string password);
        Task<List<string>> GetPasswordRequirementsAsync();
        Task<bool> IsPasswordExpiredAsync(string userId);
        Task SetPasswordExpiryAsync(string userId, DateTime expiryDate);
    }

    public class LoginActivity
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
    }
}