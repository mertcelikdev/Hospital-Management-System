using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IMongoCollection<Prescription> _prescriptions;

        public PrescriptionService(IMongoDatabase database)
        {
            _prescriptions = database.GetCollection<Prescription>("Prescriptions");
        }
        
        public async Task<List<PrescriptionDto>> GetAllPrescriptionsAsync()
        {
            var list = await _prescriptions.Find(_ => true).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PrescriptionDto> GetPrescriptionByIdAsync(string id)
        {
            var entity = await _prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
            return entity == null ? new PrescriptionDto() : ToDto(entity);
        }

        public async Task<List<PrescriptionDto>> GetPatientPrescriptionsAsync(string patientId)
        {
            var list = await _prescriptions.Find(p => p.PatientId == patientId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<PrescriptionDto>> GetDoctorPrescriptionsAsync(string doctorId)
        {
            var list = await _prescriptions.Find(p => p.DoctorId == doctorId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<PrescriptionDto>> GetNursePrescriptionsAsync(string nurseId)
        {
            var list = await _prescriptions.Find(p => p.NurseId == nurseId).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<List<PrescriptionDto>> GetActivePrescriptionsAsync()
        {
            var list = await _prescriptions.Find(p => p.Status == PrescriptionStatus.Active).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto createPrescriptionDto)
        {
            var entity = FromCreateDto(createPrescriptionDto);
            await _prescriptions.InsertOneAsync(entity);
            return ToDto(entity);
        }

        public async Task UpdatePrescriptionAsync(string id, UpdatePrescriptionDto updatePrescriptionDto)
        {
            var existing = await _prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existing == null) return;

            if (!string.IsNullOrWhiteSpace(updatePrescriptionDto.Diagnosis))
                existing.Diagnosis = updatePrescriptionDto.Diagnosis;
            if (!string.IsNullOrWhiteSpace(updatePrescriptionDto.Notes))
                existing.Notes = updatePrescriptionDto.Notes;
            if (!string.IsNullOrWhiteSpace(updatePrescriptionDto.Status) && Enum.TryParse<PrescriptionStatus>(updatePrescriptionDto.Status, true, out var status))
                existing.Status = status;
            if (!string.IsNullOrWhiteSpace(updatePrescriptionDto.NurseId))
                existing.NurseId = updatePrescriptionDto.NurseId;
            if (updatePrescriptionDto.StartDate.HasValue)
                existing.StartDate = updatePrescriptionDto.StartDate;
            if (updatePrescriptionDto.EndDate.HasValue)
                existing.EndDate = updatePrescriptionDto.EndDate;
            if (updatePrescriptionDto.Medicines != null && updatePrescriptionDto.Medicines.Any())
            {
                existing.Medicines = updatePrescriptionDto.Medicines.Select(m => new PrescriptionMedicine
                {
                    MedicineId = m.MedicineId,
                    MedicineName = string.Empty, // İlaç ismini başka servisten doldurabilirsin
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration,
                    Instructions = m.Instructions
                }).ToList();
            }

            existing.UpdatedAt = DateTime.UtcNow;
            await _prescriptions.ReplaceOneAsync(p => p.Id == id, existing);
        }

        public async Task DeletePrescriptionAsync(string id)
        {
            await _prescriptions.DeleteOneAsync(p => p.Id == id);
        }

        public async Task UpdatePrescriptionStatusAsync(string id, PrescriptionStatus status)
        {
            var update = Builders<Prescription>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _prescriptions.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task AssignNurseAsync(string prescriptionId, string nurseId)
        {
            var update = Builders<Prescription>.Update
                .Set(p => p.NurseId, nurseId)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _prescriptions.UpdateOneAsync(p => p.Id == prescriptionId, update);
        }

        // Mapping helpers
        private PrescriptionDto ToDto(Prescription p)
        {
            return new PrescriptionDto
            {
                Id = p.Id ?? string.Empty,
                PatientId = p.PatientId,
                DoctorId = p.DoctorId,
                NurseId = p.NurseId,
                Diagnosis = p.Diagnosis,
                Medicines = p.Medicines.Select(m => new PrescriptionMedicineDto
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration,
                    Instructions = m.Instructions
                }).ToList(),
                Notes = p.Notes,
                Status = p.Status.ToString(),
                PrescriptionDate = p.PrescriptionDate,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        private Prescription FromCreateDto(CreatePrescriptionDto dto)
        {
            return new Prescription
            {
                PatientId = dto.PatientId,
                DoctorId = string.Empty, // Auth context'ten set edilmeli
                NurseId = dto.NurseId,
                Diagnosis = dto.Diagnosis,
                Medicines = dto.Medicines.Select(m => new PrescriptionMedicine
                {
                    MedicineId = m.MedicineId,
                    MedicineName = string.Empty,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration,
                    Instructions = m.Instructions
                }).ToList(),
                Notes = dto.Notes ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = PrescriptionStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
