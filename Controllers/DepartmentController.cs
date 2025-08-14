using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // Departman listesi
        public async Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return View(departments);
        }

        // Departman detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // Yeni departman oluşturma formu
        public IActionResult Create()
        {
            return View();
        }

        // Yeni departman oluşturma POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                department.Id = null; // MongoDB otomatik ID oluşturacak
                department.CreatedAt = DateTime.UtcNow;
                department.IsActive = true;

                await _departmentService.CreateDepartmentAsync(department);
                TempData["SuccessMessage"] = "Departman başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // Departman düzenleme formu
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // Departman düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _departmentService.UpdateDepartmentAsync(id, department);
                TempData["SuccessMessage"] = "Departman başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // Departman silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            await _departmentService.DeleteDepartmentAsync(id);
            TempData["SuccessMessage"] = "Departman başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Departman durumu değiştirme (Aktif/Pasif)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Geçersiz departman ID" });
            }

            try
            {
                await _departmentService.ToggleDepartmentStatusAsync(id);
                return Json(new { success = true, message = "Departman durumu güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
