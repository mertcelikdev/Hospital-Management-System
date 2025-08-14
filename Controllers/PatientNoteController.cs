using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [RequirePermission("CanManagePatientNotes")]
    public class PatientNoteController : Controller
    {
        private readonly IPatientNoteService _patientNoteService;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;

        public PatientNoteController(
            IPatientNoteService patientNoteService,
            IPatientService patientService,
            IUserService userService)
        {
            _patientNoteService = patientNoteService;
            _patientService = patientService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var notes = await _patientNoteService.GetAllNotesAsync();
                return View(notes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notları yüklenirken hata oluştu: " + ex.Message;
                return View(new List<PatientNote>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var note = await _patientNoteService.GetNoteByIdAsync(id);
                if (note == null)
                {
                    return NotFound();
                }

                var patient = await _patientService.GetPatientByIdAsync(note.PatientId);
                var doctor = await _userService.GetUserByIdAsync(note.CreatedBy);

                ViewBag.Patient = patient;
                ViewBag.Doctor = doctor;

                return View(note);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notu detayları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");

                ViewBag.Patients = patients;
                ViewBag.Doctors = doctors;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notu oluşturma sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientNote note)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    note.Id = Guid.NewGuid().ToString();
                    note.CreatedAt = DateTime.Now;
                    note.CreatedBy = HttpContext.Session.GetString("UserId") ?? note.CreatedBy;

                    await _patientNoteService.CreateNoteAsync(note);
                    TempData["Success"] = "Hasta notu başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Hasta notu oluşturulurken hata oluştu: " + ex.Message;
                }
            }

            // Hata durumunda dropdown listelerini yeniden yükle
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");

                ViewBag.Patients = patients;
                ViewBag.Doctors = doctors;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Dropdown listeler yüklenirken hata oluştu: " + ex.Message;
            }

            return View(note);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var note = await _patientNoteService.GetNoteByIdAsync(id);
                if (note == null)
                {
                    return NotFound();
                }

                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");

                ViewBag.Patients = patients;
                ViewBag.Doctors = doctors;

                return View(note);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notu yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

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
                try
                {
                    note.UpdatedAt = DateTime.Now;
                    await _patientNoteService.UpdateNoteAsync(id, note);
                    TempData["Success"] = "Hasta notu başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Hasta notu güncellenirken hata oluştu: " + ex.Message;
                }
            }

            // Hata durumunda dropdown listelerini yeniden yükle
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");

                ViewBag.Patients = patients;
                ViewBag.Doctors = doctors;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Dropdown listeler yüklenirken hata oluştu: " + ex.Message;
            }

            return View(note);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var note = await _patientNoteService.GetNoteByIdAsync(id);
                if (note == null)
                {
                    return NotFound();
                }

                var patient = await _patientService.GetPatientByIdAsync(note.PatientId);
                var doctor = await _userService.GetUserByIdAsync(note.CreatedBy);

                ViewBag.Patient = patient;
                ViewBag.Doctor = doctor;

                return View(note);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notu yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _patientNoteService.DeleteNoteAsync(id);
                TempData["Success"] = "Hasta notu başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notu silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ByPatient(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return NotFound();
            }

            try
            {
                var notes = await _patientNoteService.GetNotesByPatientIdAsync(patientId);
                var patient = await _patientService.GetPatientByIdAsync(patientId);

                ViewBag.Patient = patient;
                ViewBag.NotesCount = await _patientNoteService.GetNotesCountByPatientAsync(patientId);

                return View(notes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta notları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ByDoctor(string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return NotFound();
            }

            try
            {
                var notes = await _patientNoteService.GetNotesByDoctorIdAsync(doctorId);
                var doctor = await _userService.GetUserByIdAsync(doctorId);

                ViewBag.Doctor = doctor;
                ViewBag.NotesCount = await _patientNoteService.GetNotesCountByDoctorAsync(doctorId);

                return View(notes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Doktor notları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var notes = await _patientNoteService.SearchNotesAsync(searchTerm ?? "");
                return View("Index", notes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Arama yapılırken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
