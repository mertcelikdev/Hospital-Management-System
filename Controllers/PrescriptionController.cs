using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [RequirePermission("CanManagePrescriptions")]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly IMedicationService _medicationService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IPatientService patientService,
            IUserService userService,
            IMedicationService medicationService)
        {
            _prescriptionService = prescriptionService;
            _patientService = patientService;
            _userService = userService;
            _medicationService = medicationService;
        }

        public async Task<IActionResult> Index()
        {
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
            return View(prescriptions);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync(Role.Doctor.ToString());
            ViewBag.Medications = await _medicationService.GetAllMedicationsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            if (ModelState.IsValid)
            {
                prescription.CreatedAt = DateTime.UtcNow;
                prescription.UpdatedAt = DateTime.UtcNow;
                
                await _prescriptionService.CreatePrescriptionAsync(prescription);
                TempData["SuccessMessage"] = "Prescription created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync(Role.Doctor.ToString());
            ViewBag.Medications = await _medicationService.GetAllMedicationsAsync();
            return View(prescription);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync(Role.Doctor.ToString());
            ViewBag.Medications = await _medicationService.GetAllMedicationsAsync();
            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Prescription prescription)
        {
            if (id != prescription.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                prescription.UpdatedAt = DateTime.UtcNow;
                await _prescriptionService.UpdatePrescriptionAsync(id, prescription);
                TempData["SuccessMessage"] = "Prescription updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync(Role.Doctor.ToString());
            ViewBag.Medications = await _medicationService.GetAllMedicationsAsync();
            return View(prescription);
        }

    // Interface'te olmayan aksiyonlar kaldırıldı.
    }
}

