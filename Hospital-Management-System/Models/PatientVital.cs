using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Patient_Vitals")]
[Index("NurseId", Name = "NurseID")]
[Index("VisitsId", Name = "VisitsID")]
public partial class PatientVital
{
    [Key]
    [Column("VitalsID")]
    public int VitalsId { get; set; }
    
    [Required]
    [MaxLength(20)]

    [Column("VisitsID")]
    public int VisitsId { get; set; }

    [Column("NurseID")]
    public int NurseId { get; set; }

    [Precision(10, 2)]
    public decimal? Weight { get; set; }

    [Precision(10, 2)]
    public decimal? Height { get; set; }

    [StringLength(15)]
    public string? BloodPressure { get; set; }

    [Precision(10, 2)]
    public decimal? Temperature { get; set; }

    [Column("Recorded_At", TypeName = "timestamp")]
    public DateTime? RecordedAt { get; set; }

    [ForeignKey("NurseId")]
    [InverseProperty("PatientVitals")]
    public virtual Nurse Nurse { get; set; } = null!;

    [ForeignKey("VisitsId")]
    [InverseProperty("PatientVitals")]
    public virtual Visit Visits { get; set; } = null!;
}
