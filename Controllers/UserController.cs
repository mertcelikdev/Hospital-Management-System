using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Kullanıcı listesi (Admin ve Staff)
        [AuthorizeRole("Admin", "Staff")]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // Hasta listesi (Admin, Doctor, Nurse, Staff)
        [AuthorizeRole("Admin", "Doctor", "Nurse", "Staff")]
        public async Task<IActionResult> Patients()
        {
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.CurrentUserRole = userRole;
            
            return View(patients);
        }

        // Kullanıcı detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Yeni kullanıcı oluşturma formu (Admin, Staff, Nurse)
        [AuthorizeRole("Admin", "Staff", "Nurse")]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kullanıcı oluşturma POST (Admin, Doctor, Staff)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor", "Staff")]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = null; // MongoDB otomatik ID oluşturacak
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                // Şifreyi hash'le
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                await _userService.CreateUserAsync(user);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // Kullanıcı düzenleme formu
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Kullanıcı düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Şifre değiştirilmişse hash'le
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                await _userService.UpdateUserAsync(id, user);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // Kullanıcı silme (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
