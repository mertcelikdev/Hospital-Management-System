using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse")]
    public class NurseTaskController : Controller
    {
        private readonly NurseTaskService _nurseTaskService;
        private readonly IUserService _userService;

        public NurseTaskController(NurseTaskService nurseTaskService, IUserService userService)
        {
            _nurseTaskService = nurseTaskService;
            _userService = userService;
        }

        // Hemşire görevleri listesi
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }

            List<NurseTask> tasks;

            switch (userRole)
            {
                case "Admin":
                    tasks = await _nurseTaskService.GetAllTasksAsync();
                    break;
                case "Doctor":
                    tasks = await _nurseTaskService.GetTasksByDoctorIdAsync(userId);
                    break;
                case "Nurse":
                    tasks = await _nurseTaskService.GetTasksByNurseIdAsync(userId);
                    break;
                default:
                    tasks = new List<NurseTask>();
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

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

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
        public async Task<IActionResult> Create(NurseTask task)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                task.Id = null; // MongoDB otomatik ID oluşturacak
                task.CreatedAt = DateTime.UtcNow;
                task.Status = Models.TaskStatus.Beklemede;
                
                await _nurseTaskService.CreateTaskAsync(task);
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

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var task = await _nurseTaskService.GetTaskByIdAsync(id, userId!, userRole!);
            if (task == null)
            {
                return NotFound();
            }

            // Sadece hemşire kendi görevini düzenleyebilir veya admin/doktor tüm görevleri
            if (userRole == "Nurse" && task.AssignedToNurseId != userId)
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
        public async Task<IActionResult> Edit(string id, NurseTask task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                var success = await _nurseTaskService.UpdateTaskAsync(id, task, userId!, userRole!);
                if (success)
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
        public async Task<IActionResult> UpdateStatus(string id, Models.TaskStatus status, string? notes)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var success = await _nurseTaskService.UpdateTaskStatusAsync(id, status, userId!, userRole!, notes);
            
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

            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

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
            var userId = HttpContext.Session.GetString("UserId");
            var tasks = await _nurseTaskService.GetTasksByNurseIdAsync(userId!);
            return View("Index", tasks);
        }

        // Doktor için oluşturduğu görevler (Doctor)
        [AuthorizeRole("Doctor")]
        public async Task<IActionResult> MyCreatedTasks()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var tasks = await _nurseTaskService.GetTasksByDoctorIdAsync(userId!);
            return View("Index", tasks);
        }

        // Görev filtreleme
        public async Task<IActionResult> FilterTasks(Models.TaskStatus? status, TaskPriority? priority, string? nurseId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            List<NurseTask> tasks = userRole switch
            {
                "Admin" => await _nurseTaskService.GetAllTasksAsync(),
                "Doctor" => await _nurseTaskService.GetTasksByDoctorIdAsync(userId!),
                "Nurse" => await _nurseTaskService.GetTasksByNurseIdAsync(userId!),
                _ => new List<NurseTask>()
            };

            // Filtreleme
            if (status.HasValue)
            {
                tasks = tasks.Where(t => t.Status == status.Value).ToList();
            }

            if (priority.HasValue)
            {
                tasks = tasks.Where(t => t.Priority == priority.Value).ToList();
            }

            if (!string.IsNullOrEmpty(nurseId))
            {
                tasks = tasks.Where(t => t.AssignedToNurseId == nurseId).ToList();
            }

            ViewBag.UserRole = userRole;
            return View("Index", tasks);
        }
    }
}
