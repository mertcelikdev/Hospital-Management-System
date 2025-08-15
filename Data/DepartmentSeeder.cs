using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Data
{
    public static class DepartmentSeeder
    {
    private static readonly (string Name, string Code, string Description)[] DefaultDepartments = new[]
        {
            ("Acil Servis","ACIL","24 saat acil sağlık hizmetleri"),
            ("Dahiliye (İç Hastalıkları)","DAH","Yetişkin iç hastalıkları tanı ve tedavi"),
            ("Kardiyoloji","KARD","Kalp ve damar hastalıkları tanı ve tedavi"),
            ("Göğüs Hastalıkları","GOGUS","Solunum sistemi hastalıkları"),
            ("Nöroloji","NORO","Beyin ve sinir sistemi hastalıkları"),
            ("Beyin ve Sinir Cerrahisi","BSC","Nöroşirürji operasyonları"),
            ("Ortopedi ve Travmatoloji","ORTO","Kas-iskelet sistemi ve kırıklar"),
            ("Genel Cerrahi","GENCER","Genel cerrahi girişimler"),
            ("Kardiyovasküler Cerrahi","KVC","Kalp damar cerrahisi"),
            ("Plastik, Rekonstrüktif ve Estetik Cerrahi","PLAST","Rekonstrüksiyon ve estetik cerrahi"),
            ("Göz Hastalıkları","GOZ","Oftalmoloji"),
            ("Kulak Burun Boğaz","KBB","KBB ve baş-boyun hastalıkları"),
            ("Üroloji","URO","Ürogenital sistem hastalıkları"),
            ("Kadın Hastalıkları ve Doğum","KADIN","Obstetrik ve jinekoloji"),
            ("Çocuk Sağlığı ve Hastalıkları","COCUK","Pediatri"),
            ("Çocuk Cerrahisi","COCCER","Pediatrik cerrahi"),
            ("Dermatoloji","DERM","Cilt hastalıkları"),
            ("Endokrinoloji","ENDO","Hormon ve metabolizma hastalıkları"),
            ("Gastroenteroloji","GAST","Sindirim sistemi hastalıkları"),
            ("Hematoloji","HEM","Kan hastalıkları"),
            ("Onkoloji","ONK","Kanser tanı ve tedavi"),
            ("Nefroloji","NEFR","Böbrek hastalıkları"),
            ("Enfeksiyon Hastalıkları","ENF","İnfeksiyonların tanı ve tedavisi"),
            ("Fizik Tedavi ve Rehabilitasyon","FTR","Fiziksel rehabilitasyon"),
            ("Psikiyatri","PSK","Ruh sağlığı hastalıkları"),
            ("Psikoloji","PSKLG","Psikolojik danışmanlık"),
            ("Radyoloji","RAD","Görüntüleme hizmetleri"),
            ("Anestezi ve Reanimasyon","ANES","Anestezi ve yoğun bakım desteği"),
            ("Yoğun Bakım","YB","Kritik hasta izlemi"),
            ("Algoloji (Ağrı Merkezi)","ALGO","Kronik ağrı tedavisi"),
            ("Fertilite ve Tüp Bebek","IVF","Üreme sağlığı"),
            ("Beslenme ve Diyet","DIYET","Beslenme danışmanlığı"),
            ("Ağız ve Diş Sağlığı","DIS","Diş hekimliği"),
            ("Patoloji","PAT","Doku ve hücre incelemeleri"),
            ("Genetik","GEN","Genetik tanı"),
            ("Transfüzyon Merkezi","KAN","Kan bankası"),
            ("Laboratuvar","LAB","Tıbbi analiz laboratuvarı"),
            ("Eczane","ECZ","İlaç yönetimi"),
            ("Ameliyathane Hizmetleri","AMEL","Cerrahi operasyon alanları")
        };

        public static async Task EnsureSeededAsync(IMongoDatabase db, Action<string>? log = null)
        {
            var col = db.GetCollection<Department>("Departments");
            var existingNames = await col.Find(_ => true).Project(d => d.Name).ToListAsync();
            var now = DateTime.UtcNow;
            var toInsert = new List<Department>();
            foreach (var (name, code, desc) in DefaultDepartments)
            {
                if (!existingNames.Contains(name))
                {
                    toInsert.Add(new Department
                    {
                        Name = name,
                        Code = code,
                        Description = desc,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }
            if (toInsert.Count > 0)
            {
                await col.InsertManyAsync(toInsert);
                log?.Invoke($"Department seed: {toInsert.Count} yeni departman eklendi.");
            }
            else
            {
                log?.Invoke("Department seed: Yeni eklenecek departman yok.");
            }
        }
    }
}
