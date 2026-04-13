using Hospital_Management_System.Models;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Tests.TestDoubles;

namespace Hospital_Management_System.Tests.Services;

public sealed class VisitServiceTests
{
    [Fact]
    public async Task GetVisitsByPatientPublicIdAsync_DoctorOnlySeesOwnVisitsForThatPatient()
    {
        await using var context = TestClinicContextFactory.CreateContext();

        context.Doctors.AddRange(
            new Doctor { DoctorId = 1, PublicId = "DR_ONE", FirstName = "Dia", LastName = "One" },
            new Doctor { DoctorId = 2, PublicId = "DR_TWO", FirstName = "Eli", LastName = "Two" });

        context.Patients.Add(new Patient
        {
            PatientId = 50,
            PatientPublicId = "PA_SCOPE_01",
            FirstName = "Patient",
            LastName = "Scoped",
            HealthCardNo = "HC3000000001",
            PhoneNumber = "5553000001",
            DoctorId = 1,
            Type = "Enrolled"
        });

        context.Visits.AddRange(
            new Visit
            {
                VisitsId = 100,
                VisitPublicId = "VIS_ONE",
                PatientId = 50,
                DoctorId = 1,
                Status = "Active",
                PatientClass = "Outpatient",
                AdmissionStatus = "Not Admitted",
                VisitNotes = string.Empty
            },
            new Visit
            {
                VisitsId = 101,
                VisitPublicId = "VIS_TWO",
                PatientId = 50,
                DoctorId = 2,
                Status = "Active",
                PatientClass = "Outpatient",
                AdmissionStatus = "Not Admitted",
                VisitNotes = string.Empty
            });

        await context.SaveChangesAsync();

        var service = new VisitService(context, new TestAuditService());

        var visits = (await service.GetVisitsByPatientPublicIdAsync("PA_SCOPE_01", "Doctor", 1)).ToList();

        Assert.Single(visits);
        Assert.Equal("VIS_ONE", visits[0].PublicId);
    }
}
