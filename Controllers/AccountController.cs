using HospitalManagementSystem.Models.DTOs;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
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

            var result = await _authService.LoginAsync(model);
            if (result == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Store token in session or cookie
            HttpContext.Session.SetString("AuthToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.User.Role.ToString());
            HttpContext.Session.SetString("UserId", result.User.Id);
            HttpContext.Session.SetString("UserName", result.User.FullName);

            return RedirectToAction("Index", "Dashboard");
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

            var result = await _authService.RegisterAsync(model);
            if (result == null)
            {
                ModelState.AddModelError("", "User with this email already exists");
                return View(model);
            }

            // Store token in session
            HttpContext.Session.SetString("AuthToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.User.Role.ToString());
            HttpContext.Session.SetString("UserId", result.User.Id);
            HttpContext.Session.SetString("UserName", result.User.FullName);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public IActionResult Logout()
        {
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

            var result = await _authService.ChangePasswordAsync(userId, model);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to change password. Please check your current password.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
