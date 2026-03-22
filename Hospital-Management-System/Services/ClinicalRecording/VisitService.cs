using Hospital_Management_System.Models;
using Hospital_Management_System.Utilities; // Where your IdGenerator lives
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class VisitService : IVisitService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;

    public VisitService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }


    public async Task<Doctor?> GetAvailableTriageDoctorAsync()
    {
        return await _context.Doctors
                   .Where(d => d.IsTriageQualified == true)
                   .FirstOrDefaultAsync()
               ?? throw new InvalidOperationException("No triage doctor available.");
    }



    public async Task<Visit> CreateVisitAsync(Visit visit, string actorPublicId, string role, int currentUserId, int assignedDoctorId)
    {
        visit.VisitPublicId = SecureIdGenerator.GenerateID(10); // Using the entropy method
        visit.Status = "Active"; // Set the status to Active for appointments
        visit.CheckinTime = DateTime.Now; // Log the start of their stay

        if (role == "Doctor" && assignedDoctorId != currentUserId) // check this out later (REVIEW)
        {
            // Doctors can only create visits for themselves
            throw new InvalidOperationException("Doctors cannot assign visits to other doctors.");
        }

        //======Decision tree for visit arrival source=================// 
        if (visit.AppointmentId != null)
        {

            visit.ArrivalSource = "Appointment";

            ///////////// sub branch one ////////////////////////////////   
            if (visit.PatientClass == "OutPatient")
            {
                visit.AdmissionStatus = "Not-Admitted";
            }
            /////////////////////////////////////////////////


            /////////////// sub branch two ///////////////////////////////////
            else if (visit.PatientClass == "InPatient")
            {
                visit.AdmissionStatus = "Admitted";
                //  visit.AdmissionDate = DateTime.Now; // Log the start of their stay
            }
            //////////////////////////////////////////////////


        }


        else
        {
            visit.ArrivalSource = "Walk-in";

            if (visit.PatientClass == "ER Referral")
            {
                visit.AdmissionStatus = "Priority Triage";
                visit.VisitNotes = "ER Referral detected: Patient set to Priority Triage.";
            }

            else // Default to Emergency / Self-Referral
            {
                visit.PatientClass = "Emergency";
                visit.AdmissionStatus = "Triage Pending";
            }


            try
            {
                var triageDoctor = await GetAvailableTriageDoctorAsync();
                visit.DoctorId = triageDoctor!.DoctorId; // Assign the triage doctor's ID to the visit REVIEW
            }
            catch
            {
                throw new InvalidOperationException("No triage doctor available for walk-in.");
            }

        }

        _context.Visits.Add(visit);


        var log = new AuditLog // This logs the search query 
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Visit created: {visit.VisitPublicId}."
        };


        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return visit;
    }



    //====================== Get Structed Data (3 Layers)=======================================


    // 1. THE "MANY" (The Decision Tree)
    // Handles Dashboard (DoctorId only) and Profile (Both IDs)
    // Used by the UI to get a list of visits for a specific doctor
    public async Task<IEnumerable<Visit>> GetVisitsAsync(string role, int currentUserId)
    {
        var query = _context.Visits
            .AsNoTracking()
            .AsQueryable();
        //============================= Decision Tree for visit filtering ================================//

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to view visits.");
        }

        return await query.ToListAsync();
    }





    // 2. THE "API ENTRY" (Security Layer)
    // Used by Controllers to Protect the real Database PKs.
    // used to get the visit details of ONE specific patient from the database
    public async Task<Visit?> GetVisitByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
    {
        var query = _context.Visits
            .AsNoTracking()
            .AsQueryable();



        // 2. THE SECURITY SANDBOX (IDOR Protection)
        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to view clinical notes.");

        }


        var visit = await query.FirstOrDefaultAsync(v => v.VisitPublicId == publicId);

        if (visit != null)
        {
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId, // this will be assigned to the user's Public ID in the controller
                ActionType = "Read",
                Timestamp = DateTime.UtcNow,
                Details = $"Visit details viewed by {currentUserId}."
            });
        }
        return visit;
    }





    // 3. THE "WORKHORSE" (Internal Speed)
    // Used inside the service for Updates/Business Logic.
    public async Task<Visit?> GetVisitsById(int Id)
    {
        return await _context.Visits.FindAsync(Id);

    }

    //===============================================================================================//




    public async Task<IEnumerable<Visit>> SearchVisitsAsync(string keyword, string role, string actorPublicId, int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return []; // this should return an empty list if the keyword is null or whitespace {TEST} or change to Enumerable.Empty<Visit>()
        keyword = keyword.ToLower();


        var query = _context.Visits
               .AsNoTracking()
                 .Include(v => v.Patient)
                 .AsQueryable();

        if (role == "Doctor")
        {
            query = query.Where(v => v.DoctorId == currentUserId);
        }

        else if (role == "Nurse")
        {
            query = query.Where(v => v.AdmissionStatus != "Discharged");
        }
        else
        {
            throw new UnauthorizedAccessException("You are not authorized to perform this action");
        }


        var results = await query
                 .Where(v => v.Patient != null &&
                     ((v.Patient.FirstName != null && v.Patient.FirstName.ToLower().Contains(keyword)) ||
                      (v.Patient.LastName != null && v.Patient.LastName.ToLower().Contains(keyword))))
                 .ToListAsync();



        await _auditService.LogAsync(new AuditLog // This logs the search querey 
        {
            PerformedBy = actorPublicId,
            ActionType = "Search",
            Timestamp = DateTime.UtcNow,
            Details = $"Search query: {keyword}"

        });

        return results;

    }



    public async Task<IEnumerable<Visit>> GetVisitsByDateAsync(DateTime date, string role, int currentUserId)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var query = _context.Visits
            .AsNoTracking()
            .Where(v => v.CheckinTime >= startOfDay && v.CheckinTime < endOfDay);

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;
            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to filter visits by date.");
        }

        return await query.ToListAsync();
    }



    public async Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment, int currentUserId, string actorPublicId)
    {
        var visit = await GetVisitsById(visitId); // get the visit by id (this is the "WORKHORSE"````)

        if (visit == null)
        {
            throw new KeyNotFoundException("Visit not found");
        }

        if (visit.DoctorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to perform this action");
        }


        visit.Symptoms = symptoms;
        visit.Diagnosis = diagnosis;
        visit.Treatment = treatment;

        //
        // Whenever you are changing the database (Creating, updating, or Deleting), you group everything into one big "shopping cart" and check out exactly once at the very end using await _context.SaveChangesAsync().
        //
        var log = new AuditLog // This logs the search query 
        {
            PerformedBy = actorPublicId,
            ActionType = "Update",
            Timestamp = DateTime.UtcNow,
            Details = $"Clinical notes updated for visit {visit.VisitPublicId}."
        };
        _context.AuditLogs.Add(log);



        await _context.SaveChangesAsync();
    }



    public async Task<bool> CompleteVisitAsync(int visitId, string role, int currentUserId)
    {
        var visit = await GetVisitsById(visitId);
        if (visit == null)
        {
            throw new KeyNotFoundException("Visit not found");
        }

        switch (role)
        {
            case "Doctor":
                if (visit.DoctorId != currentUserId)
                    throw new UnauthorizedAccessException("Doctors can only complete their own visits.");
                break;
            case "Nurse":
                if (visit.AdmissionStatus == "Discharged")
                    throw new InvalidOperationException("Visit is already discharged.");
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to complete visits.");
        }

        visit.Status = "Completed";
        visit.CheckoutTime = DateTime.Now;
        
        
        var log = new AuditLog 
        {
            PerformedBy = currentUserId.ToString(), // Or actorPublicId if you pass it in!
            ActionType = "Complete",
            Timestamp = DateTime.UtcNow,
            Details = $"Visit {visit.VisitPublicId} completed and discharged."
        };
        _context.AuditLogs.Add(log);
        
        
        await _context.SaveChangesAsync();
        return true;
    }

}