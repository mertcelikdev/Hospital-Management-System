using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Data;

public static class DoctorAndPatientSeeder
{
    private static readonly string[] MaleFirstNames = {"Ahmet","Mehmet","Ali","Mustafa","Murat","Hakan","Emre","Can","Kemal","Berk","Selim","Serkan","Fatih","Burak","Eren"};
    private static readonly string[] FemaleFirstNames = {"Ayşe","Fatma","Elif","Zeynep","Merve","Derya","Selin","Buse","Gökçe","İrem","Naz","Seda","Esra","Gül","Deniz"};
    private static readonly string[] LastNames = {"Yılmaz","Demir","Şahin","Çelik","Yıldız","Yalçın","Aslan","Kaya","Öztürk","Aydın","Arslan","Doğan","Kurt","Koç","Polat","Eren"};
    private static readonly string[] BloodTypes = {"A+","A-","B+","B-","AB+","AB-","0+","0-"};

    public static async Task EnsureDoctorsAndPatientsAsync(IMongoDatabase db, int doctorsPerDepartment = 6, int targetPatients = 60, Action<string>? log = null)
    {
        var userCol = db.GetCollection<User>("Users");
        var deptCol = db.GetCollection<Department>("Departments");
        var patientCol = db.GetCollection<Patient>("Patients");

        var departments = await deptCol.Find(_ => true).ToListAsync();
        var now = DateTime.UtcNow;

        int createdDoctors = 0;
        foreach (var dept in departments)
        {
            var existingCount = await userCol.CountDocumentsAsync(u => u.Role == "Doctor" && u.DepartmentId == dept.Id);
            if (existingCount >= doctorsPerDepartment) continue;
            int needed = doctorsPerDepartment - (int)existingCount;
            for (int i = 0; i < needed; i++)
            {
                var gender = (i % 2 == 0) ? Gender.Male : Gender.Female;
                var first = gender == Gender.Male ? MaleFirstNames[Random.Shared.Next(MaleFirstNames.Length)] : FemaleFirstNames[Random.Shared.Next(FemaleFirstNames.Length)];
                var last = LastNames[Random.Shared.Next(LastNames.Length)];
                var spec = MakeSpecialization(dept.Name);
                var email = await MakeUniqueEmail(userCol, $"{dept.Code?.ToLower() ?? "dept"}doc", dept.Id!, i);
                var tc = await MakeUniqueTc(userCol);
                var license = $"LIC-{dept.Code}-{Random.Shared.Next(100,999)}-{Random.Shared.Next(100,999)}";
                var username = email.Split('@')[0];
                var user = new User
                {
                    Name = $"Dr. {first} {last}",
                    FirstName = first,
                    LastName = last,
                    Username = username,
                    Email = email,
                    Phone = MakePhone(),
                    TcNo = tc,
                    Role = "Doctor",
                    DepartmentId = dept.Id,
                    Specialization = spec,
                    LicenseNumber = license,
                    Gender = gender,
                    DateOfBirth = RandomDate(1965, 1995),
                    HireDate = RandomDate(2015, 2024),
                    Shift = PickShift(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await userCol.InsertOneAsync(user);
                createdDoctors++;
            }
        }
        if (createdDoctors > 0) log?.Invoke($"Doctor seed: {createdDoctors} yeni doktor eklendi."); else log?.Invoke("Doctor seed: Ek ihtiyacı yok.");

        // Patients
        var existingPatients = await patientCol.CountDocumentsAsync(p => !p.IsDeleted);
        int needPatients = targetPatients - (int)existingPatients;
        int createdPatients = 0;
        while (needPatients > 0)
        {
            var gender = (createdPatients % 2 == 0) ? Gender.Male : Gender.Female;
            var first = gender == Gender.Male ? MaleFirstNames[Random.Shared.Next(MaleFirstNames.Length)] : FemaleFirstNames[Random.Shared.Next(FemaleFirstNames.Length)];
            var last = LastNames[Random.Shared.Next(LastNames.Length)];
            var identity = await MakeUniquePatientIdentity(patientCol);
            var email = $"patient{Guid.NewGuid().ToString("N").Substring(0,6)}@hospital.local";
            var patient = new Patient
            {
                FirstName = first,
                LastName = last,
                IdentityNumber = identity,
                Email = email,
                PhoneNumber = MakePhone(),
                DateOfBirth = RandomDate(1945, 2018),
                Gender = gender,
                Address = "Otomatik oluşturulmuş adres",
                EmergencyContactName = "Acil Kişi",
                EmergencyContactPhone = MakePhone(),
                BloodType = BloodTypes[Random.Shared.Next(BloodTypes.Length)],
                Allergies = Random.Shared.Next(0,3) == 0 ? "" : "Penisilin",
                ChronicDiseases = Random.Shared.Next(0,4) == 0 ? "Hipertansiyon" : "",
                CreatedAt = now,
                UpdatedAt = now
            };
            await patientCol.InsertOneAsync(patient);

            // eşleşen user yoksa ekle
            var userExists = await userCol.CountDocumentsAsync(u => u.Email == patient.Email) > 0;
            if(!userExists)
            {
                var user = new User
                {
                    Name = $"{patient.FirstName} {patient.LastName}",
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    Username = patient.Email!,
                    Email = patient.Email!,
                    Phone = patient.PhoneNumber,
                    TcNo = patient.IdentityNumber,
                    Role = "Patient",
                    Gender = patient.Gender,
                    DateOfBirth = patient.DateOfBirth,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await userCol.InsertOneAsync(user);
            }
            createdPatients++;
            needPatients--;
        }
        if (createdPatients > 0) log?.Invoke($"Patient seed: {createdPatients} yeni hasta eklendi."); else log?.Invoke("Patient seed: Ek hasta ihtiyacı yok.");

        // Özet
        try
        {
            var totalDoctors = await userCol.CountDocumentsAsync(u => u.Role == "Doctor");
            var totalPatients = await patientCol.CountDocumentsAsync(p => !p.IsDeleted);
            log?.Invoke($"Seed Özet => Departman: {departments.Count}, Doktor: {totalDoctors} (hedef 234), Hasta: {totalPatients} (hedef 60)");
        }
        catch { /* log yoksa yut */ }
    }

    private static DateTime RandomDate(int startYear, int endYear)
    {
        var year = Random.Shared.Next(startYear, endYear + 1);
        var month = Random.Shared.Next(1, 13);
        var day = Random.Shared.Next(1, DateTime.DaysInMonth(year, month) + 1);
        return new DateTime(year, month, day);
    }

    private static string PickShift() => Random.Shared.Next(3) switch {0 => "Morning",1=>"Evening",_=>"Night"};

    private static async Task<string> MakeUniqueEmail(IMongoCollection<User> userCol, string prefix, string deptId, int index)
    {
        for(int attempt=0; attempt<50; attempt++)
        {
            var email = $"{prefix}{index}{(attempt>0?attempt.ToString():string.Empty)}@hospital.local";
            var exists = await userCol.CountDocumentsAsync(u => u.Email == email) > 0;
            if(!exists) return email;
        }
        return $"{prefix}{Guid.NewGuid().ToString("N").Substring(0,5)}@hospital.local";
    }

    private static async Task<string> MakeUniqueTc(IMongoCollection<User> userCol)
    {
        for(int attempt=0; attempt<200; attempt++)
        {
            var tc = GenerateTc();
            var exists = await userCol.CountDocumentsAsync(u => u.TcNo == tc) > 0;
            if(!exists) return tc;
        }
        return GenerateTc();
    }

    private static async Task<string> MakeUniquePatientIdentity(IMongoCollection<Patient> patCol)
    {
        for(int attempt=0; attempt<200; attempt++)
        {
            var id = GenerateTc();
            var exists = await patCol.CountDocumentsAsync(p => p.IdentityNumber == id) > 0;
            if(!exists) return id;
        }
        return GenerateTc();
    }

    private static string GenerateTc()
    {
        // Basit random 11 haneli (gerçek algoritma yok – seed için yeterli)
        return string.Concat(Enumerable.Range(0,11).Select(_=> Random.Shared.Next(0,10).ToString()));
    }

    private static string MakePhone()
    {
        return "05" + string.Concat(Enumerable.Range(0,9).Select(_=> Random.Shared.Next(0,10).ToString()));
    }

    private static string MakeSpecialization(string departmentName)
    {
        var spec = departmentName.Split('(')[0].Trim();
        if(spec.Length > 80) spec = spec.Substring(0,80);
        return spec;
    }
}
