# Hospital Management System

Modern, web tabanlı bir hastane yönetim sistemi. .NET 9 MVC pattern'i kullanılarak geliştirilmiş, MongoDB NoSQL veritabanı ile entegre çalışan kapsamlı bir sağlık yönetim platformudur.

## 🏥 Özellikler

### Kullanıcı Yönetimi
- **Rol Tabanlı Erişim Kontrolü**: Hasta, Doktor, Hemşire, Personel ve Admin rolleri
- **Güvenli Kimlik Doğrulama**: JWT token tabanlı güvenlik
- **Profil Yönetimi**: Kullanıcı bilgileri güncelleme ve şifre değiştirme

### Hasta Yönetimi
- 👤 **Hasta Kayıtları**: Detaylı hasta bilgileri ve demografik veriler
- 📅 **Randevu Planlama**: Esnek randevu sistemi ve doktor müsaitlik kontrolü
- 📋 **Muayene Geçmişi**: Tıbbi kayıtlar ve tedavi geçmişi
- 💊 **Reçete Yönetimi**: Dijital reçete oluşturma ve takibi
- 🔬 **Test Sonuçları**: Laboratuvar test sonuçları ve raporları

### Doktor ve Hemşire Modülleri
- 📊 **Görev Takvimi**: Kişisel program ve vardiya yönetimi
- 👥 **Hasta Listesi**: Sorumlu olunan hasta kayıtları
- 📝 **Tedavi Kayıtları**: Muayene notları ve tedavi planları
- 💉 **İlaç Yönetimi**: Reçete yazma ve ilaç takibi

### Çalışan Yönetimi
- 👨‍⚕️ **Personel Listesi**: Tüm hastane personeli kayıtları
- ⏰ **Vardiya Planlama**: Esnek çalışma saatleri yönetimi
- 🏖️ **İzin Takibi**: İzin talepleri ve onay süreçleri
- 📈 **Performans Takibi**: Çalışan performans değerlendirmeleri

### İlaç Yönetimi
- 💊 **Stok Takibi**: Gerçek zamanlı ilaç stok kontrolü
- 📥 **Giriş-Çıkış İşlemleri**: Detaylı ilaç hareketleri
- 📍 **Kullanım Takibi**: Hangi hastada, nerede, ne zaman kullanıldığı
- ⚠️ **Düşük Stok Uyarıları**: Otomatik stok alarm sistemi
- 📊 **Raporlama**: Kapsamlı ilaç kullanım raporları

## 🛠️ Teknoloji Stack'i

### Backend
- **.NET 9**: Modern web application framework
- **ASP.NET Core MVC**: Model-View-Controller pattern
- **MongoDB.Driver**: NoSQL veritabanı bağlantısı
- **JWT Authentication**: Token tabanlı güvenlik
- **BCrypt**: Şifre hashleme
- **Swagger/OpenAPI**: API dokümantasyonu

### Frontend
- **Bootstrap 5**: Responsive UI framework
- **Font Awesome**: İkon kütüphanesi
- **jQuery**: JavaScript library
- **Razor Pages**: Server-side rendering

### Veritabanı
- **MongoDB**: NoSQL document database
- **Referanssal İlişkiler**: Koleksiyon arası bağlantılar

## 🚀 Kurulum

### Ön Gereksinimler
- .NET 9 SDK
- MongoDB (local veya cloud)
- Visual Studio Code / Visual Studio

### Adımlar

1. **Proje Klonlama**
   ```bash
   git clone [repository-url]
   cd HospitalManagementSystem
   ```

2. **MongoDB Kurulumu**
   - MongoDB'yi lokal olarak kurun veya MongoDB Atlas kullanın
   - Bağlantı string'ini `appsettings.json` dosyasında güncelleyin

3. **Paket Kurulumu**
   ```bash
   dotnet restore
   ```

4. **Projeyi Çalıştırma**
   ```bash
   dotnet run
   ```

5. **Tarayıcıda Açma**
   - https://localhost:5001 adresine gidin
   - Login sayfasından sisteme giriş yapın

## 📋 API Endpoints

### Kimlik Doğrulama
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/change-password` - Şifre değiştirme

### Randevular
- `GET /api/appointments` - Randevu listesi
- `POST /api/appointments` - Yeni randevu
- `PUT /api/appointments/{id}` - Randevu güncelleme
- `DELETE /api/appointments/{id}` - Randevu silme

### İlaçlar
- `GET /api/medicines` - İlaç listesi
- `POST /api/medicines` - Yeni ilaç ekleme
- `PUT /api/medicines/{id}` - İlaç güncelleme
- `POST /api/medicines/transactions` - İlaç hareketi

## 👥 Kullanıcı Rolleri

### 🏥 Admin
- Tüm sistem yönetimi
- Kullanıcı ekleme/silme
- Raporlama ve analitik

### 👨‍⚕️ Doktor
- Hasta muayenesi
- Reçete yazma
- Tıbbi kayıt oluşturma

### 👩‍⚕️ Hemşire
- Hasta bakımı
- İlaç uygulama
- Vital takibi

### 👨‍💼 Personel
- İlaç stok yönetimi
- Randevu planlama
- Operasyonel işlemler

### 🤒 Hasta
- Randevu alma
- Tıbbi geçmiş görüntüleme
- Test sonuçları

## 🔒 Güvenlik Özellikleri

- **JWT Token Authentication**: Güvenli API erişimi
- **Rol Tabanlı Yetkilendirme**: Endpoint seviyesinde güvenlik
- **Password Hashing**: BCrypt ile şifre güvenliği
- **Session Management**: Güvenli oturum yönetimi
- **Input Validation**: Veri doğrulama ve sanitization

## 📊 Veritabanı Yapısı

### Koleksiyonlar
- **Users**: Kullanıcı bilgileri
- **Appointments**: Randevu kayıtları
- **MedicalRecords**: Tıbbi kayıtlar
- **Medicines**: İlaç bilgileri
- **MedicineTransactions**: İlaç hareketleri
- **Prescriptions**: Reçete kayıtları
- **TestResults**: Test sonuçları
- **Departments**: Bölümler
- **WorkSchedules**: Çalışma programları
- **LeaveRequests**: İzin talepleri

## 🎨 UI/UX Özellikleri

- **Responsive Design**: Mobil uyumlu arayüz
- **Modern UI**: Bootstrap 5 ile şık tasarım
- **Kolay Navigasyon**: Sezgisel menü yapısı
- **Gerçek Zamanlı Feedback**: Toast bildirimleri
- **Accessibility**: Erişilebilirlik standartları

## 📈 Gelecek Geliştirmeler

- [ ] Real-time bildirimler (SignalR)
- [ ] Mobil uygulama (Xamarin/MAUI)
- [ ] Telemedicine entegrasyonu
- [ ] AI destekli tanı önerileri
- [ ] IoT cihaz entegrasyonu
- [ ] Blockchain tabanlı hasta kayıtları

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## � Güvenlik Notları

⚠️ **ÖNEMLİ**: Aşağıdaki dosyalar hassas bilgiler içerir ve GitHub'a yüklenmemelidir:

### Hassas Dosyalar:
- `appsettings.json` - Üretim ayarları
- `appsettings.Development.json` - Geliştirme ayarları  
- `appsettings.Production.json` - Üretim ortamı ayarları
- `.env` dosyaları - Çevre değişkenleri
- `*.key`, `*.pem` - SSL sertifikaları ve şifreleme anahtarları
- `secrets.json` - Gizli anahtarlar

### Kurulum için:
1. `appsettings.Example.json` dosyasını kopyalayın
2. Kendi `appsettings.json` dosyanızı oluşturun
3. MongoDB bağlantı string'inizi ve JWT anahtarınızı güncelleyin

```bash
# Windows
copy appsettings.Example.json appsettings.json

# Linux/Mac  
cp appsettings.Example.json appsettings.json
```

Bu dosyalar `.gitignore` ile korunmaktadır.

## �📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 📞 İletişim

Proje hakkında sorularınız için GitHub Issues kullanabilirsiniz.

---

**Hospital Management System** - Modern sağlık hizmetleri için kapsamlı yönetim platformu 🏥
