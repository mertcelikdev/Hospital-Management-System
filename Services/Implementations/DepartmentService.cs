using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
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

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _departments.Find(_ => true).ToListAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(string id)
        {
            return await _departments.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateDepartmentAsync(Department department)
        {
            department.CreatedAt = DateTime.UtcNow;
            department.UpdatedAt = DateTime.UtcNow;
            await _departments.InsertOneAsync(department);
        }

        public async Task UpdateDepartmentAsync(string id, Department department)
        {
            department.UpdatedAt = DateTime.UtcNow;
            await _departments.ReplaceOneAsync(d => d.Id == id, department);
        }

        public async Task DeleteDepartmentAsync(string id)
        {
            await _departments.DeleteOneAsync(d => d.Id == id);
        }

        public async Task<List<User>> GetDepartmentStaffAsync(string departmentId)
        {
            return await _users.Find(u => u.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<int> GetDepartmentStaffCountAsync(string departmentId)
        {
            return (int)await _users.CountDocumentsAsync(u => u.DepartmentId == departmentId);
        }

        public async Task<bool> AssignStaffToDepartmentAsync(string departmentId, string staffId)
        {
            var update = Builders<User>.Update
                .Set(u => u.DepartmentId, departmentId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(u => u.Id == staffId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveStaffFromDepartmentAsync(string departmentId, string staffId)
        {
            var update = Builders<User>.Update
                .Unset(u => u.DepartmentId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(u => u.Id == staffId && u.DepartmentId == departmentId, update);
            return result.ModifiedCount > 0;
        }
    }
}
