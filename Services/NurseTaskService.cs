using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class NurseTaskService
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
            return await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<NurseTask>> GetTasksByNurseIdAsync(string nurseId)
        {
            return await _nurseTasks.Find(t => t.NurseId == nurseId).ToListAsync();
        }

        public async Task<List<NurseTask>> GetTodayTasksByNurseIdAsync(string nurseId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _nurseTasks.Find(t => 
                t.NurseId == nurseId && 
                t.DueDate >= today && 
                t.DueDate < tomorrow &&
                t.Status != TaskStatus.Tamamlandi
            ).ToListAsync();
        }

        public async Task CreateTaskAsync(NurseTask task)
        {
            await _nurseTasks.InsertOneAsync(task);
        }

        public async Task UpdateTaskAsync(string id, NurseTask task)
        {
            await _nurseTasks.ReplaceOneAsync(t => t.Id == id, task);
        }

        public async Task<bool> UpdateTaskStatusAsync(string id, Models.TaskStatus status, string userId, string userRole, string? notes = null)
        {
            var existingTask = await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            
            if (existingTask == null) return false;

            // Check permissions
            if (userRole == "Nurse" && existingTask.AssignedToNurseId != userId)
            {
                return false;
            }

            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, status)
                .Set(t => t.CompletedAt, status == Models.TaskStatus.Tamamlandi ? DateTime.UtcNow : null)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(notes))
            {
                update = update.Set(t => t.Notes, notes);
            }
                
            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task DeleteTaskAsync(string id)
        {
            await _nurseTasks.DeleteOneAsync(t => t.Id == id);
        }

        public async Task<List<NurseTask>> GetTasksByAssignerAsync(string assignerId)
        {
            return await _nurseTasks.Find(t => t.AssignedBy == assignerId).ToListAsync();
        }
    }
}
