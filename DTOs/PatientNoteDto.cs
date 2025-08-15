using HospitalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreatePatientNoteDto
    {
        [Required(ErrorMessage = "Hasta ID gereklidir")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Not içeriği gereklidir")]
        [StringLength(2000, ErrorMessage = "Not içeriği en fazla 2000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;

        public string? Category { get; set; }
        public bool IsUrgent { get; set; } = false;
        public DateTime? FollowUpDate { get; set; }
    }

    public class UpdatePatientNoteDto
    {
        [Required(ErrorMessage = "Not içeriği gereklidir")]
        [StringLength(2000, ErrorMessage = "Not içeriği en fazla 2000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;

        public string? Category { get; set; }
        public bool IsUrgent { get; set; } = false;
        public DateTime? FollowUpDate { get; set; }
        public string? UpdateReason { get; set; }
    }

    public class PatientNoteDto
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsUrgent { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool IsFollowUpCompleted { get; set; }
    }
}
