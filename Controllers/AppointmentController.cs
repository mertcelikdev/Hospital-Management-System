using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public AppointmentController(IAppointmentService appointmentService, IUserService userService, IDepartmentService departmentService)
        {
            _appointmentService = appointmentService;
            _userService = userService;
            _departmentService = departmentService;
        }

        // Randevuları listele
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string sort = "date", string dir = "desc")
        {
            pageSize = pageSize is < 5 or > 100 ? 20 : pageSize;
            var appointments = await _appointmentService.GetAllAppointmentsAsync(page, pageSize, sort, dir);
            var viewModels = appointments.Select(a => new ViewModels.AppointmentViewModel
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.PatientName,
                PatientTc = string.IsNullOrWhiteSpace(a.PatientTc) ? "Bilinmiyor" : a.PatientTc,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                AppointmentDate = a.AppointmentDateTime,
                Status = a.Status,
                // BUG FIX: Previously Type column showed Notes/Reason. Use actual Type field.
                Type = a.Type ?? string.Empty,
                Notes = a.Notes ?? string.Empty,
                Department = string.Empty,
                CreatedAt = a.CreatedAt
            }).ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Sort = sort; ViewBag.Dir = dir;
            return View(viewModels);
        }

        // Randevu detayları
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound();
            return View(appointment); // AppointmentDto
        }

        // Yeni randevu oluşturma formu (Sadece Doktor ve Staff)
        [AuthorizeRole("Doctor", "Staff")]
        public async Task<IActionResult> Create()
        {
            // Lazy load için başlangıçta doktor listesi gönderilmiyor
            try
            {
                var deps = await _departmentService.GetAllDepartmentsAsync();
                ViewBag.Departments = deps;
            }
            catch(Exception ex)
            {
                // Basit log (ileride ILogger eklenebilir)
                Console.WriteLine("Departman yükleme hatası: " + ex.Message);
                ViewBag.Departments = new List<HospitalManagementSystem.DTOs.DepartmentDto>();
            }
            return View();
        }

        // Yeni randevu oluşturma POST (Sadece Doktor ve Staff)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Doctor", "Staff")]
    public async Task<IActionResult> Create(CreateAppointmentDto createAppointmentDto)
        {
            if (ModelState.IsValid)
            {
                // Seçilen doktor ve zaman çakışma kontrolü
                var dateTime = createAppointmentDto.AppointmentDate.Date + createAppointmentDto.AppointmentTime;
                var available = await _appointmentService.IsTimeSlotAvailableAsync(createAppointmentDto.DoctorId, dateTime);
                if(!available)
                {
                    ModelState.AddModelError(string.Empty, "Bu doktor için seçilen saat dolu. Lütfen başka bir saat seçin.");
                }
                else
                {
                    var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    await _appointmentService.CreateAppointmentAsync(createAppointmentDto, userId);
                    TempData["SuccessMessage"] = "Randevu başarıyla oluşturuldu";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Model validation başarısız olursa, dropdown listelerini tekrar yükle
            try
            {
                ViewBag.Departments = await _departmentService.GetAllDepartmentsAsync();
            }
            catch { ViewBag.Departments = new List<HospitalManagementSystem.DTOs.DepartmentDto>(); }
            return View(createAppointmentDto);
        }

        // Randevu iptal etme (Sadece Doktor ve Staff)
        [HttpPost]
        [AuthorizeRole("Doctor", "Staff")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                await _appointmentService.UpdateAppointmentStatusAsync(id, AppointmentStatus.İptalEdildi);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Randevu iptal edilirken hata oluştu: {ex.Message}");
            }
        }

        // Soft deleted listesi
    public async Task<IActionResult> Deleted()
        {
            var deleted = await _appointmentService.GetDeletedAppointmentsAsync();
            var userCache = (await _userService.GetAllUsersAsync()).ToDictionary(u => u.Id, u => u.Name);
            var vms = deleted.Select(a => new ViewModels.AppointmentViewModel
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientName = a.PatientName,
                PatientTc = string.IsNullOrWhiteSpace(a.PatientTc) ? "Bilinmiyor" : a.PatientTc,
                DoctorId = a.DoctorId,
                DoctorName = a.DoctorName,
                AppointmentDate = a.AppointmentDateTime,
                Status = a.Status,
                Type = a.Type ?? string.Empty,
                Notes = a.Notes ?? string.Empty,
                Department = string.Empty,
                CreatedAt = a.CreatedAt,
                DeletedAt = a.DeletedAt,
                CreatedByName = string.IsNullOrWhiteSpace(a.CreatedBy) ? null : (userCache.ContainsKey(a.CreatedBy) ? userCache[a.CreatedBy] : a.CreatedBy),
                DeletedByName = string.IsNullOrWhiteSpace(a.DeletedBy) ? null : (userCache.ContainsKey(a.DeletedBy) ? userCache[a.DeletedBy] : a.DeletedBy)
            }).ToList();
            return View(vms);
        }

        [HttpGet]
        public async Task<IActionResult> ByTc(string tc)
        {
            if(string.IsNullOrWhiteSpace(tc) || tc.Length < 3) return Json(new { success = true, data = new List<AppointmentDto>() });
            var list = await _appointmentService.GetAppointmentsByPatientTcAsync(tc);
            return Json(new { success = true, data = list });
        }

        [HttpPost]

        // Doktor ve tarih bazlı uygun slotlar (09-17 arası 30 dk). Dolu olanları da döndürür.
        [HttpGet]
        public async Task<IActionResult> TimeSlots(string doctorId, string date)
        {
            if(string.IsNullOrWhiteSpace(doctorId) || string.IsNullOrWhiteSpace(date))
                return Json(new { success = false, message = "Parametre eksik" });
            if(!DateTime.TryParse(date, out var d))
                return Json(new { success = false, message = "Geçersiz tarih" });
            var available = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, d);
            // Tüm slotları üretip available dışındakileri dolu işaretle
            var start = d.Date.AddHours(9); var end = d.Date.AddHours(17);
            var all = new List<DateTime>();
            for(var t = start; t < end; t = t.AddMinutes(30)) all.Add(t);
            var data = all.Select(t => new {
                time = t.ToString("HH:mm"),
                available = available.Any(a => a == t)
            });
            return Json(new { success = true, data });
        }
        public async Task<IActionResult> Restore(string id)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role is not ("Admin" or "Staff" or "Doctor")) return Forbid();
            var ok = await _appointmentService.RestoreAppointmentAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Randevu geri alındı";
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost]
        public async Task<IActionResult> HardDelete(string id)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role != "Admin") return Forbid();
            var ok = await _appointmentService.HardDeleteAppointmentAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Randevu kalıcı olarak silindi";
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost]
        [AuthorizeRole("Doctor","Staff","Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role is not ("Admin" or "Staff" or "Doctor")) return Forbid();
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _appointmentService.DeleteAppointmentAsync(id, userId);
            TempData["SuccessMessage"] = "Randevu silindi (geri alınabilir)";
            return RedirectToAction(nameof(Index));
        }

        // Edit GET
    [AuthorizeRole("Doctor","Admin","Staff")]
        public async Task<IActionResult> Edit(string id)
        {
            if(string.IsNullOrEmpty(id)) return NotFound();
            var appt = await _appointmentService.GetAppointmentByIdAsync(id);
            if(appt == null) return NotFound();
            // Map to Update DTO for form binding
            var dto = new UpdateAppointmentDto
            {
                AppointmentDate = appt.AppointmentDateTime.Date,
                AppointmentTime = appt.AppointmentDateTime.TimeOfDay,
                Notes = appt.Notes,
                Reason = appt.Reason,
                Status = appt.Status,
                Type = appt.Type
            };
            ViewBag.AppointmentId = appt.Id;
            return View(dto);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
    [AuthorizeRole("Doctor","Admin","Staff")]
        public async Task<IActionResult> Edit(string id, UpdateAppointmentDto dto)
        {
            if(ModelState.IsValid)
            {
                var dateTime = dto.AppointmentDate.Date + dto.AppointmentTime;
                var appt = await _appointmentService.GetAppointmentByIdAsync(id);
                if(appt == null) return NotFound();
                var available = await _appointmentService.IsTimeSlotAvailableForUpdateAsync(appt.DoctorId, dateTime, id);
                if(!available)
                {
                    ModelState.AddModelError(string.Empty, "Bu saat başka bir randevu ile çakışıyor.");
                }
                else
                {
                    await _appointmentService.UpdateAppointmentAsync(id, dto);
                    TempData["SuccessMessage"] = "Randevu güncellendi";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            ViewBag.AppointmentId = id;
            return View(dto);
        }

        // Update status (ajax)
        [HttpPost]
        [AuthorizeRole("Doctor","Admin","Staff")]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(status)) return BadRequest();
            if(!Enum.TryParse<AppointmentStatus>(status, true, out var st)) return BadRequest();
            var ok = await _appointmentService.UpdateAppointmentStatusAsync(id, st);
            if(!ok) return NotFound();
            return Ok(new { success = true });
        }
    }
}
