using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Attributes;

namespace HospitalManagementSystem.Controllers
{
    [AuthorizeRole("Admin","Doctor","Staff")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? sort = "name", string? dir = "asc", string? name = null, string? tc = null)
        {
            var (items,total) = await _patientService.GetPatientsPagedAsync(page, pageSize, sort, dir, name, tc);
            var vm = new HospitalManagementSystem.ViewModels.PatientListViewModel
            {
                Patients = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = (int)total,
                Sort = sort,
                Dir = dir,
                Search = !string.IsNullOrWhiteSpace(name)? name : tc // legacy single search alanı şimdilik dolu kalsın
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ListData(int page = 1, int pageSize = 10, string? sort = "name", string? dir = "asc", string? name = null, string? tc = null, string? letter = null)
        {
                var (items,total) = await _patientService.GetPatientsPagedAsync(page, pageSize, sort, dir, name, tc, letter);
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var data = items.Select(p => new {
                id = p.Id,
                fullName = ($"{p.FirstName} {p.LastName}").Trim(),
                tc = p.TcNo,
                phone = p.PhoneNumber,
                email = p.Email,
                age = (int)Math.Floor((DateTime.UtcNow.Date - p.DateOfBirth.Date).TotalDays / 365.2425),
                bloodType = p.BloodType
            });
            return Json(new { page, pageSize, total, totalPages, items = data });
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        public IActionResult Create()
        {
            return View(new CreatePatientDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            // TC (TcNo) benzersiz mi?
            var exists = await _patientService.PatientExistsAsync(dto.TcNo);
            if (exists)
            {
                ModelState.AddModelError("TcNo","Bu TC Kimlik No ile hasta zaten kayıtlı.");
                return View(dto);
            }
            await _patientService.CreatePatientAsync(dto);
            TempData["SuccessMessage"] = "Hasta kaydı oluşturuldu";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            var dto = new UpdatePatientDto
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                TcNo = patient.TcNo,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                BloodType = patient.BloodType
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdatePatientDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var ok = await _patientService.UpdatePatientAsync(id, dto);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Hasta güncellendi";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _patientService.DeletePatientAsync(id, userId);
            TempData["SuccessMessage"] = "Hasta silindi (geri alınabilir)";
            return RedirectToAction(nameof(Index));
        }

        // Soft deleted listesi
        public async Task<IActionResult> Deleted()
        {
            var list = await _patientService.GetDeletedPatientsAsync();
            // DeletedBy id -> isim map
            var userIds = list.Where(p=> !string.IsNullOrWhiteSpace(p.DeletedBy)).Select(p=> p.DeletedBy!).Distinct().ToList();
            var nameMap = new Dictionary<string,string>();
            if(userIds.Count>0)
            {
                // Basit çözüm: tüm kullanıcıları çekip filtrele (koleksiyon küçük varsayımı)
                // Daha iyisi ayrı IUserService ile seçici çağrı; burada varsayımsal olarak interface var.
                try
                {
                    var userService = HttpContext.RequestServices.GetService(typeof(IUserService)) as IUserService;
                    if(userService != null)
                    {
                        var all = await userService.GetAllUsersAsync();
                        foreach(var u in all.Where(u=> userIds.Contains(u.Id))) nameMap[u.Id] = u.Name;
                    }
                }
                catch { }
            }
            ViewBag.UserNameMap = nameMap;            
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if(string.IsNullOrWhiteSpace(query)) return Json(new { success = true, data = new List<PatientDto>() });
            // Servis tarafı Mongo filtreliyor
            var list = await _patientService.SearchPatientsAsync(query.Trim());
            return Json(new { success = true, data = list.Take(100) });
        }

        // Tek bir hasta TC ile bulunur (tam 11 hane eşleşme)
        [HttpGet]
        public async Task<IActionResult> FindByTc(string tc)
        {
            if(string.IsNullOrWhiteSpace(tc) || tc.Length != 11 || !tc.All(char.IsDigit))
                return Json(new { success = false, message = "Geçerli 11 haneli TC giriniz" });

            var patient = await _patientService.GetPatientByIdentityNumberAsync(tc);
            if(patient == null)
                return Json(new { success = false, message = "Hasta bulunamadı" });

            return Json(new { success = true, data = new {
                id = patient.Id,
                fullName = $"{patient.FirstName} {patient.LastName}".Trim(),
                tcNo = patient.TcNo,
                email = patient.Email,
                phoneNumber = patient.PhoneNumber
            }});
        }

        [HttpPost]
        public async Task<IActionResult> Restore(string id)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role is not ("Admin" or "Staff" or "Doctor")) return Forbid();
            var ok = await _patientService.RestorePatientAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Hasta kaydı geri alındı";
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost]
        public async Task<IActionResult> HardDelete(string id)
        {
            var role = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if(role != "Admin") return Forbid();
            var ok = await _patientService.HardDeletePatientAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Hasta kalıcı olarak silindi";
            return RedirectToAction(nameof(Deleted));
        }

    // (YANLIŞ eklenen randevu restore kodu ve hatalı bloklar kaldırıldı)
    }
}
