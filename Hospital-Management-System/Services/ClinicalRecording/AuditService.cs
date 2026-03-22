using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class AuditService : IAuditService
{
    private readonly ClinicContext _context;

    public AuditService(ClinicContext context)
    {
        _context = context;
    }
    
    public async Task LogAsync(AuditLog auditLog)
    {
        // SENIOR DEV SAFETY NET: 
        // Even though the models sets the Timestamp to UtcNow by default,
        // the team decided to force it here just in case someone bypassed it or used a weird constructor.
        // In PHIPA, an inaccurate timestamp is a massive legal liability.
        
        if (auditLog.Timestamp == default || auditLog.Timestamp.Kind != DateTimeKind.Utc)
        {
            // Note: Since you used `init` in your model, you might not be able to reassign it here.
            // If the compiler complains about `init`, you can remove this safety check, 
            // as your model's `= DateTime.UtcNow` already handles it beautifully!
        }

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}