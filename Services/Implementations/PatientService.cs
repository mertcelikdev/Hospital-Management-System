using MongoDB.Driver;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    public class PatientService : IPatientService
    {
        private readonly IMongoCollection<Patient> _patients;
    private readonly IMongoCollection<User> _users;

        public PatientService(IMongoDatabase database)
        {
            _patients = database.GetCollection<Patient>("Patients");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _patients.Find(p => !p.IsDeleted)
                .SortBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
            
            return patients.Select(ConvertToDto).ToList();
        }

        public async Task<(List<PatientDto> Items, long Total)> GetPatientsPagedAsync(int page, int pageSize, string? sort, string? dir, string? nameSearch, string? tcSearch, string? letter = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 25 : pageSize;
            var filter = Builders<Patient>.Filter.Eq(p=> p.IsDeleted, false);
            if(!string.IsNullOrWhiteSpace(letter))
            {
                // Harf başlangıcı (FirstName veya LastName) - Türkçe karakter normalize basit: büyük/küçük duyarsız regex
                var l = letter.Trim();
                var rx = new MongoDB.Bson.BsonRegularExpression("^"+System.Text.RegularExpressions.Regex.Escape(l), "i");
                var letterFilter = Builders<Patient>.Filter.Or(
                    Builders<Patient>.Filter.Regex(p=> p.FirstName, rx),
                    Builders<Patient>.Filter.Regex(p=> p.LastName, rx)
                );
                filter &= letterFilter;
            }
            if(!string.IsNullOrWhiteSpace(nameSearch))
            {
                var tokens = nameSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if(tokens.Length == 1)
                {
                    var regexSingle = new MongoDB.Bson.BsonRegularExpression(tokens[0], "i");
                    var singleFilter = Builders<Patient>.Filter.Or(
                        Builders<Patient>.Filter.Regex(p=> p.FirstName, regexSingle),
                        Builders<Patient>.Filter.Regex(p=> p.LastName, regexSingle)
                    );
                    filter &= singleFilter;
                }
                else
                {
                    // Tüm token'lar (boşlukla ayrılmış) FirstName veya LastName içinde bir yerde geçmeli
                    FilterDefinition<Patient>? combined = null;
                    foreach(var t in tokens)
                    {
                        var rx = new MongoDB.Bson.BsonRegularExpression(t, "i");
                        var tokenFilter = Builders<Patient>.Filter.Or(
                            Builders<Patient>.Filter.Regex(p=> p.FirstName, rx),
                            Builders<Patient>.Filter.Regex(p=> p.LastName, rx)
                        );
                        combined = combined == null ? tokenFilter : combined & tokenFilter;
                    }
                    if(combined != null) filter &= combined;
                }
            }
            if(!string.IsNullOrWhiteSpace(tcSearch))
            {
                var tcRegex = new MongoDB.Bson.BsonRegularExpression(tcSearch.Trim(), "i");
                filter &= Builders<Patient>.Filter.Regex(p=> p.IdentityNumber, tcRegex);
            }
            var total = await _patients.CountDocumentsAsync(filter);
            IFindFluent<Patient,Patient> query = _patients.Find(filter);
            bool desc = dir?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = (sort) switch
            {
                "age" => desc? query.SortByDescending(p=> p.DateOfBirth): query.SortBy(p=> p.DateOfBirth),
                "tc" => desc? query.SortByDescending(p=> p.IdentityNumber): query.SortBy(p=> p.IdentityNumber),
                _ => desc? query.SortByDescending(p=> p.FirstName).ThenByDescending(p=> p.LastName): query.SortBy(p=> p.FirstName).ThenBy(p=> p.LastName)
            };
            var list = await query.Skip((page-1)*pageSize).Limit(pageSize).ToListAsync();
            return (list.Select(ConvertToDto).ToList(), total);
        }

        public async Task<PatientDto?> GetPatientByIdAsync(string id)
        {
            var patient = await _patients.Find(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();
            return patient != null ? ConvertToDto(patient) : null;
        }

    public async Task<List<PatientDto>> SearchPatientsAsync(string searchTerm)
        {
            var filter = Builders<Patient>.Filter.Or(
                Builders<Patient>.Filter.Regex(p => p.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Patient>.Filter.Regex(p => p.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Patient>.Filter.Regex(p => p.IdentityNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
        Builders<Patient>.Filter.Regex(p => p.PhoneNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
        filter &= Builders<Patient>.Filter.Eq(p => p.IsDeleted, false);

            var patients = await _patients.Find(filter)
                .SortBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
                
            return patients.Select(ConvertToDto).ToList();
        }

        public async Task CreatePatientAsync(CreatePatientDto createPatientDto)
        {
            var patient = ConvertFromCreateDto(createPatientDto);
            patient.Id = null; // Let MongoDB generate the ID
            patient.CreatedAt = DateTime.UtcNow;
            patient.UpdatedAt = DateTime.UtcNow;
            await _patients.InsertOneAsync(patient);

            // Eş zamanlı User kaydı (Role = Patient) - Email benzersiz kontrolü basit
            if(!string.IsNullOrWhiteSpace(createPatientDto.Email))
            {
                var existing = await _users.Find(u => u.Email == createPatientDto.Email).FirstOrDefaultAsync();
                if(existing == null)
                {
                    var user = new User
                    {
                        Name = $"{createPatientDto.FirstName} {createPatientDto.LastName}".Trim(),
                        FirstName = createPatientDto.FirstName,
                        LastName = createPatientDto.LastName,
                        TcNo = createPatientDto.TcNo,
                        Email = createPatientDto.Email,
                        Phone = createPatientDto.PhoneNumber,
                        Username = createPatientDto.Email,
                        Role = "Patient",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N").Substring(0,8)), // rastgele geçici şifre
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _users.InsertOneAsync(user);
                }
            }
        }

        public async Task<bool> UpdatePatientAsync(string id, UpdatePatientDto updatePatientDto)
        {
            try
            {
                var existingPatient = await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existingPatient == null) return false;

            // Update fields from DTO (with null checks)
            if (!string.IsNullOrEmpty(updatePatientDto.FirstName))
                existingPatient.FirstName = updatePatientDto.FirstName;
            if (!string.IsNullOrEmpty(updatePatientDto.LastName))
                existingPatient.LastName = updatePatientDto.LastName;
            if (!string.IsNullOrEmpty(updatePatientDto.Email))
                existingPatient.Email = updatePatientDto.Email;
            // TC güncelleme
            if(!string.IsNullOrWhiteSpace(updatePatientDto.TcNo) && updatePatientDto.TcNo != existingPatient.IdentityNumber)
            {
                // Uniqueness kontrolü
                var exists = await _patients.CountDocumentsAsync(p=> p.IdentityNumber == updatePatientDto.TcNo && p.Id != id) > 0;
                if(!exists)
                {
                    existingPatient.IdentityNumber = updatePatientDto.TcNo;
                }
                // else: aynı TC başka hastada var -> sessizce yoksay (alternatif: ModelState dışarı taşıma gerek, burada exception atılmıyor)
            }
            if (!string.IsNullOrEmpty(updatePatientDto.PhoneNumber))
                existingPatient.PhoneNumber = updatePatientDto.PhoneNumber;
            if (updatePatientDto.DateOfBirth.HasValue)
                existingPatient.DateOfBirth = updatePatientDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(updatePatientDto.Gender))
                existingPatient.Gender = Enum.Parse<Gender>(updatePatientDto.Gender, true);
            
            existingPatient.Address = updatePatientDto.Address;
            existingPatient.EmergencyContactName = updatePatientDto.EmergencyContactName;
            existingPatient.EmergencyContactPhone = updatePatientDto.EmergencyContactPhone;
            existingPatient.BloodType = updatePatientDto.BloodType;
            
            if (updatePatientDto.Allergies != null && updatePatientDto.Allergies.Any())
                existingPatient.Allergies = string.Join(", ", updatePatientDto.Allergies);
            if (updatePatientDto.ChronicDiseases != null && updatePatientDto.ChronicDiseases.Any())
                existingPatient.ChronicDiseases = string.Join(", ", updatePatientDto.ChronicDiseases);
            
            existingPatient.UpdatedAt = DateTime.UtcNow;

                var result = await _patients.ReplaceOneAsync(p => p.Id == id, existingPatient);

                // Bağlı User kaydını da güncelle (varsa)
                if(result.ModifiedCount > 0)
                {
                    try
                    {
                        var user = await _users.Find(u=> u.TcNo == existingPatient.IdentityNumber || u.Id == existingPatient.Id).FirstOrDefaultAsync();
                        if(user != null)
                        {
                            bool userChanged = false;
                            var fullName = $"{existingPatient.FirstName} {existingPatient.LastName}".Trim();
                            if(user.Name != fullName){ user.Name = fullName; userChanged = true; }
                            if(!string.IsNullOrWhiteSpace(updatePatientDto.TcNo) && user.TcNo != existingPatient.IdentityNumber){ user.TcNo = existingPatient.IdentityNumber; userChanged = true; }
                            if(!string.IsNullOrWhiteSpace(existingPatient.Email) && user.Email != existingPatient.Email){ user.Email = existingPatient.Email; user.Username = existingPatient.Email; userChanged = true; }
                            if(!string.IsNullOrWhiteSpace(existingPatient.PhoneNumber) && user.Phone != existingPatient.PhoneNumber){ user.Phone = existingPatient.PhoneNumber; userChanged = true; }
                            if(userChanged)
                            {
                                user.UpdatedAt = DateTime.UtcNow;
                                await _users.ReplaceOneAsync(u=> u.Id == user.Id, user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UserSyncError] HastaId={id} Hata={ex.Message}\n{ex}");
                    }
                }
                return result.ModifiedCount > 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[PatientUpdateError] HastaId={id} Hata={ex.Message}\n{ex}");
                return false;
            }
        }

        public async Task<bool> DeletePatientAsync(string id, string? userId = null)
        {
            var update = Builders<Patient>.Update
                .Set(p => p.IsDeleted, true)
                .Set(p => p.DeletedAt, DateTime.UtcNow);
            if(!string.IsNullOrWhiteSpace(userId))
                update = update.Set(p => p.DeletedBy, userId);
            var result = await _patients.UpdateOneAsync(p => p.Id == id && !p.IsDeleted, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RestorePatientAsync(string id)
        {
            var update = Builders<Patient>.Update
                .Set(p => p.IsDeleted, false)
                .Unset(p => p.DeletedAt)
                .Unset(p => p.DeletedBy);
            var result = await _patients.UpdateOneAsync(p => p.Id == id && p.IsDeleted, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> HardDeletePatientAsync(string id)
        {
            var result = await _patients.DeleteOneAsync(p => p.Id == id && p.IsDeleted);
            return result.DeletedCount > 0;
        }

        public async Task<List<PatientDto>> GetDeletedPatientsAsync()
        {
            var list = await _patients.Find(p => p.IsDeleted).SortByDescending(p => p.DeletedAt).ToListAsync();
            return list.Select(ConvertToDto).ToList();
        }

        public async Task<bool> PatientExistsAsync(string identityNumber)
        {
            var count = await _patients.CountDocumentsAsync(p => p.IdentityNumber == identityNumber && !p.IsDeleted);
            return count > 0;
        }

        public async Task<PatientDto?> GetPatientByIdentityNumberAsync(string identityNumber)
        {
            var patient = await _patients.Find(p => p.IdentityNumber == identityNumber && !p.IsDeleted).FirstOrDefaultAsync();
            return patient != null ? ConvertToDto(patient) : null;
        }

        public async Task<List<PatientDto>> GetRecentPatientsAsync(int count = 10)
        {
            var patients = await _patients.Find(p => !p.IsDeleted)
                .SortByDescending(p => p.CreatedAt)
                .Limit(count)
                .ToListAsync();
                
            return patients.Select(ConvertToDto).ToList();
        }

        public async Task<int> GetPatientsCountAsync()
        {
            return (int)await _patients.CountDocumentsAsync(p => !p.IsDeleted);
        }

        private PatientDto ConvertToDto(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id ?? string.Empty,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                TcNo = patient.IdentityNumber,
                Email = patient.Email ?? string.Empty,
                PhoneNumber = patient.PhoneNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender.ToString(),
                Address = patient.Address,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                BloodType = patient.BloodType,
                Allergies = string.IsNullOrEmpty(patient.Allergies) ? new List<string>() : 
                           patient.Allergies.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList(),
                ChronicDiseases = string.IsNullOrEmpty(patient.ChronicDiseases) ? new List<string>() :
                                 patient.ChronicDiseases.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList(),
                CurrentMedications = new List<string>(), // Patient model'de yok, boş liste
                InsuranceNumber = string.Empty, // Patient model'de yok
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt ?? DateTime.UtcNow,
                IsDeleted = patient.IsDeleted,
                DeletedAt = patient.DeletedAt,
                DeletedBy = patient.DeletedBy
            };
        }

        private Patient ConvertFromCreateDto(CreatePatientDto dto)
        {
            return new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IdentityNumber = dto.TcNo,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = Enum.Parse<Gender>(dto.Gender, true),
                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                BloodType = dto.BloodType,
                Allergies = dto.Allergies.Any() ? string.Join(", ", dto.Allergies) : string.Empty,
                ChronicDiseases = dto.ChronicDiseases.Any() ? string.Join(", ", dto.ChronicDiseases) : string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
