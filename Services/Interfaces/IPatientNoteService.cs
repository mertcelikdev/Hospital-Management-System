using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IPatientNoteService
    {
        Task<List<PatientNote>> GetAllNotesAsync();
        Task<PatientNote?> GetNoteByIdAsync(string id);
        Task<List<PatientNote>> GetNotesByPatientIdAsync(string patientId);
        Task<List<PatientNote>> GetNotesByDoctorIdAsync(string doctorId);
        Task CreateNoteAsync(PatientNote note);
        Task UpdateNoteAsync(string id, PatientNote note);
        Task DeleteNoteAsync(string id);
        Task<List<PatientNote>> GetNotesByDateAsync(DateTime date);
        Task<List<PatientNote>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<PatientNote>> SearchNotesAsync(string searchTerm);
        Task<int> GetNotesCountByPatientAsync(string patientId);
        Task<int> GetNotesCountByDoctorAsync(string doctorId);
    }
}
