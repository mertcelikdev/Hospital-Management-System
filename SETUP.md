# Hospital Management System - Local Development Setup

## 🔧 İlk Kurulum (Sadece bir kez)

Projeyi klonladıktan sonra:

```bash
# 1. Hassas ayar dosyalarını oluştur
copy appsettings.Example.json appsettings.json
copy appsettings.Example.json appsettings.Development.json
```

## 🔑 appsettings.json Dosyasını Düzenle

Aşağıdaki ayarları kendi bilgilerinle güncelle:

```json
{
  "MongoDB": {
    "ConnectionString": "SENİN_MONGODB_ATLAS_CONNECTION_STRING",
    "DatabaseName": "HospitalManagementDB"
  },
  "Jwt": {
    "Key": "SENİN_GÜÇLÜ_JWT_ANAHTARIN_MINIMUM_32_KARAKTER",
    "Issuer": "HospitalManagementSystem",
    "Audience": "HospitalUsers",
    "ExpireDays": 30
  }
}
```

## ✅ Güvenlik Garantisi

✅ `appsettings.json` - Git tarafından takip edilmiyor  
✅ `appsettings.Development.json` - Git tarafından takip edilmiyor  
✅ `.env` dosyaları - Git tarafından takip edilmiyor  
✅ `*.key`, `*.pem` - Git tarafından takip edilmiyor  

## 🚀 Çalıştırma

```bash
dotnet restore
dotnet build
dotnet run
```

## 🔒 GitHub'a Yüklerken

Hassas dosyalar otomatik olarak .gitignore tarafından korunur:
- ❌ `appsettings.json` → GitHub'a GİTMEZ
- ❌ `appsettings.Development.json` → GitHub'a GİTMEZ  
- ✅ `appsettings.Example.json` → GitHub'a gider (template olarak)

## 🌟 Ekip Çalışması

Yeni geliştirici projeye katıldığında:
1. Projeyi klonlar
2. `appsettings.Example.json`'ı kopyalar
3. Kendi MongoDB ve JWT ayarlarını girer
4. Çalıştırır

Bu sayede herkesin kendi ayarları olur ama kimse diğerinin hassas bilgilerini görmez.
