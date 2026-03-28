using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Prescription")]
[Index("DoctorId", Name = "DoctorID")]
[Index("ResultId", Name = "ResultID")]
[Index("VisitsId", Name = "VisitsID")]
public partial class Prescription
{
    [Key]
    [Column("PrescriptionID")]
    public int PrescriptionId { get; set; }


    /// <summary>
    /// Represents a unique public identifier for the entity.
    /// This property is required and has a maximum length of 12 characters.
    /// </summary>
    [Required]
    [StringLength(20)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(15, "PRE");

    [Column("VisitsID")]
    public int? VisitsId { get; set; }

    [Column("DoctorID")]
    public int? DoctorId { get; set; }

    [Column("ResultID")]
    public int? ResultId { get; set; }

    [StringLength(100)]
    public string MedicineName { get; set; } = null!;

    [StringLength(50)]
    public string? Dosage { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("Prescriptions")]
    public virtual Doctor? Doctor { get; set; }

    [ForeignKey("ResultId")]
    [InverseProperty("Prescriptions")]
    public virtual TestResult? Result { get; set; }

    [ForeignKey("VisitsId")]
    [InverseProperty("Prescriptions")]
    public virtual Visit? Visits { get; set; }
}
