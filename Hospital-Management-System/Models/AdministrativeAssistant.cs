using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Administrative_Assistant")]
public partial class AdministrativeAssistant
{
    //========================================
    [Key]
    [Column("AdminID")]
    public int AdminId { get; set; }

    
    [Required]
    [MaxLength(20)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(10, "Ad");
    // ========================================
    
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
    
    
    [StringLength(450)]
    public string? IdentityUserId { get; set; }
    
    [InverseProperty("Admin")]
    public virtual ICollection<AdminAssistantShift> AdminAssistantShifts { get; set; } = new List<AdminAssistantShift>();
}
