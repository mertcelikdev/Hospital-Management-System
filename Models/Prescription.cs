using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum PrescriptionStatus
    {
        Pending,
        Dispensed,
        Completed,
        Cancelled
    }

    public class Prescription
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
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

        public List<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? Patient { get; set; }
        public User? Doctor { get; set; }
    }

    public class PrescriptionItem
    {
        [Required]
        public string MedicineId { get; set; } = string.Empty;

        [Required]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        public int DurationDays { get; set; }

        public string Instructions { get; set; } = string.Empty;

        // Navigation properties
        public Medicine? Medicine { get; set; }
    }
}
