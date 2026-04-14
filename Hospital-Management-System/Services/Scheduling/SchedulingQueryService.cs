using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.Scheduling;

public class SchedulingQueryService : ISchedulingQueryService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;

    public SchedulingQueryService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }
 public async Task<Shift?> GetShiftByIdAsync(int shiftId)
 {
     return await _context.Shifts
         .FirstOrDefaultAsync(s => s.ShiftId == shiftId);
 }
 
 public async Task<IEnumerable<StaffShiftDto>> GetMyShiftsAsync(DateTime startDate, DateTime endDate, string role, int currentUserId, string actorPublicId)
 {
     var start = DateOnly.FromDateTime(startDate);
     var end = DateOnly.FromDateTime(endDate);
     
     IEnumerable<StaffShiftDto> myShifts;

     switch (role)
     {
         case "Doctor":
             myShifts = await _context.DoctorsShifts
                 .AsNoTracking()
                 .Include(ds => ds.Shift) // Join the Shift table to get the times
                 .Where(ds => ds.DoctorId == currentUserId && ds.Date >= start && ds.Date <= end)
                 .Select(ds => new StaffShiftDto
                 {
                     Date = ds.Date,
                     ShiftType = ds.Shift.ShiftType,
                     StartTime = ds.Shift.StartTime,
                     EndTime = ds.Shift.EndTime
                 })
                 .OrderBy(dto => dto.Date)
                 .ToListAsync();
             break;
           
         
         case "Nurse" : 
             myShifts = await _context.NurseShifts
                 .AsNoTracking()
                 .Include(ns => ns.Shift) // Join the Shift table to get the times
                 .Where(ns => ns.NurseId == currentUserId && ns.Date >= start && ns.Date <= end)
                 .Select(ns => new StaffShiftDto
                 {
                     Date = ns.Date,
                     ShiftType = ns.Shift.ShiftType,
                     StartTime = ns.Shift.StartTime,
                     EndTime = ns.Shift.EndTime
                 })
                 .OrderBy(dto => dto.Date)
                 .ToListAsync();
             break; 
         
         case "Secretary" :
             myShifts = await _context.SecretaryShifts
                 .AsNoTracking()
                 .Include(ss => ss.Shift) // Join the Shift table to get the times
                 .Where(ss => ss.SecretaryId == currentUserId && ss.Date >= start && ss.Date <= end)
                 .Select(ss => new StaffShiftDto
                 {
                     Date = ss.Date,
                     ShiftType = ss.Shift.ShiftType,
                     StartTime = ss.Shift.StartTime,
                     EndTime = ss.Shift.EndTime
                 })
                 .OrderBy(dto => dto.Date)
                 .ToListAsync(); 
             break;
            
         
         case "Admin" :
              myShifts =  await _context.AdminAssistantShifts
                 .AsNoTracking()
                 .Include(aas => aas.Shift) // Join the Shift table to get the times
                 .Where(aas => aas.AdminId == currentUserId && aas.Date >= start && aas.Date <= end)
                 .Select(aas => new StaffShiftDto
                 {
                     Date = aas.Date,
                     ShiftType = aas.Shift.ShiftType,
                     StartTime = aas.Shift.StartTime,
                     EndTime = aas.Shift.EndTime
                 })
                 .OrderBy(dto => dto.Date)
                 .ToListAsync();
              break; 
         
         default: 
             throw new UnauthorizedAccessException("Role not authorized to view shifts.");
     }
    
     
         await _auditService.LogAsync(new AuditLog
         {
             PerformedBy = actorPublicId,
             ActionType = "Read",
             Timestamp = DateTime.UtcNow,
             Details = $" {actorPublicId} viewed their shifts."
         });
         
     return myShifts;
 }


 public async Task<IEnumerable<DailyRosterDto>> GetDailyRosterAsync(DateTime date, string role)
 {
     if (role != "Manager" && role != "Admin")
     {
         throw new UnauthorizedAccessException("You are not authorized to view the hospital-wide roster.");
     }

     var targetDate = DateOnly.FromDateTime(date);
     var fullRoster = new List<DailyRosterDto>();

     // 2. FETCH DOCTORS
     var doctorsOnDuty = await _context.DoctorsShifts
         .AsNoTracking()
         .Include(ds => ds.Doctor) // Join the left side of the junction (Name)
         .Include(ds => ds.Shift)  // Join the right side of the junction (Times)
         .Where(ds => ds.Date == targetDate)
         .Select(ds => new DailyRosterDto
         {
             ShiftPublicId = ds.PublicId,
             StaffName = $"Dr. {ds.Doctor.FirstName} {ds.Doctor.LastName}",
             Role = "Doctor",
             ShiftType = ds.Shift.ShiftType,
             StartTime = ds.Shift.StartTime,
             EndTime = ds.Shift.EndTime
         })
         .ToListAsync();
     
     fullRoster.AddRange(doctorsOnDuty);
     

     // 3. FETCH NURSES
     var nursesOnDuty = await _context.NurseShifts
         .AsNoTracking()
         .Include(ns => ns.Nurse)
         .Include(ns => ns.Shift)
         .Where(ns => ns.Date == targetDate)
         .Select(ns => new DailyRosterDto
         {
             ShiftPublicId = ns.PublicId,
             StaffName = $"{ns.Nurse.FirstName} {ns.Nurse.LastName}",
             Role = "Nurse",
             ShiftType = ns.Shift.ShiftType,
             StartTime = ns.Shift.StartTime,
             EndTime = ns.Shift.EndTime
         })
         .ToListAsync();
     
     fullRoster.AddRange(nursesOnDuty);
     
     
     var SecretariesOnDuty = await _context.SecretaryShifts
         .AsNoTracking()
         .Include(ss => ss.Secretary)
         .Include(ss => ss.Shift)
         .Where(ss => ss.Date == targetDate)
         .Select(ss => new DailyRosterDto
         {
             ShiftPublicId = ss.PublicId,
             StaffName = $"{ss.Secretary.FirstName} {ss.Secretary.LastName}",
             Role = "Secretary",
             ShiftType = ss.Shift.ShiftType,
             StartTime = ss.Shift.StartTime,
             EndTime = ss.Shift.EndTime
         })
         .ToListAsync();
     
     fullRoster.AddRange(SecretariesOnDuty);

     
     var AdminOnDuty = await _context.AdminAssistantShifts
         .AsNoTracking()
         .Include(aas => aas.Admin)
         .Include(aas => aas.Shift)
         .Where(aas => aas.Date == targetDate)
         .Select(aas => new DailyRosterDto
         {
             ShiftPublicId = aas.PublicId,
             StaffName = $"{aas.Admin.FirstName} {aas.Admin.LastName}",
             Role = "Admin",
             ShiftType = aas.Shift.ShiftType,
             StartTime = aas.Shift.StartTime,
             EndTime = aas.Shift.EndTime
         })
         .ToListAsync();
     
     fullRoster.AddRange(AdminOnDuty);
     
     // 4. SORT AND RETURN: Order by who clocks in earliest
     return fullRoster.OrderBy(r => r.StartTime).ToList();
 }

 public async Task<IEnumerable<ShiftRuleDto>> GetShiftRulesAsync(string role)
 {
     if (role != "Manager" && role != "Admin")
     {
         throw new UnauthorizedAccessException("You are not authorized to view shift rules.");
     }

     return await _context.Shifts
         .AsNoTracking()
         .OrderBy(shift => shift.StartTime)
         .Select(shift => new ShiftRuleDto(
             shift.PublicId,
             shift.ShiftType,
             shift.StartTime,
             shift.EndTime))
         .ToListAsync();
 }

} 
