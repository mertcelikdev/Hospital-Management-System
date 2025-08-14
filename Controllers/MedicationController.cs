using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    // İlaç yönetimi: sadece Admin ve Nurse (takip) erişsin (örnek basit yaklaşım)
    [Authorize]
    [AuthorizeRole("Admin","Nurse","Doctor")] // Doktor görüntüleyebilir, Admin tam yetkili
    public class MedicationController : Controller
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        public async Task<IActionResult> Index()
        {
            var medications = await _medicationService.GetAllMedicationsAsync();
            return View(medications);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medication medication)
        {
            if (ModelState.IsValid)
            {
                medication.CreatedAt = DateTime.UtcNow;
                medication.UpdatedAt = DateTime.UtcNow;
                
                await _medicationService.CreateMedicationAsync(medication);
                TempData["SuccessMessage"] = "Medication created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(medication);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Medication medication)
        {
            if (id != medication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                medication.UpdatedAt = DateTime.UtcNow;
                await _medicationService.UpdateMedicationAsync(id, medication);
                TempData["SuccessMessage"] = "Medication updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(medication);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var medication = await _medicationService.GetMedicationByIdAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _medicationService.DeleteMedicationAsync(id);
            TempData["SuccessMessage"] = "Medication deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var medications = await _medicationService.SearchMedicationsAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", medications);
        }
    }
}

