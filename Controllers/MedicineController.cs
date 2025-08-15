using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class MedicineController : Controller
    {
        private readonly IMedicineService _medicineService;

        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        // İlaç listesi
        public async Task<IActionResult> Index()
        {
            var medicines = await _medicineService.GetAllMedicinesAsync();
            return View(medicines);
        }

        // İlaç detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // Yeni ilaç oluşturma formu (Admin, Doctor)
        [AuthorizeRole("Admin", "Doctor")]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni ilaç oluşturma POST (Admin, Doctor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Create(CreateMedicineDto createMedicineDto)
        {
            if (ModelState.IsValid)
            {
                await _medicineService.CreateMedicineAsync(createMedicineDto);
                TempData["SuccessMessage"] = "İlaç başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(createMedicineDto);
        }

        // İlaç düzenleme formu (Admin, Doctor)
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // İlaç düzenleme POST (Admin, Doctor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Edit(string id, UpdateMedicineDto updateMedicineDto)
        {
            if (ModelState.IsValid)
            {
                await _medicineService.UpdateMedicineAsync(id, updateMedicineDto);
                TempData["SuccessMessage"] = "İlaç başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(updateMedicineDto);
        }

        // İlaç silme (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            await _medicineService.DeleteMedicineAsync(id);
            TempData["SuccessMessage"] = "İlaç başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Stok güncelleme (Doctor, Nurse)
        [HttpPost]
        [AuthorizeRole("Doctor", "Nurse")]
        public async Task<IActionResult> UpdateStock(string id, int newStock)
        {
            try
            {
                await _medicineService.UpdateStockAsync(id, newStock);
                return Json(new { success = true, message = "Stok başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
