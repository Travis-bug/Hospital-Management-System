
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Index("AppointmentId", Name = "AppointmentID")]
[Index("PatientId", Name = "PatientID")]
[Index(nameof(VisitPublicId), IsUnique = true)]
public partial class Visit
{
    [Key]
    [Column("VisitsID")]
    public int VisitsId { get; init; }
    
    [Required]
    [MaxLength(12)]
    [Column("PublicID")] 
    public string VisitPublicId { get; set; } = string.Empty; // new for encrypted public secure string
    
    //These 2 go hand in hand if the patient walks in the visit becoming active, and the type becomes admitted
    //if the patient's visit is completed, the status changes to discharged
    
    //=========================================================
    
    // Tracks the lifecycle of this specific visit record
    [Column(TypeName = "enum('Active', 'Completed')")] 
    [StringLength(30)]
    public string? Status { get; set; }

    // Defines the billing/care category
    [Column(TypeName = "enum('Inpatient', 'Outpatient', 'Emergency', 'ER Referral')")]
    [StringLength(30)]
    public string? PatientClass { get; set; }

    // Tracks if they are currently occupying a hospital bed
    [Column(TypeName = "enum('Admitted', 'Not Admitted', 'Discharged', 'Triage Pending')")]
    [StringLength(30)]
    public string? AdmissionStatus { get; set; }

    // How did they get here?
    [Column(TypeName = "enum('Appointment', 'Walk-in')")]
    [StringLength(30)]
    public string? ArrivalSource { get; set; }

    //=======================================================
    
    [Column("PatientID")]
    public int PatientId { get; init; }

    
    [Column("Checkin_Time", TypeName = "timestamp")]
    public DateTime? CheckinTime { get; set; }

    
    [Column("Checkout_Time", TypeName = "timestamp")]
    public DateTime? CheckoutTime { get; set; }
    
   
    [Column(TypeName = "text")]
    [StringLength(30)]
    public string? Symptoms { get; set; }

    
    [Column(TypeName = "text")]
    [StringLength(30)]
    public string? Diagnosis { get; set; }

    
    [Column(TypeName = "text")]
    [StringLength(30)]
    public string? Treatment { get; set; }

    
    [Column("AppointmentID")]
    public int? AppointmentId { get; set; }

    
    
    [Column("DoctorID")]
    public int? DoctorId { get; set; } //NEW Nullable because a visit starts with a Nurse/Triage first
    
    [Column("NurseId")]
    public int? NurseId { get; set; } 
    
    
    [Column(TypeName = "text")]
    [StringLength(100)]
    public string VisitNotes { get; set; } = null!;
    
    
    
    
    
    
    
    
    
    
    
    
    
    [ForeignKey("AppointmentId")]
    [InverseProperty("Visits")]
    public virtual Appointment? Appointment { get; set; }

    
    [InverseProperty("Visit")]
    public virtual ICollection<DiagnosticTest> DiagnosticTests { get; set; } = new List<DiagnosticTest>();

    
    [ForeignKey("PatientId")]
    [InverseProperty("Visits")]
    public virtual Patient Patient { get; set; } = null!;

    
    [InverseProperty("Visits")]
    public virtual ICollection<PatientVital> PatientVitals { get; set; } = new List<PatientVital>();

    
    [InverseProperty("Visits")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    
    [InverseProperty("Visit")]
    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();
    
    
    [InverseProperty("Visit")]
    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();
    
    
    [ForeignKey("DoctorId")]
    [InverseProperty("Visits")]
    public virtual Doctor? Doctor { get; set; }
    
    
    [ForeignKey("NurseId")]
    [InverseProperty("Visits")]
    public virtual Nurse? Nurse { get; set; }

    
}
