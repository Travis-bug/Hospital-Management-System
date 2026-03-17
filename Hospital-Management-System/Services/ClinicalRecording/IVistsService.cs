using Hospital_Management_System.Models;

namespace Hospital_Management_System.Services.ClinicalRecording;

public interface IVisitService
{
    Task <Visit> CreateVisitAsync (Visit visit);
    Task<IEnumerable<Visit>> GetVisitsById(int Id);
    Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId);
    Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment);
    Task <Doctor?> GetAvailableTriageDoctorAsync () ;
    Task<bool> CompleteVisitAsync(int visitId); 
    
}