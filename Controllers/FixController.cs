using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace HospitalManagementSystem.Controllers
{
    public class FixController : Controller
    {
        private readonly IMongoDatabase _database;

        public FixController(IMongoDatabase database)
        {
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> ClearAppointments()
        {
            try
            {
                var appointmentsCollection = _database.GetCollection<MongoDB.Bson.BsonDocument>("Appointments");
                var result = await appointmentsCollection.DeleteManyAsync(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Empty);
                
                return Json(new { 
                    success = true, 
                    deletedCount = result.DeletedCount,
                    message = $"{result.DeletedCount} appointment silindi. Artık /Appointment sayfasına gidebilirsin."
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MigrateAppointments()
        {
            // Eski belgelerde Status / Type int ise string'e çevir
            var col = _database.GetCollection<MongoDB.Bson.BsonDocument>("Appointments");
            var docs = await col.Find(Builders<MongoDB.Bson.BsonDocument>.Filter.Empty).ToListAsync();
            int migrated = 0;
            foreach (var d in docs)
            {
                bool changed = false;
                if (d.Contains("Status") && d["Status"].BsonType == MongoDB.Bson.BsonType.Int32)
                {
                    var v = d["Status"].AsInt32;
                    d["Status"] = MapStatus(v);
                    changed = true;
                }
                if (d.Contains("Type") && d["Type"].BsonType == MongoDB.Bson.BsonType.Int32)
                {
                    var t = d["Type"].AsInt32;
                    d["Type"] = MapType(t);
                    changed = true;
                }
                if (changed)
                {
                    migrated++;
                    await col.ReplaceOneAsync(Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", d["_id"]), d);
                }
            }
            return Json(new { success = true, migrated });
        }

        [HttpGet]
        public async Task<IActionResult> DropAppointments()
        {
            await _database.DropCollectionAsync("Appointments");
            return Json(new { success = true, message = "Appointments koleksiyonu drop edildi." });
        }

        private string MapStatus(int v) => v switch
        {
            0 => "Planlandı",
            1 => "Onaylandı",
            2 => "Tamamlandı",
            3 => "İptal",
            4 => "Ertelendi",
            _ => "Planlandı"
        };

        private string MapType(int v) => v switch
        {
            0 => "Muayene",
            1 => "Kontrol",
            2 => "Tedavi",
            3 => "CheckUp",
            4 => "Acil",
            _ => "Muayene"
        };
    }
}
