using Hospital_Management_System.Models;
using Hospital_Management_System.Services.PatientManagement;
using Hospital_Management_System.Tests.TestDoubles;

namespace Hospital_Management_System.Tests.Services;

public sealed class PatientServiceTests
{
    [Fact]
    public async Task SearchAsync_DoctorOnlySeesOwnPatients()
    {
        await using var context = TestClinicContextFactory.CreateContext();
        context.Patients.AddRange(
            new Patient
            {
                PatientPublicId = "PA_ALPHA",
                FirstName = "Alice",
                LastName = "DoctorOne",
                HealthCardNo = "HC1000000001",
                PhoneNumber = "5551000001",
                DoctorId = 1,
                Type = "Enrolled"
            },
            new Patient
            {
                PatientPublicId = "PA_BRAVO",
                FirstName = "Alice",
                LastName = "DoctorTwo",
                HealthCardNo = "HC1000000002",
                PhoneNumber = "5551000002",
                DoctorId = 2,
                Type = "Enrolled"
            });
        await context.SaveChangesAsync();

        var service = new PatientService(context, new TestEnrollmentService(), new TestAuditService());

        var results = (await service.SearchAsync("alice", "Doctor", 1)).ToList();

        Assert.Single(results);
        Assert.Equal("PA_ALPHA", results[0].PatientPublicId);
    }
}
