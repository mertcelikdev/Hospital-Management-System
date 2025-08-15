using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IPatientNoteService
    {
        // CRUD Operations
        Task<PatientNoteDto> CreateNoteAsync(CreatePatientNoteDto createDto, string createdBy);
        Task<PatientNoteDto?> GetNoteByIdAsync(string id);
        Task<List<PatientNoteDto>> GetAllNotesAsync();
        Task<PatientNoteDto> UpdateNoteAsync(string id, UpdatePatientNoteDto updateDto, string updatedBy);
        Task<bool> DeleteNoteAsync(string id);

        // Patient-specific operations
        Task<List<PatientNoteDto>> GetNotesByPatientIdAsync(string patientId);
        Task<List<PatientNoteDto>> GetUrgentNotesByPatientIdAsync(string patientId);
        Task<int> GetNotesCountByPatientIdAsync(string patientId);

        // Search and filter
        Task<List<PatientNoteDto>> SearchNotesAsync(BaseSearchDto searchDto);
        Task<List<PatientNoteDto>> GetNotesByCategoryAsync(string category);
        Task<List<PatientNoteDto>> GetNotesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<PatientNoteDto>> GetNotesByCreatorAsync(string createdBy);

        // Urgent and follow-up operations
        Task<List<PatientNoteDto>> GetAllUrgentNotesAsync();
        Task<List<PatientNoteDto>> GetNotesWithFollowUpAsync();
        Task<List<PatientNoteDto>> GetOverdueFollowUpsAsync();
        Task<bool> MarkFollowUpCompletedAsync(string noteId, string completedBy);

        // Statistics
        Task<int> GetTotalNotesCountAsync();
        Task<int> GetUrgentNotesCountAsync();
        Task<int> GetFollowUpNotesCountAsync();
        Task<Dictionary<string, int>> GetNotesCategoryStatisticsAsync();
        Task<Dictionary<string, int>> GetNotesCreatorStatisticsAsync();

        // Validation
        Task<bool> NoteExistsAsync(string id);
        Task<bool> CanUserAccessNoteAsync(string noteId, string userId);
        Task<bool> CanUserEditNoteAsync(string noteId, string userId);
    }
}
