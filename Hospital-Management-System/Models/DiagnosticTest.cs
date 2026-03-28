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

    /// <summary>
    /// Gets or sets a unique public identifier associated with the entity.
    /// This property is required and constrained to a maximum length of 12 characters.
    /// </summary>
    [Required]
    [StringLength(20)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(15, "TES");



    [Column("VisitID")]
    public int VisitId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [StringLength(100)]
    public string TestName { get; set; } = null!;

    [Column(TypeName = "text")]
    [StringLength(30)]
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
