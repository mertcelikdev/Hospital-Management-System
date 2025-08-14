using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;
using System.Linq;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Staff","Doctor","Admin")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        public async Task<IActionResult> Index()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var patientDtos = patients.Select(p => new HospitalManagementSystem.DTOs.PatientDto
            {
                Id = p.Id ?? "",
                FirstName = p.FirstName,
                LastName = p.LastName,
                TcNo = p.IdentityNumber,
                Email = p.Email ?? "",
                PhoneNumber = p.PhoneNumber,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender.ToString(),
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                BloodType = p.BloodType,
                Allergies = string.IsNullOrEmpty(p.Allergies) ? new List<string>() : p.Allergies.Split(',').ToList(),
                ChronicDiseases = string.IsNullOrEmpty(p.ChronicDiseases) ? new List<string>() : p.ChronicDiseases.Split(',').ToList(),
                CurrentMedications = string.IsNullOrEmpty(p.CurrentMedications) ? new List<string>() : p.CurrentMedications.Split(',').ToList(),
                InsuranceNumber = p.InsuranceNumber,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt ?? DateTime.UtcNow
            }).ToList();
            
            return View(patientDtos);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.CreatedAt = DateTime.UtcNow;
                patient.UpdatedAt = DateTime.UtcNow;
                
                await _patientService.CreatePatientAsync(patient);
                TempData["SuccessMessage"] = "Patient created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                patient.UpdatedAt = DateTime.UtcNow;
                await _patientService.UpdatePatientAsync(id, patient);
                TempData["SuccessMessage"] = "Patient updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _patientService.DeletePatientAsync(id);
            TempData["SuccessMessage"] = "Patient deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var patientsAjax = await _patientService.SearchPatientsAsync(query ?? "");
                var ajaxDtos = patientsAjax.Select(p => new HospitalManagementSystem.DTOs.PatientDto
                {
                    Id = p.Id ?? "",
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    TcNo = p.IdentityNumber,
                    Email = p.Email ?? "",
                    PhoneNumber = p.PhoneNumber,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender.ToString(),
                    Address = p.Address,
                    EmergencyContactName = p.EmergencyContactName,
                    EmergencyContactPhone = p.EmergencyContactPhone,
                    BloodType = p.BloodType,
                    Allergies = string.IsNullOrEmpty(p.Allergies) ? new List<string>() : p.Allergies.Split(',').ToList(),
                    ChronicDiseases = string.IsNullOrEmpty(p.ChronicDiseases) ? new List<string>() : p.ChronicDiseases.Split(',').ToList(),
                    CurrentMedications = string.IsNullOrEmpty(p.CurrentMedications) ? new List<string>() : p.CurrentMedications.Split(',').ToList(),
                    InsuranceNumber = p.InsuranceNumber,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? DateTime.UtcNow
                }).ToList();
                return Json(new { success = true, data = ajaxDtos });
            }
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction(nameof(Index));
            }
            var patients = await _patientService.SearchPatientsAsync(query);
            var patientDtos = patients.Select(p => new HospitalManagementSystem.DTOs.PatientDto
            {
                Id = p.Id ?? "",
                FirstName = p.FirstName,
                LastName = p.LastName,
                TcNo = p.IdentityNumber,
                Email = p.Email ?? "",
                PhoneNumber = p.PhoneNumber,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender.ToString(),
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                BloodType = p.BloodType,
                Allergies = string.IsNullOrEmpty(p.Allergies) ? new List<string>() : p.Allergies.Split(',').ToList(),
                ChronicDiseases = string.IsNullOrEmpty(p.ChronicDiseases) ? new List<string>() : p.ChronicDiseases.Split(',').ToList(),
                CurrentMedications = string.IsNullOrEmpty(p.CurrentMedications) ? new List<string>() : p.CurrentMedications.Split(',').ToList(),
                InsuranceNumber = p.InsuranceNumber,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt ?? DateTime.UtcNow
            }).ToList();
            ViewBag.SearchTerm = query;
            return View("Index", patientDtos);
        }

        [HttpGet]
        public async Task<IActionResult> LookupByTc(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc)) return Json(new { success = false, message = "TC gerekli" });
            var patient = await _patientService.GetPatientByTcNoAsync(tc);
            if (patient == null) return Json(new { success = false, message = "Hasta bulunamadı" });
            return Json(new { success = true, data = new {
                id = patient.Id,
                firstName = patient.FirstName,
                lastName = patient.LastName,
                fullName = patient.FullName,
                age = (int)((DateTime.Today - patient.DateOfBirth).TotalDays / 365.25),
                gender = patient.Gender.ToString(),
                phone = patient.PhoneNumber,
                email = patient.Email
            }});
        }

        public async Task<IActionResult> MedicalHistory(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            ViewBag.Patient = patient;
            
            // TODO: Gelecekte tıbbi geçmiş bilgileri buraya eklenecek
            // Şimdilik hasta bilgilerini gösteriyoruz
            
            return View(patient);
        }
    }
}

