using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Manager")]
public partial class Manager
{
    
    //=================================
    [Key]
    [Column("ManagerID")]
    public int ManagerId { get; set; }

    /// <summary>
    /// Gets or sets the public identifier for a Manager entity, used as a unique string-based key.
    /// This property is required and has a maximum length of 12 characters.
    /// </summary>
    [Required]
    [MaxLength(12)]
    [Column("PublicID")] 
    public string PublicId { get; set; } = Utilities.SecureIdGenerator.GenerateID(10, "MA");
    //=====================================

    [StringLength(20)]
    public string FirstName { get; set; } = null!;

    [StringLength(20)]
    public string LastName { get; set; } = null!;

    [Precision(10, 2)]
    public decimal HourlyRate { get; set; }

    [StringLength(100)]
    public string? StreetAddress { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? Province { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }
    
    public string? IdentityUserId { get; set; }
}
