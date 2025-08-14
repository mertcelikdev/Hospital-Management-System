using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDepartmentService _departmentService;

        public HomeController(IAppointmentService appointmentService, IDepartmentService departmentService)
        {
            _appointmentService = appointmentService;
            _departmentService = departmentService;
        }

        // Root: login veya role dashboard yönlendirmesi
        [HttpGet("/")]
        public IActionResult Root()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }
            return role switch
            {
                "Admin" => RedirectToAction("Admin", "Dashboard"),
                "Doctor" => RedirectToAction("Doctor", "Dashboard"),
                "Nurse" => RedirectToAction("Nurse", "Dashboard"),
                "Staff" => RedirectToAction("Index", "Dashboard"),
                _ => RedirectToAction("Index")
            };
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Ana sayfa için genel bilgiler
                var viewModel = new
                {
                    TotalAppointments = await _appointmentService.GetTotalAppointmentsCountAsync(),
                    TodayAppointments = await _appointmentService.GetTodayAppointmentsCountAsync(),
                    Departments = await _departmentService.GetAllDepartmentsAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ana sayfa yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Services()
        {
            try
            {
                // Hastane hizmetleri sayfası
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hizmetler sayfası yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> Departments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return View(departments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departmanlar yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotFound()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactForm(string name, string email, string subject, string message)
        {
            try
            {
                // İletişim formu işleme (email gönderme vs.)
                // Şimdilik sadece başarı mesajı
                TempData["Success"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Mesaj gönderilirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Contact));
            }
        }

        [Authorize]
        public async Task<IActionResult> QuickStats()
        {
            try
            {
                var stats = new
                {
                    TodayAppointments = await _appointmentService.GetTodayAppointmentsCountAsync(),
                    PendingAppointments = await _appointmentService.GetPendingAppointmentsCountAsync(),
                    CompletedAppointments = await _appointmentService.GetCompletedAppointmentsCountAsync(),
                    TotalDepartments = (await _departmentService.GetAllDepartmentsAsync()).Count
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { error = "İstatistikler yüklenirken hata oluştu: " + ex.Message });
            }
        }
    }
}
