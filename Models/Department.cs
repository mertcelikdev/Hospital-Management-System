using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Departman adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Departman adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Departman Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Departman kodu en fazla 20 karakter olabilir.")]
        [Display(Name = "Departman Kodu")]
        public string? Code { get; set; }

        [StringLength(100, ErrorMessage = "Başhekim adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Başhekim")]
        public string? HeadOfDepartment { get; set; }

        [StringLength(15, ErrorMessage = "Telefon numarası en fazla 15 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
        [Display(Name = "Konum")]
        public string? Location { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties for statistics
        [BsonIgnore]
        public int DoctorCount { get; set; }

        [BsonIgnore]
        public int NurseCount { get; set; }

        [BsonIgnore]
        public int StaffCount { get; set; }
    }
}
