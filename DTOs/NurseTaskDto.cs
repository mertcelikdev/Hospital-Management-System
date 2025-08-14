using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateNurseTaskDto
    {
        [Required(ErrorMessage = "Görev başlığı gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev açıklaması gereklidir")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Atanacak hemşire gereklidir")]
        public string AssignedToId { get; set; } = string.Empty;

        public string? PatientId { get; set; }
        public string Priority { get; set; } = "Normal";
        public DateTime? DueDate { get; set; }
        public string? Category { get; set; }
        public bool RequiresEquipment { get; set; } = false;
        public string? EquipmentNeeded { get; set; }
        public string? Instructions { get; set; }
    }

    public class UpdateNurseTaskDto
    {
        [Required(ErrorMessage = "Görev başlığı gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev açıklaması gereklidir")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        public string? AssignedToId { get; set; }
        public string? PatientId { get; set; }
        public string Priority { get; set; } = "Normal";
        public DateTime? DueDate { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = "Pending";
        public bool RequiresEquipment { get; set; } = false;
        public string? EquipmentNeeded { get; set; }
        public string? Instructions { get; set; }
        public string? Notes { get; set; }
    }

    public class NurseTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignedToId { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool RequiresEquipment { get; set; }
        public string? EquipmentNeeded { get; set; }
        public string? Instructions { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class NurseTaskSearchDto
    {
        public string? AssignedToId { get; set; }
        public string? PatientId { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsOverdue { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class TaskStatusUpdateDto
    {
        [Required(ErrorMessage = "Durum gereklidir")]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
