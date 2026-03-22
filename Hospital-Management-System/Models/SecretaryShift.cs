using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Models;

[Table("Secretary_Shifts")]
[Index("SecretaryId", Name = "SecretaryID")]
[Index("ShiftId", Name = "ShiftID")]
public partial class SecretaryShift
{
    [Key]
    [Column("Secretary_ShiftID")]
    public int SecretaryShiftId { get; set; }

    [Column("SecretaryID")]
    public int SecretaryId { get; set; }

    public DateOnly Date { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockInTime { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockOutTime { get; set; }

    [Column("ShiftID")]
    public int ShiftId { get; set; }

    [Precision(5, 2)]
    public decimal? HoursWorked { get; set; }

    [ForeignKey("SecretaryId")]
    [InverseProperty("SecretaryShifts")]
    public virtual Secretary Secretary { get; set; } = null!;

    [ForeignKey("ShiftId")]
    [InverseProperty("SecretaryShifts")]
    public virtual Shift Shift { get; set; } = null!;
}
