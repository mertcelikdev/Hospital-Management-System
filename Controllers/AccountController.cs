using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, IUserService userService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _logger.LogInformation("Login denemesi (AccountController): {Email}", model.Email);
            var result = await _authService.LoginAsync(model);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogWarning("Login başarısız (AccountController): {Email}", model.Email);
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }
            _logger.LogInformation("Login başarılı (AccountController): {Email} Rol={Role}", model.Email, result.User?.Role);

            // JWT'yi HttpOnly cookie'ye yaz
            if (!string.IsNullOrEmpty(result.Token))
            {
                Response.Cookies.Append("HMS.AuthToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
            }
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("HMS.RefreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // RegisterDto -> CreateUserDto dönüşümü
            var createUser = new CreateUserDto
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Username = model.Email.Split('@')[0],
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                EmergencyContact = model.EmergencyContact
            }; // Rol ve IsActive otomatik yönetiliyor

            _logger.LogInformation("Register denemesi: {Email}", model.Email);
            var result = await _authService.RegisterAsync(createUser);
            if (result == null || !result.IsSuccess)
            {
                _logger.LogWarning("Register başarısız: {Email} Mesaj={Message}", model.Email, result?.Message);
                ModelState.AddModelError("", result?.Message ?? "User with this email already exists");
                return View(model);
            }
            _logger.LogInformation("Register başarılı: {Email} YeniUserId={UserId}", model.Email, result.User?.Id);

            if (!string.IsNullOrEmpty(result.Token))
            {
                Response.Cookies.Append("HMS.AuthToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
            }
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("HMS.RefreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Refresh token revoke
            _logger.LogInformation("Logout (AccountController) çağrısı UserId={UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (Request.Cookies.TryGetValue("HMS.RefreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
            {
                await _authService.RevokeRefreshTokenAsync(refreshToken);
                _logger.LogDebug("Refresh token revoke edildi (AccountController)");
            }
            if (Request.Cookies.ContainsKey("HMS.AuthToken")) Response.Cookies.Delete("HMS.AuthToken");
            if (Request.Cookies.ContainsKey("HMS.RefreshToken")) Response.Cookies.Delete("HMS.RefreshToken");
            _logger.LogInformation("Logout tamamlandı (AccountController)");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Sadece same-site strict cookie olduğu için ekliyoruz; istersen CSRF token ekle
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("HMS.RefreshToken", out var oldRefresh) || string.IsNullOrEmpty(oldRefresh))
                return Unauthorized(new { message = "Refresh token yok" });

            var record = await _authService.ValidateRefreshTokenAsync(oldRefresh);
            if (record == null)
            {
                // Eski cookie'leri temizle
                Response.Cookies.Delete("HMS.AuthToken");
                Response.Cookies.Delete("HMS.RefreshToken");
                return Unauthorized(new { message = "Geçersiz veya süresi dolmuş refresh token" });
            }

            var user = await _userService.GetUserByIdAsync(record.UserId);
            if (user == null)
            {
                await _authService.RevokeRefreshTokenAsync(oldRefresh);
                Response.Cookies.Delete("HMS.AuthToken");
                Response.Cookies.Delete("HMS.RefreshToken");
                return Unauthorized(new { message = "Kullanıcı bulunamadı" });
            }

            // Token rotation: eski refresh revoke
            await _authService.RevokeRefreshTokenAsync(oldRefresh);

            // Yeni access token + yeni refresh
            var userDto = new UserDto
            {
                Id = user.Id!,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            var newAccess = await _authService.GenerateTokenAsync(userDto);
            var newRefresh = await _authService.CreateRefreshTokenAsync(user.Id!);

            Response.Cookies.Append("HMS.AuthToken", newAccess, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });
            Response.Cookies.Append("HMS.RefreshToken", newRefresh.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return Json(new { success = true, accessToken = newAccess, refreshToken = newRefresh.Token });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var result = await _authService.ChangePasswordAsync(userId, model);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to change password. Please check your current password.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Index", "Home");
        }
    }
}
