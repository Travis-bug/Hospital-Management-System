using Hospital_Management_System.Models;
namespace Hospital_Management_System.Services.PatientManagement; 

public interface IEnrollmentService
{
    Task<Patient> EnrollAsync(Patient patient);
}