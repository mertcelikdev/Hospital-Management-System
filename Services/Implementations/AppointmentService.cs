using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IMongoCollection<Appointment> _appointments;
        private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Patient> _patients;

        public AppointmentService(IMongoDatabase database)
        {
            _appointments = database.GetCollection<Appointment>("Appointments");
            _users = database.GetCollection<User>("Users");
            _patients = database.GetCollection<Patient>("Patients");
        }

        private static AppointmentDto ToDto(Appointment a, User? patientUser = null, User? doctor = null, Patient? patientEntity = null) => new()
        {
            Id = a.Id ?? string.Empty,
            PatientId = a.PatientId,
            DoctorId = a.DoctorId,
            DepartmentId = a.DepartmentId,
            AppointmentDate = a.AppointmentDate.Date,
            AppointmentTime = a.AppointmentDate.TimeOfDay,
            AppointmentDateTime = a.AppointmentDate,
            Notes = a.Notes,
            Reason = a.Notes, // TODO: Modelde ayrı Reason alanı yok, ileride eklenecekse güncellenecek
            Priority = "Normal",
            IsUrgent = false,
            Status = a.Status.ToString(),
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            CreatedBy = a.CreatedBy,
            PatientName = patientUser?.Name ?? patientEntity?.FullName ?? string.Empty,
            PatientTc = patientEntity?.IdentityNumber ?? string.Empty,
            PatientPhone = patientUser?.Phone ?? patientEntity?.PhoneNumber ?? string.Empty,
            DoctorName = doctor?.Name ?? string.Empty,
            DoctorSpecialization = doctor?.Specialization,
            IsPastDue = a.AppointmentDate < DateTime.UtcNow,
            Type = a.Type.ToString(),
            IsDeleted = a.IsDeleted,
            DeletedAt = a.DeletedAt,
            DeletedBy = a.DeletedBy
        };

        private static Appointment FromCreateDto(CreateAppointmentDto dto)
        {
            // dto.AppointmentDate tam tarih (saat 00:00) + AppointmentTime birleştirilir
            var dateTime = dto.AppointmentDate.Date + dto.AppointmentTime;
            var typeEnum = AppointmentType.Muayene;
            if(!string.IsNullOrWhiteSpace(dto.Type) && Enum.TryParse<AppointmentType>(dto.Type, true, out var parsedType))
                typeEnum = parsedType;
            return new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                DepartmentId = dto.DepartmentId,
                AppointmentDate = dateTime,
                Type = typeEnum,
                Status = AppointmentStatus.Planlandı,
                Notes = dto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = string.Empty
            };
        }

        public async Task<List<AppointmentDto>> GetAllAppointmentsAsync(int page = 1, int pageSize = 50, string? sort = null, string? dir = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;
            var query = _appointments.Find(a => !a.IsDeleted);
            // Sıralama
            IFindFluent<Appointment, Appointment> ordered = query;
            var descending = dir?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            switch(sort){
                case "date":
                    ordered = descending? query.SortByDescending(a=> a.AppointmentDate): query.SortBy(a=> a.AppointmentDate); break;
                case "created":
                    ordered = descending? query.SortByDescending(a=> a.CreatedAt): query.SortBy(a=> a.CreatedAt); break;
                default:
                    ordered = query.SortByDescending(a=> a.AppointmentDate); break; // varsayılan
            }
            var list = await ordered
                .Skip((page-1)*pageSize)
                .Limit(pageSize)
                .ToListAsync();
            return await MapWithUsers(list);
        }

    public async Task<(List<AppointmentDto> Items, long TotalCount)> GetAppointmentsPagedAsync(int page, int pageSize, string? sort = null, string? dir = null, string? q = null, string? departmentId = null, string? doctorId = null, string? patientId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, string? type = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;
            var filter = Builders<Appointment>.Filter.Where(a => !a.IsDeleted);
            if(!string.IsNullOrWhiteSpace(departmentId))
                filter &= Builders<Appointment>.Filter.Eq(a=> a.DepartmentId, departmentId);
            if(!string.IsNullOrWhiteSpace(doctorId))
                filter &= Builders<Appointment>.Filter.Eq(a=> a.DoctorId, doctorId);
            if(!string.IsNullOrWhiteSpace(patientId))
                filter &= Builders<Appointment>.Filter.Eq(a=> a.PatientId, patientId);
            if(startDate.HasValue)
                filter &= Builders<Appointment>.Filter.Gte(a=> a.AppointmentDate, startDate.Value.Date);
            if(endDate.HasValue)
                filter &= Builders<Appointment>.Filter.Lte(a=> a.AppointmentDate, endDate.Value.Date.AddDays(1).AddTicks(-1));
            if(!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var st))
                filter &= Builders<Appointment>.Filter.Eq(a=> a.Status, st);
            if(!string.IsNullOrWhiteSpace(type) && Enum.TryParse<AppointmentType>(type, true, out var typeEnum))
                filter &= Builders<Appointment>.Filter.Eq(a=> a.Type, typeEnum);
            if(!string.IsNullOrWhiteSpace(q))
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(q, "i");
                var or = Builders<Appointment>.Filter.Or(
                    Builders<Appointment>.Filter.Regex(a=> a.Notes, regex)
                );
                filter &= or;
            }
            var query = _appointments.Find(filter);
            var total = await query.CountDocumentsAsync();
            IFindFluent<Appointment, Appointment> ordered = query;
            var descending = dir?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            switch(sort){
                case "date": ordered = descending? query.SortByDescending(a=> a.AppointmentDate): query.SortBy(a=> a.AppointmentDate); break;
                case "created": ordered = descending? query.SortByDescending(a=> a.CreatedAt): query.SortBy(a=> a.CreatedAt); break;
                case "patient": ordered = descending? query.SortByDescending(a=> a.PatientId): query.SortBy(a=> a.PatientId); break; // isim yerine id (lookup sonrası UI isimle sort edilmek istenirse ek optimizasyon gerekir)
                case "doctor": ordered = descending? query.SortByDescending(a=> a.DoctorId): query.SortBy(a=> a.DoctorId); break;
                default: ordered = descending? query.SortByDescending(a=> a.AppointmentDate): query.SortBy(a=> a.AppointmentDate); break;
            }
            var skip = (page-1)*pageSize;
            var limit = pageSize;
            // Optimize: filtre + sıralama için aggregate pipeline ile hafif subset çek
            var (mapped, _) = await AggregateLightweightAsync(filter, skip, limit, sort, descending);
            return (mapped, total);
        }

        // Eski arayüz imzası (geri uyumluluk) -> varsayılan parametrelerle çağırır
        public Task<List<AppointmentDto>> GetAllAppointmentsAsync()
            => GetAllAppointmentsAsync(1, 200, "date", "desc");

        private async Task<List<AppointmentDto>> MapWithUsers(List<Appointment> appointments)
        {
            var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();
            var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
            var users = await _users.Find(u => patientIds.Contains(u.Id!) || doctorIds.Contains(u.Id!)).ToListAsync();
            var patientEntities = await _patients.Find(p => patientIds.Contains(p.Id!)).ToListAsync();
            return appointments.Select(a =>
            {
                var pu = users.FirstOrDefault(u => u.Id == a.PatientId);
                var du = users.FirstOrDefault(u => u.Id == a.DoctorId);
                var pe = patientEntities.FirstOrDefault(p => p.Id == a.PatientId);
                return ToDto(a, pu, du, pe);
            }).ToList();
        }

        // Liste görünümü için sadece gerekli alanları aggregate ile toplar (daha az bellek & network)
        private async Task<(List<AppointmentDto> Items, long Total)> AggregateLightweightAsync(FilterDefinition<Appointment> filter, int skip, int limit, string? sort, bool desc)
        {
            var pipeline = new List<PipelineStageDefinition<Appointment, Appointment>>();
            if(filter != FilterDefinition<Appointment>.Empty)
                pipeline.Add(PipelineStageDefinitionBuilder.Match(filter));
            // Sıralama
            var sortDefBuilder = Builders<Appointment>.Sort;
            SortDefinition<Appointment> sortDef = sort switch
            {
                "created" => desc ? sortDefBuilder.Descending(a=> a.CreatedAt) : sortDefBuilder.Ascending(a=> a.CreatedAt),
                _ => desc ? sortDefBuilder.Descending(a=> a.AppointmentDate) : sortDefBuilder.Ascending(a=> a.AppointmentDate)
            };
            pipeline.Add(PipelineStageDefinitionBuilder.Sort(sortDef));
            // Toplam sayıyı ayrıca say (CountDocuments daha hızlı)
            var total = await _appointments.CountDocumentsAsync(filter);
            pipeline.Add(PipelineStageDefinitionBuilder.Skip<Appointment>(skip));
            pipeline.Add(PipelineStageDefinitionBuilder.Limit<Appointment>(limit));
            // Çıkan sonuçları user & patient lookup ile enrich etmek yerine mevcut MapWithUsers çağrı (küçük subset)
            var pipelineDef = PipelineDefinition<Appointment, Appointment>.Create(pipeline);
            var docs = await _appointments.Aggregate(pipelineDef).ToListAsync();
            var mapped = await MapWithUsers(docs);
            return (mapped, total);
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(string id)
        {
            var a = await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (a == null) return null;
            var patientUser = await _users.Find(u => u.Id == a.PatientId).FirstOrDefaultAsync();
            var patientEntity = await _patients.Find(p => p.Id == a.PatientId).FirstOrDefaultAsync();
            var doctor = await _users.Find(u => u.Id == a.DoctorId).FirstOrDefaultAsync();
            return ToDto(a, patientUser, doctor, patientEntity);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            var list = await _appointments.Find(a => a.PatientId == patientId).SortByDescending(a => a.AppointmentDate).ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            var list = await _appointments.Find(a => a.DoctorId == doctorId).SortByDescending(a => a.AppointmentDate).ToListAsync();
            return await MapWithUsers(list);
        }

        // TC (IdentityNumber) üzerinden hasta randevuları
        public async Task<List<AppointmentDto>> GetAppointmentsByPatientTcAsync(string tc)
        {
            if(string.IsNullOrWhiteSpace(tc)) return new List<AppointmentDto>();
            var patientEntity = await _patients.Find(p => p.IdentityNumber == tc).FirstOrDefaultAsync();
            if(patientEntity == null || string.IsNullOrEmpty(patientEntity.Id)) return new List<AppointmentDto>();
            return await GetAppointmentsByPatientIdAsync(patientEntity.Id);
        }

        public async Task CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto, string? userId = null)
        {
            var entity = FromCreateDto(createAppointmentDto);
            if(!string.IsNullOrWhiteSpace(userId)) entity.CreatedBy = userId;
            await _appointments.InsertOneAsync(entity);
        }

        public async Task UpdateAppointmentAsync(string id, UpdateAppointmentDto updateAppointmentDto)
        {
            var update = Builders<Appointment>.Update
                .Set(a => a.AppointmentDate, updateAppointmentDto.AppointmentDate.Date + updateAppointmentDto.AppointmentTime)
                .Set(a => a.Notes, updateAppointmentDto.Notes ?? string.Empty)
                .Set(a => a.UpdatedAt, DateTime.UtcNow);
            if (Enum.TryParse<AppointmentStatus>(updateAppointmentDto.Status, true, out var st))
                update = update.Set(a => a.Status, st);
            if(!string.IsNullOrWhiteSpace(updateAppointmentDto.Type) && Enum.TryParse<AppointmentType>(updateAppointmentDto.Type, true, out var t))
                update = update.Set(a => a.Type, t);
            await _appointments.UpdateOneAsync(a => a.Id == id, update);
        }

        public async Task DeleteAppointmentAsync(string id, string? userId = null)
        {
            // soft delete
            var update = Builders<Appointment>.Update
                .Set(a => a.IsDeleted, true)
                .Set(a => a.DeletedAt, DateTime.UtcNow);
            if(!string.IsNullOrWhiteSpace(userId))
                update = update.Set(a => a.DeletedBy, userId);
            await _appointments.UpdateOneAsync(a => a.Id == id, update);
        }

        public async Task<bool> RestoreAppointmentAsync(string id)
        {
            var update = Builders<Appointment>.Update
                .Set(a => a.IsDeleted, false)
                .Unset(a => a.DeletedAt)
                .Unset(a => a.DeletedBy);
            var result = await _appointments.UpdateOneAsync(a => a.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> HardDeleteAppointmentAsync(string id)
        {
            var result = await _appointments.DeleteOneAsync(a => a.Id == id && a.IsDeleted);
            return result.DeletedCount > 0;
        }

        public async Task<List<AppointmentDto>> GetDeletedAppointmentsAsync()
        {
            var list = await _appointments.Find(a => a.IsDeleted).SortByDescending(a => a.DeletedAt).Limit(200).ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(string id, AppointmentStatus status)
        {
            var result = await _appointments.UpdateOneAsync(a => a.Id == id, Builders<Appointment>.Update
                .Set(a => a.Status, status)
                .Set(a => a.UpdatedAt, DateTime.UtcNow));
            return result.ModifiedCount > 0;
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByStatusAsync(AppointmentStatus status)
        {
            var list = await _appointments.Find(a => a.Status == status).ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDateAsync(DateTime date)
        {
            var start = date.Date; var end = start.AddDays(1);
            var list = await _appointments.Find(a => a.AppointmentDate >= start && a.AppointmentDate < end).ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var list = await _appointments.Find(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate).ToListAsync();
            return await MapWithUsers(list);
        }

        public Task<List<AppointmentDto>> GetTodaysAppointmentsAsync() => GetAppointmentsByDateAsync(DateTime.Today);
        public Task<List<AppointmentDto>> GetTodaysAppointmentsByDoctorAsync(string doctorId) => GetAppointmentsByDoctorIdAsync(doctorId); // simplification
        public Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync() => GetAppointmentsByDateRangeAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        public Task<List<AppointmentDto>> GetUpcomingAppointmentsByPatientAsync(string patientId) => GetAppointmentsByPatientIdAsync(patientId);
        public Task<List<AppointmentDto>> GetPastAppointmentsAsync() => GetAppointmentsByDateRangeAsync(DateTime.MinValue, DateTime.UtcNow.AddDays(-1));
        public Task<List<AppointmentDto>> GetPastAppointmentsByPatientAsync(string patientId) => GetAppointmentsByPatientIdAsync(patientId);
        public async Task<List<AppointmentDto>> SearchAppointmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return await GetAllAppointmentsAsync();
            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filter = Builders<Appointment>.Filter.Or(
                Builders<Appointment>.Filter.Regex(a => a.Notes, regex),
                Builders<Appointment>.Filter.Regex(a => a.PatientId, regex),
                Builders<Appointment>.Filter.Regex(a => a.DoctorId, regex)
            );
            var list = await _appointments.Find(filter).SortByDescending(a => a.AppointmentDate).Limit(200).ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByTypeAsync(AppointmentType type)
        {
            var list = await _appointments.Find(a => a.Type == type).SortByDescending(a => a.AppointmentDate).ToListAsync();
            return await MapWithUsers(list);
        }
        public async Task<int> GetTotalAppointmentsCountAsync() => (int)await _appointments.CountDocumentsAsync(_ => true);
        public async Task<int> GetAppointmentsCountByStatusAsync(AppointmentStatus status) => (int)await _appointments.CountDocumentsAsync(a => a.Status == status);
        public async Task<int> GetAppointmentsCountByDoctorAsync(string doctorId) => (int)await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId);
        public async Task<int> GetAppointmentsCountByPatientAsync(string patientId) => (int)await _appointments.CountDocumentsAsync(a => a.PatientId == patientId);
        public async Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime appointmentTime) => await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentTime) == 0;
        public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date)
        {
            // 09:00 - 17:00 arası her 30 dakikada bir slot
            var start = date.Date.AddHours(9);
            var end = date.Date.AddHours(17);
            var allSlots = new List<DateTime>();
            for (var t = start; t < end; t = t.AddMinutes(30)) allSlots.Add(t);
            var taken = await _appointments.Find(a => a.DoctorId == doctorId && a.AppointmentDate >= start && a.AppointmentDate < end)
                .Project(a => a.AppointmentDate).ToListAsync();
            return allSlots.Where(s => !taken.Contains(s)).ToList();
        }

        public async Task<bool> CheckDoctorAvailabilityAsync(string doctorId, DateTime appointmentTime, TimeSpan duration)
        {
            // Çakışan randevu var mı kontrol (başlangıç anında aynı zaman)
            var exists = await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentTime);
            return exists == 0;
        }

        public async Task<bool> IsTimeSlotAvailableForUpdateAsync(string doctorId, DateTime appointmentTime, string excludeAppointmentId)
        {
            var count = await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentTime && a.Id != excludeAppointmentId);
            return count == 0;
        }

        public async Task<bool> ValidateAppointmentAsync(CreateAppointmentDto appointmentDto)
        {
            if (appointmentDto.AppointmentDate.Date < DateTime.Today) return false;
            var dateTime = appointmentDto.AppointmentDate.Date + appointmentDto.AppointmentTime;
            return await IsTimeSlotAvailableAsync(appointmentDto.DoctorId, dateTime);
        }

        public async Task<bool> CanCancelAppointmentAsync(string appointmentId, string userId, string userRole)
        {
            var appt = await _appointments.Find(a => a.Id == appointmentId).FirstOrDefaultAsync();
            if (appt == null) return false;
            if (appt.Status == AppointmentStatus.Tamamlandı) return false;
            if (userRole == "Admin" || userRole == "Doctor") return true;
            if (userRole == "Patient" && appt.PatientId == userId && appt.AppointmentDate > DateTime.UtcNow.AddHours(2)) return true;
            return false;
        }

        public async Task<bool> CanUpdateAppointmentAsync(string appointmentId, string userId, string userRole)
        {
            var appt = await _appointments.Find(a => a.Id == appointmentId).FirstOrDefaultAsync();
            if (appt == null) return false;
            if (appt.Status == AppointmentStatus.Tamamlandı || appt.Status == AppointmentStatus.İptalEdildi) return false;
            if (userRole == "Admin" || (userRole == "Doctor" && appt.DoctorId == userId)) return true;
            return false;
        }
        public async Task<int> GetTodayAppointmentsCountAsync() => await GetAppointmentsCountByStatusAsync(AppointmentStatus.Planlandı); // simplification
        public async Task<int> GetTodayAppointmentsRealCountAsync(){
            var start = DateTime.Today;
            var end = start.AddDays(1);
            return (int)await _appointments.CountDocumentsAsync(a=> a.AppointmentDate >= start && a.AppointmentDate < end && !a.IsDeleted);
        }
        public async Task<int> GetPendingAppointmentsCountAsync() => await GetAppointmentsCountByStatusAsync(AppointmentStatus.Planlandı);
        public async Task<int> GetCompletedAppointmentsCountAsync() => await GetAppointmentsCountByStatusAsync(AppointmentStatus.Tamamlandı);
        public Task<List<AppointmentDto>> GetAppointmentsByDoctorAsync(string doctorId) => GetAppointmentsByDoctorIdAsync(doctorId);
        public Task<List<AppointmentDto>> GetTodayAppointmentsByDoctorAsync(string doctorId) => GetAppointmentsByDoctorIdAsync(doctorId);
        public async Task<int> GetPatientCountByDoctorAsync(string doctorId)
        {
            // Doktora ait benzersiz hasta sayısı
            var filter = Builders<Appointment>.Filter.Eq(a => a.DoctorId, doctorId);
            var patients = await _appointments.Distinct<string>(nameof(Appointment.PatientId), filter).ToListAsync();
            return patients.Count;
        }

        public async Task<int> GetCompletedAppointmentsByDoctorCountAsync(string doctorId)
        {
            return (int)await _appointments.CountDocumentsAsync(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Tamamlandı);
        }

        public async Task<List<AppointmentDto>> GetUpcomingAppointmentsByDoctorAsync(string doctorId, int count)
        {
            var now = DateTime.UtcNow;
            var list = await _appointments.Find(a => a.DoctorId == doctorId && a.AppointmentDate >= now)
                .SortBy(a => a.AppointmentDate)
                .Limit(count)
                .ToListAsync();
            return await MapWithUsers(list);
        }

        public async Task<List<AppointmentDto>> GetRecentAppointmentsAsync(int count)
        {
            var list = await _appointments.Find(_ => true)
                .SortByDescending(a => a.CreatedAt)
                .Limit(count)
                .ToListAsync();
            return await MapWithUsers(list);
        }
    }
}
