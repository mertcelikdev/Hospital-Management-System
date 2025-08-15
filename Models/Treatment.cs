using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum TreatmentStatus
    {
        Planned,
        InProgress,
        Completed,
        Cancelled,
        Postponed
    }

    public enum TreatmentType
    {
        Medication,
        Therapy,
        Surgery,
        Rehabilitation,
        Monitoring,
        Other
    }

    public class Treatment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        public string? NurseId { get; set; }

        [Required]
        public string TreatmentName { get; set; } = string.Empty;

        public TreatmentType Type { get; set; } = TreatmentType.Medication;

        public string Description { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public TreatmentStatus Status { get; set; } = TreatmentStatus.Planned;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? Duration { get; set; } // days

        public string? PrescriptionId { get; set; }

        public List<string> MedicineIds { get; set; } = new();

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

    // IsActive kaldırıldı (gerektiğinde Status kontrolü direkt yapılacak)

        public bool IsCompleted => Status == TreatmentStatus.Completed;
    }
}
