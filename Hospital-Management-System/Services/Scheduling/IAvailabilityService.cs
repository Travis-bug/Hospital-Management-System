using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;

namespace Hospital_Management_System.Services.Scheduling;

public interface IAvailabilityService
{
    // Assigns a specific Doctor/Nurse to a time block
    Task<string> ScheduleStaffAsync(DateTime shiftDate, string shiftRulePublicId, string staffPublicId, string staffType, string role, string actorPublicId);
        
    // If someone calls in sick or leaves, we can cancel their shift
    Task CancelShiftAsync(string shiftPublicId, string role, string actorPublicId, int currentUserId);
}