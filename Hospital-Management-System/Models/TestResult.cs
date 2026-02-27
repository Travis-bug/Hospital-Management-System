using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("TestResult")]
[Index("NurseId", Name = "NurseID")]
[Index("TestId", Name = "TestID")]
public partial class TestResult
{
    [Key]
    [Column("ResultID")]
    public int ResultId { get; set; }

    [Column("TestID")]
    public int TestId { get; set; }

    [Column("NurseID")]
    public int NurseId { get; set; }

    [Column(TypeName = "text")]
    public string Findings { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? ResultDate { get; set; }

    [ForeignKey("NurseId")]
    [InverseProperty("TestResults")]
    public virtual Nurse Nurse { get; set; } = null!;

    [InverseProperty("Result")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    [ForeignKey("TestId")]
    [InverseProperty("TestResults")]
    public virtual DiagnosticTest Test { get; set; } = null!;
}
