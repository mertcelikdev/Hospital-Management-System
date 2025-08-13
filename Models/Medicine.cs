using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum MedicineCategory
    {
        Antibiotic,
        Painkiller,
        Vitamin,
        Supplement,
        Injection,
        Syrup,
        Tablet,
        Capsule,
        Other
    }

    public class Medicine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string GenericName { get; set; } = string.Empty;

        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public MedicineCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Strength { get; set; } = string.Empty;

        [Required]
        public string Unit { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int MinimumStock { get; set; } = 10;

        public DateTime ExpiryDate { get; set; }

        public string BatchNumber { get; set; } = string.Empty;

        public List<string> SideEffects { get; set; } = new List<string>();

        public List<string> Contraindications { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool IsLowStock => StockQuantity <= MinimumStock;
    }
}
