# Veri / Koleksiyon Notları

Bu dosya seed mekanizması kaldırıldıktan sonra mevcut ve eski koleksiyon durumlarını belgelemek için oluşturuldu.

## 1. Seed Kaldırılması
- `DataSeeder` sınıfı ve `Program.cs` içerisindeki çağrısı kaldırıldı.
- Artık uygulama başlangıcında otomatik kullanıcı / ilaç / randevu / reçete / tedavi verisi oluşturulmaz.

## 2. Koleksiyon İsimleri
| Amaç | Kullanılan Koleksiyon | Not |
|------|-----------------------|-----|
| Kullanıcılar | `Users` | TcNo ve Email için unique index oluşturuluyor (TcNo_Index, Email_Index) |
| İlaçlar | `Medicines` | Daha önce bazı ortamlarda `Medications` adıyla oluşmuş olabilir |
| Randevular | `Appointments` | Eski dokümanlarda ekstra alanlar (örn. Department) tolere ediliyor |
| Reçeteler | `Prescriptions` | |
| Tedaviler | `Treatments` | |
| Hemşire Görevleri | `NurseTasks` | |
| Hasta Notları | `PatientNotes` | |
| Roller / Permissions (varsa) | `Roles`, `Permissions` veya benzeri | Projede ileride eklenecek / sadeleşecek |

## 3. Eski `Medications` Koleksiyonu
Bazı ortamlarda hem `Medications` hem `Medicines` koleksiyonları bulunabiliyordu. Standart olarak `Medicines` kullanılacak.

### Birleştirme / Taşıma Önerisi
Mongo Shell / mongosh örnekleri:
```javascript
// 1. Sayıları kontrol
use HospitalManagementDB;
db.Medications.countDocuments();
db.Medicines.countDocuments();

// 2. Medications -> Medicines kopyala (ID çakışmasını atla)
db.Medications.find().forEach(doc => {
  if (!db.Medicines.findOne({_id: doc._id})) {
    db.Medicines.insert(doc);
  }
});

// 3. Yedeklemek için rename
// db.Medications.renameCollection("Medications_backup");

// 4. Artık gerek yoksa drop
// db.Medications.drop();
```

## 4. Index Kontrol Komutları
```javascript
// Users indexleri
use HospitalManagementDB;
db.Users.getIndexes();

// Medicines indexleri (gerekirse ekleyin)
db.Medicines.getIndexes();
// Örnek index oluşturma
// db.Medicines.createIndex({ Name: 1 });
```

## 5. Yeni Ortam Kurulumu (Seed Yok)
1. Boş veritabanı oluşturulur (connection string yeterli).
2. Uygulama ilk açıldığında kullanıcı yoksa manuel ilk admin eklenmeli (şu an otomatik yok).
3. Admin ekleme seçenekleri:
   - Geçici bir Controller Action (sadece lokal) veya
   - MongoDB üzerinde manuel insert.

Örnek manuel admin insert (mongosh):
```javascript
use HospitalManagementDB;
// Şifre hash'lemek için uygulama kodunu kullanmanız gerekir; burada placeholder
// Gerçek hash'i uygulamadan alıp koyun
const now = new Date();
db.Users.insertOne({
  Name: "Sistem Yöneticisi",
  Role: "Admin",
  Email: "admin@hospital.com",
  Phone: "000",
  PasswordHash: "<BCryptHash>",
  CreatedAt: now,
  UpdatedAt: now
});
```

## 6. Eski Seed Verisinin Geri Yüklenmesi (İhtiyaç Olursa)
- Bu dosyadaki örnek listeler `DataSeeder` içindeki başlangıç içeriklerine dayanır.
- Geri yüklemek için ilgili DTO create servislerini sırayla çağıran küçük bir script / geçici endpoint yazılabilir.

## 7. Yapılabilecek Ek İyileştirmeler
- Koleksiyon adlarını konfigürasyona taşımak (appsettings.json > MongoDbSettings).
- İlaç adına unique veya case-insensitive index eklemek.
- Eski backup koleksiyonları için TTL (time to live) ya da manuel temizleme planı.

---
Bu belge operasyon / bakım amaçlıdır. Güncelledikçe commit'leyin.
