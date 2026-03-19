using Hospital_Management_System.Models;
using Hospital_Management_System.Utilities; // Where your IdGenerator lives
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class TestResultService : ITestResultsService
{
    private readonly ClinicContext _context;
    public TestResultService(ClinicContext context)
    {
        _context = context;
    }
    
    
    public async Task<TestResult>  AddTestResultAsync(TestResult Result) // NOTE:  the things inside <> are the return types STOP FORGETTING 
    {
        Result.PublicTestId = SecureIdGenerator.GenerateID(10);  // generate unique string using entropy method
        
        Result.ResultDate = DateTime.Now;
        
        _context.TestResults.Add(Result);
        
        await _context.SaveChangesAsync();

        return Result; 
    }

    
    public async Task<TestResult?> GetPatientTestResultAsync(string PublicTestId)
    {
        try
        {
            return await _context.TestResults // get the test results from the database 
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PublicTestId == PublicTestId); // 
        }
        catch
        {
            throw new  NullReferenceException ("Test result not found");
        }
    }


    public async Task<IEnumerable<TestResult>> GetPatientTestResultsAsync(string healthCardNo)
    {
        try
        {
            return await _context.TestResults
                .AsNoTracking()
                .Include(r => r.Test) // use the test entity to access and include the test details
                .Include(r => r.NurseId) // use the visit entity to access and include the nurse details
                .Include(r => r.Visit) // use the visit entity to access and include the patient details
                .ThenInclude(v => v.Patient) // use the visit entity to access and include the patient details

                .Where(r => r.Visit.Patient.HealthCardNo ==
                            healthCardNo) // filter by unique patient health card number 
                .OrderByDescending(r => r.ResultDate) // sort in descending order by date
                .ToListAsync();
        }
        catch
        {
            throw new NullReferenceException("Test results found");
        }
    }

    
    
    public async Task<IEnumerable<TestResult>> GetPatientTestResultsByDateAsync(DateTime date)
    {
        var startOfDay = date.Date; // get the start of the day and used var because it's a date (simple type) 
        var  endOfDay = startOfDay.AddDays(1); // get the end of the day
        
        try
        {
            return await _context.TestResults // get the test results from the database 
                .AsNoTracking()// avoid tracking changes
                .Where(f => f.ResultDate >= startOfDay && f.ResultDate < endOfDay) 
                .ToListAsync(); 
        }
        catch
        {
            throw new  NullReferenceException (" No test result found");
        }
    }
    
    
}