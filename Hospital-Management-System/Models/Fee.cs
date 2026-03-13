using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Models;

[Table("Fee")]
[Index("DoctorId", Name = "DoctorID")]
[Index("PatientId", Name = "PatientID")]
public partial class Fee
{
    [Key]
    [Column("FeeID")]
    public int FeeId { get; set; }

    
    
    
    [Column("PatientID")]
    public int? PatientId { get; set; }

    
    

    [Column("PatientName")]
    [StringLength(100)]
    public string PatientName { get; set; } = null!; 

    
    
    [Column("DoctorID")]
    public int? DoctorId { get; set; }

    
    
    
    [Column(TypeName = "datetime")]
    public DateTime? FeeDate { get; set; }

    
    
    [StringLength(100)]
    public string ServiceName { get; set; } = null!;


    public bool IsPaid { get; set; } = false; 
    
    
    public DateTime LastModified { get; set; } = DateTime.UtcNow; 
    
    
    
    [Precision(10, 2)]
    public decimal Amount { get; set; }

    
    
    [ForeignKey("DoctorId")]
    [InverseProperty("Fees")]
    public virtual Doctor? Doctor { get; set; }

    
    
    [ForeignKey("PatientId")]
    [InverseProperty("Fees")]
    public virtual Patient? Patient { get; set; }
}
