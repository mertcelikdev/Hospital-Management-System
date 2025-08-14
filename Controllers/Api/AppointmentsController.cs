using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments([FromQuery] string? patientId, [FromQuery] string? doctorId, [FromQuery] DateTime? date)
        {
            List<Appointment> appointments;

            if (!string.IsNullOrEmpty(patientId))
            {
                appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            }
            else if (!string.IsNullOrEmpty(doctorId))
            {
                appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
            }
            else if (date.HasValue)
            {
                appointments = await _appointmentService.GetAppointmentsByDateAsync(date.Value);
            }
            else
            {
                return BadRequest("Please provide either patientId, doctorId, or date parameter");
            }

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(string id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check doctor availability
            var isAvailable = await _appointmentService.CheckDoctorAvailabilityAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.Duration);
            
            if (!isAvailable)
            {
                return BadRequest(new { message = "Doctor is not available at the requested time" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            appointment.CreatedBy = userId ?? "";

            var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
            return CreatedAtAction(nameof(GetAppointment), new { id = createdAppointment.Id }, createdAppointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(string id, [FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _appointmentService.UpdateAppointmentAsync(id, appointment);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(string id, [FromBody] AppointmentStatus status)
        {
            var result = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] string doctorId, [FromQuery] DateTime appointmentDate, [FromQuery] int durationMinutes = 30)
        {
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var isAvailable = await _appointmentService.CheckDoctorAvailabilityAsync(doctorId, appointmentDate, duration);
            
            return Ok(new { isAvailable });
        }
    }
}
