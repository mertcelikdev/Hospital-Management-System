using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using TaskStatus = HospitalManagementSystem.Models.TaskStatus;

namespace HospitalManagementSystem.Services.Implementations
{
    public class NurseTaskService : INurseTaskService
    {
        private readonly IMongoCollection<NurseTask> _nurseTasks;

        public NurseTaskService(IMongoDatabase database)
        {
            _nurseTasks = database.GetCollection<NurseTask>("NurseTasks");
        }

        public async Task<List<NurseTask>> GetAllTasksAsync()
        {
            return await _nurseTasks.Find(_ => true).ToListAsync();
        }

        public async Task<NurseTask?> GetTaskByIdAsync(string id)
        {
            return await _nurseTasks.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateTaskAsync(NurseTask task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            await _nurseTasks.InsertOneAsync(task);
        }

        public async Task UpdateTaskAsync(string id, NurseTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            await _nurseTasks.ReplaceOneAsync(n => n.Id == id, task);
        }

        public async Task DeleteTaskAsync(string id)
        {
            await _nurseTasks.DeleteOneAsync(n => n.Id == id);
        }

        public async Task<List<NurseTask>> GetTasksByNurseIdAsync(string nurseId)
        {
            return await _nurseTasks.Find(n => n.NurseId == nurseId).ToListAsync();
        }

        public async Task<List<NurseTask>> GetTasksByPatientIdAsync(string patientId)
        {
            return await _nurseTasks.Find(n => n.PatientId == patientId).ToListAsync();
        }

        public async Task<List<NurseTask>> GetTasksByStatusAsync(TaskStatus status)
        {
            return await _nurseTasks.Find(n => n.Status == status).ToListAsync();
        }

        public async Task<bool> UpdateTaskStatusAsync(string id, TaskStatus status)
        {
            var update = Builders<NurseTask>.Update
                .Set(n => n.Status, status)
                .Set(n => n.UpdatedAt, DateTime.UtcNow)
                .Set(n => n.CompletedAt, status == TaskStatus.Tamamlandi ? DateTime.UtcNow : null);
            var result = await _nurseTasks.UpdateOneAsync(n => n.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<NurseTask>> GetTasksByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);
            return await _nurseTasks.Find(n => n.DueDate >= start && n.DueDate < end).ToListAsync();
        }

        public async Task<List<NurseTask>> GetOverdueTasksAsync()
        {
            var now = DateTime.UtcNow;
            return await _nurseTasks.Find(n => n.DueDate < now && n.Status != TaskStatus.Tamamlandi && n.Status != TaskStatus.IptalEdildi).ToListAsync();
        }

        public async Task<List<NurseTask>> GetUpcomingTasksAsync(string nurseId)
        {
            var now = DateTime.UtcNow;
            return await _nurseTasks.Find(n => n.NurseId == nurseId && n.DueDate >= now)
                .SortBy(n => n.DueDate)
                .Limit(20)
                .ToListAsync();
        }

        public async Task<int> GetTaskCountByStatusAsync(TaskStatus status)
        {
            return (int)await _nurseTasks.CountDocumentsAsync(n => n.Status == status);
        }
    }
}
