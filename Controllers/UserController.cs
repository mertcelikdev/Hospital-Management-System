using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;

        public UserController(IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
        }

        // Geçici yardımcı: Doktoru departmana ata (Admin)
        [HttpPost]
        public async Task<IActionResult> AssignDepartment(string doctorName, string departmentName)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role != "Admin") return Forbid();
            if(string.IsNullOrWhiteSpace(doctorName) || string.IsNullOrWhiteSpace(departmentName)) return BadRequest("Parametreler gerekli");
            var users = await _userService.GetAllUsersAsync();
            var doctor = users.FirstOrDefault(u => u.Role == "Doctor" && u.Name.Contains(doctorName, StringComparison.OrdinalIgnoreCase));
            if(doctor == null) return NotFound("Doktor bulunamadı");
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var dep = departments.FirstOrDefault(d => d.Name.Contains(departmentName, StringComparison.OrdinalIgnoreCase));
            if(dep == null) return NotFound("Departman bulunamadı");
            var update = new UpdateUserDto { Name = doctor.Name, Email = doctor.Email, Phone = doctor.Phone, DepartmentId = dep.Id };
            await _userService.UpdateUserAsync(doctor.Id!, update);
            return Ok(new { success = true, message = $"{doctor.Name} => {dep.Name}" });
        }

        // Kullanıcı listesi (Admin ve Staff)
        [AuthorizeRole("Admin", "Staff")]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // Hasta listesi (Admin, Doctor, Nurse, Staff)
        [AuthorizeRole("Admin", "Doctor", "Nurse", "Staff")]
        public async Task<IActionResult> Patients()
        {
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            ViewBag.CurrentUserRole = userRole;
            
            return View(patients);
        }

        // Kullanıcı detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Yeni kullanıcı oluşturma formu (Admin, Staff, Nurse)
        [AuthorizeRole("Admin", "Staff", "Nurse")]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kullanıcı oluşturma POST (Admin, Doctor, Staff)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor", "Staff")]
        public async Task<IActionResult> Create(CreateUserDto createUserDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.CreateUserAsync(createUserDto);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(createUserDto);
        }

        // Kullanıcı düzenleme formu
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Kullanıcı düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateUserDto updateUserDto)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(id, updateUserDto);
                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(updateUserDto);
        }

        // Kullanıcı silme (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
