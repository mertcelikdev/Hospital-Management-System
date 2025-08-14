using MongoDB.Driver;
using MongoDB.Bson;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class PatientNoteService
    {
        private readonly IMongoCollection<PatientNote> _patientNotes;

        public PatientNoteService(IMongoDatabase database)
        {
            _patientNotes = database.GetCollection<PatientNote>("PatientNotes");
        }

        public async Task<List<PatientNote>> GetAllNotesAsync()
        {
            return await _patientNotes.Find(_ => true).SortByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<PatientNote?> GetNoteByIdAsync(string id)
        {
            return await _patientNotes.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PatientNote>> GetNotesByPatientIdAsync(string patientId)
        {
            return await _patientNotes.Find(n => n.PatientId == patientId)
                .SortByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<PatientNote>> GetNotesByCreatorAsync(string creatorId, string userRole)
        {
            var filter = Builders<PatientNote>.Filter.Empty;
            
            // Role-based filtering
            switch (userRole)
            {
                case "Nurse":
                    // Nurses can see their own notes and public notes
                    filter = Builders<PatientNote>.Filter.Or(
                        Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, creatorId),
                        Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true)
                    );
                    break;
                    
                case "Doctor":
                    // Doctors can see all notes except private nurse notes
                    filter = Builders<PatientNote>.Filter.Or(
                        Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, creatorId),
                        Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true),
                        Builders<PatientNote>.Filter.Ne(n => n.NoteType, PatientNoteType.HemşireNotu)
                    );
                    break;
                    
                case "Admin":
                    // Admins can see all notes
                    filter = Builders<PatientNote>.Filter.Empty;
                    break;
            }
            
            return await _patientNotes.Find(filter)
                .SortByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<PatientNote>> GetNotesByPatientAndCreatorAsync(string patientId, string userId, string userRole)
        {
            var baseFilter = Builders<PatientNote>.Filter.Eq(n => n.PatientId, patientId);
            var roleFilter = Builders<PatientNote>.Filter.Empty;

            // Apply role-based filtering
            switch (userRole)
            {
                case "Nurse":
                    roleFilter = Builders<PatientNote>.Filter.Or(
                        Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, userId),
                        Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true)
                    );
                    break;

                case "Doctor":
                    roleFilter = Builders<PatientNote>.Filter.Or(
                        Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, userId),
                        Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true),
                        Builders<PatientNote>.Filter.Ne(n => n.NoteType, PatientNoteType.HemşireNotu)
                    );
                    break;

                case "Admin":
                    roleFilter = Builders<PatientNote>.Filter.Empty;
                    break;
            }

            var finalFilter = userRole == "Admin" ? baseFilter : 
                Builders<PatientNote>.Filter.And(baseFilter, roleFilter);

            return await _patientNotes.Find(finalFilter)
                .SortByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task CreateNoteAsync(PatientNote note)
        {
            note.Id = null; // Let MongoDB generate the ID
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;
            await _patientNotes.InsertOneAsync(note);
        }

        public async Task<bool> UpdateNoteAsync(string id, PatientNote note, string userId, string userRole)
        {
            var existingNote = await _patientNotes.Find(n => n.Id == id).FirstOrDefaultAsync();
            
            if (existingNote == null) return false;

            // Check permissions - only creator or admin can edit
            if (userRole != "Admin" && existingNote.CreatedByUserId != userId)
            {
                return false;
            }

            note.Id = id;
            note.CreatedAt = existingNote.CreatedAt;
            note.CreatedByUserId = existingNote.CreatedByUserId;
            note.UpdatedAt = DateTime.UtcNow;
            
            var result = await _patientNotes.ReplaceOneAsync(n => n.Id == id, note);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNoteAsync(string id, string userId, string userRole)
        {
            var existingNote = await _patientNotes.Find(n => n.Id == id).FirstOrDefaultAsync();
            
            if (existingNote == null) return false;

            // Check permissions - only creator or admin can delete
            if (userRole != "Admin" && existingNote.CreatedByUserId != userId)
            {
                return false;
            }

            var result = await _patientNotes.DeleteOneAsync(n => n.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<PatientNote>> SearchNotesAsync(string searchTerm, string userId, string userRole, string? patientId = null)
        {
            var searchFilter = Builders<PatientNote>.Filter.Or(
                Builders<PatientNote>.Filter.Regex(n => n.Title, new BsonRegularExpression(searchTerm, "i")),
                Builders<PatientNote>.Filter.Regex(n => n.Content, new BsonRegularExpression(searchTerm, "i")),
                Builders<PatientNote>.Filter.AnyIn(n => n.Tags, new[] { searchTerm })
            );

            // Apply role-based filtering
            var roleFilter = GetRoleBasedFilter(userId, userRole);
            var combinedFilter = userRole == "Admin" ? searchFilter : 
                Builders<PatientNote>.Filter.And(searchFilter, roleFilter);

            // Add patient filter if specified
            if (!string.IsNullOrEmpty(patientId))
            {
                combinedFilter = Builders<PatientNote>.Filter.And(
                    combinedFilter,
                    Builders<PatientNote>.Filter.Eq(n => n.PatientId, patientId)
                );
            }

            return await _patientNotes.Find(combinedFilter).SortByDescending(n => n.CreatedAt).ToListAsync();
        }

        // Get notes by priority
        public async Task<List<PatientNote>> GetNotesByPriorityAsync(NotePriority priority, string userId, string userRole)
        {
            var priorityFilter = Builders<PatientNote>.Filter.Eq(n => n.Priority, priority);
            var roleFilter = GetRoleBasedFilter(userId, userRole);

            var finalFilter = userRole == "Admin" ? priorityFilter : 
                Builders<PatientNote>.Filter.And(priorityFilter, roleFilter);

            return await _patientNotes.Find(finalFilter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Get recent notes for dashboard
        public async Task<List<PatientNote>> GetRecentNotesAsync(string userId, string userRole, int count = 10)
        {
            var roleFilter = GetRoleBasedFilter(userId, userRole);
            var filter = userRole == "Admin" ? Builders<PatientNote>.Filter.Empty : roleFilter;

            return await _patientNotes.Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .Limit(count)
                .ToListAsync();
        }

        // Helper method for role-based filtering
        private FilterDefinition<PatientNote> GetRoleBasedFilter(string userId, string userRole)
        {
            return userRole switch
            {
                "Nurse" => Builders<PatientNote>.Filter.Or(
                    Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, userId),
                    Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true)
                ),
                "Doctor" => Builders<PatientNote>.Filter.Or(
                    Builders<PatientNote>.Filter.Eq(n => n.CreatedByUserId, userId),
                    Builders<PatientNote>.Filter.Eq(n => n.IsPublic, true),
                    Builders<PatientNote>.Filter.Ne(n => n.NoteType, PatientNoteType.HemşireNotu)
                ),
                "Admin" => Builders<PatientNote>.Filter.Empty,
                _ => Builders<PatientNote>.Filter.Eq("_id", ObjectId.Empty)
            };
        }
    }
}
