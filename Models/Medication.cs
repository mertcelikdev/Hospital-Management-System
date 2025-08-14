using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalManagementSystem.Models
{
    public enum PrescriptionStatus
    {
        Active,
        Completed,
        Cancelled,
        Expired
    }

    public class Medication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("manufacturer")]
        public string? Manufacturer { get; set; }

        [BsonElement("dosage")]
        public string Dosage { get; set; } = string.Empty; // e.g., "10mg", "500ml"

        [BsonElement("unit")]
        public string Unit { get; set; } = string.Empty; // tablet, ml, mg

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("minimum_stock_level")]
        public int MinimumStockLevel { get; set; } = 10;

        [BsonElement("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [BsonElement("batch_number")]
        public string? BatchNumber { get; set; }

        [BsonElement("cost_per_unit")]
        public decimal CostPerUnit { get; set; }

        [BsonElement("instructions")]
        public string? Instructions { get; set; }

        [BsonElement("side_effects")]
        public List<string> SideEffects { get; set; } = new();

        [BsonElement("contraindications")]
        public List<string> Contraindications { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("is_active")]
        public bool IsActive { get; set; } = true;

        // Computed properties
        public bool IsLowStock => StockQuantity <= MinimumStockLevel;
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now;
        public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddDays(30);
    }

    public class MedicationUsage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("medication_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MedicationId { get; set; } = string.Empty;

        [BsonElement("patient_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PatientId { get; set; } = string.Empty;

        [BsonElement("prescription_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PrescriptionId { get; set; }

        [BsonElement("administered_by")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AdministeredBy { get; set; } = string.Empty; // Nurse/Doctor ID

        [BsonElement("quantity_used")]
        public int QuantityUsed { get; set; }

        [BsonElement("administration_time")]
        public DateTime AdministrationTime { get; set; } = DateTime.UtcNow;

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("patient_reaction")]
        public string? PatientReaction { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Prescription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("patient_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PatientId { get; set; } = string.Empty;

        [BsonElement("doctor_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DoctorId { get; set; } = string.Empty;

        [BsonElement("medications")]
        public List<PrescriptionMedication> Medications { get; set; } = new();

        [BsonElement("diagnosis")]
        public string? Diagnosis { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("prescription_date")]
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;

        [BsonElement("valid_until")]
        public DateTime? ValidUntil { get; set; }

        [BsonElement("is_active")]
        public bool IsActive { get; set; } = true;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PrescriptionMedication
    {
        [BsonElement("medication_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MedicationId { get; set; } = string.Empty;

        [BsonElement("dosage")]
        public string Dosage { get; set; } = string.Empty;

        [BsonElement("frequency")]
        public string Frequency { get; set; } = string.Empty; // "3 times daily", "Once a day"

        [BsonElement("duration")]
        public string Duration { get; set; } = string.Empty; // "7 days", "2 weeks"

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("instructions")]
        public string? Instructions { get; set; }
    }
}
