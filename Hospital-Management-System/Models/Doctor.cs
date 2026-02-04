using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Models;

[Table("Doctor")]
public partial class Doctor
{
    [Key]
    [Column("DoctorID")]
    public int DoctorId { get; set; }

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
