using Hospital_Management_System.Models;
using Hospital_Management_System.Services.ClinicalRecording;

namespace Hospital_Management_System.Tests.TestDoubles;

internal sealed class TestAuditService : IAuditService
{
    public List<AuditLog> Logs { get; } = [];

    public Task LogAsync(AuditLog auditLog)
    {
        Logs.Add(auditLog);
        return Task.CompletedTask;
    }
}
