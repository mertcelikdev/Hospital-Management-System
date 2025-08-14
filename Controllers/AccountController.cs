using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

        public AccountController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

    [HttpGet]
    public IActionResult Login()
        {
            // Auth klasorundeki Login view kullaniliyor
            return View("~/Views/Auth/Login.cshtml");
        }

    [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Lütfen gerekli alanları doldurun";
                return View("~/Views/Auth/Login.cshtml", model);
            }
            // AuthDto.cs içindeki LoginDto Email alanını kullanıyoruz
            var token = await _authService.LoginAsync(model.Email, model.Password);
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Geçersiz kullanıcı adı/e-posta veya şifre";
                return View("~/Views/Auth/Login.cshtml", model);
            }
            HttpContext.Session.SetString("AuthToken", token);

            // Kullanıcı bilgilerini al ve session'a koy
            var user = await _authService.GetUserByTokenAsync(token);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.Id))
                    HttpContext.Session.SetString("UserId", user.Id);
                if (!string.IsNullOrEmpty(user.Role))
                    HttpContext.Session.SetString("UserRole", user.Role);
                // Görüntülenecek isim (önce First+Last, yoksa Name, yoksa Username)
                var fullName = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName)
                    ? ($"{user.FirstName} {user.LastName}").Trim()
                    : (!string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Username);
                if (!string.IsNullOrWhiteSpace(fullName))
                    HttpContext.Session.SetString("UserFullName", fullName);
                if (!string.IsNullOrWhiteSpace(user.Username))
                    HttpContext.Session.SetString("UserName", user.Username);

                // Role bazlı yönlendirme
                    return user.Role switch
                    {
                        "Admin" => RedirectToAction("Admin", "Dashboard"),
                        "Doctor" => RedirectToAction("Doctor", "Dashboard"),
                        "Nurse" => RedirectToAction("Nurse", "Dashboard"),
                        "Staff" => RedirectToAction("Index", "Dashboard"),
                        "Patient" => RedirectToAction("Patient", "Dashboard"),
                        _ => RedirectToAction("Index", "Dashboard")
                    };
            }

            // Kullanıcı alınamazsa fallback
            return RedirectToAction("RoleDispatch", "Dashboard");
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
            var user = new HospitalManagementSystem.Models.User
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var registered = await _authService.RegisterAsync(user, model.Password);
            if (!registered)
            {
                ModelState.AddModelError("", "Bu kullanıcı adı veya e-posta zaten kayıtlı");
                return View(model);
            }

            return RedirectToAction("RoleDispatch", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                // Token'i geçersiz kıl
                await _authService.LogoutAsync(token);
            }
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
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

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var changed = await _authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
            if (!changed)
            {
                ModelState.AddModelError("", "Failed to change password. Please check your current password.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Erişim reddedildi. Yetkiniz yok.");
        }
    }
}

