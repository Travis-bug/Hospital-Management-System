using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
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
        if (role != "Admin" && role != "Manager")
        {
            throw new UnauthorizedAccessException("You are not authorized to schedule shifts.");
        }

        var baseShift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.PublicId == shiftRulePublicId);

        if (baseShift == null)
        {
            throw new KeyNotFoundException($"Shift rule with ID {shiftRulePublicId} not found.");
        }

        var dateOnly = DateOnly.FromDateTime(shiftDate);
        string newShiftPublicId;

        switch (staffType)
        {
            case "Doctor":
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.PublicId == staffPublicId);
                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {staffPublicId} not found.");
                }

                var doctorShift = new DoctorsShift
                {
                    DoctorId = doctor.DoctorId,
                    ShiftId = baseShift.ShiftId,
                    Date = dateOnly,
                    PublicId = SecureIdGenerator.GenerateID(5, "DRSH")
                };

                _context.DoctorsShifts.Add(doctorShift);
                newShiftPublicId = doctorShift.PublicId;
                break;

            case "Nurse":
                var nurse = await _context.Nurses
                    .FirstOrDefaultAsync(n => n.PublicId == staffPublicId);
                if (nurse == null)
                {
                    throw new KeyNotFoundException($"Nurse with ID {staffPublicId} not found.");
                }

                var nurseShift = new NurseShift
                {
                    NurseId = nurse.NurseId,
                    ShiftId = baseShift.ShiftId,
                    Date = dateOnly,
                    PublicId = SecureIdGenerator.GenerateID(6, "NSH")
                };

                _context.NurseShifts.Add(nurseShift);
                newShiftPublicId = nurseShift.PublicId;
                break;

            case "Secretary":
                var secretary = await _context.Secretaries
                    .FirstOrDefaultAsync(secretaryEntity => secretaryEntity.PublicId == staffPublicId);

                if (secretary == null)
                {
                    throw new KeyNotFoundException($"Secretary with ID {staffPublicId} not found.");
                }

                var secretaryShift = new SecretaryShift
                {
                    SecretaryId = secretary.SecretaryId,
                    ShiftId = baseShift.ShiftId,
                    Date = dateOnly,
                    PublicId = SecureIdGenerator.GenerateID(6, "SSH")
                };

                _context.SecretaryShifts.Add(secretaryShift);
                newShiftPublicId = secretaryShift.PublicId;
                break;

            case "Admin":
                var admin = await _context.AdministrativeAssistants
                    .FirstOrDefaultAsync(a => a.PublicId == staffPublicId);
                if (admin == null)
                {
                    throw new KeyNotFoundException($"Admin with ID {staffPublicId} not found.");
                }

                var adminShift = new AdminAssistantShift
                {
                    AdminId = admin.AdminId,
                    ShiftId = baseShift.ShiftId,
                    Date = dateOnly,
                    PublicId = SecureIdGenerator.GenerateID(6, "ASH")
                };

                _context.AdminAssistantShifts.Add(adminShift);
                newShiftPublicId = adminShift.PublicId;
                break;

            default:
                throw new ArgumentException($"Invalid staff type: {staffType}");
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Scheduled shift {newShiftPublicId} for {staffType} {staffPublicId}."
        });

        return newShiftPublicId;
    }

    
    
    
    
    
    
    public async Task CancelShiftAsync (string shiftPublicId, string role, string actorPublicId, int currentUserId)
    {
        if (role != "Manager" && role != "Admin")
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
        else
        {
            throw new KeyNotFoundException("Shift not found.");
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
