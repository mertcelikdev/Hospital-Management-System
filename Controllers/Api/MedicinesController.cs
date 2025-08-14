using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMedicines([FromQuery] MedicineCategory? category, [FromQuery] string? search, [FromQuery] bool lowStock = false)
        {
            List<Medicine> medicines;

            if (lowStock)
            {
                medicines = await _medicineService.GetLowStockMedicinesAsync();
            }
            else if (category.HasValue)
            {
                medicines = await _medicineService.GetMedicinesByCategoryAsync(category.Value);
            }
            else if (!string.IsNullOrEmpty(search))
            {
                medicines = await _medicineService.SearchMedicinesAsync(search);
            }
            else
            {
                medicines = await _medicineService.GetAllMedicinesAsync();
            }

            return Ok(medicines);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicine(string id)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }

            return Ok(medicine);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateMedicine([FromBody] Medicine medicine)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdMedicine = await _medicineService.CreateMedicineAsync(medicine);
            return CreatedAtAction(nameof(GetMedicine), new { id = createdMedicine.Id }, createdMedicine);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateMedicine(string id, [FromBody] Medicine medicine)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _medicineService.UpdateMedicineAsync(id, medicine);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStock(string id, [FromBody] int newQuantity)
        {
            var result = await _medicineService.UpdateStockAsync(id, newQuantity);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedicine(string id)
        {
            var result = await _medicineService.DeleteMedicineAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetMedicineTransactions(string id)
        {
            var transactions = await _medicineService.GetMedicineTransactionsAsync(id);
            return Ok(transactions);
        }

        [HttpPost("transactions")]
        [Authorize(Roles = "Admin,Staff,Doctor,Nurse")]
        public async Task<IActionResult> CreateTransaction([FromBody] MedicineTransaction transaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            transaction.UserId = userId ?? "";

            var createdTransaction = await _medicineService.CreateTransactionAsync(transaction);
            return Ok(createdTransaction);
        }
    }
}
