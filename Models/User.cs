using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum UserRole
    {
        Patient,
        Doctor,
        Nurse,
        Staff,
        Admin
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        // Computed property for full name display
        [BsonIgnore]
        public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
            ? $"{FirstName} {LastName}" 
            : Name;

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik Numarası 11 haneli olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik Numarası sadece rakamlardan oluşmalıdır")]
        public string TcNo { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public string PhoneNumber => Phone; // Alias for compatibility

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonIgnore]
        public string Password { get; set; } = string.Empty; // For form binding only

        [Required]
        public string Role { get; set; } = string.Empty;

        public Gender? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Address { get; set; } = string.Empty;

        [Display(Name = "Departman")]
        public string? DepartmentId { get; set; }

        public string EmergencyContact { get; set; } = string.Empty;

        // Patient-Doctor-Nurse Relationships
        public string? AssignedDoctorId { get; set; } // Hastanın doktoru
        public string? AssignedNurseId { get; set; } // Hastanın hemşiresi
        public List<string> PatientIds { get; set; } = new(); // Doktor/Hemşirenin hastaları
        public List<string> DoctorIds { get; set; } = new(); // Hemşirenin çalıştığı doktorlar

        // Professional Information
        public string? LicenseNumber { get; set; } // Doktor/Hemşire lisans numarası
        public string? Specialization { get; set; } // Doktor uzmanlık alanı
        public DateTime? HireDate { get; set; } // İşe alınma tarihi
        public string? Shift { get; set; } // Vardiya (Morning, Evening, Night)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
