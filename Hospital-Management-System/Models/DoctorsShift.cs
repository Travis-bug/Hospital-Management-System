using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Models;

[Table("Doctors_Shifts")]
[Index("DoctorId", Name = "DoctorID")]
[Index("ShiftId", Name = "ShiftID")]
public partial class DoctorsShift
{
    //==============================================================
    [Key]
    [Column("Doctors_ShiftID")]
    public int DoctorsShiftId { get; set; }
    [Required]
    [StringLength(10)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(5, "DRSH");
    //==============================================================

    public DateOnly Date { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockInTime { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? ClockOutTime { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [Column("ShiftID")]
    public int ShiftId { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("DoctorsShifts")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("ShiftId")]
    [InverseProperty("DoctorsShifts")]
    public virtual Shift Shift { get; set; } = null!;
}
