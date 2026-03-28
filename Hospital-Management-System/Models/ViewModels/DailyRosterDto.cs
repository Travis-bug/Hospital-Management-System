namespace Hospital_Management_System.Models.ViewModels
{
    public class DailyRosterDto
    {
        public string ShiftPublicId { get; set; } = null!;
        public string StaffName { get; set; } = null!;
        public string Role { get; set; } = null!; // "Doctor", "Nurse", etc.
        public string ShiftType { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}