using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Models;

[Table("Referral")]
[Index("ReferringDoctorId", Name = "ReferringDoctorID")]
[Index("SpecialistDoctorId", Name = "SpecialistDoctorID")]
[Index("VisitId", Name = "VisitID")]
public partial class Referral
{
    [Key]
    [Column("ReferralID")]
    public int ReferralId { get; set; }

    [Column("VisitID")]
    public int VisitId { get; set; }

    [Column("ReferringDoctorID")]
    public int ReferringDoctorId { get; set; }

    [Column("SpecialistDoctorID")]
    public int SpecialistDoctorId { get; set; }

    [Column(TypeName = "text")]
    public string? Notes { get; set; }

    [ForeignKey("ReferringDoctorId")]
    [InverseProperty("ReferralReferringDoctors")]
    public virtual Doctor ReferringDoctor { get; set; } = null!;

    [ForeignKey("SpecialistDoctorId")]
    [InverseProperty("ReferralSpecialistDoctors")]
    public virtual Doctor SpecialistDoctor { get; set; } = null!;

    [ForeignKey("VisitId")]
    [InverseProperty("Referrals")]
    public virtual Visit Visit { get; set; } = null!;
}
