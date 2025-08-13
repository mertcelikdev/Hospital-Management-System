using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum TestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public class TestResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        public string MedicalRecordId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TestName { get; set; } = string.Empty;

        [Required]
        public string TestType { get; set; } = string.Empty;

        public string TestCode { get; set; } = string.Empty;

        [Required]
        public DateTime TestDate { get; set; }

        public DateTime? ResultDate { get; set; }

        public TestStatus Status { get; set; } = TestStatus.Pending;

        public string Result { get; set; } = string.Empty;

        public string ReferenceRange { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public bool IsAbnormal { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string TechnicianId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? Patient { get; set; }
        public User? Doctor { get; set; }
        public User? Technician { get; set; }
    }
}
