using Hospital_Management_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Tests.TestDoubles;

internal static class TestClinicContextFactory
{
    public static ClinicContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicContext(options);
    }
}
