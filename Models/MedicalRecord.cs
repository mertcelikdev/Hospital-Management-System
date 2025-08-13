using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class MedicalRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        public string AppointmentId { get; set; } = string.Empty;

        [Required]
        public DateTime VisitDate { get; set; }

        public string ChiefComplaint { get; set; } = string.Empty;

        public string HistoryOfPresentIllness { get; set; } = string.Empty;

        public string PhysicalExamination { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentPlan { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public List<string> PrescriptionIds { get; set; } = new List<string>();

        public List<string> TestResultIds { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? Patient { get; set; }
        public User? Doctor { get; set; }
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
