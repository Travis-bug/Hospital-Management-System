using System.Security.Claims;
using Hospital_Management_System.Controllers.Api;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.PatientManagement;
using Hospital_Management_System.Services.StaffManagement;
using Hospital_Management_System.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Tests.Controllers;

public sealed class PatientControllerTests
{
    [Fact]
    public async Task SearchPatients_ReturnsDoctorScopedResults()
    {
        await using var context = TestClinicContextFactory.CreateContext();
        context.Patients.AddRange(
            new Patient
            {
                PatientPublicId = "PA_CTRL_01",
                FirstName = "Maya",
                LastName = "Owned",
                HealthCardNo = "HC4000000001",
                PhoneNumber = "5554000001",
                DoctorId = 7,
                Type = "Enrolled"
            },
            new Patient
            {
                PatientPublicId = "PA_CTRL_02",
                FirstName = "Maya",
                LastName = "Foreign",
                HealthCardNo = "HC4000000002",
                PhoneNumber = "5554000002",
                DoctorId = 8,
                Type = "Enrolled"
            });
        await context.SaveChangesAsync();

        var patientService = new PatientService(context, new TestEnrollmentService(), new TestAuditService());
        var controller = new PatientController(context, patientService, new TestEnrollmentService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = BuildUser("Doctor", 7, "DR_CTRL_07")
                }
            }
        };

        var actionResult = await controller.SearchPatients("maya");
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var patients = Assert.IsAssignableFrom<IEnumerable<Patient>>(okResult.Value).ToList();

        Assert.Single(patients);
        Assert.Equal("PA_CTRL_01", patients[0].PatientPublicId);
    }

    private static ClaimsPrincipal BuildUser(string role, int domainUserId, string publicId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, role),
            new Claim(DomainUserClaimsTransformation.DomainUserIdClaimType, domainUserId.ToString()),
            new Claim(DomainUserClaimsTransformation.PublicIdClaimType, publicId)
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }
}
