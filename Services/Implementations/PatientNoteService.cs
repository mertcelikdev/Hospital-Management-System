using MongoDB.Driver;
using MongoDB.Bson;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class PatientNoteService : IPatientNoteService
    {
        private readonly IMongoCollection<PatientNote> _patientNotes;

        public PatientNoteService(IMongoDatabase database)
        {
            _patientNotes = database.GetCollection<PatientNote>("PatientNotes");
        }

        private PatientNoteDto ToDto(PatientNote n) => new PatientNoteDto
        {
            Id = n.Id ?? string.Empty,
            PatientId = n.PatientId,
            PatientName = string.Empty, // İsteğe bağlı: kullanıcı servisinden alınabilir
            Content = n.Content,
            Category = n.NoteType.ToString(),
            IsUrgent = n.Priority == NotePriority.Yüksek || n.Priority == NotePriority.Acil,
            CreatedBy = n.CreatedByUserId,
            CreatedByName = n.CreatedByUserName,
            CreatedAt = n.CreatedAt,
            UpdatedAt = n.UpdatedAt,
            FollowUpDate = null,
            IsFollowUpCompleted = false
        };

        public async Task<List<PatientNoteDto>> GetAllNotesAsync()
        {
            var list = await _patientNotes.Find(_ => true).SortByDescending(n => n.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PatientNoteDto?> GetNoteByIdAsync(string id)
        {
            var note = await _patientNotes.Find(n => n.Id == id).FirstOrDefaultAsync();
            return note == null ? null : ToDto(note);
        }

        public async Task<List<PatientNoteDto>> GetNotesByPatientIdAsync(string patientId)
        {
            var list = await _patientNotes.Find(n => n.PatientId == patientId)
                .SortByDescending(n => n.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        // Legacy (role paramını interface istemiyor) bu yüzden sadece creatorId
        public async Task<List<PatientNoteDto>> GetNotesByCreatorAsync(string createdBy)
        {
            var list = await _patientNotes.Find(n => n.CreatedByUserId == createdBy)
                .SortByDescending(n => n.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }
        public async Task<PatientNoteDto> CreateNoteAsync(CreatePatientNoteDto createDto, string createdBy)
        {
            var model = new PatientNote
            {
                PatientId = createDto.PatientId,
                CreatedBy = createdBy,
                CreatedByUserId = createdBy,
                CreatedByName = string.Empty,
                CreatedByUserName = string.Empty,
                CreatedByRole = UserRole.Staff,
                Title = createDto.Content.Length > 50 ? createDto.Content[..50] : createDto.Content,
                Content = createDto.Content,
                NoteType = NoteType.Genel,
                Priority = createDto.IsUrgent ? NotePriority.Yüksek : NotePriority.Normal,
                Type = NoteType.Genel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _patientNotes.InsertOneAsync(model);
            return ToDto(model);
        }

        public async Task<PatientNoteDto> UpdateNoteAsync(string id, UpdatePatientNoteDto updateDto, string updatedBy)
        {
            var existing = await _patientNotes.Find(n => n.Id == id).FirstOrDefaultAsync();
            if (existing == null) throw new KeyNotFoundException("Note not found");
            if (existing.CreatedByUserId != updatedBy)
            {
                return ToDto(existing); // Yetkisiz ise değişiklik yok
            }
            existing.Content = updateDto.Content;
            existing.NoteType = updateDto.Category != null ? existing.NoteType : existing.NoteType;
            existing.Priority = updateDto.IsUrgent ? NotePriority.Yüksek : NotePriority.Normal;
            existing.UpdatedAt = DateTime.UtcNow;
            await _patientNotes.ReplaceOneAsync(n => n.Id == id, existing);
            return ToDto(existing);
        }

        public async Task<bool> DeleteNoteAsync(string id)
        {
            var result = await _patientNotes.DeleteOneAsync(n => n.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<PatientNoteDto>> SearchNotesAsync(BaseSearchDto searchDto)
        {
            var filter = Builders<PatientNote>.Filter.Empty;
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm;
                var contentFilter = Builders<PatientNote>.Filter.Regex(n => n.Content, new BsonRegularExpression(term, "i"));
                var titleFilter = Builders<PatientNote>.Filter.Regex(n => n.Title, new BsonRegularExpression(term, "i"));
                filter = Builders<PatientNote>.Filter.Or(contentFilter, titleFilter);
            }
            var list = await _patientNotes.Find(filter).SortByDescending(n => n.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public Task<List<PatientNoteDto>> GetUrgentNotesByPatientIdAsync(string patientId) => Task.FromResult(new List<PatientNoteDto>());
        public Task<int> GetNotesCountByPatientIdAsync(string patientId) => Task.FromResult(0);
        public Task<List<PatientNoteDto>> GetAllUrgentNotesAsync() => Task.FromResult(new List<PatientNoteDto>());
        public Task<List<PatientNoteDto>> GetNotesWithFollowUpAsync() => Task.FromResult(new List<PatientNoteDto>());
        public Task<List<PatientNoteDto>> GetOverdueFollowUpsAsync() => Task.FromResult(new List<PatientNoteDto>());
        public Task<bool> MarkFollowUpCompletedAsync(string noteId, string completedBy) => Task.FromResult(false);
        public Task<int> GetTotalNotesCountAsync() => Task.FromResult(0);
        public Task<int> GetUrgentNotesCountAsync() => Task.FromResult(0);
        public Task<int> GetFollowUpNotesCountAsync() => Task.FromResult(0);
        public Task<Dictionary<string, int>> GetNotesCategoryStatisticsAsync() => Task.FromResult(new Dictionary<string, int>());
        public Task<Dictionary<string, int>> GetNotesCreatorStatisticsAsync() => Task.FromResult(new Dictionary<string, int>());
        public Task<bool> NoteExistsAsync(string id) => Task.FromResult(false);
        public Task<bool> CanUserAccessNoteAsync(string noteId, string userId) => Task.FromResult(true);
        public Task<bool> CanUserEditNoteAsync(string noteId, string userId) => Task.FromResult(true);
        public Task<List<PatientNoteDto>> GetNotesByCategoryAsync(string category) => Task.FromResult(new List<PatientNoteDto>());
        public Task<List<PatientNoteDto>> GetNotesByDateRangeAsync(DateTime fromDate, DateTime toDate) => Task.FromResult(new List<PatientNoteDto>());
    }
}
