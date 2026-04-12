using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Hospital_Management_System.Models;


[Table("AuditLog")]
public partial class AuditLog
{
    [Key]
    [Column("AuditLogID")]
    public int AuditLogId { get; init; }
    
    
    
    // WHO (The Doctor or Nurse)
    [Column("PerformedBy")]
    [StringLength(30)]
    [Required]
    public string PerformedBy { get; init; } = null!; // this would be using the public entropy-based generated id of the person who performed the action


    [Column("EntityPublicId")]
    [StringLength(50)]
    public string EntityPublicId { get; init; } = string.Empty; // this would be using the public entropy-based generated id of the person who performed the action e.g., "VIS_cwy6"
    
    
    
    
    
    
    // WHAT (The Action)
    [StringLength(30)]
    [Column(TypeName = "enum('Create', 'Update', 'Cancelled' ,'Read' , 'Complete' , 'Search' )")]
    public string ActionType { get; init; } = null!;  // e.g., AuditAction.Update
    
    
    [Column("Details")]
    [StringLength(50)]
    public string? Details { get; init; }

    // WHERE (The Table and the Specific Record)
    [Column("EntityName")]
    [StringLength(30)]
    public string EntityName { get; init; } = string.Empty; // e.g., nameof(Visit)

    
    [Column("TimeStamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
}