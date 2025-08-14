using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public interface IMedicineService
    {
        // CRUD Operations
        Task<List<Medicine>> GetAllMedicinesAsync();
        Task<Medicine?> GetMedicineByIdAsync(string id);
        Task<List<Medicine>> GetActiveMedicinesAsync();
        Task CreateMedicineAsync(Medicine medicine);
        Task UpdateMedicineAsync(string id, Medicine medicine);
        Task DeleteMedicineAsync(string id);

        // Search Operations
        Task<List<Medicine>> SearchMedicinesAsync(string searchTerm);
        Task<Medicine?> GetMedicineByNameAsync(string name);
        Task<List<Medicine>> GetMedicinesByManufacturerAsync(string manufacturer);
        Task<List<Medicine>> GetMedicinesByCategoryAsync(string category);

        // Stock Operations
        Task UpdateStockAsync(string id, int newStock);
        Task IncreaseStockAsync(string id, int amount);
        Task DecreaseStockAsync(string id, int amount);
        Task<List<Medicine>> GetLowStockMedicinesAsync(int threshold = 10);
        Task<List<Medicine>> GetOutOfStockMedicinesAsync();

        // Price Operations
        Task UpdatePriceAsync(string id, decimal newPrice);
        Task<List<Medicine>> GetMedicinesByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        // Expiry Operations
        Task<List<Medicine>> GetExpiringMedicinesAsync(int daysThreshold = 30);
        Task<List<Medicine>> GetExpiredMedicinesAsync();
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
        Task<List<Medicine>> GetMedicinesByIdsAsync(List<string> ids);
        Task BulkUpdateStockAsync(Dictionary<string, int> stockUpdates);
        Task BulkUpdatePricesAsync(Dictionary<string, decimal> priceUpdates);

        // Prescription Related
        Task<bool> IsMedicinePrescriptionOnlyAsync(string id);
        Task<List<Medicine>> GetPrescriptionMedicinesAsync();
        Task<List<Medicine>> GetOverTheCounterMedicinesAsync();

        // Additional methods for Dashboard
        Task<int> GetLowStockMedicinesCountAsync();
        Task<int> GetExpiredMedicinesCountAsync();
        Task<int> GetExpiringSoonMedicinesCountAsync();
    }
}