using Hospital_Management_System.Models;
using Hospital_Management_System.Services.Scheduling;
using Hospital_Management_System.Tests.TestDoubles;

namespace Hospital_Management_System.Tests.Services;

public sealed class AppointmentServiceTests
{
    [Fact]
    public async Task GetDoctorScheduleAsync_ReturnsOnlyBookedAndArrivedAppointments()
    {
        await using var context = TestClinicContextFactory.CreateContext();

        context.Doctors.Add(new Doctor
        {
            DoctorId = 1,
            PublicId = "DR_TEST_01",
            FirstName = "Asha",
            LastName = "Brown"
        });

        context.Patients.Add(new Patient
        {
            PatientId = 10,
            PatientPublicId = "PA_TEST_01",
            FirstName = "Nina",
            LastName = "Cole",
            HealthCardNo = "HC2000000001",
            PhoneNumber = "5552000001",
            DoctorId = 1,
            Type = "Enrolled"
        });

        var targetDate = new DateTime(2026, 04, 13, 9, 0, 0, DateTimeKind.Utc);
        context.Appointments.AddRange(
            new Appointment
            {
                AppointmentId = 1,
                PublicId = "APT_BOOKED",
                PatientId = 10,
                DoctorId = 1,
                AppointmentDate = targetDate,
                Status = "Booked"
            },
            new Appointment
            {
                AppointmentId = 2,
                PublicId = "APT_ARRIVED",
                PatientId = 10,
                DoctorId = 1,
                AppointmentDate = targetDate.AddHours(1),
                Status = "Arrived"
            },
            new Appointment
            {
                AppointmentId = 3,
                PublicId = "APT_DONE",
                PatientId = 10,
                DoctorId = 1,
                AppointmentDate = targetDate.AddHours(2),
                Status = "Checked Out"
            });

        await context.SaveChangesAsync();

        var service = new AppointmentService(context, new TestAuditService());

        var schedule = (await service.GetDoctorScheduleAsync("DR_TEST_01", targetDate, "Doctor", 1)).ToList();

        Assert.Equal(2, schedule.Count);
        Assert.All(schedule, appointment => Assert.True(
            appointment.Status is "Booked" or "Arrived",
            $"Unexpected appointment status returned: {appointment.Status}"));
    }

    [Fact]
    public async Task GetDoctorScheduleAsync_RejectsDoctorsRequestingOtherDoctorsSchedules()
    {
        await using var context = TestClinicContextFactory.CreateContext();
        context.Doctors.Add(new Doctor
        {
            DoctorId = 1,
            PublicId = "DR_TEST_01",
            FirstName = "Asha",
            LastName = "Brown"
        });
        await context.SaveChangesAsync();

        var service = new AppointmentService(context, new TestAuditService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.GetDoctorScheduleAsync("DR_TEST_01", DateTime.UtcNow, "Doctor", 2));
    }
}
