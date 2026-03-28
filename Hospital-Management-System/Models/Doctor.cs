using Hospital_Management_System.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Hospital_Management_System.Models;

[Table("Doctor")]
public partial class Doctor
{
    
    [Key]
    [Column("DoctorID")]
    public int DoctorId { get; set; }

    /// <summary>
    /// Represents the public identifier for a doctor. This is a unique string value
    /// with a maximum length of 12 characters that serves as a public, required
    /// identifier for the Doctor entity.
    /// </summary>
    [Required]
    [StringLength(20)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(10, "DR");

    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [StringLength(50)]
    public string? Specialization { get; set; }

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

    
    // NEW This helps the system know if they can work the Triage desk 
    public bool IsTriageQualified { get; set; } = false;
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    [InverseProperty("Doctor")]
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
    
    
    
    [InverseProperty("Doctor")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Doctor")]
    public virtual ICollection<DiagnosticTest> DiagnosticTests { get; set; } = new List<DiagnosticTest>();

    [InverseProperty("Doctor")]
    public virtual ICollection<DoctorsShift> DoctorsShifts { get; set; } = new List<DoctorsShift>();

    [InverseProperty("Doctor")]
    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();

    [InverseProperty("Doctor")]
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    [InverseProperty("Doctor")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    [InverseProperty("ReferringDoctor")]
    public virtual ICollection<Referral> ReferralReferringDoctors { get; set; } = new List<Referral>();

    [InverseProperty("SpecialistDoctor")]
    public virtual ICollection<Referral> ReferralSpecialistDoctors { get; set; } = new List<Referral>();
}
