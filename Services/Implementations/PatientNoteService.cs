using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class PatientNoteService : IPatientNoteService
    {
        private readonly IMongoCollection<PatientNote> _patientNotes;

        public PatientNoteService(IMongoDatabase database)
        {
            _patientNotes = database.GetCollection<PatientNote>("PatientNotes");
        }

        public async Task<List<PatientNote>> GetAllNotesAsync()
        {
            return await _patientNotes.Find(_ => true).ToListAsync();
        }

        public async Task<PatientNote?> GetNoteByIdAsync(string id)
        {
            return await _patientNotes.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateNoteAsync(PatientNote note)
        {
            note.CreatedAt = DateTime.UtcNow;
            await _patientNotes.InsertOneAsync(note);
        }

        public async Task UpdateNoteAsync(string id, PatientNote note)
        {
            note.UpdatedAt = DateTime.UtcNow;
            await _patientNotes.ReplaceOneAsync(p => p.Id == id, note);
        }

        public async Task DeleteNoteAsync(string id)
        {
            await _patientNotes.DeleteOneAsync(p => p.Id == id);
        }

        public async Task<List<PatientNote>> GetNotesByPatientIdAsync(string patientId)
        {
            return await _patientNotes.Find(p => p.PatientId == patientId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientNote>> GetNotesByDoctorIdAsync(string doctorId)
        {
            // Modelde DoctorId yok; CreatedBy doktor/hemşire id'si olarak kullanılıyor
            return await _patientNotes.Find(p => p.CreatedBy == doctorId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientNote>> GetNotesByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await _patientNotes.Find(p => p.CreatedAt >= start && p.CreatedAt < end)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientNote>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _patientNotes.Find(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientNote>> SearchNotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllNotesAsync();
            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filter = Builders<PatientNote>.Filter.Or(
                Builders<PatientNote>.Filter.Regex(n => n.Title, regex),
                Builders<PatientNote>.Filter.Regex(n => n.Content, regex),
                Builders<PatientNote>.Filter.Regex(n => n.Tags, regex)
            );
            return await _patientNotes.Find(filter).SortByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<int> GetNotesCountByPatientAsync(string patientId)
        {
            return (int)await _patientNotes.CountDocumentsAsync(p => p.PatientId == patientId);
        }

        public async Task<int> GetNotesCountByDoctorAsync(string doctorId)
        {
            return (int)await _patientNotes.CountDocumentsAsync(p => p.CreatedBy == doctorId);
        }
    }
}
