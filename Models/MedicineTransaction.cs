using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum TransactionType
    {
        StockIn,
        StockOut,
        Dispensed,
        Returned,
        Expired,
        Damaged,
        Adjustment
    }

    public class MedicineTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string MedicineId { get; set; } = string.Empty;

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int QuantityBefore { get; set; }

        public int QuantityAfter { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public string PrescriptionId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Medicine? Medicine { get; set; }
        public User? Patient { get; set; }
        public User? User { get; set; }
        public Prescription? Prescription { get; set; }
    }
}
