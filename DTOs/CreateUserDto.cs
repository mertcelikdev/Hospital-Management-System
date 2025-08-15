using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateUserDto
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


        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
    public string? DepartmentId { get; set; }

    // Rol seçimi (Admin paneli için). Boş gelirse Patient varsayılacak.
    [RegularExpression("^(Patient|Doctor|Nurse|Staff|Admin)$", ErrorMessage = "Geçersiz rol")]
    public string? Role { get; set; } = "Patient";

    // Hasta kullanıcı oluşturulurken TC alınmak istenirse (opsiyonel)
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC 11 hane olmalı")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC sadece rakam olmalı")]
    public string? TcNo { get; set; }
    }

    public class CreateDoctorDto
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

        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı gereklidir")]
        [StringLength(100, ErrorMessage = "Uzmanlık alanı en fazla 100 karakter olabilir")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lisans numarası gereklidir")]
        [StringLength(50, ErrorMessage = "Lisans numarası en fazla 50 karakter olabilir")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departman seçimi gereklidir")]
        public string DepartmentId { get; set; } = string.Empty;
    }

    public class CreateNurseDto
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

        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        [StringLength(50, ErrorMessage = "Lisans numarası en fazla 50 karakter olabilir")]
        public string? LicenseNumber { get; set; }

        [Required(ErrorMessage = "Departman seçimi gereklidir")]
        public string DepartmentId { get; set; } = string.Empty;
    }
}
