using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IAuthService authService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Login denemesi: {Email} IP={IP}", model.Email, HttpContext.Connection.RemoteIpAddress);
                var loginResult = await _authService.LoginAsync(new LoginDto { Email = model.Email, Password = model.Password, RememberMe = model.RememberMe });
                if (loginResult != null && loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                {
                    _logger.LogInformation("Login başarılı: {Email} Rol={Role}", model.Email, loginResult.User?.Role);
                    // Access + refresh cookie (AccountController mantığı ile uyumlu)
                    Response.Cookies.Append("HMS.AuthToken", loginResult.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });
                    if (!string.IsNullOrEmpty(loginResult.RefreshToken))
                    {
                        Response.Cookies.Append("HMS.RefreshToken", loginResult.RefreshToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(30)
                        });
            _logger.LogDebug("Refresh token oluşturuldu: {UserId}", loginResult.User?.Id);
                    }
                    return RedirectToRoleDashboard(loginResult.User?.Role ?? "");
                }
        _logger.LogWarning("Login başarısız: {Email}", model.Email);
                ModelState.AddModelError("", "Geçersiz email veya şifre.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logout çağrısı. UserId={UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (Request.Cookies.TryGetValue("HMS.RefreshToken", out var refresh))
            {
                await _authService.RevokeRefreshTokenAsync(refresh);
                _logger.LogDebug("Refresh token revoke edildi");
            }
            if (Request.Cookies.ContainsKey("HMS.AuthToken")) Response.Cookies.Delete("HMS.AuthToken");
            if (Request.Cookies.ContainsKey("HMS.RefreshToken")) Response.Cookies.Delete("HMS.RefreshToken");
            _logger.LogInformation("Logout tamamlandı");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Dashboard"),
                "Doctor" => RedirectToAction("DoctorDashboard", "Dashboard"),
                "Nurse" => RedirectToAction("NurseDashboard", "Dashboard"),
                "Staff" => RedirectToAction("StaffDashboard", "Dashboard"),
                "Patient" => RedirectToAction("PatientDashboard", "Dashboard"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
