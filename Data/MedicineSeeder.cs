using MongoDB.Driver;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Data;

public static class MedicineSeeder
{
    private static readonly string[] Manufacturers =
    {
        "Pfizer","Novartis","Roche","Sanofi","AstraZeneca","GSK","Bayer","Merck","Johnson & Johnson","AbbVie",
        "Takeda","Amgen","Eli Lilly","Novo Nordisk","Biogen","Gilead","Teva","MSD","Boehringer Ingelheim","Bausch Health"
    };

    private static readonly (string Name,string Generic,string Form,string Strength, MedicineCategory Cat)[] BaseItems = new[]
    {
        ("Parol","Paracetamol","Tablet","500 mg", MedicineCategory.Painkiller),
        ("Aferin","Paracetamol + Fenilefrin","Tablet","500 mg", MedicineCategory.Painkiller),
        ("Augmentin","Amoksisilin + Klavulanat","Tablet","875/125 mg", MedicineCategory.Antibiotic),
        ("Majezik","Flurbiprofen","Tablet","100 mg", MedicineCategory.Painkiller),
        ("Arveles","Dexketoprofen","Tablet","25 mg", MedicineCategory.Painkiller),
        ("Nurofen","İbuprofen","Tablet","400 mg", MedicineCategory.Painkiller),
        ("Dolven","İbuprofen","Şurup","100 mg/5ml", MedicineCategory.Painkiller),
        ("Ventolin","Salbutamol","İnhaler","100 mcg", MedicineCategory.Other),
        ("Konazol","Ketokonazol","Krem","2%", MedicineCategory.Other),
        ("Bepanthol","Dexpanthenol","Merhem","5%", MedicineCategory.Other),
        ("Ferro Sanol","Demir (II)" ,"Kapsül","100 mg", MedicineCategory.Supplement),
        ("Devit-3","Vitamin D3","Damla","400 IU", MedicineCategory.Vitamin),
        ("Centrum","Multivitamin","Tablet","", MedicineCategory.Vitamin),
        ("Supradyn","Multivitamin","Tablet","", MedicineCategory.Vitamin),
        ("Nexium","Esomeprazol","Tablet","40 mg", MedicineCategory.Other),
        ("Losec","Omeprazol","Kapsül","20 mg", MedicineCategory.Other),
        ("Glifor","Metformin","Tablet","1000 mg", MedicineCategory.Other),
        ("Leponex","Klozapin","Tablet","100 mg", MedicineCategory.Other),
        ("Xanax","Alprazolam","Tablet","0.5 mg", MedicineCategory.Other),
        ("Zyrtec","Setirizin","Tablet","10 mg", MedicineCategory.Other)
    };

    public static async Task EnsureSeededAsync(IMongoDatabase db, int targetCount = 120, Action<string>? log = null)
    {
        var col = db.GetCollection<Medicine>("Medicines");
        var existing = await col.CountDocumentsAsync(_ => true);
        if (existing >= targetCount)
        {
            log?.Invoke($"Medicine seed: zaten {existing} kayıt var, eklenmedi.");
            return;
        }

        var rnd = new Random();
        var list = new List<Medicine>();
        // Önce base items ekle (varsa tekrar etmeyecek şekilde isim kontrolü)
        var existingNames = await col.Find(_ => true).Project(m => m.Name).ToListAsync();
        foreach(var b in BaseItems)
        {
            if(existingNames.Contains(b.Name)) continue;
            list.Add(MakeEntity(b.Name,b.Generic,b.Form,b.Strength,b.Cat,rnd));
        }

        // Geri kalanları türet
        var forms = new[]{"Tablet","Kapsül","Şurup","Ampul","Merhem","Damla","İnhaler"};
        var strengths = new[]{"5 mg","10 mg","20 mg","40 mg","100 mg","250 mg","500 mg","850 mg","1000 mg","5%","2%","400 IU","100 mg/5ml"};
        var genericPool = new[]{"Paracetamol","İbuprofen","Amoksisilin","Klavulanik Asit","Metformin","Esomeprazol","Omeprazol","Salbutamol","Ketokonazol","Dexpanthenol","Demir","Vitamin D3","Multivitamin","Setirizin","Alprazolam","Klozapin","Flurbiprofen","Dexketoprofen"};

        int needed = targetCount - (int)existing - list.Count;
        int safety = 0;
        while(needed > 0 && safety < 10000)
        {
            safety++;
            var gen = genericPool[rnd.Next(genericPool.Length)];
            var form = forms[rnd.Next(forms.Length)];
            var strength = strengths[rnd.Next(strengths.Length)];
            var cat = (MedicineCategory)rnd.Next(Enum.GetValues(typeof(MedicineCategory)).Length);
            var baseName = gen.Split(' ')[0];
            var name = baseName + "-" + RandomSuffix(rnd,3);
            if(await col.CountDocumentsAsync(m=> m.Name == name) > 0 || list.Any(l=> l.Name == name)) continue;
            list.Add(MakeEntity(name, gen, form, strength, cat, rnd));
            needed--;
        }

        if(list.Count > 0)
        {
            await col.InsertManyAsync(list);
            log?.Invoke($"Medicine seed: {list.Count} yeni ilaç eklendi (toplam hedef {targetCount}).");
        }
        else
        {
            log?.Invoke("Medicine seed: eklenmedi (hedef dolu ya da üretim başarısız).");
        }
    }

    private static Medicine MakeEntity(string name,string generic,string form,string strength, MedicineCategory cat, Random rnd)
    {
        return new Medicine
        {
            Name = name,
            GenericName = generic,
            Manufacturer = Manufacturers[rnd.Next(Manufacturers.Length)],
            Form = form,
            Strength = strength,
            Category = cat,
            Description = $"{name} için otomatik açıklama.",
            Instructions = "Günde 2 kez aç veya tok karnına.",
            Price = Math.Round((decimal)(rnd.NextDouble()*200 + 10),2),
            StockQuantity = rnd.Next(20,500),
            MinimumStock = rnd.Next(5,25),
            ExpiryDate = DateTime.UtcNow.AddDays(rnd.Next(60, 900)),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string RandomSuffix(Random rnd, int len)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0,len).Select(_=> chars[rnd.Next(chars.Length)]).ToArray());
    }
}
