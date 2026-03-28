using Hospital_Management_System.Models;
using Hospital_Management_System.Utilities; // Where your IdGenerator lives
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class TestResultService : ITestResultsService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;
    public TestResultService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }


    public async Task<TestResult> AddTestResultAsync(TestResult Result, int currentUserId, string role, string actorPublicId) // NOTE: the things inside <> are the return types STOP FORGETTING 
    {

        // checks if the the test exists
        var orderedTestExists = await _context.DiagnosticTests.AnyAsync(t => t.TestId == Result.TestId);
        if (!orderedTestExists)
        {
            throw new KeyNotFoundException($"Ordered test with ID {Result.TestId} not found."); // ReVIEW
        }

        // checks if the test result already exists
        var duplicateResultExists = await _context.TestResults.AnyAsync(r => r.TestId == Result.TestId); // REVIEW 
        if (duplicateResultExists)
        {
            throw new InvalidOperationException($"A result already exists for test ID {Result.TestId}.");
        }
        // checks if the visit exists
        var visit = await _context.Visits.FindAsync(Result.VisitId);
        if (visit == null)
        {
            throw new KeyNotFoundException("The associated visit does not exist.");
        }


        switch (role)
        {
            case "Doctor":
                if (visit.DoctorId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You are only authorized to add test results for your own assigned patients.");
                }
                break;

            case "Nurse":
                if (visit.AdmissionStatus == "Discharged")
                {
                    throw new InvalidOperationException("Cannot add new test results to a discharged visit.");

                }
                break;
            default:
                throw new UnauthorizedAccessException(" you are not authorized to add test results.");
        }


        Result.PublicTestId = SecureIdGenerator.GenerateID(15, "RES");
        Result.ResultDate = DateTime.Now;

        _context.TestResults.Add(Result);

        var log = new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Added Test Result to Visit {visit.VisitPublicId}."
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        return Result;
    }

    // Three-way Get Structured Data (3 Layers)=======================================

    // 1. THE "MANY" (The Decision Tree) is used to get all the test results belonging to
    // the specific doctor and all the test results where the patient isn't discharged to the nurse 
    public async Task<IEnumerable<TestResult>> GetTestResultsAsync(string role, int currentUserId, string? healthCardNo = null)
    {
        var query = _context.TestResults
            .AsNoTracking()
            .Include(r => r.Test)
            .Include(r => r.Visit)
                .ThenInclude(v => v.Patient)
            .AsQueryable();

        // The Gatekeeper Switch
        switch (role)
        {
            case "Doctor":
                query = query.Where(r => r.Visit.DoctorId == currentUserId);
                break;
            case "Nurse":
                // Nurses can see all active/non-discharged visit results
                query = query.Where(r => r.Visit.AdmissionStatus != "Discharged");
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view test results.");
        }

        if (!string.IsNullOrWhiteSpace(healthCardNo))
        {
            query = query.Where(r => r.Visit.Patient.HealthCardNo == healthCardNo);
        }

        return await query.OrderByDescending(r => r.ResultDate).ToListAsync();
    }




    // 2. THE "API ENTRY" (Security Layer) is used to get the test result details of ONE specific patient from the database
    public async Task<TestResult?> GetTestResultByPublicIdAsync(string publicTestId, string role, int currentUserId, string actorPublicId)
    {
        var query = _context.TestResults
            .AsNoTracking()
            .Include(r => r.Test)
            .AsQueryable();

        // IDOR Protection Logic
        switch (role)
        {
            case "Doctor":
                query = query.Where(r => r.Visit.DoctorId == currentUserId);
                break;
            case "Nurse":
                query = query.Where(r => r.Visit.AdmissionStatus != "Discharged");
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view specific test results.");
        }

        var result = await query.AsNoTracking().FirstOrDefaultAsync(r => r.PublicTestId == publicTestId);


        if (result != null)
        {
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId, // this will be assigned to the user's Public ID in the controller
                ActionType = "Read",
                Timestamp = DateTime.UtcNow,
                Details = $"Test result details viewed by {currentUserId}."
            });

        }
        return result ?? throw new KeyNotFoundException("Test result not found");
    }


    // 3. THE "WORKHORSE" (Internal Speed) is used inside the service for Updates/Business Logic.
    public async Task<TestResult?> GetTestResultByIdAsync(int id)
    {
        return await _context.TestResults.FindAsync(id);
    }



    public async Task<IEnumerable<TestResult>> SearchTestResultsAsync(string keyword, string role, int currentUserId,
        string actorPublicId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return
            []; // this should return an empty list if the keyword is null or whitespace {TEST} or change to Enumerable.Empty<Visit>()
        keyword = keyword.ToLower();

        var query = _context.TestResults
            .AsNoTracking()
            .Include(r => r.Test)

            .Include(r => r.Visit)
            .ThenInclude(v => v.Patient)
            .AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(r => r.Visit.DoctorId == currentUserId);
                break;
            case "Nurse":
                query = query.Where(r => r.Visit.AdmissionStatus != "Discharged");
                break;
            default:
                throw new UnauthorizedAccessException("Unauthorized search attempt.");
        }

        // Searches by Patient Name or Test Name
        var results = await query.Where(r =>
            r.Visit.Patient.FirstName.ToLower().Contains(keyword) || // TEST 
            r.Visit.Patient.LastName.ToLower().Contains(keyword) || //TEST 
            r.Test.TestName.ToLower().Contains(keyword) //TEST 
        ).ToListAsync();


        // 4. THE LOG (Audit)
        await _auditService.LogAsync(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Search",
            Timestamp = DateTime.UtcNow,
            Details = $"Searched Test Results for: {keyword}"
        });

        return results;
    }





    public async Task<IEnumerable<TestResult>> GetTestResultsByDateAsync(DateTime date, string role, int currentUserId)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var query = _context.TestResults
            .AsNoTracking()
            .Where(r => r.ResultDate >= startOfDay && r.ResultDate < endOfDay);

        // Validate
        switch (role)
        {
            case "Doctor":
                query = query.Where(r => r.Visit.DoctorId == currentUserId);
                break;
            case "Nurse":
                query = query.Where(r => r.Visit.AdmissionStatus != "Discharged");
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to filter by date.");
        }

        return await query.ToListAsync();
    }
}