using HospitalManagementSystem.Models;
using MongoDB.Driver;

namespace HospitalManagementSystem.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId);
        Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Appointment?> GetAppointmentByIdAsync(string id);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<bool> UpdateAppointmentAsync(string id, Appointment appointment);
        Task<bool> UpdateAppointmentStatusAsync(string id, AppointmentStatus status);
        Task<bool> DeleteAppointmentAsync(string id);
        Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentDate, TimeSpan duration);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly IMongoCollection<Appointment> _appointments;
        private readonly IUserService _userService;

        public AppointmentService(IMongoDbContext context, IUserService userService)
        {
            _appointments = context.GetCollection<Appointment>("Appointments");
            _userService = userService;
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            var appointments = await _appointments.Find(x => x.PatientId == patientId).ToListAsync();
            await PopulateNavigationPropertiesAsync(appointments);
            return appointments;
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            var appointments = await _appointments.Find(x => x.DoctorId == doctorId).ToListAsync();
            await PopulateNavigationPropertiesAsync(appointments);
            return appointments;
        }

        public async Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            var appointments = await _appointments.Find(x => x.AppointmentDate >= startDate && x.AppointmentDate < endDate).ToListAsync();
            await PopulateNavigationPropertiesAsync(appointments);
            return appointments;
        }

        public async Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var appointments = await _appointments.Find(x => x.AppointmentDate >= startDate && x.AppointmentDate <= endDate).ToListAsync();
            await PopulateNavigationPropertiesAsync(appointments);
            return appointments;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string id)
        {
            var appointment = await _appointments.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (appointment != null)
            {
                await PopulateNavigationPropertiesAsync(new List<Appointment> { appointment });
            }
            return appointment;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            
            await _appointments.InsertOneAsync(appointment);
            return appointment;
        }

        public async Task<bool> UpdateAppointmentAsync(string id, Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            var result = await _appointments.ReplaceOneAsync(x => x.Id == id, appointment);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(string id, AppointmentStatus status)
        {
            var update = Builders<Appointment>.Update
                .Set(x => x.Status, status)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            
            var result = await _appointments.UpdateOneAsync(x => x.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAppointmentAsync(string id)
        {
            var result = await _appointments.DeleteOneAsync(x => x.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentDate, TimeSpan duration)
        {
            var appointmentEnd = appointmentDate.Add(duration);
            
            var conflictingAppointments = await _appointments.Find(x => 
                x.DoctorId == doctorId &&
                x.Status != AppointmentStatus.Cancelled &&
                x.AppointmentDate < appointmentEnd &&
                x.AppointmentDate.Add(x.Duration) > appointmentDate
            ).ToListAsync();
            
            return !conflictingAppointments.Any();
        }

        private async Task PopulateNavigationPropertiesAsync(List<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                appointment.Patient = await _userService.GetUserByIdAsync(appointment.PatientId);
                appointment.Doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
            }
        }
    }
}
