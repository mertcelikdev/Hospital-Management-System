
using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse")]
    public class NurseTaskController : Controller
    {
    private readonly INurseTaskService _nurseTaskService;
        private readonly IUserService _userService;

    public NurseTaskController(INurseTaskService nurseTaskService, IUserService userService)
        {
            _nurseTaskService = nurseTaskService;
            _userService = userService;
        }

        // Hemşire görevleri listesi
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }

            List<NurseTaskDto> tasks;

            switch (userRole)
            {
                case "Admin":
                    tasks = await _nurseTaskService.GetAllTasksAsync();
                    break;
                case "Doctor":
                    tasks = await _nurseTaskService.GetTasksByDoctorIdAsync(userId);
                    break;
                case "Nurse":
                    tasks = await _nurseTaskService.GetTasksByAssignedNurseAsync(userId);
                    break;
                default:
                    tasks = new List<NurseTaskDto>();
                    break;
            }

            ViewBag.UserRole = userRole;
            return View(tasks);
        }

        // Görev detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var task = await _nurseTaskService.GetTaskByIdAsync(id, userId!, userRole!);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // Yeni görev oluşturma formu (Admin, Doctor)
        [AuthorizeRole("Admin", "Doctor")]
        public async Task<IActionResult> Create()
        {
            var nurses = await _userService.GetUsersByRoleAsync("Nurse");
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            ViewBag.Nurses = nurses;
            ViewBag.Patients = patients;
            return View();
        }

        // Yeni görev oluşturma POST (Admin, Doctor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Doctor")]
    public async Task<IActionResult> Create(CreateNurseTaskDto task)
        {
            if (ModelState.IsValid)
            {
    var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _nurseTaskService.CreateTaskAsync(task, userId!);
                TempData["SuccessMessage"] = "Görev başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            var nurses = await _userService.GetUsersByRoleAsync("Nurse");
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            ViewBag.Nurses = nurses;
            ViewBag.Patients = patients;
            return View(task);
        }

        // Görev düzenleme formu
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var task = await _nurseTaskService.GetTaskByIdAsync(id, userId!, userRole!);
            if (task == null)
            {
                return NotFound();
            }

            // Sadece hemşire kendi görevini düzenleyebilir veya admin/doktor tüm görevleri
            if (userRole == "Nurse" && task.AssignedToId != userId)
            {
                return Forbid();
            }

            var nurses = await _userService.GetUsersByRoleAsync("Nurse");
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            ViewBag.Nurses = nurses;
            ViewBag.Patients = patients;
            return View(task);
        }

        // Görev düzenleme POST
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateNurseTaskDto task)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
    var updated = await _nurseTaskService.UpdateTaskAsync(id, task, userId!, userRole!);
        if (updated != null)
                {
                    TempData["SuccessMessage"] = "Görev başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Görev güncelleme yetkisi yok.";
                }
            }

            var nurses = await _userService.GetUsersByRoleAsync("Nurse");
            var patients = await _userService.GetUsersByRoleAsync("Patient");
            
            ViewBag.Nurses = nurses;
            ViewBag.Patients = patients;
            return View(task);
        }

        // Görev durumu güncelleme
        [HttpPost]
    public async Task<IActionResult> UpdateStatus(string id, TaskStatusUpdateDto statusDto)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            var success = await _nurseTaskService.UpdateTaskStatusAsync(id, statusDto, userId!);
            
            if (success)
            {
                return Json(new { success = true, message = "Görev durumu güncellendi" });
            }
            else
            {
                return Json(new { success = false, message = "Görev durumu güncellenemedi" });
            }
        }

        // Görev silme (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            var success = await _nurseTaskService.DeleteTaskAsync(id, userId!, userRole!);
            if (success)
            {
                TempData["SuccessMessage"] = "Görev başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Görev silme yetkisi yok.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Hemşire için görevler (Nurse)
        [AuthorizeRole("Nurse")]
    public async Task<IActionResult> MyTasks()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var tasks = await _nurseTaskService.GetTasksByAssignedNurseAsync(userId!);
            return View("Index", tasks);
        }

        // Doktor için oluşturduğu görevler (Doctor)
        [AuthorizeRole("Doctor")]
    public async Task<IActionResult> MyCreatedTasks()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var tasks = await _nurseTaskService.GetTasksByDoctorIdAsync(userId!);
            return View("Index", tasks);
        }

        // Görev filtreleme
        public async Task<IActionResult> FilterTasks(Models.TaskStatus? status, TaskPriority? priority, string? nurseId)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            List<NurseTaskDto> tasks = userRole switch
            {
                "Admin" => await _nurseTaskService.GetAllTasksAsync(),
                "Doctor" => await _nurseTaskService.GetTasksByDoctorIdAsync(userId!),
                "Nurse" => await _nurseTaskService.GetTasksByAssignedNurseAsync(userId!),
                _ => new List<NurseTaskDto>()
            };

            // Filtreleme
            if (status.HasValue)
            {
                tasks = tasks.Where(t => string.Equals(t.Status, status.Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (priority.HasValue)
            {
                tasks = tasks.Where(t => string.Equals(t.Priority, priority.Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(nurseId))
            {
                tasks = tasks.Where(t => t.AssignedToId == nurseId).ToList();
            }

            ViewBag.UserRole = userRole;
            return View("Index", tasks);
        }
    }
}
