using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [AuthorizeRole("Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public UserController(IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

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

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;
                
                await _userService.CreateUserAsync(user);
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(user);
        }

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

            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(id, user);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _userService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var users = await _userService.SearchUsersAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", users);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByRole(Role role)
        {
            var users = await _userService.GetUsersByRoleAsync(role.ToString());
            return Json(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorsByDepartment(string departmentId)
        {
            var users = await _userService.GetUsersByDepartmentAsync(departmentId);
            var doctors = users.Where(u => u.Role == Role.Doctor.ToString()).ToList();
            return Json(doctors);
        }
    }
}

