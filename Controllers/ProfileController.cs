using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly IAuthService _authService;

        public ProfileController(
            IUserService userService,
            IDepartmentService departmentService,
            IAuthService authService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                // Departman bilgisini al
                if (!string.IsNullOrEmpty(user.DepartmentId))
                {
                    var department = await _departmentService.GetDepartmentByIdAsync(user.DepartmentId);
                    ViewBag.Department = department;
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Profil yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                var departments = await _departmentService.GetAllDepartmentsAsync();
                ViewBag.Departments = departments;

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Profil düzenleme sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId) || userId != user.Id)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    user.UpdatedAt = DateTime.Now;
                    var result = await _userService.UpdateUserProfileAsync(userId, user);
                    
                    if (result)
                    {
                        TempData["Success"] = "Profil başarıyla güncellendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Profil güncellenemedi.";
                    }
                }

                var departments = await _departmentService.GetAllDepartmentsAsync();
                ViewBag.Departments = departments;

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Profil güncellenirken hata oluştu: " + ex.Message;
                return View(user);
            }
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    TempData["Error"] = "Tüm alanları doldurunuz.";
                    return View();
                }

                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "Yeni şifreler uyuşmuyor.";
                    return View();
                }

                if (newPassword.Length < 6)
                {
                    TempData["Error"] = "Yeni şifre en az 6 karakter olmalıdır.";
                    return View();
                }

                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
                
                if (result)
                {
                    TempData["Success"] = "Şifre başarıyla değiştirildi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Mevcut şifre yanlış veya şifre değiştirilemedi.";
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Şifre değiştirilirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> Settings()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ayarlar sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(User user)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId) || userId != user.Id)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Sadece belirli alanları güncelle (güvenlik için)
                var existingUser = await _userService.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Address = user.Address;
                existingUser.UpdatedAt = DateTime.Now;

                var result = await _userService.UpdateUserProfileAsync(userId, existingUser);
                
                if (result)
                {
                    TempData["Success"] = "Ayarlar başarıyla güncellendi.";
                    return RedirectToAction(nameof(Settings));
                }
                else
                {
                    TempData["Error"] = "Ayarlar güncellenemedi.";
                }

                return View("Settings", existingUser);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ayarlar güncellenirken hata oluştu: " + ex.Message;
                return View("Settings", user);
            }
        }

        [RequirePermission("CanDeactivateAccount")]
        public async Task<IActionResult> DeactivateAccount()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hesap deaktivasyonu sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [RequirePermission("CanDeactivateAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAccountConfirmed()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _userService.DeactivateUserAsync(userId);
                
                if (result)
                {
                    TempData["Success"] = "Hesabınız başarıyla deaktive edildi.";
                    return RedirectToAction("Logout", "Account");
                }
                else
                {
                    TempData["Error"] = "Hesap deaktive edilemedi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hesap deaktive edilirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
