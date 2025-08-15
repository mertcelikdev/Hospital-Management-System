using System.ComponentModel.DataAnnotations;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre onayı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçimi gereklidir")]
        public string Role { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string? RefreshToken { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}
