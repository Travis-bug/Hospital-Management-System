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
    public int TestId { get; set; } // Foreign key column

    [Column("VisitID")]
    public int VisitId { get; set; }


    /// <summary>
    /// Represents the publicly accessible identifier for a specific test result.
    /// </summary>
    /// <remarks>
    /// This is a unique, alphanumeric string required for identification of test results in public contexts.
    /// It is automatically generated and assigned when a new test result is created.
    /// This property has a maximum length of 12 characters and must be provided for the entity.
    /// </remarks>
    [Required]
    [MaxLength(12)]
    [Column("PublicTestId")] 
    public string PublicTestId { get; set; } = string.Empty; 
    
    
    [Column("NurseID")]
    public int NurseId { get; set; }

    [Column(TypeName = "text")]
    [StringLength(50)]
        
    public string Findings { get; set; } = null!;

    [Column(TypeName = "timestamp")]
    public DateTime? ResultDate { get; set; }

    
    
    
    [ForeignKey("VisitId")]
    public virtual Visit Visit { get; set; } = null!; // new 
    
    [ForeignKey("NurseId")]
    [InverseProperty("TestResults")]
    public virtual Nurse Nurse { get; set; } = null!;

    [InverseProperty("Result")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    
    
    [ForeignKey("TestId")]
    [InverseProperty("TestResults")]
    public virtual DiagnosticTest Test { get; set; } = null!;
}
