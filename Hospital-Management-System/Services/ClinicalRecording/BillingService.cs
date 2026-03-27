using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Utilities; // 
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class BillingService : IBillingService
{
    private readonly ClinicContext _context;
    private readonly IVisitService _visitService;
    private readonly IAuditService _auditService;

    public BillingService(ClinicContext context, IVisitService visitService, IAuditService auditService)
    {
        _context = context;
        _visitService = visitService;
        _auditService = auditService;
    }



    // =========================================================================================================
    public async Task<Fee> CreateBillingAsync(Fee fee, int currentUserId, string role, string actorPublicId)
    {
        if (role != "Secretary")
        {
            throw new UnauthorizedAccessException("You are not authorized to create a billing record.");
        }


        var VisitExist = await _visitService.GetVisitsByIdAsync(fee.VisitId);

        if (VisitExist == null)
        {
            throw new KeyNotFoundException($"The associated visit does not exist.");
        }


        fee.PublicId = SecureIdGenerator.GenerateID(12);
        fee.FeeDate = DateTime.Now;
        fee.IsPaid = false;


        var log = new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Billing created for patient visit {VisitExist.VisitPublicId}." // use visit exist because we know its not null from here 
        };


        _context.Fees.Add(fee);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return fee;
    }




    // =========================================================================================================
    public async Task<IEnumerable<Fee>> CreateBillingRangeAsync(IEnumerable<Fee> fees)
    {
        await _context.Fees.AddRangeAsync(fees);
        await _context.SaveChangesAsync();
        return fees;
    }

    // =========================================================================================================


    public async Task UpdateFeeAsync(Fee fee, string role, int currentUserId)
    {
        var existingFee = await _context.Fees.FirstOrDefaultAsync(f => f.FeeId == fee.FeeId); //REVIEW !!!!!!!!!!
        if (existingFee == null)
            throw new KeyNotFoundException("Fee not found.");

        switch (role)
        {
            case "Doctor":
                if (existingFee.DoctorId != currentUserId)
                    throw new UnauthorizedAccessException("Doctors can only update fees for their own patients.");
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to update fees.");
        }
        existingFee.Amount = fee.Amount;
        existingFee.ServiceName = fee.ServiceName;

        await _context.SaveChangesAsync();
    }


    // =========================================================================================================
    public async Task DeleteFeeAsync(int feeId, string role, int currentUserId)
    {
        var existingFee = await _context.Fees.FirstOrDefaultAsync(f => f.FeeId == feeId);
        
        if (existingFee == null)
            throw new KeyNotFoundException("Fee not found.");

        switch (role)
        {
            case "Doctor":
                if (existingFee.DoctorId != currentUserId)
                    throw new UnauthorizedAccessException("Doctors can only delete fees for their own patients.");
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to delete fees.");
        }
        _context.Fees.Remove(existingFee);
        await _context.SaveChangesAsync();
    }


    // =========================================================================================================

    public async Task MarkAsPaidAsync(int FeeId)
    {
        await _context.Fees
            .Where(f => f.FeeId == FeeId && f.Amount > 0)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(f => f.IsPaid, true) // mark as paid
                .SetProperty(f => f.LastModified, DateTime.UtcNow)); // update last-modified date
    }


    // ============================================================================================================
    
    // 1. THE "MANY" – get all bills, scoped by role
    public async Task<IEnumerable<Fee>> GetBillsAsync(string role, int currentUserId)
    {
        var query = _context.Fees.AsNoTracking().Include(f => f.Patient).AsQueryable();
        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view bills.");
        }
        return await query.OrderByDescending(f => f.FeeDate).ToListAsync();
    }

    
    // 2. THE "API ENTRY" (Security Layer) – get a specific bill by PublicId
    public async Task<Fee?> GetBillByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
    {
        var query = _context.Fees.AsNoTracking().Include(f => f.Patient).AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view bill details.");
        }

        var fee = await query.FirstOrDefaultAsync(f => f.PublicId == publicId);
        if (fee != null)
        {
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId,
                ActionType = "Read",
                Timestamp = DateTime.UtcNow,
                Details = $"Bill details viewed by {actorPublicId}."
            });
        }
        return fee;
    }
    
    // 3. THE "WORKHORSE" (Internal) – used inside the service for Updates/Business Logic
    public async Task<Fee?> GetBillByIdAsync(int feeId)
    {
        return await _context.Fees.FirstOrDefaultAsync(f => f.FeeId == feeId);
    }
    
    //===================================================================================

    public async Task<IEnumerable<Fee>> SearchBillsAsync(string keyword, string role, int currentUserId, string actorPublicId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return [];

        keyword = keyword.ToLower().Trim();

        var query = _context.Fees
            .AsNoTracking()
            .Include(f => f.Patient)
            .AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to search bills.");
        }

        var results = await query
            .Where(f =>
                f.PublicId.ToLower().Contains(keyword) ||
                (f.Patient != null && (
                    (f.Patient.FirstName != null && f.Patient.FirstName.ToLower().Contains(keyword)) ||
                    (f.Patient.LastName != null && f.Patient.LastName.ToLower().Contains(keyword)))))
            .OrderByDescending(f => f.FeeDate)
            .ToListAsync();

        await _auditService.LogAsync(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Search",
            Timestamp = DateTime.UtcNow,
            Details = $"Searched bills for: {keyword}"
        });

        return results;
    }

    // =========================================================================================================

    public async Task<IEnumerable<Fee>> GetUnpaidBillsAsync(int patientId, string role, int currentUserId)
    {
        var query = _context.Fees
            .AsNoTracking()
            .Include(f => f.Patient)
            .Where(f => f.Amount > 0 && f.PatientId == patientId);

        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view unpaid bills.");
        }

        return await query.OrderByDescending(f => f.FeeDate).ToListAsync();
    }




    // =========================================================================================================
    public async Task<IEnumerable<Fee>> GetBillByDateAsync(DateTime date, string role, int currentUserId)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var query = _context.Fees
            .AsNoTracking()
            .Where(f => f.FeeDate >= startOfDay && f.FeeDate < endOfDay);

        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to filter bills by date.");
        }

        return await query.OrderByDescending(f => f.FeeDate).ToListAsync();
    }

    // =========================================================================================================

    public async Task<decimal> GetOutstandingBalanceAsync(int patientId, string role, int currentUserId)
    {
        var query = _context.Fees
            .AsNoTracking()
            .Where(f => f.Amount > 0 && f.PatientId == patientId);

        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Secretary":
            case "Manager":
            case "Nurse":
                break;
            default:
                throw new UnauthorizedAccessException("Role not authorized to view outstanding balance.");
        }

        return await query.SumAsync(f => f.Amount);
    }
}

