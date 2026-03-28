
namespace Hospital_Management_System.Models.ViewModels
{
    public class StaffShiftDto
    {
        public DateOnly Date { get; set; }
        public string ShiftType { get; set; } = null!; // "Morning", "Evening" ---REVIEW
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}