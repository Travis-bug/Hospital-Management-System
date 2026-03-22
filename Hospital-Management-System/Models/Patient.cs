using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Patient")]
[Index("DoctorId", Name = "DoctorID")]
[Index("HealthCardNo", Name = "HealthCardNo", IsUnique = true)]
[Index("PhoneNumber", Name = "PhoneNumber", IsUnique = true)]
[Index("PrimaryMemberId", Name = "PrimaryMemberID")]
public partial class Patient
{
    [Key]
    [Column("PatientID")]
    public int PatientId { get; init; }

    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string LastName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Address { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)] 
    public string? Email { get; set; } = ""; 
    
    

    [StringLength(20)]
    public string HealthCardNo { get; set; } = null!;

    
    
    
    [Column(TypeName = "enum('Enrolled','Walk-in')")]
    [StringLength(50)]
    public string Type { get; set; } = null!;

    [Column("DoctorID")]
    public int? DoctorId { get; init; }

    [Column("PrimaryMemberID")]
    public int? PrimaryMemberId { get; init; }

    [Column(TypeName = "enum('Husband','Wife','Son','Daughter','Father','Mother','Other')")]
    [StringLength(50)]
    public string? Relationship { get; init; } // mnight change back to set later

    
    [InverseProperty("Patient")]
    public virtual ICollection<Appointment> Appointments { get; init; } = new List<Appointment>();

    [ForeignKey("DoctorId")]
    [InverseProperty("Patients")]
    public virtual Doctor? Doctor { get; init; }

    [InverseProperty("Patient")]
    public virtual ICollection<Fee> Fees { get; init; } = new List<Fee>();

    [InverseProperty("PrimaryMember")]
    public virtual ICollection<Patient> InversePrimaryMember { get; init; } = new List<Patient>();

    [ForeignKey("PrimaryMemberId")]
    [InverseProperty("InversePrimaryMember")]
    public virtual Patient? PrimaryMember { get; init; }

    [InverseProperty("Patient")]
    public virtual ICollection<Visit> Visits { get; init; } = new List<Visit>();
}
