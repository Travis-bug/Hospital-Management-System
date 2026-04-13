using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;

namespace Hospital_Management_System.Services.PatientManagement
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync(string role, int currentUserId); // this was used in patient controller check it out 
        
        Task<Patient?> GetByIdAsync(int patientid);
         
        Task <Patient?> GetByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId);
        
        
        Task CreateAsync(EnrollPatientDto dto);

        Task AssignDoctorAsync(string patientpublicId, AssignPatientDoctorDto dto, string role, string actorPublicId);

        Task UpdateAsync(string patientpublicId, UpdatePatientDto dto,  string role, int currentUserId, string actorPublicId);
        
        
        Task DeleteAsync(string patientpublicId, string role, string actorPublicId, int currentUserId); 
        
        
        Task<IEnumerable<Patient>> SearchAsync(string keyword, string role, int currentUserId);
    }
}
