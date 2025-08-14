using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface IAppointmentService
    {
        // CRUD Operations
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(string id);
        Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId);
        Task CreateAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(string id, Appointment appointment);
        Task DeleteAppointmentAsync(string id);

        // Status Operations
        Task<bool> UpdateAppointmentStatusAsync(string id, AppointmentStatus status);
        Task<List<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status);

        // Date Operations
        Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Today's Appointments
        Task<List<Appointment>> GetTodaysAppointmentsAsync();
        Task<List<Appointment>> GetTodaysAppointmentsByDoctorAsync(string doctorId);

        // Upcoming Appointments
        Task<List<Appointment>> GetUpcomingAppointmentsAsync();
        Task<List<Appointment>> GetUpcomingAppointmentsByPatientAsync(string patientId);

        // Past Appointments
        Task<List<Appointment>> GetPastAppointmentsAsync();
        Task<List<Appointment>> GetPastAppointmentsByPatientAsync(string patientId);

        // Search and Filter
        Task<List<Appointment>> SearchAppointmentsAsync(string searchTerm);
        Task<List<Appointment>> GetAppointmentsByTypeAsync(AppointmentType type);

        // Statistics
        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetAppointmentsCountByStatusAsync(AppointmentStatus status);
        Task<int> GetAppointmentsCountByDoctorAsync(string doctorId);
        Task<int> GetAppointmentsCountByPatientAsync(string patientId);

        // Availability
        Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime appointmentTime);
        Task<List<DateTime>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date);
        Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentTime, TimeSpan duration);

        // Validation
        Task<bool> ValidateAppointmentAsync(Appointment appointment);
        Task<bool> CanCancelAppointmentAsync(string appointmentId, string userId, string userRole);
        Task<bool> CanUpdateAppointmentAsync(string appointmentId, string userId, string userRole);

        // Additional methods for Dashboard
        Task<int> GetTodayAppointmentsCountAsync();
        Task<int> GetPendingAppointmentsCountAsync();
        Task<int> GetCompletedAppointmentsCountAsync();
        Task<List<object>> GetAppointmentsByDoctorAsync(string doctorId);
        Task<List<object>> GetTodayAppointmentsByDoctorAsync(string doctorId);
        Task<int> GetPatientCountByDoctorAsync(string doctorId);
        Task<int> GetCompletedAppointmentsByDoctorCountAsync(string doctorId);
        Task<List<object>> GetUpcomingAppointmentsByDoctorAsync(string doctorId, int count);
        Task<List<object>> GetRecentAppointmentsAsync(int count);
    }
}