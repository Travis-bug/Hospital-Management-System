using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
namespace Hospital_Management_System.Services.Scheduling
{
    public interface IAppointmentService
    {
        // ============================================================================
        // GATE 1: THE WORKHORSE (Internal Use Only - Uses Database IDs)
        // ============================================================================
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);


        // ============================================================================
        // GATE 2: THE GETS (Frontend Facing - Uses Public IDs & Role Checks)
        // ============================================================================
        
        // The one you just wrote! For the Secretary or Doctor checking the daily roster.
        Task<IEnumerable<Appointment>> GetDoctorScheduleAsync(string doctorPublicId, DateTime date, string role, int currentUserId);
        
        // For clicking on a specific appointment to see the details.
        Task<Appointment?> GetAppointmentByPublicIdAsync(string appointmentPublicId, string role, int currentUserId, string actorPublicId);


        // ============================================================================
        // GATE 3: THE ACTIONS (Creating/Modifying Data - Needs Audit Logging)
        // ============================================================================
        
        // The hardest one: Must check for double-booking before saving!
        Task<Appointment> BookAppointmentAsync(BookAppointmentDto dto, string role, string actorPublicId);
        
        // We use 'Cancel' instead of 'Delete' to keep the hospital records intact.
       Task CancelAppointmentAsync(string appointmentPublicId, string role, string actorPublicId, int currentUserId) ;
    }
}
