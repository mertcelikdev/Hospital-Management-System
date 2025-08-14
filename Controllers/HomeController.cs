using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole))
            {
                // Role'e göre uygun dashboard'a yönlendir
                return userRole switch
                {
                    "Admin" => RedirectToAction("AdminDashboard", "Dashboard"),
                    "Doctor" => RedirectToAction("DoctorDashboard", "Dashboard"),
                    "Nurse" => RedirectToAction("NurseDashboard", "Dashboard"),
                    "Staff" => RedirectToAction("StaffDashboard", "Dashboard"),
                    "Patient" => RedirectToAction("PatientDashboard", "Dashboard"),
                    _ => RedirectToAction("Login", "Auth")
                };
            }

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
