using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly IMongoCollection<Treatment> _treatments;

        public TreatmentService(IMongoDatabase database)
        {
            _treatments = database.GetCollection<Treatment>("Treatments");
        }

        public async Task<List<TreatmentDto>> GetAllTreatmentsAsync()
        {
            var list = await _treatments.Find(_ => true).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<TreatmentDto> GetTreatmentByIdAsync(string id)
        {
            var entity = await _treatments.Find(t => t.Id == id).FirstOrDefaultAsync();
            return entity == null ? new TreatmentDto() : ToDto(entity);
        }

        public async Task<List<TreatmentDto>> GetPatientTreatmentsAsync(string patientId)
        {
            var list = await _treatments.Find(t => t.PatientId == patientId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<TreatmentDto>> GetDoctorTreatmentsAsync(string doctorId)
        {
            var list = await _treatments.Find(t => t.DoctorId == doctorId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<TreatmentDto>> GetNurseTreatmentsAsync(string nurseId)
        {
            var list = await _treatments.Find(t => t.NurseId == nurseId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<TreatmentDto>> GetActiveTreatmentsAsync()
        {
            var list = await _treatments.Find(t => t.Status == TreatmentStatus.InProgress || t.Status == TreatmentStatus.Planned).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<TreatmentDto> CreateTreatmentAsync(CreateTreatmentDto createTreatmentDto)
        {
            var entity = FromCreateDto(createTreatmentDto);
            await _treatments.InsertOneAsync(entity);
            return ToDto(entity);
        }

        public async Task UpdateTreatmentAsync(string id, UpdateTreatmentDto updateTreatmentDto)
        {
            var existing = await _treatments.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (existing == null) return;

            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.TreatmentName))
                existing.TreatmentName = updateTreatmentDto.TreatmentName;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Type) && Enum.TryParse<TreatmentType>(updateTreatmentDto.Type, true, out var tType))
                existing.Type = tType;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Description))
                existing.Description = updateTreatmentDto.Description;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Instructions))
                existing.Instructions = updateTreatmentDto.Instructions;
            TreatmentStatus parsedStatus;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Status) && Enum.TryParse<TreatmentStatus>(updateTreatmentDto.Status, true, out parsedStatus))
                existing.Status = parsedStatus;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.NurseId))
                existing.NurseId = updateTreatmentDto.NurseId;
            if (updateTreatmentDto.StartDate.HasValue)
                existing.StartDate = updateTreatmentDto.StartDate.Value;
            if (updateTreatmentDto.EndDate.HasValue)
                existing.EndDate = updateTreatmentDto.EndDate.Value;
            if (updateTreatmentDto.CompletedDate.HasValue && existing.Status == TreatmentStatus.Completed)
                existing.EndDate = updateTreatmentDto.CompletedDate.Value;
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Results))
                existing.Notes = (existing.Notes + "\nResults: " + updateTreatmentDto.Results).Trim();
            if (!string.IsNullOrWhiteSpace(updateTreatmentDto.Notes))
                existing.Notes = updateTreatmentDto.Notes;

            existing.UpdatedAt = DateTime.UtcNow;
            await _treatments.ReplaceOneAsync(t => t.Id == id, existing);
        }

        public async Task DeleteTreatmentAsync(string id)
        {
            await _treatments.DeleteOneAsync(t => t.Id == id);
        }

        public async Task UpdateTreatmentStatusAsync(string id, TreatmentStatus status)
        {
            var update = Builders<Treatment>.Update
                .Set(t => t.Status, status)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            await _treatments.UpdateOneAsync(t => t.Id == id, update);
        }

        public async Task AssignNurseAsync(string treatmentId, string nurseId)
        {
            var update = Builders<Treatment>.Update
                .Set(t => t.NurseId, nurseId)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            await _treatments.UpdateOneAsync(t => t.Id == treatmentId, update);
        }

        // Mapping helpers
        private TreatmentDto ToDto(Treatment t)
        {
            return new TreatmentDto
            {
                Id = t.Id ?? string.Empty,
                PatientId = t.PatientId,
                DoctorId = t.DoctorId,
                NurseId = t.NurseId,
                TreatmentName = t.TreatmentName,
                Type = t.Type.ToString(),
                Description = t.Description,
                Instructions = t.Instructions,
                Status = t.Status.ToString(),
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Notes = t.Notes
            };
        }

        private Treatment FromCreateDto(CreateTreatmentDto dto)
        {
            return new Treatment
            {
                PatientId = dto.PatientId,
                DoctorId = string.Empty, // Auth context'ten alınmalı
                NurseId = dto.NurseId,
                TreatmentName = dto.TreatmentName,
                Type = Enum.TryParse<TreatmentType>(dto.Type, true, out var tType) ? tType : TreatmentType.Other,
                Description = dto.Description,
                Instructions = dto.Instructions,
                Status = TreatmentStatus.Planned,
                StartDate = dto.StartDate ?? DateTime.UtcNow,
                EndDate = dto.EndDate,
                Notes = dto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
