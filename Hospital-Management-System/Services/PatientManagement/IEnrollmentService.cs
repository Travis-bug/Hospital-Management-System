using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
namespace Hospital_Management_System.Services.PatientManagement; 

public interface IEnrollmentService
{
    Task<Patient> EnrollAsync(EnrollPatientDto dto);
}