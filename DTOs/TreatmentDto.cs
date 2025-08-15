using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class TreatmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? NurseId { get; set; }
        public string? NurseName { get; set; }
        public string TreatmentName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Results { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTreatmentDto
    {
        [Required(ErrorMessage = "Hasta seçimi gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tedavi adı gereklidir")]
        [StringLength(200, ErrorMessage = "Tedavi adı en fazla 200 karakter olabilir")]
        public string TreatmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tedavi türü gereklidir")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama gereklidir")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Talimatlar en fazla 1000 karakter olabilir")]
        public string Instructions { get; set; } = string.Empty;

        public string? NurseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notes { get; set; }
    }

    public class UpdateTreatmentDto
    {
        [StringLength(200, ErrorMessage = "Tedavi adı en fazla 200 karakter olabilir")]
        public string? TreatmentName { get; set; }

        public string? Type { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }

        [StringLength(1000, ErrorMessage = "Talimatlar en fazla 1000 karakter olabilir")]
        public string? Instructions { get; set; }

        public string? Status { get; set; }
        public string? NurseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [StringLength(1000, ErrorMessage = "Sonuçlar en fazla 1000 karakter olabilir")]
        public string? Results { get; set; }

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notes { get; set; }
    }

    public class TreatmentSearchDto
    {
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? NurseId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
