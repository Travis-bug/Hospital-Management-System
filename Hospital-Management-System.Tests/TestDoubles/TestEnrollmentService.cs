using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.PatientManagement;

namespace Hospital_Management_System.Tests.TestDoubles;

internal sealed class TestEnrollmentService : IEnrollmentService
{
    public Task<Patient> EnrollAsync(EnrollPatientDto dto)
    {
        throw new NotSupportedException("Enrollment is not exercised in these tests.");
    }
}
