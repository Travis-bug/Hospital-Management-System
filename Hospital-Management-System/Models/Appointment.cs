using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Appointment")]
[Index("DoctorId", Name = "DoctorID")]
[Index("NurseId", Name = "NurseID")]
[Index("PatientId", Name = "PatientID")]
public partial class Appointment
{
    [Key]
    [Column("AppointmentID")]
    public int AppointmentId { get; set; }


    /// <summary>
    /// A unique identifier for the entity, represented as a required string with a maximum length of 12.
    /// This property is used to publicly reference the entity in a consistent manner.
    /// </summary>
    [Required]
    [MaxLength(12)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(8, "APT"); 


    [Column("PatientID")] public int PatientId { get; set; }

    [Column("DoctorID")]
    public int? DoctorId { get; init; }

    [Column("NurseID")]
    public int? NurseId { get; init; }

    // The ? makes it required in the database (unless you explicitly allow nulls)
    public DateTime AppointmentDate { get; set; } // NOTE: this is new 
    
    public DateTime? BookedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "enum('Booked','Cancelled','Arrived','Checked In','Checked Out','LWT','No-Show')")]
    [StringLength(30)]
    public string? Status { get; set; }

    [Column(TypeName = "text")]
    [StringLength(30)]
    public string? Notes { get; set; }

    
    
    
    
    
    
    
    
    
    
    [ForeignKey("DoctorId")]
    [InverseProperty("Appointments")]
    public virtual Doctor? Doctor { get; set; }

    [ForeignKey("NurseId")]
    [InverseProperty("Appointments")]
    public virtual Nurse? Nurse { get; set; }

    [ForeignKey("PatientId")]
    [InverseProperty("Appointments")]
    public virtual Patient? Patient { get; set; }

    [InverseProperty("Appointment")]
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
