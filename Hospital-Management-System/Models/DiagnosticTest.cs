using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("DiagnosticTest")]
[Index("DoctorId", Name = "DoctorID")]
[Index("VisitId", Name = "VisitID")]
public partial class DiagnosticTest
{
    [Key]
    [Column("TestID")]
    public int TestId { get; set; }

    [Column("VisitID")]
    public int VisitId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [StringLength(100)]
    public string TestName { get; set; } = null!;

    [Column(TypeName = "text")]
    public string ClinicalNotes { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? OrderedAt { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("DiagnosticTests")]
    public virtual Doctor Doctor { get; set; } = null!;

    [InverseProperty("Test")]
    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    [ForeignKey("VisitId")]
    [InverseProperty("DiagnosticTests")]
    public virtual Visit Visit { get; set; } = null!;
}
