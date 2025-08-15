using MongoDB.Driver;
using MongoDB.Bson;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class NurseTaskService : INurseTaskService
    {
        private readonly IMongoCollection<NurseTask> _nurseTasks;

        public NurseTaskService(IMongoDatabase database)
        {
            _nurseTasks = database.GetCollection<NurseTask>("NurseTasks");
        }

        private async Task<List<NurseTask>> GetTasksByNurseIdInternalAsync(string nurseId)
        {
            return await _nurseTasks.Find(t => t.AssignedToId == nurseId).ToListAsync();
        }

        public async Task<List<NurseTaskDto>> GetTasksByNurseIdAsync(string nurseId)
        {
            var tasks = await GetTasksByNurseIdInternalAsync(nurseId);
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTodayTasksByNurseIdAsync(string nurseId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var tasks = await _nurseTasks.Find(t => 
                t.AssignedToId == nurseId && 
                t.DueDate >= today && 
                t.DueDate < tomorrow &&
                t.Status != Models.TaskStatus.Tamamlandi
            ).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<bool> UpdateTaskStatusAsync(string id, Models.TaskStatus status, string userId, string userRole, string? notes = null)
        {
            var existingTask = await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTask == null) return false;
            if (userRole == "Nurse" && existingTask.AssignedToId != userId) return false;

            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, status)
                .Set(t => t.CompletedAt, status == Models.TaskStatus.Tamamlandi ? DateTime.UtcNow : null)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            if (!string.IsNullOrEmpty(notes)) update = update.Set(t => t.Notes, notes);
            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<NurseTaskDto>> GetTasksByAssignerAsync(string assignerId)
        {
            var tasks = await _nurseTasks.Find(t => t.AssignedBy == assignerId).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        // Interface implementations (DTO based)
        public async Task<NurseTaskDto> CreateTaskAsync(CreateNurseTaskDto createDto, string createdBy)
        {
            var task = new NurseTask
            {
                Title = createDto.Title,
                Description = createDto.Description,
                AssignedToId = createDto.AssignedToId,
                PatientId = createDto.PatientId,
                Priority = Enum.Parse<TaskPriority>(createDto.Priority),
                DueDate = createDto.DueDate ?? DateTime.UtcNow.AddDays(1),
                Category = createDto.Category,
                RequiresEquipment = createDto.RequiresEquipment,
                EquipmentNeeded = createDto.EquipmentNeeded,
                Instructions = createDto.Instructions,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                Status = Models.TaskStatus.Beklemede
            };

            await _nurseTasks.InsertOneAsync(task);
            return ConvertToDto(task);
        }

        public async Task<NurseTaskDto?> GetTaskByIdAsync(string id)
        {
            var task = await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            return task != null ? ConvertToDto(task) : null;
        }

        public async Task<NurseTaskDto?> GetTaskByIdAsync(string id, string userId, string userRole)
        {
            var task = await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return null;

            // Role-based access control
            if (userRole == "Nurse" && task.AssignedToId != userId)
                return null;

            return ConvertToDto(task);
        }

        public async Task<List<NurseTaskDto>> GetAllTasksAsync()
        {
            var tasks = await _nurseTasks.Find(_ => true).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<NurseTaskDto> UpdateTaskAsync(string id, UpdateNurseTaskDto updateDto, string updatedBy)
        {
            var update = Builders<NurseTask>.Update
                .Set(t => t.Title, updateDto.Title)
                .Set(t => t.Description, updateDto.Description)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(updateDto.AssignedToId))
                update = update.Set(t => t.AssignedToId, updateDto.AssignedToId);

            await _nurseTasks.UpdateOneAsync(t => t.Id == id, update);
            var task = await _nurseTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            return ConvertToDto(task);
        }

        public async Task<NurseTaskDto> UpdateTaskAsync(string id, UpdateNurseTaskDto updateDto, string updatedBy, string userRole)
        {
            return await UpdateTaskAsync(id, updateDto, updatedBy);
        }

        public async Task<bool> DeleteTaskAsync(string id)
        {
            var result = await _nurseTasks.DeleteOneAsync(t => t.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteTaskAsync(string id, string userId, string userRole)
        {
            return await DeleteTaskAsync(id);
        }

        public async Task<List<NurseTaskDto>> GetTasksByAssignedNurseAsync(string nurseId)
        {
                return await GetTasksByNurseIdAsync(nurseId);
        }

        public async Task<List<NurseTaskDto>> GetTasksByPatientAsync(string patientId)
        {
            var tasks = await _nurseTasks.Find(t => t.PatientId == patientId).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksByDoctorIdAsync(string doctorId)
        {
            var tasks = await _nurseTasks.Find(t => t.CreatedBy == doctorId).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<bool> AssignTaskToNurseAsync(string taskId, string nurseId, string assignedBy)
        {
            var update = Builders<NurseTask>.Update
                .Set(t => t.AssignedToId, nurseId)
                .Set(t => t.AssignedBy, assignedBy)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == taskId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ReassignTaskAsync(string taskId, string newNurseId, string reassignedBy)
        {
            return await AssignTaskToNurseAsync(taskId, newNurseId, reassignedBy);
        }

        public async Task<bool> UpdateTaskStatusAsync(string taskId, TaskStatusUpdateDto statusDto, string updatedBy)
        {
            var status = Enum.Parse<Models.TaskStatus>(statusDto.Status);
            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, status)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(statusDto.Notes))
                update = update.Set(t => t.Notes, statusDto.Notes);

            if (statusDto.CompletedAt.HasValue)
                update = update.Set(t => t.CompletedAt, statusDto.CompletedAt.Value);

            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == taskId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> StartTaskAsync(string taskId, string startedBy)
        {
            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, Models.TaskStatus.DevamEdiyor)
                .Set(t => t.StartedAt, DateTime.UtcNow)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == taskId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CompleteTaskAsync(string taskId, string completedBy, string? notes = null)
        {
            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, Models.TaskStatus.Tamamlandi)
                .Set(t => t.CompletedAt, DateTime.UtcNow)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(notes))
                update = update.Set(t => t.Notes, notes);

            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == taskId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CancelTaskAsync(string taskId, string cancelledBy, string? reason = null)
        {
            var update = Builders<NurseTask>.Update
                .Set(t => t.Status, Models.TaskStatus.IptalEdildi)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(reason))
                update = update.Set(t => t.Notes, reason);

            var result = await _nurseTasks.UpdateOneAsync(t => t.Id == taskId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<NurseTaskDto>> SearchTasksAsync(NurseTaskSearchDto searchDto)
        {
            var filter = Builders<NurseTask>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchDto.AssignedToId))
                filter &= Builders<NurseTask>.Filter.Eq(t => t.AssignedToId, searchDto.AssignedToId);

            if (!string.IsNullOrEmpty(searchDto.PatientId))
                filter &= Builders<NurseTask>.Filter.Eq(t => t.PatientId, searchDto.PatientId);

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                var status = Enum.Parse<Models.TaskStatus>(searchDto.Status);
                filter &= Builders<NurseTask>.Filter.Eq(t => t.Status, status);
            }

            var tasks = await _nurseTasks.Find(filter).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksByStatusAsync(string status)
        {
            var taskStatus = Enum.Parse<Models.TaskStatus>(status);
            var tasks = await _nurseTasks.Find(t => t.Status == taskStatus).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksByPriorityAsync(string priority)
        {
            if (Enum.TryParse<TaskPriority>(priority, true, out var priorityEnum))
            {
                var tasks = await _nurseTasks.Find(t => t.Priority == priorityEnum).ToListAsync();
                return tasks.Select(ConvertToDto).ToList();
            }
            return new List<NurseTaskDto>();
        }

        public async Task<List<NurseTaskDto>> GetTasksByCategoryAsync(string category)
        {
            var tasks = await _nurseTasks.Find(t => t.Category == category).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var tasks = await _nurseTasks.Find(t => t.DueDate >= fromDate && t.DueDate <= toDate).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetOverdueTasksAsync()
        {
            var now = DateTime.UtcNow;
            var tasks = await _nurseTasks.Find(t => t.DueDate < now && t.Status != Models.TaskStatus.Tamamlandi).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetHighPriorityTasksAsync()
        {
            var tasks = await _nurseTasks.Find(t => t.Priority == TaskPriority.Yuksek || t.Priority == TaskPriority.Acil).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksDueTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var tasks = await _nurseTasks.Find(t => t.DueDate >= today && t.DueDate < tomorrow).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksDueThisWeekAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            var tasks = await _nurseTasks.Find(t => t.DueDate >= startOfWeek && t.DueDate < endOfWeek).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public async Task<List<NurseTaskDto>> GetTasksRequiringEquipmentAsync()
        {
            var tasks = await _nurseTasks.Find(t => t.RequiresEquipment == true).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        // Statistics methods
        public async Task<int> GetTotalTasksCountAsync()
        {
            return (int)await _nurseTasks.CountDocumentsAsync(_ => true);
        }

        public async Task<int> GetPendingTasksCountAsync()
        {
            return (int)await _nurseTasks.CountDocumentsAsync(t => t.Status == Models.TaskStatus.Beklemede);
        }

        public async Task<int> GetCompletedTasksCountAsync()
        {
            return (int)await _nurseTasks.CountDocumentsAsync(t => t.Status == Models.TaskStatus.Tamamlandi);
        }

        public async Task<int> GetOverdueTasksCountAsync()
        {
            var now = DateTime.UtcNow;
            return (int)await _nurseTasks.CountDocumentsAsync(t => t.DueDate < now && t.Status != Models.TaskStatus.Tamamlandi);
        }

        public async Task<Dictionary<string, int>> GetTasksStatusStatisticsAsync()
        {
            var stats = new Dictionary<string, int>();
            foreach (var status in Enum.GetValues<Models.TaskStatus>())
            {
                var count = await _nurseTasks.CountDocumentsAsync(t => t.Status == status);
                stats[status.ToString()] = (int)count;
            }
            return stats;
        }

        public async Task<Dictionary<string, int>> GetTasksPriorityStatisticsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Priority" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };
            
            var result = await _nurseTasks.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(x => x["_id"].ToString() ?? "Unknown", x => x["count"].ToInt32());
        }

        public async Task<Dictionary<string, int>> GetTasksCategoryStatisticsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Category" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };
            
            var result = await _nurseTasks.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(x => x["_id"].ToString() ?? "Unknown", x => x["count"].ToInt32());
        }

        public async Task<Dictionary<string, int>> GetNurseWorkloadStatisticsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$AssignedToId" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };
            
            var result = await _nurseTasks.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return result.ToDictionary(x => x["_id"].ToString() ?? "Unknown", x => x["count"].ToInt32());
        }

        // Validation methods
        public async Task<bool> TaskExistsAsync(string id)
        {
            var count = await _nurseTasks.CountDocumentsAsync(t => t.Id == id);
            return count > 0;
        }

        public async Task<bool> CanUserAccessTaskAsync(string taskId, string userId)
        {
            var task = await _nurseTasks.Find(t => t.Id == taskId).FirstOrDefaultAsync();
            return task != null && (task.AssignedToId == userId || task.CreatedBy == userId);
        }

        public async Task<bool> CanUserEditTaskAsync(string taskId, string userId)
        {
            return await CanUserAccessTaskAsync(taskId, userId);
        }

        public async Task<bool> IsTaskAssignedToNurseAsync(string taskId, string nurseId)
        {
            var count = await _nurseTasks.CountDocumentsAsync(t => t.Id == taskId && t.AssignedToId == nurseId);
            return count > 0;
        }

        // Notification methods
        public async Task<List<NurseTaskDto>> GetTasksNeedingAttentionAsync(string nurseId)
        {
            var now = DateTime.UtcNow;
            var tasks = await _nurseTasks.Find(t => 
                t.AssignedToId == nurseId && 
                (t.DueDate < now || t.Priority == TaskPriority.Yuksek || t.Priority == TaskPriority.Acil)).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        public Task<bool> SendTaskReminderAsync(string taskId)
        {
            // Implementation for sending reminders
            return Task.FromResult(true);
        }

        public async Task<List<NurseTaskDto>> GetTasksForNotificationAsync()
        {
            var now = DateTime.UtcNow;
            var tasks = await _nurseTasks.Find(t => t.DueDate < now.AddHours(1) && t.Status == Models.TaskStatus.Beklemede).ToListAsync();
            return tasks.Select(ConvertToDto).ToList();
        }

        // Helper method to convert NurseTask to NurseTaskDto
        private NurseTaskDto ConvertToDto(NurseTask task)
        {
            return new NurseTaskDto
            {
                Id = task.Id ?? string.Empty,
                Title = task.Title,
                Description = task.Description,
                AssignedToId = task.AssignedToId,
                AssignedToName = task.AssignedToName ?? string.Empty,
                PatientId = task.PatientId,
                PatientName = task.PatientName,
                Priority = task.Priority.ToString(),
                DueDate = task.DueDate,
                Category = task.Category,
                Status = task.Status.ToString(),
                RequiresEquipment = task.RequiresEquipment,
                EquipmentNeeded = task.EquipmentNeeded,
                Instructions = task.Instructions,
                Notes = task.Notes,
                CreatedBy = task.CreatedBy,
                CreatedByName = task.CreatedByName ?? string.Empty,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                IsOverdue = task.DueDate < DateTime.UtcNow && task.Status != Models.TaskStatus.Tamamlandi
            };
        }
    }
}
