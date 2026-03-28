using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Index("NurseId", Name = "NurseID")]
[Index("ShiftId", Name = "ShiftID")]
public partial class NurseShift
{
    //==============================================================
    [Key]
    [Column("NurseShiftID")]
    public int NurseShiftId { get; set; }
    
    [Required]
    [StringLength(10)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(15, "SH");
    //==============================================================

    public DateOnly Date { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockInTime { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockOutTime { get; set; }

    [Column("NurseID")]
    public int NurseId { get; set; }

    [Column("ShiftID")]
    public int ShiftId { get; set; }

    [Precision(5, 2)]
    public decimal? HoursWorked { get; set; }

    [ForeignKey("NurseId")]
    [InverseProperty("NurseShifts")]
    public virtual Nurse Nurse { get; set; } = null!;

    [ForeignKey("ShiftId")]
    [InverseProperty("NurseShifts")]
    public virtual Shift Shift { get; set; } = null!;
}
