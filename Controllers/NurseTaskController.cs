using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;
using TaskStatus = HospitalManagementSystem.Models.TaskStatus;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [RequirePermission("CanManageNurseTasks")]
    public class NurseTaskController : Controller
    {
        private readonly INurseTaskService _nurseTaskService;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;

        public NurseTaskController(
            INurseTaskService nurseTaskService,
            IUserService userService,
            IPatientService patientService)
        {
            _nurseTaskService = nurseTaskService;
            _userService = userService;
            _patientService = patientService;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _nurseTaskService.GetAllTasksAsync();
            return View(tasks);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var task = await _nurseTaskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Nurses = await _userService.GetUsersByRoleAsync(Role.Nurse.ToString());
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NurseTask task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;
                task.Status = TaskStatus.Beklemede;
                await _nurseTaskService.CreateTaskAsync(task);
                TempData["SuccessMessage"] = "Nurse task created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Nurses = await _userService.GetUsersByRoleAsync(Role.Nurse.ToString());
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            return View(task);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var task = await _nurseTaskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Nurses = await _userService.GetUsersByRoleAsync(Role.Nurse.ToString());
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            return View(task);
        }

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
                task.UpdatedAt = DateTime.UtcNow;
                await _nurseTaskService.UpdateTaskAsync(id, task);
                TempData["SuccessMessage"] = "Nurse task updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Nurses = await _userService.GetUsersByRoleAsync(Role.Nurse.ToString());
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, TaskStatus status)
        {
            await _nurseTaskService.UpdateTaskStatusAsync(id, status);
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetTasksByNurse(string nurseId)
        {
            var tasks = await _nurseTaskService.GetTasksByNurseIdAsync(nurseId);
            return Json(tasks);
        }

        public async Task<IActionResult> GetTasksByPatient(string patientId)
        {
            var tasks = await _nurseTaskService.GetTasksByPatientIdAsync(patientId);
            return Json(tasks);
        }
    }
}

