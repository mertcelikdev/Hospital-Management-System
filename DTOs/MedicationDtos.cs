using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateMedicationDto
    {
        [Required(ErrorMessage = "İlaç adı gereklidir")]
        [StringLength(100, ErrorMessage = "İlaç adı 100 karakterden uzun olamaz")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Üretici adı 100 karakterden uzun olamaz")]
        public string? Manufacturer { get; set; }

        [Required(ErrorMessage = "Doz bilgisi gereklidir")]
        [StringLength(50, ErrorMessage = "Doz bilgisi 50 karakterden uzun olamaz")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birim gereklidir")]
        [StringLength(20, ErrorMessage = "Birim 20 karakterden uzun olamaz")]
        public string Unit { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır")]
        public int MinimumStockLevel { get; set; } = 10;

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50, ErrorMessage = "Parti numarası 50 karakterden uzun olamaz")]
        public string? BatchNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat 0 veya daha büyük olmalıdır")]
        public decimal CostPerUnit { get; set; }

        [StringLength(1000, ErrorMessage = "Kullanım talimatları 1000 karakterden uzun olamaz")]
        public string? Instructions { get; set; }

        public List<string> SideEffects { get; set; } = new();
        public List<string> Contraindications { get; set; } = new();
    }

    public class UpdateMedicationDto
    {
        [StringLength(100, ErrorMessage = "İlaç adı 100 karakterden uzun olamaz")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Üretici adı 100 karakterden uzun olamaz")]
        public string? Manufacturer { get; set; }

        [StringLength(50, ErrorMessage = "Doz bilgisi 50 karakterden uzun olamaz")]
        public string? Dosage { get; set; }

        [StringLength(20, ErrorMessage = "Birim 20 karakterden uzun olamaz")]
        public string? Unit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int? StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0 veya daha büyük olmalıdır")]
        public int? MinimumStockLevel { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50, ErrorMessage = "Parti numarası 50 karakterden uzun olamaz")]
        public string? BatchNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat 0 veya daha büyük olmalıdır")]
        public decimal? CostPerUnit { get; set; }

        [StringLength(1000, ErrorMessage = "Kullanım talimatları 1000 karakterden uzun olamaz")]
        public string? Instructions { get; set; }

        public List<string>? SideEffects { get; set; }
        public List<string>? Contraindications { get; set; }
        public bool? IsActive { get; set; }
    }

    public class MedicationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int MinimumStockLevel { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? BatchNumber { get; set; }
        public decimal CostPerUnit { get; set; }
        public string? Instructions { get; set; }
        public List<string> SideEffects { get; set; } = new();
        public List<string> Contraindications { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
    }

    public class CreateMedicationUsageDto
    {
        [Required(ErrorMessage = "İlaç ID gereklidir")]
        public string MedicationId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasta ID gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        public string? PrescriptionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kullanılan miktar 1 veya daha büyük olmalıdır")]
        public int QuantityUsed { get; set; }

        public DateTime AdministrationTime { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz")]
        public string? Notes { get; set; }

        [StringLength(500, ErrorMessage = "Hasta reaksiyonu 500 karakterden uzun olamaz")]
        public string? PatientReaction { get; set; }
    }

    public class MedicationUsageDto
    {
        public string Id { get; set; } = string.Empty;
        public string MedicationId { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? PrescriptionId { get; set; }
        public string AdministeredBy { get; set; } = string.Empty;
        public string AdministeredByName { get; set; } = string.Empty;
        public int QuantityUsed { get; set; }
        public DateTime AdministrationTime { get; set; }
        public string? Notes { get; set; }
        public string? PatientReaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePrescriptionDto
    {
        [Required(ErrorMessage = "Hasta ID gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "En az bir ilaç belirtilmelidir")]
        public List<CreatePrescriptionMedicationDto> Medications { get; set; } = new();

        [StringLength(500, ErrorMessage = "Tanı 500 karakterden uzun olamaz")]
        public string? Diagnosis { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar 1000 karakterden uzun olamaz")]
        public string? Notes { get; set; }

        public DateTime? ValidUntil { get; set; }
    }

    public class CreatePrescriptionMedicationDto
    {
        [Required(ErrorMessage = "İlaç ID gereklidir")]
        public string MedicationId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doz gereklidir")]
        [StringLength(50, ErrorMessage = "Doz 50 karakterden uzun olamaz")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sıklık gereklidir")]
        [StringLength(50, ErrorMessage = "Sıklık 50 karakterden uzun olamaz")]
        public string Frequency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre gereklidir")]
        [StringLength(50, ErrorMessage = "Süre 50 karakterden uzun olamaz")]
        public string Duration { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Miktar 1 veya daha büyük olmalıdır")]
        public int Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Talimatlar 500 karakterden uzun olamaz")]
        public string? Instructions { get; set; }
    }

    public class PrescriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public List<PrescriptionMedicationDto> Medications { get; set; } = new();
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PrescriptionMedicationDto
    {
        public string MedicationId { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Instructions { get; set; }
    }
}
