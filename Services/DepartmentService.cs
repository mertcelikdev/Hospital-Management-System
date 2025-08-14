using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class DepartmentService
    {
        private readonly IMongoCollection<Department> _departments;
        private readonly IMongoCollection<User> _users;

        public DepartmentService(IMongoDatabase database)
        {
            _departments = database.GetCollection<Department>("departments");
            _users = database.GetCollection<User>("users");
        }

        // Get all departments
        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            var departments = await _departments.Find(_ => true)
                .SortBy(d => d.Name)
                .ToListAsync();

            // Add user counts for each department
            foreach (var department in departments)
            {
                department.DoctorCount = await GetDepartmentUserCountAsync(department.Id, "Doctor");
                department.NurseCount = await GetDepartmentUserCountAsync(department.Id, "Nurse");
                department.StaffCount = await GetDepartmentUserCountAsync(department.Id, "Staff");
            }

            return departments;
        }

        // Get active departments only
        public async Task<List<Department>> GetActiveDepartmentsAsync()
        {
            return await _departments.Find(d => d.IsActive)
                .SortBy(d => d.Name)
                .ToListAsync();
        }

        // Get department by ID
        public async Task<Department?> GetDepartmentByIdAsync(string id)
        {
            var department = await _departments.Find(d => d.Id == id).FirstOrDefaultAsync();
            
            if (department != null)
            {
                department.DoctorCount = await GetDepartmentUserCountAsync(department.Id, "Doctor");
                department.NurseCount = await GetDepartmentUserCountAsync(department.Id, "Nurse");
                department.StaffCount = await GetDepartmentUserCountAsync(department.Id, "Staff");
            }

            return department;
        }

        // Get department by name
        public async Task<Department?> GetDepartmentByNameAsync(string name)
        {
            return await _departments.Find(d => d.Name == name).FirstOrDefaultAsync();
        }

        // Get department by code
        public async Task<Department?> GetDepartmentByCodeAsync(string code)
        {
            return await _departments.Find(d => d.Code == code).FirstOrDefaultAsync();
        }

        // Create new department
        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            department.Id = null; // Let MongoDB generate the ID
            department.CreatedAt = DateTime.UtcNow;
            department.UpdatedAt = DateTime.UtcNow;
            
            await _departments.InsertOneAsync(department);
            return department;
        }

        // Update department
        public async Task<bool> UpdateDepartmentAsync(string id, Department department)
        {
            var existingDepartment = await _departments.Find(d => d.Id == id).FirstOrDefaultAsync();
            
            if (existingDepartment == null) return false;

            department.Id = id;
            department.CreatedAt = existingDepartment.CreatedAt;
            department.UpdatedAt = DateTime.UtcNow;

            var result = await _departments.ReplaceOneAsync(d => d.Id == id, department);
            return result.ModifiedCount > 0;
        }

        // Delete department (soft delete by setting IsActive = false)
        public async Task<bool> DeleteDepartmentAsync(string id)
        {
            var update = Builders<Department>.Update
                .Set(d => d.IsActive, false)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            var result = await _departments.UpdateOneAsync(d => d.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Hard delete department (only if no users assigned)
        public async Task<bool> HardDeleteDepartmentAsync(string id)
        {
            // Check if any users are assigned to this department
            var userCount = await _users.CountDocumentsAsync(u => u.DepartmentId == id);
            
            if (userCount > 0)
            {
                return false; // Cannot delete department with assigned users
            }

            var result = await _departments.DeleteOneAsync(d => d.Id == id);
            return result.DeletedCount > 0;
        }

        // Restore department (set IsActive = true)
        public async Task<bool> RestoreDepartmentAsync(string id)
        {
            var update = Builders<Department>.Update
                .Set(d => d.IsActive, true)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            var result = await _departments.UpdateOneAsync(d => d.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // Search departments
        public async Task<List<Department>> SearchDepartmentsAsync(string searchTerm)
        {
            var filter = Builders<Department>.Filter.Or(
                Builders<Department>.Filter.Regex(d => d.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Department>.Filter.Regex(d => d.Code, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Department>.Filter.Regex(d => d.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _departments.Find(filter)
                .SortBy(d => d.Name)
                .ToListAsync();
        }

        // Check if department name exists
        public async Task<bool> DepartmentNameExistsAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Department>.Filter.Eq(d => d.Name, name);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Department>.Filter.And(
                    filter,
                    Builders<Department>.Filter.Ne(d => d.Id, excludeId)
                );
            }

            var count = await _departments.CountDocumentsAsync(filter);
            return count > 0;
        }

        // Check if department code exists
        public async Task<bool> DepartmentCodeExistsAsync(string code, string? excludeId = null)
        {
            var filter = Builders<Department>.Filter.Eq(d => d.Code, code);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<Department>.Filter.And(
                    filter,
                    Builders<Department>.Filter.Ne(d => d.Id, excludeId)
                );
            }

            var count = await _departments.CountDocumentsAsync(filter);
            return count > 0;
        }

        // Get user count by department and role
        private async Task<int> GetDepartmentUserCountAsync(string? departmentId, string role)
        {
            if (string.IsNullOrEmpty(departmentId)) return 0;

            var count = await _users.CountDocumentsAsync(u => 
                u.DepartmentId == departmentId && 
                u.Role == role && 
                u.IsActive
            );

            return (int)count;
        }

        // Get all users in a department
        public async Task<List<User>> GetDepartmentUsersAsync(string departmentId, string? role = null)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.DepartmentId, departmentId),
                Builders<User>.Filter.Eq(u => u.IsActive, true)
            );

            if (!string.IsNullOrEmpty(role))
            {
                filter = Builders<User>.Filter.And(
                    filter,
                    Builders<User>.Filter.Eq(u => u.Role, role)
                );
            }

            return await _users.Find(filter)
                .SortBy(u => u.Name)
                .ToListAsync();
        }

        // Get department statistics
        public async Task<object> GetDepartmentStatisticsAsync()
        {
            var totalDepartments = await _departments.CountDocumentsAsync(_ => true);
            var activeDepartments = await _departments.CountDocumentsAsync(d => d.IsActive);
            var inactiveDepartments = totalDepartments - activeDepartments;

            return new
            {
                TotalDepartments = totalDepartments,
                ActiveDepartments = activeDepartments,
                InactiveDepartments = inactiveDepartments
            };
        }
    }
}
