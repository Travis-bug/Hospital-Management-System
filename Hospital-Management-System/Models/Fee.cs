using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Models;

[Table("Fee")]
[Index("DoctorId", Name = "DoctorID")]
[Index("PatientId", Name = "PatientID")]
[Index("VisitId", Name = "VisitID")]
public partial class Fee
{
    [Key]
    [Column("FeeID")]
    public int FeeId { get; set; }

    /// <summary>
    /// Represents the public identifier for an entity.
    /// </summary>
    /// <remarks>
    /// This property is a required field with a maximum length of 12 characters.
    /// It is used as a unique external identifier for the entity.
    /// </remarks>
    [Required]
    [MaxLength(20)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(15, "FEE");
    
    [Column("DoctorID")]
    public int? DoctorId { get; set; }
    
    [Column("PatientID")]
    public int? PatientId { get; set; }
    
    [Column("VisitID")]
    public int VisitId { get; set; }
    
    
    
    
    
    [Column("PatientName")]
    [StringLength(100)]
    public string PatientName { get; set; } = null!; 

    [StringLength(100)]
    public string ServiceName { get; set; } = null!;
    
    public bool IsPaid { get; set; } = false; 
    
    
    
    [Column(TypeName = "datetime")]
    public DateTime? FeeDate { get; set; }
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow; 
    
    
    [Precision(10, 2)]
    public decimal Amount { get; set; }

    
    
    
    
    [ForeignKey("DoctorId")]
    [InverseProperty("Fees")]
    public virtual Doctor? Doctor { get; set; }
    
    [ForeignKey("PatientId")]
    [InverseProperty("Fees")]
    public virtual Patient? Patient { get; set; }
    
    [ForeignKey("VisitId")]
    [InverseProperty("Fees")]
    public virtual Visit?  Visit { get; set; } = null!;
}
