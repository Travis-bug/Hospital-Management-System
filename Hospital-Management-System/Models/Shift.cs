using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Hospital_Management_System.Models;
[Table("Shift")]
public partial class Shift
{
    //==============================================================
    [Key]
    [Column("ShiftID")]
    public int ShiftId { get; init; }
    
    [Required]
    [StringLength(20)]
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(15);
    //==============================================================
    
    
    [Column(TypeName = "enum('Morning','Evening')")]
    [StringLength(30)]
    public string ShiftType { get; set; } = null!;

    [Column(TypeName = "time")]
    public TimeOnly StartTime { get; set; }

    [Column(TypeName = "time")]
    public TimeOnly EndTime { get; set; }

    [InverseProperty("Shift")]
    public virtual ICollection<AdminAssistantShift> AdminAssistantShifts { get; set; } = new List<AdminAssistantShift>();

    [InverseProperty("Shift")]
    public virtual ICollection<DoctorsShift> DoctorsShifts { get; set; } = new List<DoctorsShift>();

    [InverseProperty("Shift")]
    public virtual ICollection<NurseShift> NurseShifts { get; set; } = new List<NurseShift>();

    [InverseProperty("Shift")]
    public virtual ICollection<SecretaryShift> SecretaryShifts { get; set; } = new List<SecretaryShift>();
}
