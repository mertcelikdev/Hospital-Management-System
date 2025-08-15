using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum PrescriptionStatus
    {
        Active,
        Completed,
        Cancelled,
        Expired
    }

    public class PrescriptionMedicine
    {
        public string MedicineId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int Duration { get; set; } // days
        public string Instructions { get; set; } = string.Empty;
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

        public string? NurseId { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public List<PrescriptionMedicine> Medicines { get; set; } = new();

        public string Notes { get; set; } = string.Empty;

        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

    // IsActive kaldırıldı (gerektiğinde Status == Active kontrolü direkt yapılacak)

        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    }
}