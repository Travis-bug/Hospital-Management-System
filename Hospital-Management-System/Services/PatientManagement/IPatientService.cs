using Hospital_Management_System.Models;
namespace Hospital_Management_System.Services.PatientManagement
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int id);
        Task CreateAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task DeleteAsync(int id);
        Task<IEnumerable<Patient>> SearchAsync(string keyword);
    }
}