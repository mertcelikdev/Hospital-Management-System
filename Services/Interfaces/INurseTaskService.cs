using HospitalManagementSystem.Models;
using TaskStatus = HospitalManagementSystem.Models.TaskStatus;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface INurseTaskService
    {
        Task<List<NurseTask>> GetAllTasksAsync();
        Task<NurseTask?> GetTaskByIdAsync(string id);
        Task<List<NurseTask>> GetTasksByNurseIdAsync(string nurseId);
        Task<List<NurseTask>> GetTasksByPatientIdAsync(string patientId);
        Task<List<NurseTask>> GetTasksByStatusAsync(TaskStatus status);
        Task CreateTaskAsync(NurseTask task);
        Task UpdateTaskAsync(string id, NurseTask task);
        Task DeleteTaskAsync(string id);
        Task<bool> UpdateTaskStatusAsync(string id, TaskStatus status);
        Task<List<NurseTask>> GetTasksByDateAsync(DateTime date);
        Task<List<NurseTask>> GetOverdueTasksAsync();
        Task<List<NurseTask>> GetUpcomingTasksAsync(string nurseId);
        Task<int> GetTaskCountByStatusAsync(TaskStatus status);
    }
}
