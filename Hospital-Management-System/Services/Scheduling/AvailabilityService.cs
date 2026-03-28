using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Utilities;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.Scheduling; 

public class AvailabilityService : IAvailabilityService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;
    public AvailabilityService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }
    
    public async Task<string> ScheduleStaffAsync(DateTime shiftDate, string shiftRulePublicId, string staffPublicId, string staffType, string role, string actorPublicId)
    {
    // 1. THE BOUNCER: Authorization Check
    // TODO: Check if the 'role' is "Manager" or "Admin". 
    // If they are not, throw an UnauthorizedAccessException.
    
    if (role != "Admin" && role != "Manager")
    {
        throw new UnauthorizedAccessException("You are not authorized to schedule shifts.");
    }


    // 2. THE RULE LOOKUP: Find the Base Shift
    // TODO: Query the _context.Shifts table for the shift that matches 'shiftRulePublicId'.
    // If it doesn't exist, throw a KeyNotFoundException ("Shift rule not found").
    var baseShift = await _context.Shifts
        .FirstOrDefaultAsync(s => s.PublicId == shiftRulePublicId);

    if (baseShift == null)
    {
        throw new KeyNotFoundException($"Shift rule with ID {shiftRulePublicId} not found.");
    }


    // 3. CONVERT THE DATE
    // TODO: Convert the 'shiftDate' (DateTime) parameter into a 'DateOnly' variable for the database.
    var dateOnly = DateOnly.FromDateTime(shiftDate);
   


    // 4. THE ROUTER: Insert into the correct Staff table
    switch (staffType)
    {
        case "Doctor":
            // TODO a: Query _context.Doctors to find the doctor using the publicId and comparing it with the 'staffPublicId' parameter . Throw exception if not found.
          
            var doctor = await _context.Doctors
              .FirstOrDefaultAsync(d => d.PublicId == staffPublicId);
            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {staffPublicId} not found.");
            }
            
            // TODO b: Create a new DoctorsShift entity.
            // Map the DoctorId (internal int), ShiftId (internal int), and the DateOnly variable.
            // Don't forget to generate a new PublicId! (e.g., SecureIdGenerator.GenerateId(15, "DrShft"))
            
            var doctorShift = new DoctorsShift
            {
                DoctorId = doctor.DoctorId,   // The internal INT
                ShiftId = baseShift.ShiftId,  // The internal INT
                Date = dateOnly,
                PublicId = SecureIdGenerator.GenerateID(15,"DrSh") //NOTE: This isn't really needed because i already have it set in the entity itself
            };
            
            // TODO c: Add the new entity to _context.DoctorsShifts.
            _context.DoctorsShifts.Add(doctorShift);
            break;
        
        

        case "Nurse":
            // TODO a: Query _context.Nurses to find the nurse using 'staffPublicId'. Throw exception if not found.
            var nurse = await _context.Nurses
                .FirstOrDefaultAsync(n => n.PublicId == staffPublicId);
            if (nurse == null)
            {
                throw new KeyNotFoundException($"Nurse with ID {staffPublicId} not found.");
            }
            
            // TODO b: Create a new NurseShift entity, map the properties, and generate a PublicId.
            var nurseShift = new NurseShift
            {
                NurseId = nurse.NurseId,
                ShiftId = baseShift.ShiftId,
                Date = dateOnly,
                PublicId = SecureIdGenerator.GenerateID(15, "NrSh")

            }; 
            // TODO c: Add to _context.NurseShifts.
            // TODO d: Log this action using your AuditService.
            _context.NurseShifts.Add(nurseShift);
            break;

        
        
        case "Secretary":
            var secretary = await _context.Secretaries
                .FirstOrDefaultAsync(n => n.PublicId == staffPublicId);

            if (secretary == null)
            {
                throw new KeyNotFoundException($"Nurse with ID {staffPublicId} not found.");
            }

            var secretaryShift = new SecretaryShift
            {
                SecretaryId = secretary.SecretaryId,
                ShiftId = baseShift.ShiftId,
                Date = dateOnly,
                PublicId = SecureIdGenerator.GenerateID(15, "SeSh")
            }; 
            _context.SecretaryShifts.Add(secretaryShift);
            break; 
        
        case "Admin":
            var admin = await _context.AdministrativeAssistants
                .FirstOrDefaultAsync(a => a.PublicId == staffPublicId);
            if (admin == null)
            {
                throw new KeyNotFoundException($"Nurse with ID {staffPublicId} not found.");
            }

            var adminShift = new AdminAssistantShift
            {
                AdminShiftId = admin.AdminId,
                ShiftId = baseShift.ShiftId,
                Date = dateOnly,
                PublicId = SecureIdGenerator.GenerateID(15, "AdSh")
            }; 
            _context.AdminAssistantShifts.Add(adminShift);
            break; 
        
        default:
            throw new ArgumentException($"Invalid staff type: {staffType}");
    }

    // 5. COMMIT TO DATABASE
        // TODO: Call SaveChangesAsync() to officially write the records to MySQL.
    await _context.SaveChangesAsync();

    await _auditService.LogAsync(new AuditLog
    {
        PerformedBy = actorPublicId,
        ActionType = "Create",
        Timestamp = DateTime.UtcNow,
        Details = $" {actorPublicId} viewed their shifts."
    });

    
    // 6. RETURN SUCCESS
    // TODO: Return a success message or the new Shift's PublicId!
    return "Shift successfully scheduled!"; 
}

    
    
    
    
    
    
    public async Task CancelShiftAsync (string shiftPublicId, string role, string actorPublicId, int currentUserId)
    {
        // 1. THE BOUNCER: Authorization Check
        // TODO: Only Managers, Admins, or the actual Staff Member who owns the shift can cancel it.
        // For now, let's just make sure they are at least a valid role.
        if (role != "Manager" && role != "Admin" && role != "Doctor" && role != "Nurse" && role != "Secretary")
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel shifts.");
        }
        
        
        
        var docShift = await _context.DoctorsShifts.FirstOrDefaultAsync(ds => ds.PublicId == shiftPublicId);
        if (docShift != null)
        {
            _context.DoctorsShifts.Remove(docShift);
        }

        
        // TODO b: If it wasn't a Doctor, check NurseShifts
        else if ( await _context.NurseShifts.FirstOrDefaultAsync(ns => ns.PublicId == shiftPublicId) is { } nurseShift)
        {
            _context.NurseShifts.Remove(nurseShift); 
        }

        
        // TODO c: If it wasn't a Nurse, check SecretaryShift
        else if ( await _context.SecretaryShifts.FirstOrDefaultAsync(ss => ss.PublicId == shiftPublicId) is { } secretaryShift ) // NOTE: "is { } secretaryShift" here essentially means "if not null"   
        {
            _context.SecretaryShifts.Remove(secretaryShift); 
        }
             
        
        // TODO d: If it wasn't a Secretary, check AdminassitantShift
        else if ( await _context.AdminAssistantShifts.FirstOrDefaultAsync(aas => aas.PublicId == shiftPublicId) is { } adminShift )
        {
            _context.AdminAssistantShifts.Remove(adminShift);
        }
        
        await _context.SaveChangesAsync();
        
        await _auditService.LogAsync(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Delete",
            Timestamp = DateTime.UtcNow,
            Details = $" {shiftPublicId} was cancelled by {actorPublicId}."
        });
        
    }
    
}