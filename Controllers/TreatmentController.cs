using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse")]
    public class TreatmentController : Controller
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;

        public TreatmentController(ITreatmentService treatmentService, IUserService userService, IPatientService patientService)
        {
            _treatmentService = treatmentService;
            _userService = userService;
            _patientService = patientService;
        }

        public async Task<IActionResult> Index(string? patientId)
        {
            List<TreatmentDto> list;
            if (!string.IsNullOrEmpty(patientId))
                list = await _treatmentService.GetPatientTreatmentsAsync(patientId);
            else
                list = await _treatmentService.GetAllTreatmentsAsync();
            ViewBag.SelectedPatientId = patientId;
            return View(list);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var treatment = await _treatmentService.GetTreatmentByIdAsync(id);
            return treatment == null ? NotFound() : View(treatment);
        }

        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Create(string? patientId)
        {
            await LoadFormDataAsync();
            var model = new CreateTreatmentDto { PatientId = patientId ?? string.Empty };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Create(CreateTreatmentDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync();
                return View(model);
            }
            var created = await _treatmentService.CreateTreatmentAsync(model);
            TempData["SuccessMessage"] = "Tedavi eklendi";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Edit(string id)
        {
            var treatment = await _treatmentService.GetTreatmentByIdAsync(id);
            if (treatment == null) return NotFound();
            await LoadFormDataAsync();
            var update = new UpdateTreatmentDto
            {
                Description = treatment.Description,
                Notes = treatment.Notes,
                StartDate = treatment.StartDate,
                EndDate = treatment.EndDate,
                Status = treatment.Status
            };
            ViewBag.TreatmentId = id;
            return View(update);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Edit(string id, UpdateTreatmentDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync();
                ViewBag.TreatmentId = id;
                return View(model);
            }
            await _treatmentService.UpdateTreatmentAsync(id, model);
            TempData["SuccessMessage"] = "Tedavi güncellendi";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [AuthorizeRole("Admin", "Doctor", "Nurse")]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            if (!Enum.TryParse<TreatmentStatus>(status, true, out var enumStatus))
            {
                return Json(new { success = false, message = "Geçersiz durum" });
            }
            await _treatmentService.UpdateTreatmentStatusAsync(id, enumStatus);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Delete(string id)
        {
            await _treatmentService.DeleteTreatmentAsync(id);
            TempData["SuccessMessage"] = "Tedavi silindi";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFormDataAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Patients = patients;
            ViewBag.Nurses = users.Where(u => u.Role == "Nurse").ToList();
            ViewBag.Doctors = users.Where(u => u.Role == "Doctor").ToList();
        }
    }
}
