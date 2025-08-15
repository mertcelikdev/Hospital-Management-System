using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface IAppointmentService
    {
        // CRUD Operations
    Task<List<AppointmentDto>> GetAllAppointmentsAsync(); // eski kullanım (geri uyum)
    Task<List<AppointmentDto>> GetAllAppointmentsAsync(int page, int pageSize, string? sort = null, string? dir = null);
    Task<(List<AppointmentDto> Items, long TotalCount)> GetAppointmentsPagedAsync(int page, int pageSize, string? sort = null, string? dir = null, string? q = null, string? departmentId = null, string? doctorId = null, string? patientId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, string? type = null);
        Task<AppointmentDto?> GetAppointmentByIdAsync(string id);
        Task<List<AppointmentDto>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<List<AppointmentDto>> GetAppointmentsByDoctorIdAsync(string doctorId);
    Task<List<AppointmentDto>> GetAppointmentsByPatientTcAsync(string tc);
    Task CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto, string? userId = null);
        Task UpdateAppointmentAsync(string id, UpdateAppointmentDto updateAppointmentDto);
    Task DeleteAppointmentAsync(string id, string? userId = null);
    Task<bool> RestoreAppointmentAsync(string id);
    Task<bool> HardDeleteAppointmentAsync(string id);
    Task<List<AppointmentDto>> GetDeletedAppointmentsAsync();

        // Status Operations
        Task<bool> UpdateAppointmentStatusAsync(string id, AppointmentStatus status);
        Task<List<AppointmentDto>> GetAppointmentsByStatusAsync(AppointmentStatus status);

        // Date Operations
        Task<List<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date);
        Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Today's Appointments
        Task<List<AppointmentDto>> GetTodaysAppointmentsAsync();
        Task<List<AppointmentDto>> GetTodaysAppointmentsByDoctorAsync(string doctorId);

        // Upcoming Appointments
        Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync();
        Task<List<AppointmentDto>> GetUpcomingAppointmentsByPatientAsync(string patientId);

        // Past Appointments
        Task<List<AppointmentDto>> GetPastAppointmentsAsync();
        Task<List<AppointmentDto>> GetPastAppointmentsByPatientAsync(string patientId);

        // Search and Filter
        Task<List<AppointmentDto>> SearchAppointmentsAsync(string searchTerm);
        Task<List<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type);

        // Statistics
        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetAppointmentsCountByStatusAsync(AppointmentStatus status);
        Task<int> GetAppointmentsCountByDoctorAsync(string doctorId);
        Task<int> GetAppointmentsCountByPatientAsync(string patientId);

        // Availability
        Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime appointmentTime);
        Task<List<DateTime>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date);
        Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentTime, TimeSpan duration);
    Task<bool> IsTimeSlotAvailableForUpdateAsync(string doctorId, DateTime appointmentTime, string excludeAppointmentId);

        // Validation
        Task<bool> ValidateAppointmentAsync(CreateAppointmentDto appointmentDto);
        Task<bool> CanCancelAppointmentAsync(string appointmentId, string userId, string userRole);
        Task<bool> CanUpdateAppointmentAsync(string appointmentId, string userId, string userRole);

        // Dashboard metrics
    Task<int> GetTodayAppointmentsCountAsync(); // mevcut (status tabanlı simplification)
    Task<int> GetTodayAppointmentsRealCountAsync(); // gerçek bugünün tüm randevuları
        Task<int> GetPendingAppointmentsCountAsync();
        Task<int> GetCompletedAppointmentsCountAsync();
        Task<List<AppointmentDto>> GetAppointmentsByDoctorAsync(string doctorId);
        Task<List<AppointmentDto>> GetTodayAppointmentsByDoctorAsync(string doctorId);
        Task<int> GetPatientCountByDoctorAsync(string doctorId);
        Task<int> GetCompletedAppointmentsByDoctorCountAsync(string doctorId);
        Task<List<AppointmentDto>> GetUpcomingAppointmentsByDoctorAsync(string doctorId, int count);
        Task<List<AppointmentDto>> GetRecentAppointmentsAsync(int count);
    }
}