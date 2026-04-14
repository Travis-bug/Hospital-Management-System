using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
namespace Hospital_Management_System.Services.ClinicalRecording

{
    public interface ITestResultsService
    {
        
        Task<TestResult> AddTestResultAsync(TestResult Result, int currentUserId, string role, string actorPublicId);
        Task<TestResult> AddTestResultAsync(AddTestResultDto dto, int currentUserId, string role, string actorPublicId);
        Task<IEnumerable<TestResult>> GetTestResultsAsync(string role, int currentUserId, string? healthCardNo = null); // multiple results
        Task<TestResult?> GetTestResultByPublicIdAsync(string publicTestId, string role, int currentUserId, string actorPublicId);
        Task<IEnumerable<TestResult>> SearchTestResultsAsync(string keyword, string role, int currentUserId, string actorPublicId);
        Task<IEnumerable<PendingDiagnosticTestDto>> GetPendingTestsAsync(string role, int currentUserId);
        
        Task<TestResult?> GetTestResultByIdAsync(int id);
        Task<IEnumerable<TestResult>> GetTestResultsByDateAsync(DateTime date, string role, int currentUserId);
        
    }
}
