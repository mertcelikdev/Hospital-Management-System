using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse")]
    public class PatientNoteController : Controller
    {
        private readonly PatientNoteService _patientNoteService;
        private readonly IUserService _userService;

        public PatientNoteController(PatientNoteService patientNoteService, IUserService userService)
        {
            _patientNoteService = patientNoteService;
            _userService = userService;
        }

        // Hasta notları listesi
        public async Task<IActionResult> Index(string? patientId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }

            List<PatientNote> notes;

            if (!string.IsNullOrEmpty(patientId))
            {
                notes = await _patientNoteService.GetNotesByPatientAndCreatorAsync(patientId, userId, userRole);
            }
            else
            {
                notes = await _patientNoteService.GetNotesByCreatorAsync(userId, userRole);
            }

            ViewBag.PatientId = patientId;
            return View(notes);
        }

        // Hasta notu detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var note = await _patientNoteService.GetNoteByIdAsync(id, userId!, userRole!);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // Yeni hasta notu oluşturma formu
        public async Task<IActionResult> Create(string? patientId)
        {
            ViewBag.PatientId = patientId;
            await PopulateCreateViewBagsAsync(HttpContext.Session.GetString("UserRole")!);
            return View();
        }

        // Yeni hasta notu oluşturma POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientNote note)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                note.Id = null; // MongoDB otomatik ID oluşturacak
                note.CreatedAt = DateTime.UtcNow;
                note.CreatedByUserName = HttpContext.Session.GetString("UserName") ?? "";
                
                await _patientNoteService.CreateNoteAsync(note);
                TempData["SuccessMessage"] = "Hasta notu başarıyla oluşturuldu.";
                
                if (!string.IsNullOrEmpty(note.PatientId))
                {
                    return RedirectToAction(nameof(Index), new { patientId = note.PatientId });
                }
                
                return RedirectToAction(nameof(Index));
            }

            await PopulateCreateViewBagsAsync(HttpContext.Session.GetString("UserRole")!);
            return View(note);
        }

        // Hasta notu düzenleme formu
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var note = await _patientNoteService.GetNoteByIdAsync(id, userId!, userRole!);
            if (note == null)
            {
                return NotFound();
            }

            ViewBag.Note = note;
            await PopulateCreateViewBagsAsync(userRole!);
            return View(note);
        }

        // Hasta notu düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PatientNote note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                var success = await _patientNoteService.UpdateNoteAsync(id, note, userId!, userRole!);
                if (success)
                {
                    TempData["SuccessMessage"] = "Hasta notu başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Nota güncelleme yetkisi yok.";
                }
            }

            await PopulateCreateViewBagsAsync(HttpContext.Session.GetString("UserRole")!);
            return View(note);
        }

        // Hasta notu silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var note = await _patientNoteService.GetNoteByIdAsync(id, userId!, userRole!);
            if (note == null)
            {
                return NotFound();
            }

            var success = await _patientNoteService.DeleteNoteAsync(id, userId!, userRole!);
            if (success)
            {
                TempData["SuccessMessage"] = "Hasta notu başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nota silme yetkisi yok.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Hasta notu arama
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm, string? patientId)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index), new { patientId });
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var notes = await _patientNoteService.SearchNotesAsync(searchTerm, userId!, userRole!, patientId);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.PatientId = patientId;
            return View("Index", notes);
        }

        // Hasta seçim için hastalar listesi
        public async Task<IActionResult> SelectPatient()
        {
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            return View(patients);
        }

        // Hastaya ait notlar
        public async Task<IActionResult> PatientNotes(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var notes = await _patientNoteService.GetNotesByPatientAndCreatorAsync(patientId, userId!, userRole!);
            var patient = await _userService.GetUserByIdAsync(patientId);

            ViewBag.Patient = patient;
            return View("Index", notes);
        }

        private async Task PopulateCreateViewBagsAsync(string userRole)
        {
            // Kullanıcının rolüne göre hasta listesi
            switch (userRole)
            {
                case "Admin":
                    ViewBag.Patients = await _userService.GetUsersByRoleAsync("Patient");
                    break;
                case "Doctor":
                    // Doktor sadece kendi hastalarını görebilir
                    ViewBag.Patients = await _userService.GetUsersByRoleAsync("Patient");
                    break;
                case "Nurse":
                    // Hemşire sadece kendi hastalarını görebilir
                    ViewBag.Patients = await _userService.GetUsersByRoleAsync("Patient");
                    break;
                default:
                    ViewBag.Patients = new List<User>();
                    break;
            }
        }
    }
}
