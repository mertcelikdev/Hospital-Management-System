using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IMongoCollection<Department> _departments;
        private readonly IMongoCollection<User> _users;

        public DepartmentService(IMongoDatabase database)
        {
            _departments = database.GetCollection<Department>("Departments");
            _users = database.GetCollection<User>("Users");
        }

        private static DepartmentDto ToDto(Department d) => new()
        {
            Id = d.Id ?? string.Empty,
            Name = d.Name,
            Description = d.Description,
            Code = d.Code,
            HeadOfDepartment = d.HeadOfDepartment,
            Phone = d.Phone,
            Email = d.Email,
            Location = d.Location,
            // IsActive kaldırıldı
            DoctorCount = d.DoctorCount,
            NurseCount = d.NurseCount,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var list = await _departments.Find(_ => true).SortBy(d => d.Name).ToListAsync();
            await PopulateCounts(list);
            return list.Select(ToDto).ToList();
        }

        private async Task PopulateCounts(List<Department> list)
        {
            foreach (var d in list)
            {
                d.DoctorCount = await GetDepartmentUserCountAsync(d.Id!, "Doctor");
                d.NurseCount = await GetDepartmentUserCountAsync(d.Id!, "Nurse");
            }
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(string id)
        {
            var d = await _departments.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (d == null) return null;
            await PopulateCounts(new List<Department> { d });
            return ToDto(d);
        }

    // GetActiveDepartmentsAsync kaldırıldı (tüm departmanlar aktif varsayılıyor)

        public async Task CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var entity = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                Code = dto.Code,
                HeadOfDepartment = dto.HeadOfDepartment,
                Phone = dto.Phone,
                Email = dto.Email,
                Location = dto.Location,
                // IsActive kaldırıldı
                CreatedAt = DateTime.UtcNow
            };
            await _departments.InsertOneAsync(entity);
        }

        public async Task UpdateDepartmentAsync(string id, UpdateDepartmentDto dto)
        {
            var update = Builders<Department>.Update.Set(d => d.UpdatedAt, DateTime.UtcNow);
            if (dto.Name != null) update = update.Set(d => d.Name, dto.Name);
            if (dto.Description != null) update = update.Set(d => d.Description, dto.Description);
            if (dto.Code != null) update = update.Set(d => d.Code, dto.Code);
            if (dto.HeadOfDepartment != null) update = update.Set(d => d.HeadOfDepartment, dto.HeadOfDepartment);
            if (dto.Phone != null) update = update.Set(d => d.Phone, dto.Phone);
            if (dto.Email != null) update = update.Set(d => d.Email, dto.Email);
            if (dto.Location != null) update = update.Set(d => d.Location, dto.Location);
            // IsActive kaldırıldı
            await _departments.UpdateOneAsync(d => d.Id == id, update);
        }

        public async Task DeleteDepartmentAsync(string id)
        {
            await _departments.DeleteOneAsync(d => d.Id == id); // soft delete yerine kalıcı
        }

    // Toggle/Activate/Deactivate kaldırıldı

        public async Task<int> GetDepartmentUserCountAsync(string departmentId, string role)
        {
            var count = await _users.CountDocumentsAsync(u => u.DepartmentId == departmentId && u.Role == role);
            return (int)count;
        }

        public async Task<List<UserDto>> GetDepartmentUsersAsync(string departmentId, string? role = null)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.DepartmentId, departmentId),
                Builders<User>.Filter.Empty);
            if (!string.IsNullOrEmpty(role))
                filter &= Builders<User>.Filter.Eq(u => u.Role, role);
            var list = await _users.Find(filter).SortBy(u => u.Name).Limit(200).ToListAsync();
            return list.Select(u => new UserDto { Id = u.Id!, Name = u.Name, Email = u.Email, Phone = u.Phone, Role = u.Role }).ToList();
        }

        public async Task<List<DepartmentDto>> SearchDepartmentsAsync(string searchTerm)
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filter = Builders<Department>.Filter.Or(
                Builders<Department>.Filter.Regex(d => d.Name, regex),
                Builders<Department>.Filter.Regex(d => d.Code, regex),
                Builders<Department>.Filter.Regex(d => d.Description, regex));
            var list = await _departments.Find(filter).SortBy(d => d.Name).ToListAsync();
            await PopulateCounts(list);
            return list.Select(ToDto).ToList();
        }

        public async Task<DepartmentDto?> GetDepartmentByNameAsync(string name)
        {
            var d = await _departments.Find(d => d.Name == name).FirstOrDefaultAsync();
            if (d == null) return null;
            await PopulateCounts(new List<Department> { d });
            return ToDto(d);
        }

        public async Task<int> GetTotalDepartmentsCountAsync() => (int)await _departments.CountDocumentsAsync(_ => true);
    // GetActiveDepartmentsCountAsync kaldırıldı

        public async Task<Dictionary<string, int>> GetDepartmentUserStatisticsAsync(string departmentId)
        {
            var roles = new[] { "Doctor", "Nurse", "Staff" };
            var dict = new Dictionary<string, int>();
            foreach (var r in roles)
                dict[r] = await GetDepartmentUserCountAsync(departmentId, r);
            return dict;
        }

        public async Task<bool> IsDepartmentNameUniqueAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Department>.Filter.Eq(d => d.Name, name);
            if (!string.IsNullOrEmpty(excludeId))
                filter &= Builders<Department>.Filter.Ne(d => d.Id, excludeId);
            return await _departments.CountDocumentsAsync(filter) == 0;
        }

        public async Task<bool> CanDeleteDepartmentAsync(string id)
        {
            var userCount = await _users.CountDocumentsAsync(u => u.DepartmentId == id);
            return userCount == 0;
        }

        public Task AssignDepartmentHeadAsync(string departmentId, string userId)
        {
            return _departments.UpdateOneAsync(d => d.Id == departmentId, Builders<Department>.Update.Set(d => d.HeadOfDepartment, userId).Set(d => d.UpdatedAt, DateTime.UtcNow));
        }

        public Task RemoveDepartmentHeadAsync(string departmentId)
        {
            return _departments.UpdateOneAsync(d => d.Id == departmentId, Builders<Department>.Update.Unset(d => d.HeadOfDepartment).Set(d => d.UpdatedAt, DateTime.UtcNow));
        }

        public async Task<UserDto?> GetDepartmentHeadAsync(string departmentId)
        {
            var dep = await _departments.Find(d => d.Id == departmentId).FirstOrDefaultAsync();
            if (dep == null || string.IsNullOrEmpty(dep.HeadOfDepartment)) return null;
            var user = await _users.Find(u => u.Id == dep.HeadOfDepartment).FirstOrDefaultAsync();
            return user == null ? null : new UserDto { Id = user.Id!, Name = user.Name, Email = user.Email, Phone = user.Phone, Role = user.Role };
        }

        public async Task<List<DepartmentDto>> GetDepartmentsByIdsAsync(List<string> ids)
        {
            var list = await _departments.Find(d => ids.Contains(d.Id!)).ToListAsync();
            await PopulateCounts(list);
            return list.Select(ToDto).ToList();
        }

    // BulkUpdateDepartmentStatusAsync kaldırıldı
    }
}
