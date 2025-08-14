using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace HospitalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public DatabaseController(IMongoDatabase database)
        {
            _database = database;
        }

        [HttpPost("clear-appointments")]
        public async Task<IActionResult> ClearAppointments()
        {
            try
            {
                var collection = _database.GetCollection<object>("Appointments");
                await collection.DeleteManyAsync(Builders<object>.Filter.Empty);
                return Ok(new { success = true, message = "Appointments collection cleared successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
