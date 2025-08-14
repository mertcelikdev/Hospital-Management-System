using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum NoteType
    {
        Genel,
        TedaviNotu,
        HemşireNotu,
        DoktorNotu,
        IlacUygulamasi,
        VitalBelirtiler
    }

    public class PatientNote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string CreatedBy { get; set; } = string.Empty; // Doktor veya Hemşire ID'si

        [Required]
        public string CreatedByName { get; set; } = string.Empty; // İsim cache için

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty; // Service için

        [Required]
        public string CreatedByUserName { get; set; } = string.Empty; // Controller için

        [Required]
        public UserRole CreatedByRole { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = true; // Service için

        public NoteType NoteType { get; set; } = NoteType.Genel; // Service için

        public NotePriority Priority { get; set; } = NotePriority.Normal; // Service için

        public NoteType Type { get; set; } = NoteType.Genel;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsPrivate { get; set; } = false; // Sadece oluşturan görebilir

        public string Tags { get; set; } = string.Empty; // Virgülle ayrılmış etiketler
    }

    public enum PatientNoteType
    {
        [Display(Name = "Hemşire Notu")]
        HemşireNotu = 1,
        
        [Display(Name = "Doktor Notu")]
        DoktorNotu = 2,
        
        [Display(Name = "Tedavi Notu")]
        TedaviNotu = 3,
        
        [Display(Name = "İlaç Notu")]
        İlaçNotu = 4,
        
        [Display(Name = "Genel Not")]
        GenelNot = 5
    }

    public enum NotePriority
    {
        [Display(Name = "Düşük")]
        Düşük = 1,
        
        [Display(Name = "Normal")]
        Normal = 2,
        
        [Display(Name = "Yüksek")]
        Yüksek = 3,
        
        [Display(Name = "Acil")]
        Acil = 4
    }
}
