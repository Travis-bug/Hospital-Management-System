using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Models;

[Index("AppointmentId", Name = "AppointmentID")]
[Index("PatientId", Name = "PatientID")]
public partial class Visit
{
    [Key]
    [Column("VisitsID")]
    public int VisitsId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("Checkin_Time", TypeName = "timestamp")]
    public DateTime? CheckinTime { get; set; }

    [Column("Checkout_Time", TypeName = "timestamp")]
    public DateTime? CheckoutTime { get; set; }

    [Column(TypeName = "text")]
    public string? Symptoms { get; set; }

    [Column(TypeName = "text")]
    public string? Diagnosis { get; set; }

    [Column(TypeName = "text")]
    public string? Treatment { get; set; }

    [Column("AppointmentID")]
    public int? AppointmentId { get; set; }

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
}
