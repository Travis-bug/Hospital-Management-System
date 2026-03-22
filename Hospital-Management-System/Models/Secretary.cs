using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Secretary")]
public partial class Secretary
{
    [Key]
    [Column("SecretaryID")]
    public int SecretaryId { get; set; }

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
    
    [StringLength(30)]
    public string? IdentityUserId { get; set; }

    [InverseProperty("Secretary")]
    public virtual ICollection<SecretaryShift> SecretaryShifts { get; set; } = new List<SecretaryShift>();
}
