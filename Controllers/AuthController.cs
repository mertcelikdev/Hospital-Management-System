using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
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
                var user = await _userService.AuthenticateAsync(model.Email, model.Password);
                if (user != null)
                {
                    // Session'a kullanıcı bilgilerini kaydet
                    HttpContext.Session.SetString("UserId", user.Id!);
                    HttpContext.Session.SetString("UserName", user.Name);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Role'e göre uygun dashboard'a yönlendir
                    return RedirectToRoleDashboard(user.Role);
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz email veya şifre.");
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
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
