using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class PrescriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? NurseId { get; set; }
        public string? NurseName { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public List<PrescriptionMedicineDto> Medicines { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PrescriptionMedicineDto
    {
        public string MedicineId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }

    public class CreatePrescriptionDto
    {
        [Required(ErrorMessage = "Hasta seçimi gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanı gereklidir")]
        [StringLength(500, ErrorMessage = "Tanı en fazla 500 karakter olabilir")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "En az bir ilaç eklenmesi gereklidir")]
        public List<CreatePrescriptionMedicineDto> Medicines { get; set; } = new();

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        public string? NurseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreatePrescriptionMedicineDto
    {
        [Required(ErrorMessage = "İlaç seçimi gereklidir")]
        public string MedicineId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doz gereklidir")]
        [StringLength(100, ErrorMessage = "Doz en fazla 100 karakter olabilir")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanım sıklığı gereklidir")]
        [StringLength(100, ErrorMessage = "Kullanım sıklığı en fazla 100 karakter olabilir")]
        public string Frequency { get; set; } = string.Empty;

        [Range(1, 365, ErrorMessage = "Süre 1-365 gün arasında olmalıdır")]
        public int Duration { get; set; }

        [StringLength(500, ErrorMessage = "Talimatlar en fazla 500 karakter olabilir")]
        public string Instructions { get; set; } = string.Empty;
    }

    public class UpdatePrescriptionDto
    {
        [StringLength(500, ErrorMessage = "Tanı en fazla 500 karakter olabilir")]
        public string? Diagnosis { get; set; }

        public List<CreatePrescriptionMedicineDto>? Medicines { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        public string? Status { get; set; }
        public string? NurseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PrescriptionSearchDto
    {
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
