using Hospital_Management_System.Models;

namespace Hospital_Management_System.Services.ClinicalRecording;


// this is for ordering tests 
public interface IDiagnosticTestService 
{
        Task<DiagnosticTest> OrderTestAsync(DiagnosticTest test);
        Task<IEnumerable<DiagnosticTest>> GetPatientTestByGeneratdeIdAsync();
        Task<IEnumerable<DiagnosticTest>> GetPatientTestByDateAsync(DateTime date);
        Task<IEnumerable<DiagnosticTest>> GetTestsByGeneratedVisitIdAsync(int visitId);
        Task<bool> HasResultsAsync(int testId);
        
} 
