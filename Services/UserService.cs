using HospitalManagementSystem.Models;
using MongoDB.Driver;

namespace HospitalManagementSystem.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> VerifyPasswordAsync(string email, string password);
        Task<string> HashPasswordAsync(string password);
        Task<bool> ChangePasswordAsync(string userId, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDbContext context)
        {
            _users = context.GetCollection<User>("Users");
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _users.Find(x => x.Id == id && x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(x => x.Email == email && x.IsActive).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _users.Find(x => x.Role == role && x.IsActive).ToListAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(x => x.IsActive).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.PasswordHash = await HashPasswordAsync(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var result = await _users.ReplaceOneAsync(x => x.Id == id, user);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var update = Builders<User>.Update.Set(x => x.IsActive, false);
            var result = await _users.UpdateOneAsync(x => x.Id == id, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;
            
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<string> HashPasswordAsync(string password)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
        }

        public async Task<bool> ChangePasswordAsync(string userId, string newPassword)
        {
            var hashedPassword = await HashPasswordAsync(newPassword);
            var update = Builders<User>.Update
                .Set(x => x.PasswordHash, hashedPassword)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            
            var result = await _users.UpdateOneAsync(x => x.Id == userId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
