using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Models;

[Table("Fee")]
[Index("DoctorId", Name = "DoctorID")]
[Index("PatientId", Name = "PatientID")]
public partial class Fee
{
    [Key]
    [Column("FeeID")]
    public int FeeId { get; set; }

    [Column("PatientID")]
    public int? PatientId { get; set; }

    [Column("DoctorID")]
    public int? DoctorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FeeDate { get; set; }

    [StringLength(100)]
    public string ServiceName { get; set; } = null!;

    [Precision(10, 2)]
    public decimal Amount { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("Fees")]
    public virtual Doctor? Doctor { get; set; }

    [ForeignKey("PatientId")]
    [InverseProperty("Fees")]
    public virtual Patient? Patient { get; set; }
}
