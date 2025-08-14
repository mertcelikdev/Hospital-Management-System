using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = "Hasta ID gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doktor ID gereklidir")]
        public string DoctorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Randevu tarihi gereklidir")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati gereklidir")]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(500, ErrorMessage = "Randevu nedeni en fazla 500 karakter olabilir")]
        public string? Reason { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        public string Priority { get; set; } = "Normal";
        public bool IsUrgent { get; set; } = false;
    }

    public class UpdateAppointmentDto
    {
        [Required(ErrorMessage = "Randevu tarihi gereklidir")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati gereklidir")]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(500, ErrorMessage = "Randevu nedeni en fazla 500 karakter olabilir")]
        public string? Reason { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        public string Priority { get; set; } = "Normal";
        public bool IsUrgent { get; set; } = false;
        public string Status { get; set; } = "Scheduled";
    }

    public class AppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? DoctorSpecialization { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string Priority { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public bool IsPastDue { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class AppointmentSearchDto
    {
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsUrgent { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AppointmentStatusUpdateDto
    {
        [Required(ErrorMessage = "Durum gereklidir")]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class RescheduleAppointmentDto
    {
        [Required(ErrorMessage = "Yeni randevu tarihi gereklidir")]
        public DateTime NewAppointmentDate { get; set; }

        [Required(ErrorMessage = "Yeni randevu saati gereklidir")]
        public TimeSpan NewAppointmentTime { get; set; }

        [StringLength(200, ErrorMessage = "Erteleme nedeni en fazla 200 karakter olabilir")]
        public string? RescheduleReason { get; set; }
    }
}
