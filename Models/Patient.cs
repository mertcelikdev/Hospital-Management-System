using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Patient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No gereklidir.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece sayılardan oluşmalıdır.")]
        [Display(Name = "TC Kimlik No")]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum tarihi gereklidir.")]
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi gereklidir.")]
        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Meslek en fazla 100 karakter olabilir.")]
        [Display(Name = "Meslek")]
        public string? Occupation { get; set; }

        [StringLength(50, ErrorMessage = "Kan grubu en fazla 50 karakter olabilir.")]
        [Display(Name = "Kan Grubu")]
        public string? BloodType { get; set; }

        [StringLength(500, ErrorMessage = "Alerjiler en fazla 500 karakter olabilir.")]
        [Display(Name = "Alerjiler")]
        public string? Allergies { get; set; }

        [StringLength(500, ErrorMessage = "Kronik hastalıklar en fazla 500 karakter olabilir.")]
        [Display(Name = "Kronik Hastalıklar")]
        public string? ChronicDiseases { get; set; }

        [StringLength(100, ErrorMessage = "Acil durum kişisi adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Acil Durum Kişisi")]
        public string? EmergencyContactName { get; set; }

        [StringLength(15, ErrorMessage = "Acil durum telefonu en fazla 15 karakter olabilir.")]
        [Display(Name = "Acil Durum Telefonu")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(50, ErrorMessage = "Yakınlık en fazla 50 karakter olabilir.")]
        [Display(Name = "Yakınlık")]
        public string? EmergencyContactRelation { get; set; }

    // IsActive kaldırıldı; eski belgelerdeki fazladan alanları yutmak için ExtraElements
    [BsonExtraElements]
    public Dictionary<string, object>? LegacyFields { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

    // Soft delete alanları
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

        // Computed properties
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Yaş")]
        public int Age => DateTime.Now.Year - DateOfBirth.Year - 
                         (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}
