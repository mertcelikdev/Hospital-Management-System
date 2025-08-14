using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;

        public AppointmentController(IAppointmentService appointmentService, IUserService userService)
        {
            _appointmentService = appointmentService;
            _userService = userService;
        }

        // Randevuları listele
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            
            // Her randevu için doktor ve hasta bilgilerini yükle
            foreach (var appointment in appointments)
            {
                appointment.Doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
                appointment.Patient = await _userService.GetUserByIdAsync(appointment.PatientId);
            }

            return View(appointments);
        }

        // Randevu detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Doktor ve hasta bilgilerini yükle
            appointment.Doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
            appointment.Patient = await _userService.GetUserByIdAsync(appointment.PatientId);

            return View(appointment);
        }

        // Yeni randevu oluşturma formu (Sadece Doktor ve Staff)
        [AuthorizeRole("Doctor", "Staff")]
        public async Task<IActionResult> Create()
        {
            var users = await _userService.GetAllUsersAsync();
            ViewBag.Doctors = users.Where(u => u.Role == "Doctor").ToList();
            ViewBag.Patients = users.Where(u => u.Role == "Patient").ToList();
            return View();
        }

        // Yeni randevu oluşturma POST (Sadece Doktor ve Staff)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Doctor", "Staff")]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                appointment.Id = null; // MongoDB otomatik ID oluşturacak
                appointment.CreatedAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.Status = AppointmentStatus.Planlandı; // Varsayılan durum
                
                await _appointmentService.CreateAppointmentAsync(appointment);
                TempData["SuccessMessage"] = "Randevu başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            // Model validation başarısız olursa, dropdown listelerini tekrar yükle
            var users = await _userService.GetAllUsersAsync();
            ViewBag.Doctors = users.Where(u => u.Role == "Doctor").ToList();
            ViewBag.Patients = users.Where(u => u.Role == "Patient").ToList();
            return View(appointment);
        }

        // Randevu iptal etme (Sadece Doktor ve Staff)
        [HttpPost]
        [AuthorizeRole("Doctor", "Staff")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                await _appointmentService.UpdateAppointmentStatusAsync(id, AppointmentStatus.İptalEdildi);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Randevu iptal edilirken hata oluştu: {ex.Message}");
            }
        }
    }
}
