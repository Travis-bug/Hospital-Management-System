using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Models;

[Table("Prescription")]
[Index("DoctorId", Name = "DoctorID")]
[Index("ResultId", Name = "ResultID")]
[Index("VisitsId", Name = "VisitsID")]
public partial class Prescription
{
    [Key]
    [Column("PrescriptionID")]
    public int PrescriptionId { get; set; }

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
