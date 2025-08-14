using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IMedicationService
    {
        Task<List<Medication>> GetAllMedicationsAsync();
        Task<Medication?> GetMedicationByIdAsync(string id);
        Task<Medication?> GetMedicationByNameAsync(string name);
        Task CreateMedicationAsync(Medication medication);
        Task UpdateMedicationAsync(string id, Medication medication);
        Task DeleteMedicationAsync(string id);
        Task<List<Medication>> SearchMedicationsAsync(string searchTerm);
        Task<List<Medication>> GetMedicationsByTypeAsync(string type);
        Task<List<Medication>> GetMedicationsByManufacturerAsync(string manufacturer);
        Task<int> GetTotalMedicationsCountAsync();
        Task<bool> IsMedicationAvailableAsync(string medicationId, int requiredQuantity);
        Task<bool> UpdateMedicationStockAsync(string medicationId, int quantity);
        Task<List<Medication>> GetLowStockMedicationsAsync();
        Task<List<Medication>> GetExpiredMedicationsAsync();
        Task<List<Medication>> GetExpiringMedicationsAsync(int daysFromNow);
    }
}
