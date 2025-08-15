using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden uzun olamaz")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden uzun olamaz")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No gereklidir")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 karakter olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        public string TcNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçersiz telefon numarası")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi gereklidir")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Cinsiyet gereklidir")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Adres 200 karakterden uzun olamaz")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Acil durum kişisi adı 100 karakterden uzun olamaz")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Geçersiz acil durum telefon numarası")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(5, ErrorMessage = "Kan grubu 5 karakterden uzun olamaz")]
        public string? BloodType { get; set; }

        public List<string> Allergies { get; set; } = new();
        public List<string> ChronicDiseases { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();

        [StringLength(20, ErrorMessage = "Sigorta numarası 20 karakterden uzun olamaz")]
        public string? InsuranceNumber { get; set; }
    }

    public class UpdatePatientDto
    {
        [StringLength(50, ErrorMessage = "Ad 50 karakterden uzun olamaz")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Soyad 50 karakterden uzun olamaz")]
        public string? LastName { get; set; }

        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 karakter olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        public string? TcNo { get; set; }

        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçersiz telefon numarası")]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        [StringLength(200, ErrorMessage = "Adres 200 karakterden uzun olamaz")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Acil durum kişisi adı 100 karakterden uzun olamaz")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Geçersiz acil durum telefon numarası")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(5, ErrorMessage = "Kan grubu 5 karakterden uzun olamaz")]
        public string? BloodType { get; set; }

        public List<string>? Allergies { get; set; }
        public List<string>? ChronicDiseases { get; set; }
        public List<string>? CurrentMedications { get; set; }

        [StringLength(20, ErrorMessage = "Sigorta numarası 20 karakterden uzun olamaz")]
        public string? InsuranceNumber { get; set; }
    }

    public class PatientDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string TcNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        public string Gender { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? BloodType { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> ChronicDiseases { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();
        public string? InsuranceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    }
}