using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    // Randevular: Doktor ve Staff (ve Admin) tam CRUD, diğer roller sınırlı
    [AuthorizeRole("Doctor","Staff","Admin")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly IDepartmentService _departmentService;

        public AppointmentController(
            IAppointmentService appointmentService,
            IUserService userService,
            IPatientService patientService,
            IDepartmentService departmentService)
        {
            _appointmentService = appointmentService;
            _userService = userService;
            _patientService = patientService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var list = new List<HospitalManagementSystem.ViewModels.AppointmentViewModel>();
                var userDict = new Dictionary<string, string>();

                foreach (var appt in appointments)
                {
                    var patient = await _patientService.GetPatientByIdAsync(appt.PatientId);
                    var doctor = await _userService.GetUserByIdAsync(appt.DoctorId);
                    Models.User? creatorUser = null;

                    string createdByName = string.Empty;
                    if (!string.IsNullOrEmpty(appt.CreatedBy))
                    {
                        if (!userDict.ContainsKey(appt.CreatedBy))
                        {
                            creatorUser = await _userService.GetUserByIdAsync(appt.CreatedBy);
                            if (creatorUser != null) userDict[appt.CreatedBy] = creatorUser.FullName;
                        }
                        if (!userDict.TryGetValue(appt.CreatedBy!, out var tmpCreatedName) || string.IsNullOrEmpty(tmpCreatedName))
                        {
                            createdByName = string.Empty;
                        }
                        else
                        {
                            createdByName = tmpCreatedName;
                        }
                    }

                    var vm = new HospitalManagementSystem.ViewModels.AppointmentViewModel
                    {
                        Id = appt.Id ?? string.Empty,
                        PatientId = appt.PatientId,
                        PatientName = patient?.FullName ?? "Bilinmeyen Hasta",
                        PatientTc = patient?.IdentityNumber ?? patient?.TcNo ?? string.Empty,
                        DoctorId = appt.DoctorId,
                        DoctorName = doctor?.FullName ?? "Bilinmeyen Doktor",
                        AppointmentDate = appt.AppointmentDate,
                        Status = appt.Status,
                        Type = appt.Type,
                        Notes = appt.Notes ?? string.Empty,
                        Department = appt.Department ?? string.Empty,
                        CreatedAt = appt.CreatedAt,
                        CreatedBy = appt.CreatedBy ?? string.Empty,
                        CreatedByName = string.IsNullOrWhiteSpace(createdByName) ? "Bilinmiyor" : createdByName,
                        CreatedByRole = creatorUser?.Role ?? string.Empty,
                        CreatedByTc = creatorUser?.TcNo ?? string.Empty,
                        DeletedAt = appt.DeletedAt,
                        DeletedBy = appt.DeletedBy,
                        DeletedByName = string.Empty
                    };

                    if (string.IsNullOrEmpty(vm.Department) && doctor != null)
                    {
                        vm.Department = doctor.DoctorDepartment ?? doctor.Specialization ?? "Genel";
                    }

                    list.Add(vm);
                }

                ViewBag.UserNameMap = userDict;
                return View(list);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Cannot deserialize") || ex.Message.Contains("BsonType"))
                {
                    ViewBag.SerializationError = true;
                    ViewBag.ErrorMessage = "MongoDB'de uyumsuz veri formatı tespit edildi. Verileri temizlemeniz gerekiyor.";
                    return View(new List<HospitalManagementSystem.ViewModels.AppointmentViewModel>());
                }
                throw;
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync("Doctor");
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                appointment.CreatedAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.Status = string.IsNullOrWhiteSpace(appointment.Status) ? "Planlandı" : appointment.Status;
                var creatorId = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(creatorId)) appointment.CreatedBy = creatorId;

                if (string.IsNullOrEmpty(appointment.Department) && !string.IsNullOrEmpty(appointment.DoctorId))
                {
                    var doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
                    if (doctor != null)
                    {
                        appointment.Department = doctor.DoctorDepartment ?? doctor.Specialization ?? appointment.Department;
                    }
                }

                await _appointmentService.CreateAppointmentAsync(appointment);
                TempData["SuccessMessage"] = "Randevu oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Doctors = await _userService.GetUsersByRoleAsync("Doctor");
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(appointment);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null || appointment.DeletedAt != null) return NotFound();
            // Hasta & Doktor bilgilerini getir
            var patient = await _patientService.GetPatientByIdAsync(appointment.PatientId);
            var doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
            // Departman boşsa doktor bilgilerinden doldur (persist etmeden sadece ekranda göster)
            if (string.IsNullOrWhiteSpace(appointment.Department) && doctor != null)
            {
                appointment.Department = doctor.DoctorDepartment ?? doctor.Specialization ?? string.Empty;
            }
            ViewBag.EditPatient = patient;
            ViewBag.EditDoctor = doctor;
            ViewBag.Doctors = await _userService.GetUsersByRoleAsync("Doctor");
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                appointment.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(appointment.Department) && !string.IsNullOrEmpty(appointment.DoctorId))
                {
                    var doctor = await _userService.GetUserByIdAsync(appointment.DoctorId);
                    if (doctor != null)
                    {
                        appointment.Department = doctor.DoctorDepartment ?? doctor.Specialization ?? appointment.Department;
                    }
                }
                await _appointmentService.UpdateAppointmentAsync(id, appointment);
                TempData["SuccessMessage"] = "Randevu güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Doctors = await _userService.GetUsersByRoleAsync("Doctor");
            ViewBag.Patients = await _patientService.GetAllPatientsAsync();
            ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            return View(appointment);
        }

        [HttpPost]
    public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            return Json(new { success = true });
        }

        // Soft delete işlemi için ayrı endpoint
        [HttpPost]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var userId = HttpContext.Session.GetString("UserId") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (string.IsNullOrEmpty(userRole)) return Forbid();
            // Yetki kontrolü (Admin, Staff, Doctor kendi randevusu)
            var can = await _appointmentService.CanCancelAppointmentAsync(id, userId, userRole);
            if (!can) return Forbid();
            var ok = await _appointmentService.SoftDeleteAppointmentAsync(id, userId);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

    // Eski javascript çağrısı /Appointment/Cancel/... olduğundan geriye dönük uyumluluk için alias
    [HttpPost]
    public Task<IActionResult> Cancel(string id) => SoftDelete(id);

        // Silinen randevular listesi
        public async Task<IActionResult> Deleted()
        {
            var deleted = await _appointmentService.GetDeletedAppointmentsAsync();
            var list = new List<HospitalManagementSystem.ViewModels.AppointmentViewModel>();
            var userDict = new Dictionary<string, string>();

            foreach (var appt in deleted)
            {
                var patient = await _patientService.GetPatientByIdAsync(appt.PatientId);
                var doctor = await _userService.GetUserByIdAsync(appt.DoctorId);
                Models.User? creatorUser = null;
                Models.User? deleterUser = null;

                string createdByName = string.Empty;
                string deletedByName = string.Empty;

                if (!string.IsNullOrEmpty(appt.CreatedBy))
                {
                    if (!userDict.ContainsKey(appt.CreatedBy))
                    {
                        creatorUser = await _userService.GetUserByIdAsync(appt.CreatedBy);
                        if (creatorUser != null) userDict[appt.CreatedBy] = creatorUser.FullName;
                    }
                    if (!userDict.TryGetValue(appt.CreatedBy!, out var tmpCreatedName2) || string.IsNullOrEmpty(tmpCreatedName2))
                    {
                        createdByName = string.Empty;
                    }
                    else
                    {
                        createdByName = tmpCreatedName2;
                    }
                }

                if (!string.IsNullOrEmpty(appt.DeletedBy))
                {
                    if (!userDict.ContainsKey(appt.DeletedBy))
                    {
                        deleterUser = await _userService.GetUserByIdAsync(appt.DeletedBy);
                        if (deleterUser != null) userDict[appt.DeletedBy] = deleterUser.FullName;
                    }
                    if (!userDict.TryGetValue(appt.DeletedBy!, out var tmpDeletedName) || string.IsNullOrEmpty(tmpDeletedName))
                    {
                        deletedByName = string.Empty;
                    }
                    else
                    {
                        deletedByName = tmpDeletedName;
                    }
                }

                var vm = new HospitalManagementSystem.ViewModels.AppointmentViewModel
                {
                    Id = appt.Id ?? string.Empty,
                    PatientId = appt.PatientId,
                    PatientName = patient?.FullName ?? "Bilinmeyen Hasta",
                    PatientTc = patient?.IdentityNumber ?? patient?.TcNo ?? string.Empty,
                    DoctorId = appt.DoctorId,
                    DoctorName = doctor?.FullName ?? "Bilinmeyen Doktor",
                    AppointmentDate = appt.AppointmentDate,
                    Status = appt.Status,
                    Type = appt.Type,
                    Notes = appt.Notes ?? string.Empty,
                    Department = appt.Department ?? string.Empty,
                    CreatedAt = appt.CreatedAt,
                    CreatedBy = appt.CreatedBy ?? string.Empty,
                    CreatedByName = string.IsNullOrWhiteSpace(createdByName) ? "Bilinmiyor" : createdByName,
                    CreatedByRole = creatorUser?.Role ?? string.Empty,
                    CreatedByTc = creatorUser?.TcNo ?? string.Empty,
                    DeletedAt = appt.DeletedAt,
                    DeletedBy = appt.DeletedBy,
                    DeletedByName = string.IsNullOrWhiteSpace(deletedByName) ? string.Empty : deletedByName
                };

                if (string.IsNullOrEmpty(vm.Department) && doctor != null)
                {
                    vm.Department = doctor.DoctorDepartment ?? doctor.Specialization ?? "Genel";
                }

                list.Add(vm);
            }

            ViewBag.UserNameMap = userDict;
            return View(list);
        }

        // Kalıcı silme (sadece Admin veya Staff veya Doctor kendi randevusu) - admin & staff geniş yetki
        [HttpPost]
        public async Task<IActionResult> HardDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var userId = HttpContext.Session.GetString("UserId") ?? string.Empty;
            var appt = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appt == null) return NotFound();
            // Admin & Staff her şeyi silebilir; Doctor sadece kendi oluşturduğu ve kendi randevusu ise silebilir
            var doctorCan = userRole == "Doctor" && (appt.DoctorId == userId || appt.CreatedBy == userId);
            if (userRole != "Admin" && userRole != "Staff" && !doctorCan) return Forbid();
            await _appointmentService.DeleteAppointmentAsync(id);
            TempData["SuccessMessage"] = "Randevu kalıcı olarak silindi.";
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var ok = await _appointmentService.RestoreAppointmentAsync(id);
            if (!ok) TempData["ErrorMessage"] = "Randevu geri yüklenemedi."; else TempData["SuccessMessage"] = "Randevu geri yüklendi.";
            return RedirectToAction(nameof(Deleted));
        }

        // Belirli bir tarih ve doktor için dolu saatleri getir
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(string doctorId, string date)
        {
            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(date))
            {
                return Json(new { success = false, message = "Doktor ID ve tarih gereklidir." });
            }

            if (!DateTime.TryParse(date, out DateTime selectedDate))
            {
                return Json(new { success = false, message = "Geçersiz tarih formatı." });
            }

            // O gün için doktorun randevularını al
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var doctorAppointments = appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == selectedDate.Date && 
                           a.DeletedAt == null && 
                           a.Status != "İptalEdildi")
                .Select(a => a.AppointmentDate.ToString("HH:mm"))
                .ToList();

            // Tüm olası saat aralıkları
            var allTimeSlots = new List<string>
            {
                "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
                "11:00", "11:30", "12:00", "12:30", "13:00", "13:30",
                "14:00", "14:30", "15:00", "15:30", "16:00", "16:30",
                "17:00", "17:30"
            };

            // Dolu olan saatleri çıkar
            var availableSlots = allTimeSlots.Except(doctorAppointments).ToList();

            // Geçmiş saatleri de çıkar (bugün ise)
            if (selectedDate.Date == DateTime.Today)
            {
                var currentTime = DateTime.Now.ToString("HH:mm");
                availableSlots = availableSlots.Where(slot => string.Compare(slot, currentTime) > 0).ToList();
            }

            return Json(new { 
                success = true, 
                availableSlots = availableSlots,
                occupiedSlots = doctorAppointments 
            });
        }

        // MongoDB Serialization sorunları için collection temizleme (sadece Admin)
        [AuthorizeRole("Admin")]
        [HttpPost]
        public async Task<IActionResult> ClearAllAppointments()
        {
            try
            {
                var result = await _appointmentService.ClearAllAppointmentsAsync();
                if (result)
                {
                    TempData["SuccessMessage"] = "Tüm randevu verileri başarıyla temizlendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Veri temizleme işlemi başarısız oldu.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

    // Eski GetPatientAppointments / GetDoctorAppointments / GetDailyAppointments endpointleri kaldırıldı.

    // Mapping helper'ları kaldırıldı (string kullanımına geri dönüldü)
    }
}
