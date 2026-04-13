using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.Scheduling;
using Hospital_Management_System.Services.StaffManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentController(IAppointmentService appointmentService) : ControllerBase
{
    /// <summary>
    /// Retrieves a specific doctor's scheduled appointments for a given day.
    /// </summary>
    [HttpGet("doctor-schedule")]
    public async Task<ActionResult<IEnumerable<AppointmentScheduleItemDto>>> GetDoctorSchedule([FromQuery] string doctorPublicId, [FromQuery] DateTime date)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();

        var schedule = await appointmentService.GetDoctorScheduleAsync(doctorPublicId, date, role, currentUserId);
        return Ok(schedule);
    }

    /// <summary>
    /// Retrieves the details of a specific appointment via its Public ID.
    /// </summary>
    [HttpGet("{publicId}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetAppointment(string publicId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var appointment = await appointmentService.GetAppointmentByPublicIdAsync(publicId, role, currentUserId, actorPublicId);
        if (appointment == null) return NotFound();
        
        return Ok(appointment);
    }

    /// <summary>
    /// Books a new appointment, checking for double-booking conflicts.
    /// </summary>
    [HttpPost("book")]
    public async Task<ActionResult<AppointmentDetailDto>> BookAppointment([FromBody] BookAppointmentDto dto)
    {
        var role = User.GetRequiredRole();
        var actorPublicId = User.GetRequiredActorPublicId();

        var result = await appointmentService.BookAppointmentAsync(dto, role, actorPublicId);
        return CreatedAtAction(nameof(GetAppointment), new { publicId = result.PublicId }, result);
    }

    /// <summary>
    /// Cancels an existing appointment without deleting the database record.
    /// </summary>
    [HttpDelete("cancel/{publicId}")]
    public async Task<IActionResult> CancelAppointment(string publicId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        await appointmentService.CancelAppointmentAsync(publicId, role, actorPublicId, currentUserId);
        return NoContent();
    }
}
