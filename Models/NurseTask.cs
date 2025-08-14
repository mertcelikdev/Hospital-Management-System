using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum TaskStatus
    {
        Beklemede,
        DevamEdiyor,
        Tamamlandi,
        IptalEdildi
    }

    public enum TaskPriority
    {
        Dusuk,
        Normal,
        Yuksek,
        Acil
    }

    public class NurseTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string NurseId { get; set; } = string.Empty;

        [Required]
        public string AssignedToNurseId { get; set; } = string.Empty; // Service için

        [Required]
        public string AssignedBy { get; set; } = string.Empty; // Doktor veya Admin ID'si

        public string? PatientId { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Beklemede;

        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } // Service için

        public DateTime? CompletedAt { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
