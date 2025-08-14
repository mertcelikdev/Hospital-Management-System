using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _users.Find(u => u.Role == role).ToListAsync();
        }

        public async Task<List<User>> GetUsersByDepartmentAsync(string departmentId)
        {
            return await _users.Find(u => u.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllUsersAsync();

            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.FirstName, regex),
                Builders<User>.Filter.Regex(u => u.LastName, regex),
                Builders<User>.Filter.Regex(u => u.Username, regex),
                Builders<User>.Filter.Regex(u => u.Email, regex)
            );
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<bool> ChangeUserPasswordAsync(string userId, string newPassword)
        {
            var update = Builders<User>.Update
                .Set(u => u.Password, newPassword)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, User updatedUser)
        {
            updatedUser.UpdatedAt = DateTime.UtcNow;
            var result = await _users.ReplaceOneAsync(u => u.Id == userId, updatedUser);
            return result.ModifiedCount > 0;
        }

        public async Task<int> GetUserCountByRoleAsync(string role)
        {
            return (int)await _users.CountDocumentsAsync(u => u.Role == role);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return (int)await _users.CountDocumentsAsync(_ => true);
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var update = Builders<User>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var update = Builders<User>.Update
                .Set(u => u.IsActive, true)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }
    }
}