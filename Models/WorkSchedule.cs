using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum ShiftType
    {
        Morning,
        Evening,
        Night,
        FullDay
    }

    public enum ShiftStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public class WorkSchedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public ShiftType ShiftType { get; set; }

        public ShiftStatus Status { get; set; } = ShiftStatus.Scheduled;

        public string DepartmentId { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        public User? User { get; set; }
        public Department? Department { get; set; }
    }
}
