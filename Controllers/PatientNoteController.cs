using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse")]
    public class PatientNoteController : Controller
    {
    private readonly IPatientNoteService _patientNoteService;
    private readonly IUserService _userService;

    public PatientNoteController(IPatientNoteService patientNoteService, IUserService userService)
        {
            _patientNoteService = patientNoteService;
            _userService = userService;
        }

        // Hasta notları listesi
        public async Task<IActionResult> Index(string? patientId)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }

            List<PatientNoteDto> notes;

            if (!string.IsNullOrEmpty(patientId))
            {
                notes = await _patientNoteService.GetNotesByPatientIdAsync(patientId);
            }
            else
            {
                notes = await _patientNoteService.GetNotesByCreatorAsync(userId);
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

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var note = await _patientNoteService.GetNoteByIdAsync(id);
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
            await PopulateCreateViewBagsAsync(HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value!);
            return View();
        }

        // Yeni hasta notu oluşturma POST
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientNoteDto model)
        {
            if (ModelState.IsValid)
            {
    var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var created = await _patientNoteService.CreateNoteAsync(model, userId);
                TempData["SuccessMessage"] = "Hasta notu başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index), new { patientId = model.PatientId });
            }

            await PopulateCreateViewBagsAsync(HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value!);
        return View(model);
        }

        // Hasta notu düzenleme formu
    public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var note = await _patientNoteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            ViewBag.Note = note;
            await PopulateCreateViewBagsAsync(userRole!);
                var updateModel = new UpdatePatientNoteDto
                {
                    Content = note.Content,
                    Category = note.Category,
                    IsUrgent = note.IsUrgent,
                    FollowUpDate = note.FollowUpDate
                };
            return View(updateModel);
        }

        // Hasta notu düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdatePatientNoteDto model)
        {
            if (ModelState.IsValid)
            {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var updated = await _patientNoteService.UpdateNoteAsync(id, model, userId);
        if (updated != null)
                {
                    TempData["SuccessMessage"] = "Hasta notu başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Nota güncelleme yetkisi yok.";
                }
            }

            await PopulateCreateViewBagsAsync(HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value!);
        return View(model);
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

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var note = await _patientNoteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            var success = await _patientNoteService.DeleteNoteAsync(id);
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

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var notes = await _patientNoteService.SearchNotesAsync(new BaseSearchDto { SearchTerm = searchTerm });

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

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var notes = await _patientNoteService.GetNotesByPatientIdAsync(patientId);
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
                    ViewBag.Patients = new List<UserDto>();
                    break;
            }
        }
    }
}
