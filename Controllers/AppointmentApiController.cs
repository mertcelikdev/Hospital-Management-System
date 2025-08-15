using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.Controllers
{
    [Route("api/appointments")] 
    [ApiController]
    public class AppointmentApiController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentApiController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 20, string? sort = "date", string? dir = "desc", string? q = null,
            string? departmentId = null, string? doctorId = null, string? patientId = null, DateTime? startDate = null, DateTime? endDate = null, string? status = null, string? type = null)
        {
            var (items,total) = await _appointmentService.GetAppointmentsPagedAsync(page, pageSize, sort, dir, q, departmentId, doctorId, patientId, startDate, endDate, status, type);
            return Ok(new { page, pageSize, total, items });
        }

        [HttpGet("stats/today" )]
        public async Task<IActionResult> GetTodayStats(){
            var planned = await _appointmentService.GetTodayAppointmentsCountAsync();
            var real = await _appointmentService.GetTodayAppointmentsRealCountAsync();
            var completed = await _appointmentService.GetCompletedAppointmentsCountAsync();
            return Ok(new { plannedToday = planned, totalToday = real, completedTotal = completed });
        }
    }
}
