using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Services.StaffManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VisitController(IVisitService visitService) : ControllerBase
{
    /// <summary>
    /// Creates a new visit/triage record for a patient arriving at the hospital.
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<Visit>> CreateVisit([FromBody] CreateVisitDto dto)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var createdVisit = await visitService.CreateVisitAsync(dto, actorPublicId, role, currentUserId);
        return CreatedAtAction(nameof(GetVisit), new { publicId = createdVisit.VisitPublicId }, createdVisit);
    }

    [HttpGet("{publicId}")]
    public async Task<ActionResult<Visit>> GetVisit(string publicId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var visit = await visitService.GetVisitByPublicIdAsync(publicId, role, currentUserId, actorPublicId);
        if (visit == null)
        {
            return NotFound();
        }

        return Ok(visit);
    }

    /// <summary>
    /// Retrieves all active visits scoped to the logged-in user's role.
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Visit>>> GetVisits()
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();

        var visits = await visitService.GetVisitsAsync(role, currentUserId);
        return Ok(visits);
    }

    /// <summary>
    /// Updates the clinical notes (symptoms, diagnosis, treatment) for an active visit.
    /// </summary>
    [HttpPatch("{publicId}/notes")]
    [Authorize(Roles = "Doctor,Nurse")]
    public async Task<IActionResult> UpdateClinicalNotes(string publicId, [FromBody] UpdateClinicalNotesDto dto)
    {
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();
        var role = User.GetRequiredRole();

        await visitService.UpdateClinicalNotesAsync(publicId, dto, currentUserId, role, actorPublicId);
        return NoContent();
    }

    [HttpPatch("{publicId}/classifications")]
    [Authorize(Roles = "Doctor,Nurse")]
    public async Task<IActionResult> UpdateVisitClassifications(string publicId, [FromBody] UpdateVisitEnumsDto dto)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        await visitService.UpdateVisitClassificationsAsync(publicId, dto, role, currentUserId, actorPublicId);
        return NoContent();
    }

    /// <summary>
    /// Discharges a patient by marking their visit as completed.
    /// </summary>
    [HttpPost("{publicId}/complete")]
    public async Task<IActionResult> CompleteVisit(string publicId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var success = await visitService.CompleteVisitAsync(publicId, role, currentUserId, actorPublicId);
        if (!success)
        {
            return BadRequest("Unable to complete visit. Check permissions.");
        }

        return Ok();
    }
}
