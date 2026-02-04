using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Models;

[Table("Appointment")]
[Index("DoctorId", Name = "DoctorID")]
[Index("NurseId", Name = "NurseID")]
[Index("PatientId", Name = "PatientID")]
public partial class Appointment
{
    [Key]
    [Column("AppointmentID")]
    public int AppointmentId { get; set; }

    [Column("PatientID")]
    public int? PatientId { get; set; }

    [Column("DoctorID")]
    public int? DoctorId { get; set; }

    [Column("NurseID")]
    public int? NurseId { get; set; }

    public DateOnly Date { get; set; }

    [Column(TypeName = "time")]
    public TimeOnly Time { get; set; }

    [Column(TypeName = "enum('Booked','Cancelled','Arrived','Checked In','Checked Out','LWT','No-Show')")]
    public string? Status { get; set; }

    [Column(TypeName = "text")]
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
