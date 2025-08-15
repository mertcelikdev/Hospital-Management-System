using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string? Username { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? DepartmentId { get; set; }
    }
}
