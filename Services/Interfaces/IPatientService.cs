using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IPatientService
    {
        Task<List<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(string id);
        Task<Patient?> GetPatientByTcNoAsync(string tcNo);
        Task CreatePatientAsync(Patient patient);
        Task UpdatePatientAsync(string id, Patient patient);
        Task DeletePatientAsync(string id);
        Task<List<Patient>> SearchPatientsAsync(string searchTerm);
        Task<List<Patient>> GetPatientsByDoctorAsync(string doctorId);
        Task<int> GetTotalPatientsCountAsync();
        Task<bool> PatientExistsAsync(string tcNo);
        Task<List<Appointment>> GetPatientAppointmentsAsync(string patientId);
        Task<List<PatientNote>> GetPatientNotesAsync(string patientId);
        Task<Patient?> GetPatientWithDetailsAsync(string id);
    }
}
