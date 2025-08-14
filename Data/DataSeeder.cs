using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Implementations;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Data
{
    public class DataSeeder
    {
    private readonly IUserService _userService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly ITreatmentService _treatmentService;
    private readonly IMedicationService _medicationService;
    private readonly IDepartmentService _departmentService;

        public DataSeeder(
            IUserService userService,
            IAppointmentService appointmentService,
            IPrescriptionService prescriptionService,
            ITreatmentService treatmentService,
            IMedicationService medicationService,
            IDepartmentService departmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
            _prescriptionService = prescriptionService;
            _treatmentService = treatmentService;
            _medicationService = medicationService;
            _departmentService = departmentService;
        }

        public async Task SeedAsync()
        {
            Console.WriteLine("[Seed] Başladı");
            await MigrateLegacyAppointmentsAsync();
            await EnsureUsersAsync();
            await EnsureDepartmentsAsync();
            await EnsureMedicationsAsync();
            await EnsureAppointmentsAsync();
            await EnsureTreatmentsAsync();
            await EnsurePrescriptionsAsync();
            Console.WriteLine("[Seed] Bitti");
        }

        // Eski enum/int depolanmış Appointment.Status / Type alanlarını string'e çevir
        private async Task MigrateLegacyAppointmentsAsync()
        {
            try
            {
                var mongo = (_appointmentService as AppointmentService);
                if (mongo == null) return; // concrete erişim yoksa geç
                // Reflection ile private _appointments alanına eriş
                var field = typeof(AppointmentService).GetField("_appointments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field == null) return;
                var collection = field.GetValue(_appointmentService) as MongoDB.Driver.IMongoCollection<Appointment>;
                if (collection == null) return;

                // Int Status veya Type içeren belgeleri ham BSON ile bul
                var rawCollection = collection.Database.GetCollection<MongoDB.Bson.BsonDocument>("Appointments");
                var cursor = await rawCollection.FindAsync(new MongoDB.Bson.BsonDocument(), new MongoDB.Driver.FindOptions<MongoDB.Bson.BsonDocument>{ Limit = 200 });
                var list = new List<MongoDB.Bson.BsonDocument>();
                while (await cursor.MoveNextAsync())
                {
                    list.AddRange(cursor.Current);
                }
                int updated = 0;
                foreach (var doc in list)
                {
                    bool changed = false;
                    // Status int ise
                    if (doc.Contains("Status") && doc["Status"].BsonType == MongoDB.Bson.BsonType.Int32)
                    {
                        var intVal = doc["Status"].AsInt32;
                        doc["Status"] = MapLegacyStatus(intVal);
                        changed = true;
                    }
                    // Type int ise
                    if (doc.Contains("Type") && doc["Type"].BsonType == MongoDB.Bson.BsonType.Int32)
                    {
                        var intVal = doc["Type"].AsInt32;
                        doc["Type"] = MapLegacyType(intVal);
                        changed = true;
                    }
                    if (changed)
                    {
                        var id = doc["_id"].AsObjectId;
                        await rawCollection.ReplaceOneAsync(new MongoDB.Bson.BsonDocument("_id", id), doc);
                        updated++;
                    }
                }
                if (updated > 0) Console.WriteLine($"[Seed][MIGRATE] {updated} appointment dönüştürüldü");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Seed][MIGRATE] Hata: " + ex.Message);
            }
        }

        private string MapLegacyStatus(int v) => v switch
        {
            0 => "Planlandı",
            1 => "Onaylandı",
            2 => "DevamEdiyor",
            3 => "Tamamlandı",
            4 => "İptalEdildi",
            5 => "Gelmedi",
            _ => "Planlandı"
        };

        private string MapLegacyType(int v) => v switch
        {
            0 => "Muayene",
            1 => "Kontrol",
            2 => "Acil",
            3 => "Ameliyat",
            4 => "İnceleme",
            5 => "CheckUp",
            6 => "Tedavi",
            _ => "Muayene"
        };

        private async Task EnsureUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users.Any()) { Console.WriteLine($"[Seed][Users] Var (count={users.Count}) – atlandı"); return; }
            await SeedUsersAsync();
            Console.WriteLine("[Seed][Users] 7 kullanıcı eklendi");
        }

        private async Task EnsureDepartmentsAsync()
        {
            var list = await _departmentService.GetAllDepartmentsAsync();
            if (list.Count >= 5) { Console.WriteLine($"[Seed][Departments] Var (count={list.Count}) – atlandı"); return; }
            await SeedDepartmentsAsync();
            Console.WriteLine("[Seed][Departments] 5 departman eklendi");
        }

        private async Task EnsureMedicationsAsync()
        {
            var meds = await _medicationService.GetAllMedicationsAsync();
            if (meds.Count >= 5) { Console.WriteLine($"[Seed][Medications] Var (count={meds.Count}) – atlandı"); return; }
            await SeedMedicationsAsync();
            Console.WriteLine("[Seed][Medications] 5 ilaç eklendi");
        }

        private async Task EnsureAppointmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var doctor = users.FirstOrDefault(u => u.Role == "Doctor");
            if (doctor == null) { Console.WriteLine("[Seed][Appointments] Doktor yok – atlandı"); return; }
            var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
            if (allAppointments.Count >= 5) { Console.WriteLine($"[Seed][Appointments] Var (count={allAppointments.Count}) – atlandı"); return; }
            await SeedAppointmentsAsync();
            Console.WriteLine("[Seed][Appointments] 5 randevu eklendi");
        }

        private async Task EnsureTreatmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            if (!users.Any()) return;
            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            if (treatments.Count >= 5) { Console.WriteLine($"[Seed][Treatments] Var (count={treatments.Count}) – atlandı"); return; }
            await SeedTreatmentsAsync();
            Console.WriteLine("[Seed][Treatments] 5 tedavi eklendi");
        }

        private async Task EnsurePrescriptionsAsync()
        {
            // Şimdilik gerçek implementasyon yoksa sadece log
            Console.WriteLine("[Seed][Prescriptions] Placeholder");
            await SeedPrescriptionsAsync();
        }

    private async Task SeedDepartmentsAsync()
        {
            var departments = new List<Department>
            {
                new Department { Name = "Kardiyoloji", Code = "KARD", Description = "Kalp ve damar" },
                new Department { Name = "Nöroloji", Code = "NEUR", Description = "Sinir sistemi" },
                new Department { Name = "Ortopedi", Code = "ORTH", Description = "Kas iskelet" },
                new Department { Name = "Pediatri", Code = "PED", Description = "Çocuk sağlığı" },
                new Department { Name = "Genel Cerrahi", Code = "SURG", Description = "Cerrahi işlemler" }
            };
            // Duplicate engelle
            foreach (var d in departments)
            {
                // Department servisinde duplicate kontrol metodu yok; basit ekle (tekrar insert tolere edilebilir)
                try { await _departmentService.CreateDepartmentAsync(d); } catch { }
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                // Admin
                new User
                {
                    Name = "Sistem Yöneticisi",
                    Username = "admin",
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    TcNo = "12345678901",
                    Email = "admin@hms.local",
                    Phone = "0532 111 1111",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1980, 5, 15),
                    Address = "Ankara Merkez",
                    EmergencyContact = "0532 111 1112",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Doktor
                new User
                {
                    Name = "Dr. Mehmet Öztürk",
                    Username = "drmehmet",
                    FirstName = "Mehmet",
                    LastName = "Öztürk",
                    TcNo = "12345678902",
                    Email = "doctor@hms.local",
                    Phone = "0532 222 2222",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                    Role = "Doctor",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1975, 8, 20),
                    Address = "İstanbul Beşiktaş",
                    EmergencyContact = "0532 222 2223",
                    LicenseNumber = "DOC123456",
                    Specialization = "Kardiyoloji",
                    HireDate = new DateTime(2020, 1, 15),
                    Shift = "Morning",
                        DoctorDepartment = "Kardiyoloji",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Hemşire
                new User
                {
                    Name = "Hemşire Ayşe Kaya",
                    Username = "ayse",
                    FirstName = "Ayşe",
                    LastName = "Kaya",
                    TcNo = "12345678903",
                    Email = "nurse@hms.local",
                    Phone = "0532 333 3333",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("nurse123"),
                    Role = "Nurse",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1985, 3, 10),
                    Address = "Ankara Çankaya",
                    EmergencyContact = "0532 333 3334",
                    LicenseNumber = "NUR789012",
                    HireDate = new DateTime(2021, 6, 1),
                    Shift = "Evening",
                        DoctorDepartment = "Hemşirelik",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Personel
                new User
                {
                    Name = "Personel Ali Demir",
                    Username = "ali",
                    FirstName = "Ali",
                    LastName = "Demir",
                    TcNo = "12345678904",
                    Email = "staff@hms.local",
                    Phone = "0532 444 4444",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                    Role = "Staff",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1990, 7, 25),
                    Address = "İzmir Konak",
                    EmergencyContact = "0532 444 4445",
                    HireDate = new DateTime(2022, 3, 15),
                    Shift = "Morning",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Hasta 1
                new User
                {
                    Name = "Hasta Fatma Yılmaz",
                    Username = "fatma",
                    FirstName = "Fatma",
                    LastName = "Yılmaz",
                    TcNo = "12345678905",
                    Email = "patient1@example.com",
                    Phone = "0532 555 5555",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                    Role = "Patient",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1960, 12, 5),
                    Address = "İstanbul Kadıköy",
                    EmergencyContact = "0532 555 5556",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                
                // Hasta 2
                new User
                {
                    Name = "Hasta Ahmet Çelik",
                    Username = "ahmet",
                    FirstName = "Ahmet",
                    LastName = "Çelik",
                    TcNo = "12345678906",
                    Email = "patient2@example.com",
                    Phone = "0532 666 6666",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                    Role = "Patient",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1945, 4, 18),
                    Address = "Ankara Kızılay",
                    EmergencyContact = "0532 666 6667",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                
                // Hasta 3
                new User
                {
                    Name = "Hasta Zeynep Arslan",
                    Username = "zeynep",
                    FirstName = "Zeynep",
                    LastName = "Arslan",
                    TcNo = "12345678907",
                    Email = "patient3@example.com",
                    Phone = "0532 777 7777",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                    Role = "Patient",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1995, 9, 30),
                    Address = "İzmir Bornova",
                    EmergencyContact = "0532 777 7778",
                    // IsActive kaldırıldı,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            foreach (var user in users)
            {
                await _userService.CreateUserAsync(user);
            }

            // Hasta-Doktor-Hemşire atamalarını yap
            var allUsers = await _userService.GetAllUsersAsync();
            var doctor = allUsers.First(u => u.Role == "Doctor");
            var nurse = allUsers.First(u => u.Role == "Nurse");
            var patients = allUsers.Where(u => u.Role == "Patient").ToList();

            // Kullanıcı atama metotları interface'te bulunmadığı için kaldırıldı.
        }

        private async Task SeedAppointmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var doctor = users.FirstOrDefault(u => u.Role == "Doctor");
            var patients = users.Where(u => u.Role == "Patient").Take(5).ToList();
            if (doctor == null || patients.Count < 1) return;

            var baseDate = DateTime.Today.AddDays(1);
            var list = new List<Appointment>();
            // 5 adet oluştur
            list.Add(new Appointment { PatientId = patients[0].Id!, DoctorId = doctor.Id!, AppointmentDate = baseDate.AddHours(9), Type = "Muayene", Status = "Planlandı", Notes = "Muayene" });
            if (patients.Count > 1) list.Add(new Appointment { PatientId = patients[1].Id!, DoctorId = doctor.Id!, AppointmentDate = baseDate.AddDays(1).AddHours(10), Type = "Kontrol", Status = "Planlandı", Notes = "Kontrol" });
            if (patients.Count > 2) list.Add(new Appointment { PatientId = patients[2].Id!, DoctorId = doctor.Id!, AppointmentDate = baseDate.AddDays(2).AddHours(11), Type = "CheckUp", Status = "Onaylandı", Notes = "CheckUp" });
            if (patients.Count > 0) list.Add(new Appointment { PatientId = patients[0].Id!, DoctorId = doctor.Id!, AppointmentDate = baseDate.AddDays(-2).AddHours(14), Type = "Tedavi", Status = "Tamamlandı", Notes = "Tamamlandı" });
            if (patients.Count > 1) list.Add(new Appointment { PatientId = patients[1].Id!, DoctorId = doctor.Id!, AppointmentDate = baseDate.AddDays(5).AddHours(15), Type = "Acil", Status = "Planlandı", Notes = "Acil durum" });

            foreach (var a in list)
            {
                await _appointmentService.CreateAppointmentAsync(a);
            }
        }

        private async Task SeedPrescriptionsAsync()
        {
            // Basit placeholder: 5 reçete (ilaç + hasta + doktor bağlanması ileride implement edilebilir)
            // Şu an prescription modeli / servis implementasyonu yoksa atla
            try
            {
                // Varsayım: PrescriptionService içinde CreatePrescriptionAsync metodu olacak (yoksa skip)
                // Reflection veya dynamic çağrı yerine try-catch ile yoksay
                // await _prescriptionService.CreatePrescriptionAsync(...);
            }
            catch { }
            await Task.CompletedTask;
        }

        private async Task SeedTreatmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var medicines = await _medicationService.GetAllMedicationsAsync();
            var doctor = users.FirstOrDefault(u => u.Role == "Doctor");
            var nurse = users.FirstOrDefault(u => u.Role == "Nurse");
            var patients = users.Where(u => u.Role == "Patient").Take(5).ToList();
            if (doctor == null || nurse == null || patients.Count == 0 || medicines.Count == 0) return;

            var list = new List<Treatment>();
            list.Add(new Treatment { PatientId = patients[0].Id!, DoctorId = doctor.Id!, NurseId = nurse.Id!, TreatmentName = "Rehab", Type = TreatmentType.Rehabilitation, Description = "Rehab programı", Instructions = "Egzersiz", Status = TreatmentStatus.InProgress, StartDate = DateTime.Today.AddDays(-3), Duration = 20, MedicineIds = new(){ medicines[0].Id! }, Notes = "İlerleme iyi" });
            if (patients.Count > 1 && medicines.Count > 1) list.Add(new Treatment { PatientId = patients[1].Id!, DoctorId = doctor.Id!, NurseId = nurse.Id!, TreatmentName = "Antibiyotik", Type = TreatmentType.Medication, Description = "Antibiyotik", Instructions = "Düzenli al", Status = TreatmentStatus.Planned, StartDate = DateTime.Today.AddDays(1), Duration = 7, MedicineIds = new(){ medicines[1].Id! }, Notes = "Takip" });
            if (patients.Count > 2) list.Add(new Treatment { PatientId = patients[2].Id!, DoctorId = doctor.Id!, NurseId = nurse.Id!, TreatmentName = "Fizik Tedavi", Type = TreatmentType.Therapy, Description = "Fiziksel terapi", Instructions = "Seanslara katıl", Status = TreatmentStatus.InProgress, StartDate = DateTime.Today.AddDays(-1), Duration = 15, MedicineIds = new(), Notes = "Stabil" });
            if (patients.Count > 3 && medicines.Count > 2) list.Add(new Treatment { PatientId = patients[3].Id!, DoctorId = doctor.Id!, NurseId = nurse.Id!, TreatmentName = "İlaç Düzeni", Type = TreatmentType.Medication, Description = "İlaç ayarı", Instructions = "Sabah/Akşam", Status = TreatmentStatus.Planned, StartDate = DateTime.Today, Duration = 30, MedicineIds = new(){ medicines[2].Id! }, Notes = "Planlandı" });
            if (patients.Count > 4 && medicines.Count > 3) list.Add(new Treatment { PatientId = patients[4].Id!, DoctorId = doctor.Id!, NurseId = nurse.Id!, TreatmentName = "Kontrol Programı", Type = TreatmentType.Monitoring, Description = "Gözlem", Instructions = "Değerleri gir", Status = TreatmentStatus.Planned, StartDate = DateTime.Today.AddDays(2), Duration = 5, MedicineIds = new(){ medicines[3].Id! }, Notes = "Takip edilecek" });

            foreach (var t in list)
            {
                await _treatmentService.CreateTreatmentAsync(t);
            }
        }

        private async Task SeedMedicationsAsync()
        {
            var medications = new List<Medication>
            {
                new Medication
                {
                    Name = "Paracetamol",
                    Description = "Ağrı kesici ve ateş düşürücü ilaç",
                    Manufacturer = "Pfizer",
                    Dosage = "500mg",
                    Unit = "tablet",
                    StockQuantity = 1000,
                    MinimumStockLevel = 50,
                    ExpiryDate = DateTime.Now.AddYears(2),
                    BatchNumber = "PAR2024001",
                    CostPerUnit = 0.25m,
                    Instructions = "Yemeklerden sonra alınız. Günde en fazla 4 tablet.",
                    SideEffects = new List<string> { "Mide bulantısı", "Baş dönmesi" },
                    Contraindications = new List<string> { "Karaciğer hastalığı", "Alkol bağımlılığı" }
                },
                
                new Medication
                {
                    Name = "Amoksisilin",
                    Description = "Geniş spektrumlu antibiyotik",
                    Manufacturer = "Novartis",
                    Dosage = "500mg",
                    Unit = "kapsül",
                    StockQuantity = 500,
                    MinimumStockLevel = 30,
                    ExpiryDate = DateTime.Now.AddMonths(18),
                    BatchNumber = "AMX2024002",
                    CostPerUnit = 1.50m,
                    Instructions = "8 saatte bir, 7 gün boyunca kullanın. Yemekle birlikte alın.",
                    SideEffects = new List<string> { "İshal", "Alerjik reaksiyon", "Mide bulantısı" },
                    Contraindications = new List<string> { "Penisilin alerjisi", "Mononükleoz" }
                },
                
                new Medication
                {
                    Name = "İbuprofen",
                    Description = "Nonsteroid antiinflamatuar ilaç",
                    Manufacturer = "Johnson & Johnson",
                    Dosage = "400mg",
                    Unit = "tablet",
                    StockQuantity = 800,
                    MinimumStockLevel = 40,
                    ExpiryDate = DateTime.Now.AddYears(3),
                    BatchNumber = "IBU2024003",
                    CostPerUnit = 0.35m,
                    Instructions = "Ağrı olduğunda günde en fazla 3 tablet. Yemekle birlikte alın.",
                    SideEffects = new List<string> { "Mide ağrısı", "Baş ağrısı", "Mide kanaması" },
                    Contraindications = new List<string> { "Mide ülseri", "Böbrek hastalığı", "Kalp hastalığı" }
                },
                
                new Medication
                {
                    Name = "Insulin",
                    Description = "Diyabet tedavisinde kullanılan hormon",
                    Manufacturer = "Novo Nordisk",
                    Dosage = "100IU/ml",
                    Unit = "vial",
                    StockQuantity = 200,
                    MinimumStockLevel = 20,
                    ExpiryDate = DateTime.Now.AddMonths(12),
                    BatchNumber = "INS2024004",
                    CostPerUnit = 25.00m,
                    Instructions = "Doktor tarafından belirlenen dozda subkutan enjeksiyon yapın.",
                    SideEffects = new List<string> { "Hipoglisemi", "Enjeksiyon bölgesinde reaksiyon", "Kilo alımı" },
                    Contraindications = new List<string> { "Hipoglisemi atakları", "İnsülin alerjisi" }
                },
                
                new Medication
                {
                    Name = "Aspirin",
                    Description = "Kalp koruyucu düşük doz aspirin",
                    Manufacturer = "Bayer",
                    Dosage = "100mg",
                    Unit = "tablet",
                    StockQuantity = 1500,
                    MinimumStockLevel = 100,
                    ExpiryDate = DateTime.Now.AddYears(4),
                    BatchNumber = "ASP2024005",
                    CostPerUnit = 0.15m,
                    Instructions = "Günde 1 tablet, yemekle birlikte alın.",
                    SideEffects = new List<string> { "Mide tahrişi", "Kanama eğilimi", "Kulak çınlaması" },
                    Contraindications = new List<string> { "Kanama bozukluğu", "Mide ülseri", "Aspirin alerjisi" }
                }
            };

            foreach (var medication in medications)
            {
                await _medicationService.CreateMedicationAsync(medication);
            }
        }
    }
}
