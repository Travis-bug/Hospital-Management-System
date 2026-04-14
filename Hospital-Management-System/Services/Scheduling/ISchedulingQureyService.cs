using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;

namespace Hospital_Management_System.Services.Scheduling;

public interface ISchedulingQueryService
{

// ============================================================================
// GATE 1: THE WORKHORSE (Internal IDs)
// ============================================================================
    Task<Shift?> GetShiftByIdAsync(int shiftId);

// ============================================================================
// GATE 2: THE GETS (Frontend Facing - Public IDs & Role Checks)
// ============================================================================

// For a Doctor/Nurse looking at their own schedule for the week
    Task<IEnumerable<StaffShiftDto>> GetMyShiftsAsync(DateTime startDate, DateTime endDate, string role, int currentUserId, string actorPublicId);

// For the Secretary or Manager seeing everyone who is in the building today
    Task<IEnumerable<DailyRosterDto>> GetDailyRosterAsync(DateTime date, string role);

    Task<IEnumerable<ShiftRuleDto>> GetShiftRulesAsync(string role);
    
} 
