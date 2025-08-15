using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    // ==================== COMMON DTOs ====================
    
    /// <summary>
    /// Genel API yanıt standardı
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Basit API yanıt standardı (data olmayan)
    /// </summary>
    public class SimpleApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // ==================== SEARCH DTOs ====================
    
    /// <summary>
    /// Genel arama DTO'su - tüm entity'ler için kullanılabilir
    /// </summary>
    public class BaseSearchDto
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortField { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// Kullanıcı arama DTO'su
    /// </summary>
    public class UserSearchDto : BaseSearchDto
    {
        public string? Role { get; set; }
        public string? DepartmentId { get; set; }
    }

    /// <summary>
    /// Hasta arama DTO'su
    /// </summary>
    public class PatientSearchDto : BaseSearchDto
    {
        public string? IdentityNumber { get; set; }
        public string? Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }

    /// <summary>
    /// Randevu arama DTO'su
    /// </summary>
    public class AppointmentSearchDto : BaseSearchDto
    {
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public bool? IsUrgent { get; set; }
    }

    /// <summary>
    /// İlaç arama DTO'su
    /// </summary>
    public class MedicineSearchDto : BaseSearchDto
    {
        public string? Category { get; set; }
        public string? Manufacturer { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsLowStock { get; set; }
        public bool? IsExpired { get; set; }
    }

    /// <summary>
    /// Bölüm arama DTO'su
    /// </summary>
    public class DepartmentSearchDto : BaseSearchDto
    {
        public string? DepartmentType { get; set; }
        public string? HeadId { get; set; }
    }

    // ==================== STATUS UPDATE DTOs ====================
    
    /// <summary>
    /// Genel durum güncelleme DTO'su
    /// </summary>
    public class StatusUpdateDto
    {
        [Required(ErrorMessage = "Durum gereklidir")]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        public string? Notes { get; set; }
        
        public string? Reason { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Randevu durum güncelleme DTO'su
    /// </summary>
    public class AppointmentStatusUpdateDto : StatusUpdateDto
    {
        public string? CancellationReason { get; set; }
        public DateTime? RescheduleDate { get; set; }
        public TimeSpan? RescheduleTime { get; set; }
    }

    // ==================== PAGINATION DTOs ====================
    
    /// <summary>
    /// Sayfalama sonucu DTO'su
    /// </summary>
    public class PaginatedResultDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    // ==================== CONTACT & ADDRESS DTOs ====================
    
    /// <summary>
    /// İletişim bilgileri DTO'su
    /// </summary>
    public class ContactInfoDto
    {
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }
        
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }
        
        public string? EmergencyContact { get; set; }
        public string? EmergencyContactRelation { get; set; }
    }

    /// <summary>
    /// Adres bilgileri DTO'su
    /// </summary>
    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; } = "Türkiye";
        public string? FullAddress { get; set; }
    }

    // ==================== AUDIT DTOs ====================
    
    /// <summary>
    /// Audit bilgileri DTO'su
    /// </summary>
    public class AuditDto
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    // ==================== VALIDATION DTOs ====================
    
    /// <summary>
    /// Şifre değişiklik DTO'su
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre onayı gereklidir")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ==================== FILE UPLOAD DTOs ====================
    
    /// <summary>
    /// Dosya yükleme DTO'su
    /// </summary>
    public class FileUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsPublic { get; set; } = false;
    }

    /// <summary>
    /// Dosya bilgisi DTO'su
    /// </summary>
    public class FileInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }

    // ==================== DASHBOARD DTOs ====================
    
    /// <summary>
    /// Dashboard istatistik DTO'su
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalNurses { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int TotalMedicines { get; set; }
        public int LowStockMedicines { get; set; }
        public int ExpiredMedicines { get; set; }
        public int ActiveTasks { get; set; }
        public int CompletedTasks { get; set; }
    }

    /// <summary>
    /// Dashboard grafik verisi DTO'su
    /// </summary>
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
        public string ChartType { get; set; } = "line";
        public string Title { get; set; } = string.Empty;
    }
}
