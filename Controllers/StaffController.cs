using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    [RequirePermission("CanAccessStaffPanel")]
    public class StaffController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;

        public StaffController(
            IPatientService patientService,
            IAppointmentService appointmentService,
            IUserService userService)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
            _userService = userService;
        }

        // Staff Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                // Staff için genel istatistikler
                var patients = await _patientService.GetAllPatientsAsync();
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var todayAppointments = appointments.Where(a => a.AppointmentDate.Date == DateTime.Today).ToList();

                ViewBag.TotalPatients = patients.Count();
                ViewBag.TodayAppointments = todayAppointments.Count;
                ViewBag.PendingAppointments = appointments.Count(a => a.Status == "Planlandı" || a.Status == "Onaylandı");
                ViewBag.RecentPatients = patients.OrderByDescending(p => p.CreatedAt).Take(5).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Veri yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Hasta Kayıt İşlemleri
        [RequirePermission("CanCreatePatients")]
        public IActionResult RegisterPatient()
        {
            return View(new CreatePatientDto());
        }

        [HttpPost]
        [RequirePermission("CanCreatePatients")]
        public async Task<IActionResult> RegisterPatient(CreatePatientDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // DTO -> Patient mapping
                var patient = new Patient
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    TcNo = model.TcNo,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = Enum.TryParse<Gender>(model.Gender, out var g) ? g : Gender.Male,
                    Address = model.Address,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    BloodType = model.BloodType,
                    Allergies = model.Allergies != null ? string.Join(",", model.Allergies) : null,
                    ChronicDiseases = model.ChronicDiseases != null ? string.Join(",", model.ChronicDiseases) : null,
                    CurrentMedications = model.CurrentMedications != null ? string.Join(",", model.CurrentMedications) : null,
                    InsuranceNumber = model.InsuranceNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // IsActive kaldırıldı
                };
                await _patientService.CreatePatientAsync(patient);
                TempData["Success"] = "Hasta başarıyla kaydedildi.";
                return RedirectToAction("PatientList");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta kaydı sırasında bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // Hasta Listesi
        [RequirePermission("CanViewPatients")]
        public async Task<IActionResult> PatientList(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();

                // Arama filtresi
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    patients = patients.Where(p =>
                        p.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.TcNo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(p.PhoneNumber) && p.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // Pagination
                var totalCount = patients.Count();
                var pagedPatients = patients
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.TotalCount = totalCount;

                return View(pagedPatients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta listesi yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<PatientDto>());
            }
        }

        // Randevu Yönetimi
        [RequirePermission("CanViewAppointments")]
        public async Task<IActionResult> AppointmentManagement()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var todayAppointments = appointments.Where(a => a.AppointmentDate.Date == DateTime.Today).ToList();
                
                ViewBag.TodayAppointments = todayAppointments;
                ViewBag.AllAppointments = appointments.Take(20).ToList(); // Son 20 randevu

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu verileri yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Randevu Oluştur
        [RequirePermission("CanCreateAppointments")]
        public async Task<IActionResult> CreateAppointment()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");
                
                ViewBag.Patients = patients.Select(p => new { p.Id, p.FullName, p.TcNo }).ToList();
                ViewBag.Doctors = doctors.Select(d => new { d.Id, d.FullName }).ToList();
                
                return View(new CreateAppointmentDto());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Form verileri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new CreateAppointmentDto());
            }
        }

        [HttpPost]
        [RequirePermission("CanCreateAppointments")]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentDto model)
        {
            if (!ModelState.IsValid)
            {
                // ViewBag'leri yeniden yükle
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");
                ViewBag.Patients = patients.Select(p => new { p.Id, p.FullName, p.TcNo }).ToList();
                ViewBag.Doctors = doctors.Select(d => new { d.Id, d.FullName }).ToList();
                
                return View(model);
            }

            try
            {
                // DTO -> Appointment mapping
                var appointment = new Appointment
                {
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    AppointmentDate = model.AppointmentDate.Date + model.AppointmentTime,
                    Type = "Muayene",
                    Status = "Planlandı",
                    Notes = (model.Reason ?? string.Empty) + (string.IsNullOrEmpty(model.Notes) ? string.Empty : " - " + model.Notes),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _appointmentService.CreateAppointmentAsync(appointment);
                TempData["Success"] = "Randevu başarıyla oluşturuldu.";
                return RedirectToAction("AppointmentManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu oluşturulurken bir hata oluştu: " + ex.Message;
                
                // ViewBag'leri yeniden yükle
                var patients = await _patientService.GetAllPatientsAsync();
                var doctors = await _userService.GetUsersByRoleAsync("Doctor");
                ViewBag.Patients = patients.Select(p => new { p.Id, p.FullName, p.TcNo }).ToList();
                ViewBag.Doctors = doctors.Select(d => new { d.Id, d.FullName }).ToList();
                
                return View(model);
            }
        }

        // Hızlı İşlemler
        [RequirePermission("CanEditPatients")]
        public async Task<IActionResult> QuickPatientEdit(string id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    TempData["Error"] = "Hasta bulunamadı.";
                    return RedirectToAction("PatientList");
                }

                var updateDto = new UpdatePatientDto
                {
                    TcNo = patient.TcNo,
                    Email = patient.Email,
                    Address = patient.Address,
                    BloodType = patient.BloodType,
                    Allergies = patient.Allergies?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    ChronicDiseases = patient.ChronicDiseases?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    CurrentMedications = patient.CurrentMedications?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    EmergencyContactName = patient.EmergencyContactName,
                    EmergencyContactPhone = patient.EmergencyContactPhone,
                    Gender = patient.Gender.ToString(),
                    DateOfBirth = patient.DateOfBirth
                }; 

                return View(updateDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("PatientList");
            }
        }

        [HttpPost]
        [RequirePermission("CanEditPatients")]
        public async Task<IActionResult> QuickPatientEdit(UpdatePatientDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (string.IsNullOrEmpty(model.TcNo))
                {
                    TempData["Error"] = "Geçerli bir TcNo bulunamadı.";
                    return View(model);
                }

                var existing = await _patientService.GetPatientByTcNoAsync(model.TcNo);
                if (existing == null)
                {
                    TempData["Error"] = "Hasta bulunamadı.";
                    return View(model);
                }

                // Güncellenecek alanlar
                if (!string.IsNullOrEmpty(model.FirstName)) existing.FirstName = model.FirstName;
                if (!string.IsNullOrEmpty(model.LastName)) existing.LastName = model.LastName;
                if (!string.IsNullOrEmpty(model.Email)) existing.Email = model.Email;
                if (!string.IsNullOrEmpty(model.PhoneNumber)) existing.PhoneNumber = model.PhoneNumber;
                if (model.DateOfBirth.HasValue) existing.DateOfBirth = model.DateOfBirth.Value;
                if (!string.IsNullOrEmpty(model.Gender) && Enum.TryParse<Gender>(model.Gender, out var g)) existing.Gender = g;
                if (!string.IsNullOrEmpty(model.Address)) existing.Address = model.Address;
                if (!string.IsNullOrEmpty(model.EmergencyContactName)) existing.EmergencyContactName = model.EmergencyContactName;
                if (!string.IsNullOrEmpty(model.EmergencyContactPhone)) existing.EmergencyContactPhone = model.EmergencyContactPhone;
                if (!string.IsNullOrEmpty(model.BloodType)) existing.BloodType = model.BloodType;
                if (model.Allergies != null) existing.Allergies = string.Join(",", model.Allergies);
                if (model.ChronicDiseases != null) existing.ChronicDiseases = string.Join(",", model.ChronicDiseases);
                if (model.CurrentMedications != null) existing.CurrentMedications = string.Join(",", model.CurrentMedications);
                if (!string.IsNullOrEmpty(model.InsuranceNumber)) existing.InsuranceNumber = model.InsuranceNumber;
                // IsActive kaldırıldı

                existing.UpdatedAt = DateTime.UtcNow;
                await _patientService.UpdatePatientAsync(existing.Id!, existing);
                TempData["Success"] = "Hasta bilgileri başarıyla güncellendi.";
                return RedirectToAction("PatientList");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hasta bilgileri güncellenirken bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // Staff Profil
        public async Task<IActionResult> Profile()
        {
            try
            {
                var currentUser = await _userService.GetUserByIdAsync(User.FindFirst("UserId")?.Value ?? "");
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                return View(currentUser);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Profil bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Raporlar
        [RequirePermission("CanViewReports")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var appointments = await _appointmentService.GetAllAppointmentsAsync();

                // Günlük istatistikler
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                ViewBag.TodayPatients = patients.Count(p => p.CreatedAt.Date == today);
                ViewBag.WeeklyPatients = patients.Count(p => p.CreatedAt >= thisWeek);
                ViewBag.MonthlyPatients = patients.Count(p => p.CreatedAt >= thisMonth);

                ViewBag.TodayAppointments = appointments.Count(a => a.AppointmentDate.Date == today);
                ViewBag.WeeklyAppointments = appointments.Count(a => a.AppointmentDate >= thisWeek);
                ViewBag.MonthlyAppointments = appointments.Count(a => a.AppointmentDate >= thisMonth);

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Rapor verileri yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }
    }
}

