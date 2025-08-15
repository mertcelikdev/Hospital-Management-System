using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateMedicineDto
    {
        [Required(ErrorMessage = "İlaç adı gereklidir")]
        [StringLength(200, ErrorMessage = "İlaç adı en fazla 200 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori gereklidir")]
        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz")]
        public int StockQuantity { get; set; }

        [StringLength(100, ErrorMessage = "Üretici adı en fazla 100 karakter olabilir")]
        public string? Manufacturer { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50, ErrorMessage = "Lot numarası en fazla 50 karakter olabilir")]
        public string? LotNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi negatif olamaz")]
        public int MinimumStockLevel { get; set; } = 10;

        public bool RequiresPrescription { get; set; } = false;

        [StringLength(1000, ErrorMessage = "Kullanım talimatları en fazla 1000 karakter olabilir")]
        public string? UsageInstructions { get; set; }

        [StringLength(500, ErrorMessage = "Yan etkiler açıklaması en fazla 500 karakter olabilir")]
        public string? SideEffects { get; set; }
    }

    public class UpdateMedicineDto
    {
        [Required(ErrorMessage = "İlaç adı gereklidir")]
        [StringLength(200, ErrorMessage = "İlaç adı en fazla 200 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori gereklidir")]
        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz")]
        public int StockQuantity { get; set; }

        [StringLength(100, ErrorMessage = "Üretici adı en fazla 100 karakter olabilir")]
        public string? Manufacturer { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50, ErrorMessage = "Lot numarası en fazla 50 karakter olabilir")]
        public string? LotNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi negatif olamaz")]
        public int MinimumStockLevel { get; set; } = 10;

        public bool RequiresPrescription { get; set; } = false;

        [StringLength(1000, ErrorMessage = "Kullanım talimatları en fazla 1000 karakter olabilir")]
        public string? UsageInstructions { get; set; }

        [StringLength(500, ErrorMessage = "Yan etkiler açıklaması en fazla 500 karakter olabilir")]
        public string? SideEffects { get; set; }
    }

    public class MedicineDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? LotNumber { get; set; }
        public int MinimumStockLevel { get; set; }
        public bool RequiresPrescription { get; set; }
        public string? UsageInstructions { get; set; }
        public string? SideEffects { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
    }

    public class StockUpdateDto
    {
        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz")]
        public int NewStockQuantity { get; set; }

        [StringLength(200, ErrorMessage = "Güncelleme nedeni en fazla 200 karakter olabilir")]
        public string? UpdateReason { get; set; }
    }
}
