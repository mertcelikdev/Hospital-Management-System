using MongoDB.Driver;
using BCrypt.Net;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Cryptography;

namespace HospitalManagementSystem.Services
{
    public class AuthService : IAuthService
    {
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<UserSession> _sessions;
    private readonly IMongoCollection<UserRoleAssignments> _userRoles;
    private readonly IMongoCollection<UserPermissionAssignments> _userPermissions;
    private readonly IMongoCollection<LoginActivityRecord> _loginActivities;
    private readonly IMongoCollection<RefreshTokenRecord> _refreshTokens;
    private readonly string _jwtSecret;
    private readonly string? _jwtIssuer;
    private readonly string? _jwtAudience;
    private readonly TimeSpan _tokenLifetime;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IMongoDatabase database, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _users = database.GetCollection<User>("Users");
            _sessions = database.GetCollection<UserSession>("UserSessions");
            _userRoles = database.GetCollection<UserRoleAssignments>("UserRoleAssignments");
            _userPermissions = database.GetCollection<UserPermissionAssignments>("UserPermissionAssignments");
            _loginActivities = database.GetCollection<LoginActivityRecord>("LoginActivities");
            _refreshTokens = database.GetCollection<RefreshTokenRecord>("RefreshTokens");
            _jwtSecret = configuration["Jwt:Key"] ?? configuration["Jwt:Secret"] ?? "default-secret-key-for-development";
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
            if (int.TryParse(configuration["Jwt:AccessTokenHours"], out var hours))
                _tokenLifetime = TimeSpan.FromHours(hours);
            else if (int.TryParse(configuration["Jwt:ExpireDays"], out var legacyDays))
                _tokenLifetime = TimeSpan.FromDays(legacyDays);
            else
                _tokenLifetime = TimeSpan.FromHours(8);
            _logger = logger;

            // Index ve temizlik (id=null belgelerini kaldır) - hata önleme
            try
            {
                _refreshTokens.DeleteMany(Builders<RefreshTokenRecord>.Filter.Or(
                    Builders<RefreshTokenRecord>.Filter.Eq(r => r.Id, null),
                    Builders<RefreshTokenRecord>.Filter.Eq(r => r.Id, "")));

                // Token için benzersiz index
                var tokenIndex = new CreateIndexModel<RefreshTokenRecord>(
                    Builders<RefreshTokenRecord>.IndexKeys.Ascending(r => r.Token),
                    new CreateIndexOptions { Unique = true, Name = "UX_RefreshTokens_Token" });
                // Kullanıcı + Revoked kombinasyonu sorgu optimizasyonu
                var userRevokedIndex = new CreateIndexModel<RefreshTokenRecord>(
                    Builders<RefreshTokenRecord>.IndexKeys.Ascending(r => r.UserId).Ascending(r => r.IsRevoked),
                    new CreateIndexOptions { Name = "IX_RefreshTokens_User_IsRevoked" });
                _refreshTokens.Indexes.CreateMany(new[] { tokenIndex, userRevokedIndex });
            }
            catch { /* index hataları sessiz geçilecek */ }
        }

    public async Task<UserDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return MapToUserDto(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("LoginAsync çağrısı: {Email}", loginDto.Email);
            var user = await _users.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();
            
            if (user == null)
            {
                _logger.LogWarning("Login başarısız (email yok): {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Token = null,
                    User = null
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login başarısız (şifre uyuşmadı): {Email}", loginDto.Email);
                return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
            }

            var token = await GenerateTokenAsync(MapToUserDto(user));
            var refresh = await CreateRefreshTokenAsync(user.Id!);

            _logger.LogInformation("Login başarılı: {Email} UserId={UserId}", user.Email, user.Id);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = MapToUserDto(user),
                RefreshToken = refresh.Token,
                TokenExpiry = DateTime.UtcNow.Add(_tokenLifetime)
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto)
        {
            _logger.LogInformation("RegisterAsync çağrısı: {Email}", createUserDto.Email);
            var existingUser = await _users.Find(u => u.Email == createUserDto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                _logger.LogWarning("Register başarısız (email mevcut): {Email}", createUserDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists",
                    Token = null,
                    User = null
                };
            }

            var hashedPassword = await HashPasswordAsync(createUserDto.Password);
            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PasswordHash = hashedPassword,
                Role = "Patient", // formdan rol kaldırıldı, kayıt olan hasta
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(user);
            _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email} UserId={UserId}", user.Email, user.Id);

            var token = await GenerateTokenAsync(MapToUserDto(user));
            var refresh = await CreateRefreshTokenAsync(user.Id!);

            _logger.LogInformation("Register başarılı: {Email} UserId={UserId}", user.Email, user.Id);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                User = MapToUserDto(user),
                RefreshToken = refresh.Token,
                TokenExpiry = DateTime.UtcNow.Add(_tokenLifetime)
            };
        }

        public async Task LogoutAsync(string userId)
        {
            // JWT ile klasik session kaldırıldı; burada sadece refresh token'ları revoke edebiliriz.
            await RevokeAllRefreshTokensForUserAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false;

            var newHashedPassword = await HashPasswordAsync(changePasswordDto.NewPassword);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, newHashedPassword)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            var ok = result.ModifiedCount > 0;
            if (ok) _logger.LogInformation("Şifre değişti UserId={UserId}", userId);
            else _logger.LogWarning("Şifre değişmedi UserId={UserId}", userId);
            return ok;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return false;

            // Generate temporary password
            var tempPassword = GenerateTemporaryPassword();
            var hashedPassword = await HashPasswordAsync(tempPassword);

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, hashedPassword)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Email == email, update);
            
            // TODO: Send email with temporary password
            
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ValidatePasswordAsync(string password, string hashedPassword)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, hashedPassword));
        }

        public async Task<string> HashPasswordAsync(string password)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
        }

    public async Task<string> GenerateTokenAsync(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_tokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = string.IsNullOrWhiteSpace(_jwtAudience) ? null : _jwtAudience,
                Issuer = string.IsNullOrWhiteSpace(_jwtIssuer) ? null : _jwtIssuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

    public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(_jwtIssuer),
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(_jwtAudience),
                    ValidAudience = _jwtAudience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

    public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(_jwtIssuer),
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(_jwtAudience),
                    ValidAudience = _jwtAudience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return null;

                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                return user == null ? null : MapToUserDto(user);
            }
            catch
            {
                return null;
            }
        }

        public async Task RevokeTokenAsync(string token)
        {
            // For JWT tokens, we can't revoke them directly
            // This would require a token blacklist in production
            await Task.CompletedTask;
        }

        // --- Refresh Token Methods ---
        public async Task<RefreshTokenRecord> CreateRefreshTokenAsync(string userId, TimeSpan? lifetime = null)
        {
            string token;
            int attempt = 0;
            do
            {
                token = GenerateSecureToken();
                attempt++;
                if (attempt > 5) // çok düşük ihtimalde çakışma döngüsünü kır
                    token = token + Guid.NewGuid().ToString("N");
            }
            while (await _refreshTokens.Find(r => r.Token == token).AnyAsync());

            var refresh = new RefreshTokenRecord
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(30)),
                IsRevoked = false
            };
            if (string.IsNullOrEmpty(refresh.Id))
                refresh.Id = ObjectId.GenerateNewId().ToString();
            await _refreshTokens.InsertOneAsync(refresh);
            _logger.LogDebug("Refresh token oluşturuldu UserId={UserId} Expires={Exp}", userId, refresh.ExpiresAt);
            return refresh;
        }

        public async Task<RefreshTokenRecord?> ValidateRefreshTokenAsync(string token)
        {
            var record = await _refreshTokens.Find(r => r.Token == token).FirstOrDefaultAsync();
            if (record == null) return null;
            if (record.IsRevoked || record.ExpiresAt <= DateTime.UtcNow) return null;
            return record;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var update = Builders<RefreshTokenRecord>.Update.Set(r => r.IsRevoked, true).Set(r => r.RevokedAt, DateTime.UtcNow);
            await _refreshTokens.UpdateOneAsync(r => r.Token == token, update);
            _logger.LogDebug("Refresh token revoke edildi: {TokenHash}", token?.GetHashCode());
        }

        public async Task RevokeAllRefreshTokensForUserAsync(string userId)
        {
            var update = Builders<RefreshTokenRecord>.Update.Set(r => r.IsRevoked, true).Set(r => r.RevokedAt, DateTime.UtcNow);
            var result = await _refreshTokens.UpdateManyAsync(r => r.UserId == userId && !r.IsRevoked, update);
            _logger.LogInformation("Tüm refresh tokenlar revoke edildi UserId={UserId} Count={Count}", userId, result.ModifiedCount);
        }
        // --- Refresh Token Methods Sonu ---

    // Eski session metodları kaldırıldı (JWT ile gereksiz). İleride audit gerekirse yeniden eklenebilir.

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return false;
        }

        public async Task LockAccountAsync(string email, int lockoutMinutes = 30)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return;
            await Task.CompletedTask;
        }

        public async Task UnlockAccountAsync(string email)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            if (!string.IsNullOrEmpty(excludeUserId))
            {
                filter &= Builders<User>.Filter.Ne(u => u.Id, excludeUserId);
            }
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user == null;
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone, string? excludeUserId = null)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Phone, phone);
            if (!string.IsNullOrEmpty(excludeUserId))
            {
                filter &= Builders<User>.Filter.Ne(u => u.Id, excludeUserId);
            }
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user == null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserDto?> GetUserByPhoneAsync(string phone)
        {
            var user = await _users.Find(u => u.Phone == phone).FirstOrDefaultAsync();
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<bool> HasRoleAsync(string userId, string role)
        {
            var user = await _users.Find(u => u.Id == userId && u.Role == role).FirstOrDefaultAsync();
            if (user != null) return true;
            var extra = await _userRoles.Find(r => r.UserId == userId && r.Roles.Contains(role)).FirstOrDefaultAsync();
            return extra != null;
        }

        public async Task<bool> HasAnyRoleAsync(string userId, params string[] roles)
        {
            if (roles == null || roles.Length == 0) return false;
            var set = roles.ToHashSet();
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null && set.Contains(user.Role)) return true;
            var extra = await _userRoles.Find(r => r.UserId == userId).FirstOrDefaultAsync();
            return extra != null && extra.Roles.Any(r => set.Contains(r));
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var list = new List<string>();
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null) list.Add(user.Role);
            var extra = await _userRoles.Find(r => r.UserId == userId).FirstOrDefaultAsync();
            if (extra != null) list.AddRange(extra.Roles);
            return list.Distinct().ToList();
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var update = Builders<UserRoleAssignments>.Update
                .AddToSet(r => r.Roles, role)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);
            await _userRoles.UpdateOneAsync(r => r.UserId == userId, update, new UpdateOptions{ IsUpsert = true});
        }

        public async Task RemoveRoleAsync(string userId, string role)
        {
            var update = Builders<UserRoleAssignments>.Update
                .Pull(r => r.Roles, role)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);
            await _userRoles.UpdateOneAsync(r => r.UserId == userId, update);
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            var direct = await _userPermissions.Find(p => p.UserId == userId && p.Permissions.Contains(permission)).FirstOrDefaultAsync();
            if (direct != null) return true;
            var roles = await GetUserRolesAsync(userId);
            foreach (var r in roles)
            {
                if (Security.PermissionCatalog.DefaultRolePermissions.TryGetValue(r, out var perms) && perms.Contains(permission))
                    return true;
            }
            return false;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var set = new HashSet<string>();
            var direct = await _userPermissions.Find(p => p.UserId == userId).FirstOrDefaultAsync();
            if (direct != null) foreach (var p in direct.Permissions) set.Add(p);
            var roles = await GetUserRolesAsync(userId);
            foreach (var r in roles)
            {
                if (Security.PermissionCatalog.DefaultRolePermissions.TryGetValue(r, out var perms))
                    foreach (var p in perms) set.Add(p);
            }
            return set.ToList();
        }

        public async Task<bool> CanAccessResourceAsync(string userId, string resource, string action)
        {
            var permission = $"{resource}.{action}".ToLowerInvariant();
            return await HasPermissionAsync(userId, permission);
        }

        public async Task LogLoginActivityAsync(string userId, string ipAddress, string userAgent)
        {
            var record = new LoginActivityRecord
            {
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true
            };
            await _loginActivities.InsertOneAsync(record);
        }

        public async Task LogLogoutActivityAsync(string userId)
        {
            var filter = Builders<LoginActivityRecord>.Filter.Eq(a => a.UserId, userId) & Builders<LoginActivityRecord>.Filter.Eq(a => a.LogoutTime, null);
            var update = Builders<LoginActivityRecord>.Update.Set(a => a.LogoutTime, DateTime.UtcNow);
            await _loginActivities.UpdateOneAsync(filter, update);
        }

        public async Task<List<LoginActivity>> GetUserLoginHistoryAsync(string userId, int limit = 10)
        {
            var list = await _loginActivities.Find(a => a.UserId == userId).SortByDescending(a => a.LoginTime).Limit(limit).ToListAsync();
            return list.Select(a => new LoginActivity
            {
                Id = a.Id ?? string.Empty,
                UserId = a.UserId,
                LoginTime = a.LoginTime,
                LogoutTime = a.LogoutTime,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                IsSuccessful = a.IsSuccessful,
                FailureReason = a.FailureReason
            }).ToList();
        }

        public async Task<List<LoginActivity>> GetRecentLoginActivitiesAsync(int limit = 50)
        {
            var list = await _loginActivities.Find(_ => true).SortByDescending(a => a.LoginTime).Limit(limit).ToListAsync();
            return list.Select(a => new LoginActivity
            {
                Id = a.Id ?? string.Empty,
                UserId = a.UserId,
                LoginTime = a.LoginTime,
                LogoutTime = a.LogoutTime,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                IsSuccessful = a.IsSuccessful,
                FailureReason = a.FailureReason
            }).ToList();
        }

        public Task<bool> ValidatePasswordPolicyAsync(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return Task.FromResult(false);
            if (!password.Any(char.IsUpper)) return Task.FromResult(false);
            if (!password.Any(char.IsLower)) return Task.FromResult(false);
            if (!password.Any(char.IsDigit)) return Task.FromResult(false);
            return Task.FromResult(true);
        }

        public Task<List<string>> GetPasswordRequirementsAsync()
        {
            return Task.FromResult(new List<string>{"Min 8 karakter","Bir büyük harf","Bir küçük harf","Bir rakam"});
        }

        public Task<bool> IsPasswordExpiredAsync(string userId)
        {
            return Task.FromResult(false);
        }

        public Task SetPasswordExpiryAsync(string userId, DateTime expiryDate)
        {
            return Task.CompletedTask;
        }

        public Task IncrementFailedLoginAttemptsAsync(string email)
        {
            return Task.CompletedTask;
        }

        public Task ResetFailedLoginAttemptsAsync(string email)
        {
            return Task.CompletedTask;
        }

        private string GenerateTemporaryPassword()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private UserDto MapToUserDto(User user) => new UserDto
        {
            Id = user.Id ?? string.Empty,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone ?? string.Empty,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        private string GenerateSecureToken(int size = 64)
        {
            // size bayt; Base64 uzunluğu ~4/3 * size
            var bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    public class UserSession
    {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class RefreshTokenRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
