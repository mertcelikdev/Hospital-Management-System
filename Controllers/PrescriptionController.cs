using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Controllers
{
	[AuthorizeRole("Admin", "Doctor", "Nurse")]
	public class PrescriptionController : Controller
	{
		private readonly IPrescriptionService _prescriptionService;
		private readonly IUserService _userService;
		private readonly IMedicineService _medicineService;

		public PrescriptionController(IPrescriptionService prescriptionService, IUserService userService, IMedicineService medicineService)
		{
			_prescriptionService = prescriptionService;
			_userService = userService;
			_medicineService = medicineService;
		}

		// Liste
		public async Task<IActionResult> Index(string? patientId)
		{
			List<PrescriptionDto> list;
			if (!string.IsNullOrEmpty(patientId))
				list = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
			else
				list = await _prescriptionService.GetAllPrescriptionsAsync();

			ViewBag.SelectedPatientId = patientId;
			return View(list);
		}

		// Detay
		public async Task<IActionResult> Details(string id)
		{
			if (string.IsNullOrEmpty(id)) return NotFound();
			var pres = await _prescriptionService.GetPrescriptionByIdAsync(id);
			return pres == null ? NotFound() : View(pres);
		}

		// Oluştur GET (Doctor, Admin)
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> Create(string? patientId)
		{
			await LoadFormDataAsync();
			var model = new CreatePrescriptionDto { PatientId = patientId ?? string.Empty };
			return View(model);
		}

		// Oluştur POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> Create(CreatePrescriptionDto model)
		{
			if (!ModelState.IsValid)
			{
				await LoadFormDataAsync();
				return View(model);
			}

			var created = await _prescriptionService.CreatePrescriptionAsync(model);
			TempData["SuccessMessage"] = "Reçete oluşturuldu";
			return RedirectToAction(nameof(Details), new { id = created.Id });
		}

		// Düzenle GET
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> Edit(string id)
		{
			var pres = await _prescriptionService.GetPrescriptionByIdAsync(id);
			if (pres == null) return NotFound();
			await LoadFormDataAsync();
			var update = new UpdatePrescriptionDto
			{
				Diagnosis = pres.Diagnosis,
				Notes = pres.Notes,
				NurseId = pres.NurseId,
				StartDate = pres.StartDate,
				EndDate = pres.EndDate
			};
			ViewBag.PrescriptionId = id;
			return View(update);
		}

		// Düzenle POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> Edit(string id, UpdatePrescriptionDto model)
		{
			if (!ModelState.IsValid)
			{
				await LoadFormDataAsync();
				ViewBag.PrescriptionId = id;
				return View(model);
			}
			await _prescriptionService.UpdatePrescriptionAsync(id, model);
			TempData["SuccessMessage"] = "Reçete güncellendi";
			return RedirectToAction(nameof(Details), new { id });
		}

		// Durum Güncelle
		[HttpPost]
		[AuthorizeRole("Admin", "Doctor", "Nurse")]
		public async Task<IActionResult> UpdateStatus(string id, string status)
		{
			if (Enum.TryParse<PrescriptionStatus>(status, out var enumStatus))
			{
				await _prescriptionService.UpdatePrescriptionStatusAsync(id, enumStatus);
				return Json(new { success = true });
			}
			return Json(new { success = false, message = "Geçersiz durum" });
		}

		// Hemşire ata
		[HttpPost]
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> AssignNurse(string id, string nurseId)
		{
			await _prescriptionService.AssignNurseAsync(id, nurseId);
			return RedirectToAction(nameof(Details), new { id });
		}

		// Sil (Admin, Doctor)
		[HttpPost]
		[ValidateAntiForgeryToken]
		[AuthorizeRole("Admin", "Doctor")]
		public async Task<IActionResult> Delete(string id)
		{
			await _prescriptionService.DeletePrescriptionAsync(id);
			TempData["SuccessMessage"] = "Reçete silindi";
			return RedirectToAction(nameof(Index));
		}

		private async Task LoadFormDataAsync()
		{
			var users = await _userService.GetAllUsersAsync();
			var medicines = await _medicineService.GetAllMedicinesAsync();
			ViewBag.Patients = users.Where(u => u.Role == "Patient").ToList();
			ViewBag.Nurses = users.Where(u => u.Role == "Nurse").ToList();
			ViewBag.Medicines = medicines;
		}
	}
}
