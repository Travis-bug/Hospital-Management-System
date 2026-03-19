using Hospital_Management_System.Models;
namespace Hospital_Management_System.Services.ClinicalRecording

{
    public interface ITestResultsService
    {
        
        Task<TestResult> AddTestResultAsync(TestResult Result); 
        
        Task<TestResult?> GetPatientTestResultAsync(string PublicTestId);
        Task<IEnumerable<TestResult>> GetPatientTestResultsAsync(string healthCardNo); // multiple results
        Task<IEnumerable<TestResult>> GetPatientTestResultsByDateAsync(DateTime date);
        
    }
}