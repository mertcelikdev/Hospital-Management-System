using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface IMedicineService
    {
        // CRUD Operations
        Task<List<MedicineDto>> GetAllMedicinesAsync();
        Task<MedicineDto?> GetMedicineByIdAsync(string id);
        Task<List<MedicineDto>> GetActiveMedicinesAsync();
        Task CreateMedicineAsync(CreateMedicineDto createMedicineDto);
        Task UpdateMedicineAsync(string id, UpdateMedicineDto updateMedicineDto);
        Task DeleteMedicineAsync(string id);

        // Search Operations
        Task<List<MedicineDto>> SearchMedicinesAsync(string searchTerm);
        Task<MedicineDto?> GetMedicineByNameAsync(string name);
        Task<List<MedicineDto>> GetMedicinesByManufacturerAsync(string manufacturer);
        Task<List<MedicineDto>> GetMedicinesByCategoryAsync(string category);

        // Stock Operations
        Task UpdateStockAsync(string id, int newStock);
        Task IncreaseStockAsync(string id, int amount);
        Task DecreaseStockAsync(string id, int amount);
        Task<List<MedicineDto>> GetLowStockMedicinesAsync(int threshold = 10);
        Task<List<MedicineDto>> GetOutOfStockMedicinesAsync();

        // Price Operations
        Task UpdatePriceAsync(string id, decimal newPrice);
        Task<List<MedicineDto>> GetMedicinesByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        // Expiry Operations
        Task<List<MedicineDto>> GetExpiringMedicinesAsync(int daysThreshold = 30);
        Task<List<MedicineDto>> GetExpiredMedicinesAsync();
        Task UpdateExpiryDateAsync(string id, DateTime newExpiryDate);

        // Category Operations
        Task<List<string>> GetAllCategoriesAsync();
        Task<Dictionary<string, int>> GetMedicineCountByCategoryAsync();

        // Statistics
        Task<int> GetTotalMedicinesCountAsync();
        Task<int> GetActiveMedicinesCountAsync();
        Task<decimal> GetTotalInventoryValueAsync();
        Task<Dictionary<string, object>> GetMedicineStatisticsAsync();

        // Validation
        Task<bool> IsMedicineNameUniqueAsync(string name, string? excludeId = null);
        Task<bool> IsStockSufficientAsync(string id, int requiredAmount);
        Task<bool> IsMedicineExpiredAsync(string id);

        // Bulk Operations
        Task<List<MedicineDto>> GetMedicinesByIdsAsync(List<string> ids);
        Task BulkUpdateStockAsync(Dictionary<string, int> stockUpdates);
        Task BulkUpdatePricesAsync(Dictionary<string, decimal> priceUpdates);

        // Prescription Related
        Task<bool> IsMedicinePrescriptionOnlyAsync(string id);
        Task<List<MedicineDto>> GetPrescriptionMedicinesAsync();
        Task<List<MedicineDto>> GetOverTheCounterMedicinesAsync();

        // Dashboard metrics
        Task<int> GetLowStockMedicinesCountAsync();
        Task<int> GetExpiredMedicinesCountAsync();
        Task<int> GetExpiringSoonMedicinesCountAsync();
    }
}