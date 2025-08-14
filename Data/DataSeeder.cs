using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.Data
{
    public class DataSeeder
    {
        private readonly UserService _userService;
        private readonly AppointmentService _appointmentService;
        private readonly MedicineService _medicineService;
        private readonly PrescriptionService _prescriptionService;
        private readonly TreatmentService _treatmentService;

        public DataSeeder(
            UserService userService,
            AppointmentService appointmentService,
            MedicineService medicineService,
            PrescriptionService prescriptionService,
            TreatmentService treatmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
            _medicineService = medicineService;
            _prescriptionService = prescriptionService;
            _treatmentService = treatmentService;
        }

        public async Task SeedAsync()
        {
            // Önce mevcut veri var mı kontrol et
            var existingUsers = await _userService.GetAllUsersAsync();
            if (existingUsers.Any())
            {
                return; // Zaten veri var, seed etme
            }

            // 1. Kullanıcıları ekle
            await SeedUsersAsync();
            
            // 2. İlaçları ekle
            await SeedMedicinesAsync();
            
            // 3. Randevuları ekle
            await SeedAppointmentsAsync();
            
            // 4. Reçeteleri ekle
            await SeedPrescriptionsAsync();
            
            // 5. Tedavileri ekle
            await SeedTreatmentsAsync();
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                // Admin
                new User
                {
                    Name = "Sistem Yöneticisi",
                    Email = "admin@hospital.com",
                    Phone = "0532 111 1111",
                    Role = "Admin",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1980, 5, 15),
                    Address = "Ankara Merkez",
                    EmergencyContact = "0532 111 1112",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Doktor
                new User
                {
                    Name = "Dr. Mehmet Öztürk",
                    Email = "doctor@hospital.com",
                    Phone = "0532 222 2222",
                    Role = "Doctor",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1975, 8, 20),
                    Address = "İstanbul Beşiktaş",
                    EmergencyContact = "0532 222 2223",
                    LicenseNumber = "DOC123456",
                    Specialization = "Kardiyoloji",
                    HireDate = new DateTime(2020, 1, 15),
                    Shift = "Morning",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Hemşire
                new User
                {
                    Name = "Hemşire Ayşe Kaya",
                    Email = "nurse@hospital.com",
                    Phone = "0532 333 3333",
                    Role = "Nurse",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1985, 3, 10),
                    Address = "Ankara Çankaya",
                    EmergencyContact = "0532 333 3334",
                    LicenseNumber = "NUR789012",
                    HireDate = new DateTime(2021, 6, 1),
                    Shift = "Evening",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Personel
                new User
                {
                    Name = "Personel Ali Demir",
                    Email = "staff@hospital.com",
                    Phone = "0532 444 4444",
                    Role = "Staff",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1990, 7, 25),
                    Address = "İzmir Konak",
                    EmergencyContact = "0532 444 4445",
                    HireDate = new DateTime(2022, 3, 15),
                    Shift = "Morning",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Hasta 1
                new User
                {
                    Name = "Hasta Fatma Yılmaz",
                    Email = "patient1@example.com",
                    Phone = "0532 555 5555",
                    Role = "Patient",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1960, 12, 5),
                    Address = "İstanbul Kadıköy",
                    EmergencyContact = "0532 555 5556",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                
                // Hasta 2
                new User
                {
                    Name = "Hasta Ahmet Çelik",
                    Email = "patient2@example.com",
                    Phone = "0532 666 6666",
                    Role = "Patient",
                    Gender = Gender.Male,
                    DateOfBirth = new DateTime(1945, 4, 18),
                    Address = "Ankara Kızılay",
                    EmergencyContact = "0532 666 6667",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                
                // Hasta 3
                new User
                {
                    Name = "Hasta Zeynep Arslan",
                    Email = "patient3@example.com",
                    Phone = "0532 777 7777",
                    Role = "Patient",
                    Gender = Gender.Female,
                    DateOfBirth = new DateTime(1995, 9, 30),
                    Address = "İzmir Bornova",
                    EmergencyContact = "0532 777 7778",
                    IsActive = true,
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

            foreach (var patient in patients)
            {
                await _userService.AssignPatientToDoctorAsync(patient.Id!, doctor.Id!);
                await _userService.AssignPatientToNurseAsync(patient.Id!, nurse.Id!);
            }
        }

        private async Task SeedMedicinesAsync()
        {
            var medicines = new List<Medicine>
            {
                new Medicine
                {
                    Name = "Paracetamol 500mg",
                    GenericName = "Paracetamol",
                    Manufacturer = "Eczacıbaşı",
                    Form = "Tablet",
                    Strength = "500mg",
                    Category = MedicineCategory.Painkiller,
                    Description = "Ağrı kesici ve ateş düşürücü",
                    Instructions = "Günde 3 kez, yemeklerden sonra alınız",
                    Price = 15.50m,
                    StockQuantity = 100,
                    ExpiryDate = DateTime.UtcNow.AddMonths(18),
                    CreatedAt = DateTime.UtcNow
                },
                
                new Medicine
                {
                    Name = "Amoksisilin 1000mg",
                    GenericName = "Amoxicillin",
                    Manufacturer = "Pfizer",
                    Form = "Tablet",
                    Strength = "1000mg",
                    Category = MedicineCategory.Antibiotic,
                    Description = "Geniş spektrumlu antibiyotik",
                    Instructions = "Günde 2 kez, 12 saat arayla alınız",
                    Price = 45.75m,
                    StockQuantity = 50,
                    ExpiryDate = DateTime.UtcNow.AddMonths(24),
                    CreatedAt = DateTime.UtcNow
                },
                
                new Medicine
                {
                    Name = "Vitamin D3 1000 IU",
                    GenericName = "Cholecalciferol",
                    Manufacturer = "Solgar",
                    Form = "Damla",
                    Strength = "1000 IU",
                    Category = MedicineCategory.Vitamin,
                    Description = "D vitamini takviyesi",
                    Instructions = "Günde 1 kez, 5 damla",
                    Price = 28.90m,
                    StockQuantity = 30,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    CreatedAt = DateTime.UtcNow
                },
                
                new Medicine
                {
                    Name = "İbuprofen 400mg",
                    GenericName = "Ibuprofen",
                    Manufacturer = "Bayer",
                    Form = "Tablet",
                    Strength = "400mg",
                    Category = MedicineCategory.Painkiller,
                    Description = "Non-steroid antienflamatuar",
                    Instructions = "Günde 3 kez, yemekle birlikte",
                    Price = 22.30m,
                    StockQuantity = 8, // Düşük stok örneği
                    ExpiryDate = DateTime.UtcNow.AddMonths(15),
                    CreatedAt = DateTime.UtcNow
                },
                
                new Medicine
                {
                    Name = "Omega-3 1000mg",
                    GenericName = "Fish Oil",
                    Manufacturer = "Nature Made",
                    Form = "Kapsül",
                    Strength = "1000mg",
                    Category = MedicineCategory.Supplement,
                    Description = "Balık yağı takviyesi",
                    Instructions = "Günde 1 kez, yemekle birlikte",
                    Price = 65.40m,
                    StockQuantity = 25,
                    ExpiryDate = DateTime.UtcNow.AddMonths(20),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var medicine in medicines)
            {
                await _medicineService.CreateMedicineAsync(medicine);
            }
        }

        private async Task SeedAppointmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var doctor = users.First(u => u.Role == "Doctor");
            var patients = users.Where(u => u.Role == "Patient").ToList();

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    PatientId = patients[0].Id!,
                    DoctorId = doctor.Id!,
                    AppointmentDate = DateTime.Today.AddDays(1).AddHours(9),
                    Type = AppointmentType.Muayene,
                    Status = AppointmentStatus.Planlandı,
                    Notes = "Rutin kontrol muayenesi",
                    CreatedAt = DateTime.UtcNow
                },
                
                new Appointment
                {
                    PatientId = patients[1].Id!,
                    DoctorId = doctor.Id!,
                    AppointmentDate = DateTime.Today.AddDays(2).AddHours(14),
                    Type = AppointmentType.CheckUp,
                    Status = AppointmentStatus.Planlandı,
                    Notes = "Kalp kontrolü",
                    CreatedAt = DateTime.UtcNow
                },
                
                new Appointment
                {
                    PatientId = patients[2].Id!,
                    DoctorId = doctor.Id!,
                    AppointmentDate = DateTime.Today.AddDays(-5).AddHours(10),
                    Type = AppointmentType.Tedavi,
                    Status = AppointmentStatus.Tamamlandı,
                    Notes = "Tedavi tamamlandı",
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                
                new Appointment
                {
                    PatientId = patients[0].Id!,
                    DoctorId = doctor.Id!,
                    AppointmentDate = DateTime.Today.AddDays(7).AddHours(11),
                    Type = AppointmentType.Kontrol,
                    Status = AppointmentStatus.Planlandı,
                    Notes = "Kontrol randevusu",
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var appointment in appointments)
            {
                await _appointmentService.CreateAppointmentAsync(appointment);
            }
        }

        private async Task SeedPrescriptionsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var medicines = await _medicineService.GetAllMedicinesAsync();
            var doctor = users.First(u => u.Role == "Doctor");
            var nurse = users.First(u => u.Role == "Nurse");
            var patients = users.Where(u => u.Role == "Patient").ToList();

            var prescriptions = new List<Prescription>
            {
                new Prescription
                {
                    PatientId = patients[0].Id!,
                    DoctorId = doctor.Id!,
                    NurseId = nurse.Id!,
                    Diagnosis = "Hipertansiyon",
                    Medicines = new List<PrescriptionMedicine>
                    {
                        new PrescriptionMedicine
                        {
                            MedicineId = medicines[0].Id!,
                            MedicineName = medicines[0].Name,
                            Dosage = "500mg",
                            Frequency = "Günde 2 kez",
                            Duration = 7,
                            Instructions = "Yemeklerden sonra alınız"
                        }
                    },
                    Notes = "İlacı düzenli kullanınız",
                    Status = PrescriptionStatus.Active,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                },
                
                new Prescription
                {
                    PatientId = patients[1].Id!,
                    DoctorId = doctor.Id!,
                    NurseId = nurse.Id!,
                    Diagnosis = "Enfeksiyon",
                    Medicines = new List<PrescriptionMedicine>
                    {
                        new PrescriptionMedicine
                        {
                            MedicineId = medicines[1].Id!,
                            MedicineName = medicines[1].Name,
                            Dosage = "1000mg",
                            Frequency = "Günde 2 kez",
                            Duration = 10,
                            Instructions = "12 saat arayla alınız"
                        }
                    },
                    Notes = "Antibiyotik tedavisini tamamlayınız",
                    Status = PrescriptionStatus.Active,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(10),
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var prescription in prescriptions)
            {
                await _prescriptionService.CreatePrescriptionAsync(prescription);
            }
        }

        private async Task SeedTreatmentsAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var medicines = await _medicineService.GetAllMedicinesAsync();
            var doctor = users.First(u => u.Role == "Doctor");
            var nurse = users.First(u => u.Role == "Nurse");
            var patients = users.Where(u => u.Role == "Patient").ToList();

            var treatments = new List<Treatment>
            {
                new Treatment
                {
                    PatientId = patients[0].Id!,
                    DoctorId = doctor.Id!,
                    NurseId = nurse.Id!,
                    TreatmentName = "Kalp Rehabilitasyonu",
                    Type = TreatmentType.Rehabilitation,
                    Description = "Kardiyak rehabilitasyon programı",
                    Instructions = "Haftada 3 gün, 45 dakika egzersiz",
                    Status = TreatmentStatus.InProgress,
                    StartDate = DateTime.Today.AddDays(-7),
                    Duration = 30,
                    MedicineIds = new List<string> { medicines[0].Id! },
                    Notes = "İlerlemesi iyi",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                
                new Treatment
                {
                    PatientId = patients[1].Id!,
                    DoctorId = doctor.Id!,
                    NurseId = nurse.Id!,
                    TreatmentName = "Antibiyotik Tedavisi",
                    Type = TreatmentType.Medication,
                    Description = "Sistemik enfeksiyon tedavisi",
                    Instructions = "İlaçları düzenli kullanın",
                    Status = TreatmentStatus.Planned,
                    StartDate = DateTime.Today.AddDays(1),
                    Duration = 10,
                    MedicineIds = new List<string> { medicines[1].Id! },
                    Notes = "Yan etkiler takip edilecek",
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var treatment in treatments)
            {
                await _treatmentService.CreateTreatmentAsync(treatment);
            }
        }
    }
}
