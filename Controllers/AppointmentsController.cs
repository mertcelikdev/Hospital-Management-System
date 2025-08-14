using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;

        public AppointmentsController(IAppointmentService appointmentService, IUserService userService)
        {
            _appointmentService = appointmentService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            List<Appointment> appointments;

            if (userRole == "Patient")
            {
                appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(userId);
            }
            else if (userRole == "Doctor")
            {
                appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(userId);
            }
            else
            {
                appointments = await _appointmentService.GetAppointmentsByDateAsync(DateTime.Today);
            }

            ViewBag.UserRole = userRole;
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get doctors for dropdown
            var doctors = await _userService.GetUsersByRoleAsync(UserRole.Doctor);
            ViewBag.Doctors = doctors;

            // Get patients for dropdown (if not a patient)
            if (userRole != "Patient")
            {
                var patients = await _userService.GetUsersByRoleAsync(UserRole.Patient);
                ViewBag.Patients = patients;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                var doctors = await _userService.GetUsersByRoleAsync(UserRole.Doctor);
                ViewBag.Doctors = doctors;
                
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole != "Patient")
                {
                    var patients = await _userService.GetUsersByRoleAsync(UserRole.Patient);
                    ViewBag.Patients = patients;
                }
                
                return View(appointment);
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRoleSession = HttpContext.Session.GetString("UserRole");
            
            // If user is a patient, set patient ID
            if (userRoleSession == "Patient")
            {
                appointment.PatientId = userId!;
            }

            appointment.CreatedBy = userId ?? "";

            // Check doctor availability
            var isAvailable = await _appointmentService.CheckDoctorAvailabilityAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.Duration);
            
            if (!isAvailable)
            {
                ModelState.AddModelError("", "Doctor is not available at the requested time");
                
                var doctors = await _userService.GetUsersByRoleAsync(UserRole.Doctor);
                ViewBag.Doctors = doctors;
                
                if (userRoleSession != "Patient")
                {
                    var patients = await _userService.GetUsersByRoleAsync(UserRole.Patient);
                    ViewBag.Patients = patients;
                }
                
                return View(appointment);
            }

            await _appointmentService.CreateAppointmentAsync(appointment);
            TempData["SuccessMessage"] = "Appointment created successfully";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, AppointmentStatus status)
        {
            var result = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            if (!result)
            {
                TempData["ErrorMessage"] = "Failed to update appointment status";
            }
            else
            {
                TempData["SuccessMessage"] = "Appointment status updated successfully";
            }

            return RedirectToAction("Index");
        }
    }
}
