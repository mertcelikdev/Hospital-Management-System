using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IMongoCollection<Patient> _patients;
        private readonly IMongoCollection<Appointment> _appointments;
        private readonly IMongoCollection<PatientNote> _patientNotes;

        public PatientService(IMongoDatabase database)
        {
            _patients = database.GetCollection<Patient>("Patients");
            _appointments = database.GetCollection<Appointment>("Appointments");
            _patientNotes = database.GetCollection<PatientNote>("PatientNotes");
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _patients.Find(_ => true).ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(string id)
        {
            return await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Patient?> GetPatientByTcNoAsync(string tcNo)
        {
            return await _patients.Find(p => p.TcNo == tcNo).FirstOrDefaultAsync();
        }

        public async Task CreatePatientAsync(Patient patient)
        {
            patient.CreatedAt = DateTime.UtcNow;
            patient.UpdatedAt = DateTime.UtcNow;
            await _patients.InsertOneAsync(patient);
        }

        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            patient.UpdatedAt = DateTime.UtcNow;
            await _patients.ReplaceOneAsync(p => p.Id == id, patient);
        }

        public async Task DeletePatientAsync(string id)
        {
            await _patients.DeleteOneAsync(p => p.Id == id);
        }

        public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllPatientsAsync();
            }
            // Yalnızca TC Kimlik No üzerinden arama (sayısal değilse veya 3 haneden azsa sonuç dönme)
            if (!searchTerm.All(char.IsDigit) || searchTerm.Length < 3)
            {
                return new List<Patient>();
            }

            FilterDefinition<Patient> filter = searchTerm.Length == 11
                ? Builders<Patient>.Filter.Eq(p => p.IdentityNumber, searchTerm)
                : Builders<Patient>.Filter.Regex(p => p.IdentityNumber, new MongoDB.Bson.BsonRegularExpression($"^{searchTerm}", "i"));

            return await _patients.Find(filter).Limit(50).ToListAsync();
        }

        public async Task<List<Patient>> GetPatientsByDoctorAsync(string doctorId)
        {
            // Doktora ait randevulardan distinct patient id listesi
            var patientIds = await _appointments
                .Find(a => a.DoctorId == doctorId)
                .Project(a => a.PatientId)
                .ToListAsync();

            var distinctIds = patientIds.Distinct().ToList();
            if (!distinctIds.Any()) return new List<Patient>();

            var patients = await _patients.Find(p => p.Id != null && distinctIds.Contains(p.Id)).ToListAsync();
            return patients;
        }

        public async Task<int> GetTotalPatientsCountAsync()
        {
            return (int)await _patients.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> PatientExistsAsync(string tcNo)
        {
            var count = await _patients.CountDocumentsAsync(p => p.TcNo == tcNo);
            return count > 0;
        }

        public async Task<List<Appointment>> GetPatientAppointmentsAsync(string patientId)
        {
            return await _appointments.Find(a => a.PatientId == patientId).ToListAsync();
        }

        public async Task<List<PatientNote>> GetPatientNotesAsync(string patientId)
        {
            return await _patientNotes.Find(n => n.PatientId == patientId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientWithDetailsAsync(string id)
        {
            // Şimdilik sadece hastayı döndürüyoruz; detaylar gerektiğinde ayrı servis çağrılarıyla alınabilir.
            return await GetPatientByIdAsync(id);
        }
    }
}
