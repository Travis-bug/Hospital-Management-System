using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.Scheduling;
using Hospital_Management_System.Services.StaffManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulingController(
    IAvailabilityService availabilityService,
    ISchedulingQueryService schedulingQueryService) : ControllerBase
{
    /// <summary>
    /// Retrieves the schedule for the currently authenticated staff member within a date range.
    /// </summary>
    [HttpGet("my-shifts")]
    public async Task<ActionResult<IEnumerable<StaffShiftDto>>> GetMyShifts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var role = User.GetRequiredRole();
        var actorPublicId = User.GetRequiredActorPublicId();
        var currentUserId = User.GetRequiredDomainUserId();

        var shifts = await schedulingQueryService.GetMyShiftsAsync(startDate, endDate, role, currentUserId, actorPublicId);
        return Ok(shifts);
    }

    /// <summary>
    /// Retrieves the daily roster of all staff scheduled for a specific date.
    /// </summary>
    [HttpGet("daily-roster")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<IEnumerable<DailyRosterDto>>> GetDailyRoster([FromQuery] DateTime date)
    {
        var role = User.GetRequiredRole();
        var roster = await schedulingQueryService.GetDailyRosterAsync(date, role);
        return Ok(roster);
    }

    /// <summary>
    /// Assigns a staff member to a specific shift block.
    /// </summary>
    [HttpPost("schedule-staff")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<string>> ScheduleStaff(
        [FromQuery] DateTime shiftDate,
        [FromQuery] string shiftRulePublicId,
        [FromQuery] string staffPublicId,
        [FromQuery] string staffType)
    {
        var role = User.GetRequiredRole();
        var actorPublicId = User.GetRequiredActorPublicId();

        var newShiftId = await availabilityService.ScheduleStaffAsync(
            shiftDate, shiftRulePublicId, staffPublicId, staffType, role, actorPublicId);

        return Ok(new { ShiftId = newShiftId });
    }

    /// <summary>
    /// Cancels a staff member's shift and removes them from the daily roster.
    /// </summary>
    [HttpDelete("cancel-shift/{shiftPublicId}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CancelShift(string shiftPublicId)
    {
        var role = User.GetRequiredRole();
        var actorPublicId = User.GetRequiredActorPublicId();
        var currentUserId = User.GetRequiredDomainUserId();

        await availabilityService.CancelShiftAsync(shiftPublicId, role, actorPublicId, currentUserId);
        return NoContent();
    }
}
