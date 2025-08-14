using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Services
{
    public class UserService
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

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return user;
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

        public async Task<List<User>> GetDoctorsAsync()
        {
            return await _users.Find(u => u.Role == "Doctor").ToListAsync();
        }

        public async Task<List<User>> GetPatientsAsync()
        {
            return await _users.Find(u => u.Role == "Patient").ToListAsync();
        }

        public async Task<List<User>> GetNursesAsync()
        {
            return await _users.Find(u => u.Role == "Nurse").ToListAsync();
        }

        public async Task<List<User>> GetStaffAsync()
        {
            return await _users.Find(u => u.Role == "Staff").ToListAsync();
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _users.Find(u => u.Role == role).ToListAsync();
        }

        public async Task<List<User>> GetDoctorPatientsAsync(string doctorId)
        {
            return await _users.Find(u => u.AssignedDoctorId == doctorId).ToListAsync();
        }

        public async Task<List<User>> GetNursePatientsAsync(string nurseId)
        {
            return await _users.Find(u => u.AssignedNurseId == nurseId).ToListAsync();
        }

        public async Task AssignPatientToDoctorAsync(string patientId, string doctorId)
        {
            var update = Builders<User>.Update
                .Set(u => u.AssignedDoctorId, doctorId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            await _users.UpdateOneAsync(u => u.Id == patientId, update);

            // Doktorun hasta listesine ekle
            var doctorUpdate = Builders<User>.Update
                .AddToSet(u => u.PatientIds, patientId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            await _users.UpdateOneAsync(u => u.Id == doctorId, doctorUpdate);
        }

        public async Task AssignPatientToNurseAsync(string patientId, string nurseId)
        {
            var update = Builders<User>.Update
                .Set(u => u.AssignedNurseId, nurseId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            await _users.UpdateOneAsync(u => u.Id == patientId, update);

            // Hemşirenin hasta listesine ekle
            var nurseUpdate = Builders<User>.Update
                .AddToSet(u => u.PatientIds, patientId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
            await _users.UpdateOneAsync(u => u.Id == nurseId, nurseUpdate);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null && user.IsActive;
            // TODO: Implement proper password hashing and validation
        }
    }
}