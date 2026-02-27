using System;
using System.Collections.Generic;
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
