using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [RequirePermission("CanManageDepartments")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;

        public DepartmentController(IDepartmentService departmentService, IUserService userService)
        {
            _departmentService = departmentService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return View(departments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departmanlar yüklenirken hata oluştu: " + ex.Message;
                return View(new List<Department>());
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
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }

                var staff = await _departmentService.GetDepartmentStaffAsync(id);
                ViewBag.Staff = staff;
                ViewBag.StaffCount = await _departmentService.GetDepartmentStaffCountAsync(id);

                return View(department);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departman detayları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    department.Id = Guid.NewGuid().ToString();
                    department.CreatedAt = DateTime.Now;
                    await _departmentService.CreateDepartmentAsync(department);
                    TempData["Success"] = "Departman başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Departman oluşturulurken hata oluştu: " + ex.Message;
                }
            }

            return View(department);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }

                return View(department);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departman yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

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
                try
                {
                    department.UpdatedAt = DateTime.Now;
                    await _departmentService.UpdateDepartmentAsync(id, department);
                    TempData["Success"] = "Departman başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Departman güncellenirken hata oluştu: " + ex.Message;
                }
            }

            return View(department);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }

                return View(department);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departman yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _departmentService.DeleteDepartmentAsync(id);
                TempData["Success"] = "Departman başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Departman silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("CanManageStaff")]
        public async Task<IActionResult> AssignStaff(string departmentId)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(departmentId);
                if (department == null)
                {
                    return NotFound();
                }

                var allUsers = await _userService.GetAllUsersAsync();
                var departmentStaff = await _departmentService.GetDepartmentStaffAsync(departmentId);
                
                ViewBag.Department = department;
                ViewBag.DepartmentStaff = departmentStaff;
                ViewBag.AvailableUsers = allUsers.Where(u => !departmentStaff.Any(ds => ds.Id == u.Id)).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Personel atama sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [RequirePermission("CanManageStaff")]
        public async Task<IActionResult> AssignStaff(string departmentId, string staffId)
        {
            try
            {
                var result = await _departmentService.AssignStaffToDepartmentAsync(departmentId, staffId);
                if (result)
                {
                    TempData["Success"] = "Personel başarıyla departmana atandı.";
                }
                else
                {
                    TempData["Error"] = "Personel atanamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Personel atanırken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(AssignStaff), new { departmentId });
        }

        [HttpPost]
        [RequirePermission("CanManageStaff")]
        public async Task<IActionResult> RemoveStaff(string departmentId, string staffId)
        {
            try
            {
                var result = await _departmentService.RemoveStaffFromDepartmentAsync(departmentId, staffId);
                if (result)
                {
                    TempData["Success"] = "Personel başarıyla departmandan çıkarıldı.";
                }
                else
                {
                    TempData["Error"] = "Personel çıkarılamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Personel çıkarılırken hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(AssignStaff), new { departmentId });
        }
    }
}
