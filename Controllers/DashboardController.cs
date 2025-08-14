using HospitalManagementSystem.Attributes;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Doctor", "Nurse", "Receptionist")]
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        private readonly IDepartmentService _departmentService;
        private readonly IMedicineService _medicineService;
        private readonly IPatientNoteService _patientNoteService;
        private readonly INurseTaskService _nurseTaskService;

        public DashboardController(
            IUserService userService,
            IAppointmentService appointmentService,
            IDepartmentService departmentService,
            IMedicineService medicineService,
            IPatientNoteService patientNoteService,
            INurseTaskService nurseTaskService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
            _departmentService = departmentService;
            _medicineService = medicineService;
            _patientNoteService = patientNoteService;
            _nurseTaskService = nurseTaskService;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardViewModel
            {
                UserRole = userRole,
                UserId = userId
            };

            try
            {
                // Basic statistics that work with existing interfaces
                var allUsers = await _userService.GetAllUsersAsync();
                var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                var allDepartments = await _departmentService.GetAllDepartmentsAsync();
                var allMedicines = await _medicineService.GetAllMedicinesAsync();

                // Admin Dashboard
                if (userRole == "Admin")
                {
                    model.TotalUsers = allUsers.Count();
                    model.TotalDoctors = allUsers.Count(u => u.Role == "Doctor");
                    model.TotalNurses = allUsers.Count(u => u.Role == "Nurse");
                    model.TotalPatients = allUsers.Count(u => u.Role == "Patient");
                    model.TotalDepartments = allDepartments.Count();
                    model.TotalAppointments = allAppointments.Count();
                    model.TodayAppointments = allAppointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
                    model.PendingAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Planlandı);
                    model.TotalMedicines = allMedicines.Count();
                    model.LowStockMedicines = allMedicines.Count(m => m.StockQuantity <= 10); // Default minimum stock level
                    model.ExpiredMedicines = allMedicines.Count(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value < DateTime.Now);
                }
                // Doctor Dashboard
                else if (userRole == "Doctor")
                {
                    var myAppointments = allAppointments.Where(a => a.DoctorId == userId).ToList();
                    model.TodayAppointments = myAppointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
                    model.PendingAppointments = myAppointments.Count(a => a.Status == AppointmentStatus.Planlandı);
                }
                // Nurse Dashboard
                else if (userRole == "Nurse")
                {
                    var myTasks = await _nurseTaskService.GetTasksByAssignedNurseAsync(userId);
                    var allTasks = await _nurseTaskService.GetAllTasksAsync();
                    // Convert to object lists for display
                    model.MyTasks = myTasks.Cast<object>().ToList();
                    model.PendingTasks = allTasks.Where(t => t.Status == "Pending").Cast<object>().ToList();
                    model.CompletedTasks = allTasks.Where(t => t.Status == "Completed").Cast<object>().ToList();
                }
                // Receptionist Dashboard
                else if (userRole == "Receptionist")
                {
                    model.TodayAppointments = allAppointments.Count(a => a.AppointmentDate.Date == DateTime.Today);
                    model.PendingAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Planlandı);
                    model.TotalPatients = allUsers.Count(u => u.Role == "Patient");
                }
            }
            catch (Exception)
            {
                // Log the exception and show a user-friendly error
                ViewBag.ErrorMessage = "Veriler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return Forbid();
            }

            try
            {
                var statistics = new
                {
                    users = new
                    {
                        total = await _userService.GetTotalUsersCountAsync(),
                        doctors = await _userService.GetUserCountByRoleAsync("Doctor"),
                        nurses = await _userService.GetUserCountByRoleAsync("Nurse"),
                        patients = await _userService.GetUserCountByRoleAsync("Patient"),
                        active = await _userService.GetActiveUsersCountAsync()
                    },
                    appointments = new
                    {
                        total = await _appointmentService.GetTotalAppointmentsCountAsync(),
                        today = await _appointmentService.GetTodayAppointmentsCountAsync(),
                        pending = await _appointmentService.GetPendingAppointmentsCountAsync(),
                        completed = await _appointmentService.GetCompletedAppointmentsCountAsync()
                    },
                    medicines = new
                    {
                        total = await _medicineService.GetTotalMedicinesCountAsync(),
                        lowStock = await _medicineService.GetLowStockMedicinesCountAsync(),
                        expired = await _medicineService.GetExpiredMedicinesCountAsync(),
                        expiringSoon = await _medicineService.GetExpiringSoonMedicinesCountAsync()
                    },
                    departments = new
                    {
                        total = await _departmentService.GetTotalDepartmentsCountAsync()
                    }
                };

                return Json(statistics);
            }
            catch (Exception)
            {
                return Json(new { error = "İstatistikler yüklenirken hata oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                var activities = new
                {
                    recentAppointments = await _appointmentService.GetRecentAppointmentsAsync(5),
                    recentUsers = await _userService.GetRecentUsersAsync(5),
                    recentNotes = await _patientNoteService.GetAllNotesAsync(),
                    recentTasks = await _nurseTaskService.GetAllTasksAsync()
                };

                return Json(activities);
            }
            catch (Exception)
            {
                return Json(new { error = "Aktiviteler yüklenirken hata oluştu." });
            }
        }
    }

    public class DashboardViewModel
    {
        public string UserRole { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        // General Statistics
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalNurses { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int TotalMedicines { get; set; }
        public int LowStockMedicines { get; set; }
        public int ExpiredMedicines { get; set; }

        // Doctor-specific
        public List<object> MyAppointments { get; set; } = new List<object>();
        public List<object> TodayMyAppointments { get; set; } = new List<object>();
        public int MyPatientsCount { get; set; }
        public int MyCompletedAppointments { get; set; }
        public List<object> UpcomingAppointments { get; set; } = new List<object>();

        // Nurse-specific
        public List<object> MyTasks { get; set; } = new List<object>();
        public List<object> PendingTasks { get; set; } = new List<object>();
        public List<object> CompletedTasks { get; set; } = new List<object>();
        public List<object> OverdueTasks { get; set; } = new List<object>();
        public List<object> TodayTasks { get; set; } = new List<object>();
        public List<object> HighPriorityTasks { get; set; } = new List<object>();

        // Receptionist-specific
        public int NewPatientsThisMonth { get; set; }

        // Common
        public List<object> RecentAppointments { get; set; } = new List<object>();
        public List<object> RecentUsers { get; set; } = new List<object>();
        public List<object> RecentPatientNotes { get; set; } = new List<object>();
    }
}
