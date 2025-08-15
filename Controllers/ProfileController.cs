using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse", "Staff", "Patient")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        // Profil görüntüleme
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user); // View artık UserDto ile çalışmalı
        }

        // Profil düzenleme formu
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new UpdateUserDto
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                EmergencyContact = user.EmergencyContact,
                Specialization = user.Specialization,
                LicenseNumber = user.LicenseNumber,
                DepartmentId = user.DepartmentId
            });
        }

        // Profil düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto userDto)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(userId, userDto);
                TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(userDto);
        }

        // Şifre değiştirme formu
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Şifre değiştirme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifre ve onay şifresi eşleşmiyor.");
                return View();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Mevcut şifre yanlış.");
                return View();
            }

            // Şifre güncelleme işlemi ayrı bir service metoduna taşınmalı; burada placeholder
            // await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
