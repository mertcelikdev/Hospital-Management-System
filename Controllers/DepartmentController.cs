using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin","Doctor","Staff")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;

        public DepartmentController(IDepartmentService departmentService, IUserService userService)
        {
            _departmentService = departmentService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Doctors(string departmentId)
        {
            if(string.IsNullOrWhiteSpace(departmentId)) return Json(new { success = false, message = "departmentId gerekli" });
            try
            {
                var doctors = await _departmentService.GetDepartmentUsersAsync(departmentId, "Doctor");
                var data = doctors.Select(u => new { id = u.Id, name = u.Name, role = u.Role });
                return Json(new { success = true, data });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
    public async Task<IActionResult> Create(CreateDepartmentDto department)
        {
            if (ModelState.IsValid)
            {
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
    public async Task<IActionResult> Edit(string id, UpdateDepartmentDto department)
        {
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

    // IsActive / durum yönetimi kaldırıldığı için ToggleStatus aksiyonu silindi
    }
}
