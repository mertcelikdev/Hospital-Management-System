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
        public string Form { get; set; } = string.Empty;

        [Required]
        public string Strength { get; set; } = string.Empty;

        [Required]
        public MedicineCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        public int MinimumStock { get; set; } = 10;

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

    public bool IsLowStock => StockQuantity <= MinimumStock;
    }
}