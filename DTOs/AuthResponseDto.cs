using System.Text.Json.Serialization;

namespace HospitalManagementSystem.DTOs
{
	public class AuthResponseDto
	{
		// Başarılı mı bayrağı (Success alias'ı geriye dönük uyum için tutuldu)
		public bool IsSuccess { get; set; }
		[JsonIgnore]
		public bool Success
		{
			get => IsSuccess;
			set => IsSuccess = value;
		}

		// Mesaj (hata veya bilgi)
		public string Message { get; set; } = string.Empty;

		// Minimal kullanıcı bilgisi
		public UserDto? User { get; set; }

		// JWT Access Token
		public string? Token { get; set; }
		public DateTime? TokenExpiry { get; set; }

		// Roller
		public List<string> Roles { get; set; } = new();

		// Opsiyonel refresh token
		public string? RefreshToken { get; set; }
		public DateTime? RefreshTokenExpiry { get; set; }

		// Ek dinamik veriler
		public Dictionary<string, object> AdditionalData { get; set; } = new();
	}
}
