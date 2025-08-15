using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using System.Diagnostics;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

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
