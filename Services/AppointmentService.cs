using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class AppointmentService
    {
        private readonly IMongoCollection<Appointment> _appointments;
        private readonly IMongoCollection<User> _users;

        public AppointmentService(IMongoDatabase database)
        {
            _appointments = database.GetCollection<Appointment>("Appointments");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _appointments.Find(_ => true).ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(string id)
        {
            return await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            await _appointments.InsertOneAsync(appointment);
            return appointment;
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

        public async Task<List<Appointment>> GetPatientAppointmentsAsync(string patientId)
        {
            return await _appointments.Find(a => a.PatientId == patientId).ToListAsync();
        }

        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId)
        {
            return await _appointments.Find(a => a.DoctorId == doctorId).ToListAsync();
        }

        public async Task<List<Appointment>> GetDailyAppointmentsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);
            return await _appointments.Find(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate).ToListAsync();
        }

        public async Task UpdateAppointmentStatusAsync(string id, AppointmentStatus status)
        {
            var update = Builders<Appointment>.Update
                .Set(a => a.Status, status)
                .Set(a => a.UpdatedAt, DateTime.UtcNow);
            
            await _appointments.UpdateOneAsync(a => a.Id == id, update);
        }
    }
}
