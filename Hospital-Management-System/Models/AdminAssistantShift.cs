using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("AdminAssistant_Shifts")]
[Index("AdminId", Name = "AdminID")]
[Index("ShiftId", Name = "ShiftID")]
public partial class AdminAssistantShift
{ 
    //==============================================================
    [Key]
    [Column("AdminShiftID")]
    public int AdminShiftId { get; set; }
    
    [Required]
    [StringLength(10)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(6, "ASH"); // public id 
    //==============================================================

    public DateOnly Date { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockInTime { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockOutTime { get; set; }

    [Column("AdminID")]
    public int AdminId { get; set; }

    [Column("ShiftID")]
    public int ShiftId { get; set; }

    [Precision(5, 2)]
    public decimal? HoursWorked { get; set; }

    [ForeignKey("AdminId")]
    [InverseProperty("AdminAssistantShifts")]
    public virtual AdministrativeAssistant Admin { get; set; } = null!;

    [ForeignKey("ShiftId")]
    [InverseProperty("AdminAssistantShifts")]
    public virtual Shift Shift { get; set; } = null!;
}
