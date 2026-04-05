using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.PatientManagement;
using Hospital_Management_System.Services.StaffManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientController(
    IPatientService patientService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    /// <summary>
    /// Enrolls a brand-new patient into the hospital system.
    /// </summary>
    [HttpPost("enroll")]
    [Authorize(Roles = "Manager,Admin,Secretary")]
    public async Task<ActionResult<Patient>> EnrollPatient([FromBody] EnrollPatientDto dto)
    {
        var enrolledPatient = await enrollmentService.EnrollAsync(dto);
        return CreatedAtAction(nameof(GetPatient), new { publicId = enrolledPatient.PatientPublicId }, enrolledPatient);
    }

    /// <summary>
    /// Assigns or reassigns a doctor to an existing patient after enrollment.
    /// </summary>
    [HttpPatch("{patientpublicId}/assign-doctor")]
    [Authorize(Roles = "Manager,Admin,Secretary")]
    public async Task<IActionResult> AssignDoctor(string patientpublicId, [FromBody] AssignPatientDoctorDto dto)
    {
        var role = User.GetRequiredRole();
        var actorPublicId = User.GetRequiredActorPublicId();

        await patientService.AssignDoctorAsync(patientpublicId, dto, role, actorPublicId);
        return NoContent();
    }

    /// <summary>
    /// Retrieves a single patient by their Public ID.
    /// </summary>
    [HttpGet("{publicId}")]
    [Authorize(Roles = "Manager,Admin,Secretary,Doctor,Nurse")]
    public async Task<ActionResult<Patient>> GetPatient(string publicId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var patient = await patientService.GetByPublicIdAsync(publicId, role, currentUserId, actorPublicId);
        if (patient == null)
        {
            return NotFound();
        }

        return Ok(patient);
    }

    /// <summary>
    /// Retrieves a list of all patients.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Manager,Admin,Secretary,Doctor,Nurse")]
    public async Task<ActionResult<IEnumerable<Patient>>> GetAllPatients()
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();

        var patients = await patientService.GetAllPatientsAsync(role, currentUserId);
        return Ok(patients);
    }

    /// <summary>
    /// Searches the patient registry by a keyword (Name, MRN, etc.).
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Manager,Admin,Secretary,Doctor,Nurse")]
    public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("Search keyword cannot be empty.");
        }

        keyword = keyword.Trim();
        var patients = await patientService.SearchAsync(keyword);
        return Ok(patients);
    }

    /// <summary>
    /// Updates an existing patient's demographic or contact information.
    /// </summary>
    [HttpPut("{patientpublicId}")]
    [Authorize(Roles = "Doctor,Secretary")]
    public async Task<IActionResult> UpdatePatient(string patientpublicId, [FromBody] UpdatePatientDto dto)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        await patientService.UpdateAsync(patientpublicId, dto, role, currentUserId, actorPublicId);
        return NoContent();
    }

    /// <summary>
    /// Deletes a patient from the system.
    /// </summary>
    [HttpDelete("{patientpublicId}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> DeleteAsync(string patientpublicId)
    {
        if (string.IsNullOrWhiteSpace(patientpublicId))
        {
            return BadRequest("Invalid patient public ID.");
        }

        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        await patientService.DeleteAsync(patientpublicId, role, actorPublicId, currentUserId);
        return NoContent();
    }
}
