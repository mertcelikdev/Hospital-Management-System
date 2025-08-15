using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        // Mapping helpers
        private static UserDto MapToDto(User u) => new()
        {
            Id = u.Id ?? string.Empty,
            Name = u.Name,
            Email = u.Email,
            Username = u.Username,
            Phone = u.Phone,
            Role = u.Role,
            FirstName = u.FirstName,
            LastName = u.LastName,
            DateOfBirth = u.DateOfBirth,
            Gender = u.Gender?.ToString(),
            Address = u.Address,
            EmergencyContact = u.EmergencyContact,
            Specialization = u.Specialization,
            LicenseNumber = u.LicenseNumber,
            DepartmentId = u.DepartmentId,
            ProfileImageUrl = null,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };

        private static User MapFromCreate(CreateUserDto dto)
        {
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Patient" : dto.Role!;
            return new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Username = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email : dto.Username!,
                Phone = dto.Phone ?? string.Empty,
                Role = role,
                TcNo = dto.TcNo ?? string.Empty,
                PasswordHash = string.IsNullOrEmpty(dto.Password) ? string.Empty : BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var list = await _users.Find(_ => true).ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var u = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            return MapToDto(u!);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = MapFromCreate(createUserDto);
            await _users.InsertOneAsync(user);
            return MapToDto(user);
        }

        public async Task UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var updateDef = Builders<User>.Update
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            if (updateUserDto.Name != null) updateDef = updateDef.Set(u => u.Name, updateUserDto.Name);
            if (updateUserDto.Email != null) updateDef = updateDef.Set(u => u.Email, updateUserDto.Email);
            if (updateUserDto.Phone != null) updateDef = updateDef.Set(u => u.Phone, updateUserDto.Phone);
            if (updateUserDto.Username != null) updateDef = updateDef.Set(u => u.Username, updateUserDto.Username);
            if (updateUserDto.FirstName != null) updateDef = updateDef.Set(u => u.FirstName, updateUserDto.FirstName);
            if (updateUserDto.LastName != null) updateDef = updateDef.Set(u => u.LastName, updateUserDto.LastName);
            if (updateUserDto.DateOfBirth.HasValue) updateDef = updateDef.Set(u => u.DateOfBirth, updateUserDto.DateOfBirth);
            if (updateUserDto.Gender != null && Enum.TryParse<Gender>(updateUserDto.Gender, true, out var g)) updateDef = updateDef.Set(u => u.Gender, g);
            if (updateUserDto.Address != null) updateDef = updateDef.Set(u => u.Address, updateUserDto.Address);
            if (updateUserDto.EmergencyContact != null) updateDef = updateDef.Set(u => u.EmergencyContact, updateUserDto.EmergencyContact);
            if (updateUserDto.Specialization != null) updateDef = updateDef.Set(u => u.Specialization, updateUserDto.Specialization);
            if (updateUserDto.LicenseNumber != null) updateDef = updateDef.Set(u => u.LicenseNumber, updateUserDto.LicenseNumber);
            if (updateUserDto.DepartmentId != null) updateDef = updateDef.Set(u => u.DepartmentId, updateUserDto.DepartmentId);

            await _users.UpdateOneAsync(u => u.Id == id, updateDef);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        private async Task<IEnumerable<UserDto>> GetByRoleInternalAsync(string role)
        {
            var list = await _users.Find(u => u.Role == role).ToListAsync();
            return list.Select(MapToDto);
        }

        public Task<IEnumerable<UserDto>> GetDoctorsAsync() => GetByRoleInternalAsync("Doctor");
        public Task<IEnumerable<UserDto>> GetPatientsAsync() => GetByRoleInternalAsync("Patient");
        public Task<IEnumerable<UserDto>> GetNursesAsync() => GetByRoleInternalAsync("Nurse");
        public Task<IEnumerable<UserDto>> GetStaffAsync() => GetByRoleInternalAsync("Staff");
        public Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role) => GetByRoleInternalAsync(role);

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var u = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return MapToDto(u!);
        }

        public async Task<UserDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
            return MapToDto(user);
        }

        // Dashboard metrics
    public async Task<int> GetTotalUsersCountAsync() => (int)await _users.CountDocumentsAsync(_ => true);
    public async Task<int> GetUserCountByRoleAsync(string role) => (int)await _users.CountDocumentsAsync(u => u.Role == role);
    public async Task<int> GetUsersCountAsync() => (int)await _users.CountDocumentsAsync(_ => true);
        public async Task<List<UserDto>> GetRecentUsersAsync(int count)
        {
            var list = await _users.Find(_ => true).SortByDescending(u => u.CreatedAt).Limit(count).ToListAsync();
            return list.Select(u => MapToDto(u)).ToList();
        }
        public async Task<int> GetNewUsersCountThisMonthAsync(string role)
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var filter = Builders<User>.Filter.Gte(u => u.CreatedAt, start) & Builders<User>.Filter.Eq(u => u.Role, role);
            return (int)await _users.CountDocumentsAsync(filter);
        }
    }
}