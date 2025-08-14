using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    [BsonIgnoreExtraElements]
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

    [Required]
    public string Status { get; set; } = "Planlandı";

    [Required]
    public string Type { get; set; } = "Muayene";

        public string Notes { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);

        // Navigation properties
        [BsonIgnore]
        public User? Patient { get; set; }

        [BsonIgnore]
        public User? Doctor { get; set; }

        // Soft delete support fields
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Helper properties
        [BsonIgnore]
        public bool IsDeleted => DeletedAt.HasValue;

        [BsonIgnore]
        public string FormattedDate => AppointmentDate.ToString("dd.MM.yyyy");

        [BsonIgnore]
        public string FormattedTime => AppointmentDate.ToString("HH:mm");
    }
}
