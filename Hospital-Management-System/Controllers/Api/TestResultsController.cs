using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Services.StaffManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor,Nurse")]
public class TestResultsController(ITestResultsService testResultsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestResult>>> GetTestResults([FromQuery] string? healthCardNo = null)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();

        var results = await testResultsService.GetTestResultsAsync(role, currentUserId, healthCardNo);
        return Ok(results);
    }

    [HttpGet("pending-tests")]
    public async Task<ActionResult<IEnumerable<PendingDiagnosticTestDto>>> GetPendingTests()
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();

        var tests = await testResultsService.GetPendingTestsAsync(role, currentUserId);
        return Ok(tests);
    }

    [HttpPost]
    public async Task<ActionResult<TestResult>> AddTestResult([FromBody] AddTestResultDto dto)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var createdResult = await testResultsService.AddTestResultAsync(dto, currentUserId, role, actorPublicId);
        return CreatedAtAction(nameof(GetTestResult), new { publicTestId = createdResult.PublicTestId }, createdResult);
    }

    [HttpGet("{publicTestId}")]
    public async Task<ActionResult<TestResult>> GetTestResult(string publicTestId)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var result = await testResultsService.GetTestResultByPublicIdAsync(publicTestId, role, currentUserId, actorPublicId);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TestResult>>> SearchTestResults([FromQuery] string keyword)
    {
        var role = User.GetRequiredRole();
        var currentUserId = User.GetRequiredDomainUserId();
        var actorPublicId = User.GetRequiredActorPublicId();

        var results = await testResultsService.SearchTestResultsAsync(keyword, role, currentUserId, actorPublicId);
        return Ok(results);
    }
}
