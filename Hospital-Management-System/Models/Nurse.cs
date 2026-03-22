using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Nurse")]
public partial class Nurse
{
    [Key]
    [Column("NurseID")]
    public int NurseId { get; set; }
    
    
    /// <summary>
    /// Gets or sets the public identifier for a Nurse entity, used as a unique string-based key.
    /// This property is required and has a maximum length of 12 characters.
    /// </summary>
    [Required]
    [MaxLength(12)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = string.Empty;

    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Precision(10, 2)]
    public decimal? HourlyRate { get; set; }

    [StringLength(100)]
    public string? StreetAddress { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? Province { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }
    
    [StringLength(30)]
    public string? IdentityUserId { get; set; }

    [InverseProperty("Nurse")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Nurse")]
    public virtual ICollection<NurseShift> NurseShifts { get; set; } = new List<NurseShift>();

    [InverseProperty("Nurse")]
    public virtual ICollection<PatientVital> PatientVitals { get; set; } = new List<PatientVital>();

    [InverseProperty("Nurse")]
    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
