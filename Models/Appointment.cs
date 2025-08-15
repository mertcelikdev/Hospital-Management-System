using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum AppointmentStatus
    {
        Planlandı,     // Scheduled
        Scheduled = Planlandı,
        Onaylandı,     // Confirmed  
        Confirmed = Onaylandı,
        Devam,         // InProgress
        InProgress = Devam,
        Tamamlandı,    // Completed
        Completed = Tamamlandı,
        İptalEdildi,   // Cancelled
        Cancelled = İptalEdildi,
        Gelmedi,       // NoShow
        NoShow = Gelmedi
    }

    public enum AppointmentType
    {
        Muayene,       // Consultation
        Consultation = Muayene,
        Kontrol,       // FollowUp
        FollowUp = Kontrol,
        Acil,          // Emergency
        Emergency = Acil,
        Ameliyat,      // Surgery
        Surgery = Ameliyat,
        İnceleme,      // Examination
        Examination = İnceleme,
        CheckUp,       // Checkup
        Checkup = CheckUp,
        Tedavi,        // Treatment
        Treatment = Tedavi
    }

    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
    // Eski dökümanlardaki fazladan alanları yoksaymak için
    [BsonExtraElements]
    public Dictionary<string, object>? ExtraElements { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Planlandı;

        [Required]
        public AppointmentType Type { get; set; }

    // İlgili departman (doktor üzerinden seçilecek)
    public string? DepartmentId { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

    // Soft delete alanları
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

        // Additional properties for compatibility
        public string CreatedBy { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);

        // Navigation properties
        [BsonIgnore]
        public User? Patient { get; set; }

        [BsonIgnore]
        public User? Doctor { get; set; }
    }
}