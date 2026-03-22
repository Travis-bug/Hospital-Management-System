using Hospital_Management_System.Models;
namespace Hospital_Management_System.Services.ClinicalRecording;

public interface  IAuditService
{
    Task LogAsync(AuditLog auditLog); 
}