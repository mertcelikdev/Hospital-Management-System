using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly INurseTaskService _nurseTaskService;
        private readonly IMedicationService _medicationService;

        public DashboardController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IUserService userService,
            INurseTaskService nurseTaskService,
            IMedicationService medicationService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _userService = userService;
            _nurseTaskService = nurseTaskService;
            _medicationService = medicationService;
        }

    // Staff paneli: sadece Staff ve Admin
    [AuthorizeRole("Staff", "Admin")]
    public async Task<IActionResult> Index()
        {
            try
            {
                var lowStockList = await _medicationService.GetLowStockMedicationsAsync();
                var todaysAppointments = await _appointmentService.GetTodaysAppointmentsAsync();
                var recentAppointments = await _appointmentService.GetRecentAppointmentsAsync(8);

                ViewBag.TotalPatients = await _patientService.GetTotalPatientsCountAsync();
                ViewBag.TotalUsers = await _userService.GetTotalUsersCountAsync();
                ViewBag.TodayAppointments = await _appointmentService.GetTodayAppointmentsCountAsync();
                ViewBag.PendingAppointments = await _appointmentService.GetPendingAppointmentsCountAsync();
                ViewBag.CompletedAppointments = await _appointmentService.GetCompletedAppointmentsCountAsync();
                ViewBag.TotalMedications = await _medicationService.GetTotalMedicationsCountAsync();
                ViewBag.LowStockMedications = lowStockList.Count;
                ViewBag.LowStockMedicationList = lowStockList;
                ViewBag.RecentAppointments = recentAppointments;
                ViewBag.TodaysAppointments = todaysAppointments;
                ViewBag.UserName = HttpContext.Session.GetString("UserFullName") ?? HttpContext.Session.GetString("UserName");
                ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

                return View("StaffDashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dashboard yüklenirken hata oluştu: " + ex.Message;
                return View("StaffDashboard");
            }
        }

    [HttpGet]
        public IActionResult RoleDispatch()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }
            return role switch
            {
                "Admin" => RedirectToAction("Admin"),
                "Doctor" => RedirectToAction("Doctor"),
                "Nurse" => RedirectToAction("Nurse"),
                "Staff" => RedirectToAction("Index"),
                "Patient" => RedirectToAction("Patient"),
                _ => RedirectToAction("Index")
            };
        }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Admin()
        {
            try
            {
                var viewModel = new
                {
                    TotalDoctors = await _userService.GetUserCountByRoleAsync("Doctor"),
                    TotalNurses = await _userService.GetUserCountByRoleAsync("Nurse"),
                    TotalStaff = await _userService.GetUserCountByRoleAsync("Staff"),
                    TotalPatients = await _patientService.GetTotalPatientsCountAsync(),
                    MonthlyAppointments = await _appointmentService.GetTotalAppointmentsCountAsync(),
                    RecentUsers = await _userService.GetAllUsersAsync()
                };

                return View("AdminDashboard", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Admin dashboard yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

    // Doktor paneli: sadece Doctor ve Admin
    [AuthorizeRole("Doctor", "Admin")]
    public async Task<IActionResult> Doctor()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new
                {
                    TodayAppointments = await _appointmentService.GetTodayAppointmentsByDoctorAsync(userId),
                    UpcomingAppointments = await _appointmentService.GetUpcomingAppointmentsByDoctorAsync(userId, 10),
                    PatientCount = await _appointmentService.GetPatientCountByDoctorAsync(userId),
                    CompletedAppointments = await _appointmentService.GetCompletedAppointmentsByDoctorCountAsync(userId)
                };

                return View("DoctorDashboard", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Doktor dashboard yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

    // Hemşire paneli: sadece Nurse ve Admin
    [AuthorizeRole("Nurse", "Admin")]
    public async Task<IActionResult> Nurse()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new
                {
                    AssignedTasks = await _nurseTaskService.GetTasksByNurseIdAsync(userId),
                    UpcomingTasks = await _nurseTaskService.GetUpcomingTasksAsync(userId),
                    OverdueTasks = await _nurseTaskService.GetOverdueTasksAsync(),
                    // Enum standardizasyonu: Completed -> Tamamlandi
                    CompletedTasksCount = await _nurseTaskService.GetTaskCountByStatusAsync(HospitalManagementSystem.Models.TaskStatus.Tamamlandi)
                };

                return View("NurseDashboard", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hemşire dashboard yüklenirken hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Hasta paneli: sadece Patient ve Admin (Admin inceleme icin girebilir)
        [AuthorizeRole("Patient", "Admin")]
        public IActionResult Patient()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var name = HttpContext.Session.GetString("UserId"); // Daha sonra kullanıcı adı çekilebilir
            ViewBag.UserName = name;
            // Basit istatistik placeholder; servis entegrasyonu eklenebilir
            ViewBag.UpcomingAppointments = 0;
            ViewBag.ActivePrescriptions = 0;
            ViewBag.ActiveTreatments = 0;
            return View("PatientDashboard");
        }

    }
}
