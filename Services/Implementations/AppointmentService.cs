using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IMongoCollection<Appointment> _appointments;
        private readonly IMongoCollection<User> _users;

        public AppointmentService(IMongoDatabase database)
        {
            _appointments = database.GetCollection<Appointment>("Appointments");
            _users = database.GetCollection<User>("Users");
        }

        // CRUD Operations
        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _appointments.Find(a => a.DeletedAt == null).ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string id)
        {
            return await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _appointments.Find(a => a.PatientId == patientId).ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            return await _appointments.Find(a => a.DoctorId == doctorId).ToListAsync();
        }

        public async Task CreateAppointmentAsync(Appointment appointment)
        {
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointments.InsertOneAsync(appointment);
        }

        public async Task UpdateAppointmentAsync(string id, Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointments.ReplaceOneAsync(a => a.Id == id, appointment);
        }

        public async Task DeleteAppointmentAsync(string id)
        {
            await _appointments.DeleteOneAsync(a => a.Id == id);
        }

        // Soft delete (iptal kaydı tut)
        public async Task<bool> SoftDeleteAppointmentAsync(string id, string deletedBy)
        {
            var update = Builders<Appointment>.Update
                .Set(a => a.Status, "İptalEdildi")
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
                .Set(a => a.DeletedAt, DateTime.UtcNow)
                .Set(a => a.DeletedBy, deletedBy);
            var result = await _appointments.UpdateOneAsync(a => a.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Status Operations
    public async Task<bool> UpdateAppointmentStatusAsync(string id, string status)
        {
            var update = Builders<Appointment>.Update
        .Set(a => a.Status, status)
                .Set(a => a.UpdatedAt, DateTime.UtcNow);
            
            var result = await _appointments.UpdateOneAsync(a => a.Id == id, update);
            return result.ModifiedCount > 0;
        }

    public async Task<List<Appointment>> GetAppointmentsByStatusAsync(string status)
        {
            return await _appointments.Find(a => a.Status == status).ToListAsync();
        }

        // Date Operations
        public async Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);
            return await _appointments.Find(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate).ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _appointments.Find(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate).ToListAsync();
        }

        // Today's Appointments
        public async Task<List<Appointment>> GetTodaysAppointmentsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _appointments.Find(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow && a.DeletedAt == null).ToListAsync();
        }

        public async Task<List<Appointment>> GetTodaysAppointmentsByDoctorAsync(string doctorId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _appointments.Find(a => a.DoctorId == doctorId && a.AppointmentDate >= today && a.AppointmentDate < tomorrow && a.DeletedAt == null).ToListAsync();
        }

        // Upcoming Appointments
        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync()
        {
            var now = DateTime.UtcNow;
            return await _appointments.Find(a => a.AppointmentDate > now && a.DeletedAt == null).ToListAsync();
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsByPatientAsync(string patientId)
        {
            var now = DateTime.UtcNow;
            return await _appointments.Find(a => a.PatientId == patientId && a.AppointmentDate > now && a.DeletedAt == null).ToListAsync();
        }

        // Past Appointments
        public async Task<List<Appointment>> GetPastAppointmentsAsync()
        {
            var now = DateTime.UtcNow;
            return await _appointments.Find(a => a.AppointmentDate < now && a.DeletedAt == null).ToListAsync();
        }

        public async Task<List<Appointment>> GetPastAppointmentsByPatientAsync(string patientId)
        {
            var now = DateTime.UtcNow;
            return await _appointments.Find(a => a.PatientId == patientId && a.AppointmentDate < now && a.DeletedAt == null).ToListAsync();
        }

        // Search and Filter
    public async Task<List<Appointment>> SearchAppointmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAppointmentsAsync();

            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");

            // Mevcut model alanları: Notes, Type, Status. Kullanıcı/ hasta isimleri Appointment içinde yok.
            var filter = Builders<Appointment>.Filter.Or(
                Builders<Appointment>.Filter.Regex(a => a.Notes, regex),
                Builders<Appointment>.Filter.Regex(a => a.Type, regex),
                Builders<Appointment>.Filter.Regex(a => a.Status, regex)
            );
            return await _appointments.Find(filter).ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByTypeAsync(string type)
        {
            return await _appointments.Find(a => a.Type == type).ToListAsync();
        }

        // Statistics
        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.DeletedAt == null);
        }

    public async Task<int> GetAppointmentsCountByStatusAsync(string status)
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.Status == status && a.DeletedAt == null);
        }

        public async Task<int> GetAppointmentsCountByDoctorAsync(string doctorId)
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.DeletedAt == null);
        }

        public async Task<int> GetAppointmentsCountByPatientAsync(string patientId)
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.PatientId == patientId && a.DeletedAt == null);
        }

        // Availability
        public async Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime appointmentTime)
        {
            var existingAppointment = await _appointments
                .Find(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentTime)
                .FirstOrDefaultAsync();
            return existingAppointment == null;
        }

        public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date)
        {
            var availableSlots = new List<DateTime>();
            var workingHours = Enumerable.Range(9, 8); // 9 AM to 5 PM
            
            foreach (var hour in workingHours)
            {
                var timeSlot = date.Date.AddHours(hour);
                if (await IsTimeSlotAvailableAsync(doctorId, timeSlot))
                {
                    availableSlots.Add(timeSlot);
                }
            }
            
            return availableSlots;
        }

        public async Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentTime, TimeSpan duration)
        {
            var endTime = appointmentTime.Add(duration);
            var conflictingAppointments = await _appointments
                .Find(a => a.DoctorId == doctorId && 
                          ((a.AppointmentDate >= appointmentTime && a.AppointmentDate < endTime) ||
                           (a.AppointmentDate.Add(TimeSpan.FromMinutes(30)) > appointmentTime && a.AppointmentDate < appointmentTime)))
                .ToListAsync();
            
            return conflictingAppointments.Count == 0;
        }

        // Validation
        public async Task<bool> ValidateAppointmentAsync(Appointment appointment)
        {
            if (appointment.AppointmentDate <= DateTime.UtcNow)
                return false;
                
            return await IsTimeSlotAvailableAsync(appointment.DoctorId, appointment.AppointmentDate);
        }

        public async Task<bool> CanCancelAppointmentAsync(string appointmentId, string userId, string userRole)
        {
            var appointment = await GetAppointmentByIdAsync(appointmentId);
            if (appointment == null) return false;
            
            // Admin can cancel any appointment
            if (userRole == "Admin" || userRole == "Staff") return true;
            
            // Doctor can cancel their own appointments
            if (userRole == "Doctor" && appointment.DoctorId == userId) return true;
            
            // Patient can cancel their own appointments if it's more than 24 hours away
            if (userRole == "Patient" && appointment.PatientId == userId)
            {
                return appointment.AppointmentDate > DateTime.UtcNow.AddHours(24);
            }
            
            return false;
        }

        public async Task<bool> CanUpdateAppointmentAsync(string appointmentId, string userId, string userRole)
        {
            var appointment = await GetAppointmentByIdAsync(appointmentId);
            if (appointment == null) return false;
            
            // Admin can update any appointment
            if (userRole == "Admin" || userRole == "Staff") return true;
            
            // Doctor can update their own appointments
            if (userRole == "Doctor" && appointment.DoctorId == userId) return true;
            
            // Patient can update their own appointments if it's more than 24 hours away
            if (userRole == "Patient" && appointment.PatientId == userId)
            {
                return appointment.AppointmentDate > DateTime.UtcNow.AddHours(24);
            }
            
            return false;
        }

        // Additional methods for Dashboard
        public async Task<int> GetTodayAppointmentsCountAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return (int)await _appointments.CountDocumentsAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow);
        }

        public async Task<int> GetPendingAppointmentsCountAsync()
        {
            return await GetAppointmentsCountByStatusAsync("Planlandı");
        }

        public async Task<int> GetCompletedAppointmentsCountAsync()
        {
            return await GetAppointmentsCountByStatusAsync("Tamamlandı");
        }

        public async Task<List<object>> GetAppointmentsByDoctorAsync(string doctorId)
        {
            var appointments = await _appointments.Find(a => a.DoctorId == doctorId).ToListAsync();
            return appointments.Cast<object>().ToList();
        }

        public async Task<List<object>> GetTodayAppointmentsByDoctorAsync(string doctorId)
        {
            var appointments = await GetTodaysAppointmentsByDoctorAsync(doctorId);
            return appointments.Cast<object>().ToList();
        }

        public async Task<int> GetPatientCountByDoctorAsync(string doctorId)
        {
            var patientIds = await _appointments
                .Find(a => a.DoctorId == doctorId)
                .Project(a => a.PatientId)
                .ToListAsync();
            
            return patientIds.Distinct().Count();
        }

        public async Task<int> GetCompletedAppointmentsByDoctorCountAsync(string doctorId)
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.Status == "Tamamlandı");
        }

        public async Task<List<object>> GetUpcomingAppointmentsByDoctorAsync(string doctorId, int count)
        {
            var now = DateTime.UtcNow;
            var appointments = await _appointments
                .Find(a => a.DoctorId == doctorId && a.AppointmentDate > now && a.DeletedAt == null)
                .SortBy(a => a.AppointmentDate)
                .Limit(count)
                .ToListAsync();
            
            return appointments.Cast<object>().ToList();
        }

        public async Task<List<object>> GetRecentAppointmentsAsync(int count)
        {
            var appointments = await _appointments
                .Find(a => a.DeletedAt == null)
                .SortByDescending(a => a.CreatedAt)
                .Limit(count)
                .ToListAsync();
            
            return appointments.Cast<object>().ToList();
        }

        // Silinen randevuları listeleme
        public async Task<List<Appointment>> GetDeletedAppointmentsAsync()
        {
            return await _appointments.Find(a => a.DeletedAt != null).SortByDescending(a => a.DeletedAt).ToListAsync();
        }

        // Restore silinen randevu
        public async Task<bool> RestoreAppointmentAsync(string id)
        {
            var update = Builders<Appointment>.Update
                .Set(a => a.DeletedAt, null)
                .Set(a => a.DeletedBy, null)
                .Set(a => a.Status, "Planlandı")
                .Set(a => a.UpdatedAt, DateTime.UtcNow);
            var result = await _appointments.UpdateOneAsync(a => a.Id == id && a.DeletedAt != null, update);
            return result.ModifiedCount > 0;
        }

        // Collection temizleme (serialization sorunları için)
        public async Task<bool> ClearAllAppointmentsAsync()
        {
            var result = await _appointments.DeleteManyAsync(Builders<Appointment>.Filter.Empty);
            return result.DeletedCount > 0;
        }
    }
}
