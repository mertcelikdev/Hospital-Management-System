using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; // Only for legacy SHA256 fallback

namespace HospitalManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private class TokenDocument
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Value { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        private readonly IMongoCollection<TokenDocument> _tokens; // Store valid tokens
        private readonly JwtSettings _jwtSettings; // Config'd JWT ayarları

        public AuthService(IMongoDatabase database, JwtSettings jwtSettings)
        {
            _users = database.GetCollection<User>("Users");
            _tokens = database.GetCollection<TokenDocument>("ValidTokens");
            _jwtSettings = jwtSettings;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username || u.Email == username).FirstOrDefaultAsync();
            
            if (user == null || !user.IsActive || !VerifyPassword(password, user.PasswordHash))
                return string.Empty; // Interface null'a izin vermiyor

            // Generate JWT token
            var token = GenerateJwtToken(user);
            
            // Store token for validation
            await _tokens.InsertOneAsync(new TokenDocument { Value = token });
            
            return token;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            // Check if user already exists
            var existingUser = await _users.Find(u => u.Email == user.Email || u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null) return false;

            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken _);

                // Check if token is in our valid tokens collection
                var storedToken = await _tokens.Find(t => t.Value == token).FirstOrDefaultAsync();
                return storedToken != null;            
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim == null) return null;

                var user = await _users.Find(u => u.Id == userIdClaim.Value).FirstOrDefaultAsync();
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null || !VerifyPassword(oldPassword, user.PasswordHash))
            {
                return false;
            }

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(newPassword))
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return false;

            // Generate temporary password
            var tempPassword = GenerateTemporaryPassword();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, hashedPassword)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Email == email, update);
            
            // TODO: Send email with temporary password
            
            return result.ModifiedCount > 0;
        }

        public async Task LogoutAsync(string token)
        {
            // Remove token from valid tokens collection
            await _tokens.DeleteOneAsync(t => t.Value == token);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username || u.Email == username).FirstOrDefaultAsync();
            return user != null && user.IsActive && VerifyPassword(password, user.PasswordHash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;
            // BCrypt hash kontrolü ($2 ile başlar)
            if (storedHash.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            // Legacy SHA256 fallback (geçici)
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString() == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", user.Id),
                    new Claim("username", user.Username),
                    new Claim("email", user.Email),
                    new Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays > 0 ? _jwtSettings.ExpireDays : 7),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
