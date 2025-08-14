using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public interface INurseTaskService
    {
        // CRUD Operations
        Task<NurseTaskDto> CreateTaskAsync(CreateNurseTaskDto createDto, string createdBy);
        Task<NurseTaskDto?> GetTaskByIdAsync(string id);
        Task<List<NurseTaskDto>> GetAllTasksAsync();
        Task<NurseTaskDto> UpdateTaskAsync(string id, UpdateNurseTaskDto updateDto, string updatedBy);
        Task<bool> DeleteTaskAsync(string id);

        // Task assignment operations
        Task<List<NurseTaskDto>> GetTasksByAssignedNurseAsync(string nurseId);
        Task<List<NurseTaskDto>> GetTasksByPatientAsync(string patientId);
        Task<bool> AssignTaskToNurseAsync(string taskId, string nurseId, string assignedBy);
        Task<bool> ReassignTaskAsync(string taskId, string newNurseId, string reassignedBy);

        // Status management
        Task<bool> UpdateTaskStatusAsync(string taskId, TaskStatusUpdateDto statusDto, string updatedBy);
        Task<bool> StartTaskAsync(string taskId, string startedBy);
        Task<bool> CompleteTaskAsync(string taskId, string completedBy, string? notes = null);
        Task<bool> CancelTaskAsync(string taskId, string cancelledBy, string? reason = null);

        // Search and filter
        Task<List<NurseTaskDto>> SearchTasksAsync(NurseTaskSearchDto searchDto);
        Task<List<NurseTaskDto>> GetTasksByStatusAsync(string status);
        Task<List<NurseTaskDto>> GetTasksByPriorityAsync(string priority);
        Task<List<NurseTaskDto>> GetTasksByCategoryAsync(string category);
        Task<List<NurseTaskDto>> GetTasksByDateRangeAsync(DateTime fromDate, DateTime toDate);

        // Overdue and priority operations
        Task<List<NurseTaskDto>> GetOverdueTasksAsync();
        Task<List<NurseTaskDto>> GetHighPriorityTasksAsync();
        Task<List<NurseTaskDto>> GetTasksDueTodayAsync();
        Task<List<NurseTaskDto>> GetTasksDueThisWeekAsync();
        Task<List<NurseTaskDto>> GetTasksRequiringEquipmentAsync();

        // Statistics and reporting
        Task<int> GetTotalTasksCountAsync();
        Task<int> GetPendingTasksCountAsync();
        Task<int> GetCompletedTasksCountAsync();
        Task<int> GetOverdueTasksCountAsync();
        Task<Dictionary<string, int>> GetTasksStatusStatisticsAsync();
        Task<Dictionary<string, int>> GetTasksPriorityStatisticsAsync();
        Task<Dictionary<string, int>> GetTasksCategoryStatisticsAsync();
        Task<Dictionary<string, int>> GetNurseWorkloadStatisticsAsync();

        // Validation
        Task<bool> TaskExistsAsync(string id);
        Task<bool> CanUserAccessTaskAsync(string taskId, string userId);
        Task<bool> CanUserEditTaskAsync(string taskId, string userId);
        Task<bool> IsTaskAssignedToNurseAsync(string taskId, string nurseId);

        // Notification and alerts
        Task<List<NurseTaskDto>> GetTasksNeedingAttentionAsync(string nurseId);
        Task<bool> SendTaskReminderAsync(string taskId);
        Task<List<NurseTaskDto>> GetTasksForNotificationAsync();
    }
}
