using Hospital_Management_System.Data;
using Hospital_Management_System.Models; // 
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.ClinicalRecording;

public class BillingService : IBillingService
{
    private readonly ClinicContext _context;
    
    public BillingService(ClinicContext context)
    {
        _context = context;
    }
    
    
    
    // =========================================================================================================
    public async Task<Fee> CreateBillingAsync(Fee fee)
    {
        _context.Fees.Add(fee);
        await _context.SaveChangesAsync();
        return fee;
    }
    
    
    // =========================================================================================================
    public async Task <IEnumerable<Fee>> CreateBillingRangeAsync(IEnumerable <Fee> fees)
    {
            await _context.Fees.AddRangeAsync(fees);
            await _context.SaveChangesAsync();
        return fees;
    }

    // =========================================================================================================
    
    
    public async Task UpdateFeeAsync(Fee fee)
    {
        var existingFee = await _context.Fees // get the fee from the database 
            .FirstOrDefaultAsync(f => f.FeeId == fee.FeeId); // find the fee by id

        if (existingFee == null)
            throw new Exception("Payment record not found");
            
        var isDuplicate = await _context.Fees
            .AnyAsync(f => f.FeeId == fee.FeeId && f.FeeId != fee.FeeId);
                
        if (isDuplicate)
            throw new Exception("This payment record already exists in the database.");
            
                
        existingFee.Amount= fee.Amount;
        existingFee.ServiceName = fee.ServiceName;
        existingFee.FeeId = fee.FeeId; 

        await _context.SaveChangesAsync();
    }
    
    
    // =========================================================================================================
    public async Task DeleteFeeAsync(int FeeId)
        {
            var existingFee = await _context.Fees 
                .FirstOrDefaultAsync(f => f.FeeId == FeeId);
    
            if (existingFee == null)
                throw new Exception("Patient not found");
    
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
    public async Task<Fee?> GetBillByIdAsync(int FeeId)
        {
            return await _context.Fees
                .FirstOrDefaultAsync(f => f.FeeId == FeeId); 
        }
    
    
    // =========================================================================================================
    
    public async Task<IEnumerable<Fee>> GetUnpaidBillsAsync(int patientId)
    {
        return await _context.Fees
            .AsNoTracking()
            .Include(f => f.Patient)
            .Where(f => f.Amount > 0 && f.PatientId == patientId)
            .OrderByDescending(f => f.FeeDate)
            .ToListAsync();
    }

    
    
    
    // =========================================================================================================
    public async Task<IEnumerable<Fee>> GetBillByDateAsync(DateTime date)
    {
        DateTime startOfDay = date.Date; // get the start of the day
        DateTime endOfDay = startOfDay.AddDays(1); // get the end of the day

        return await _context.Fees // get all fees from the database
            .AsNoTracking()// avoid tracking changes
            .Where(f => f.FeeDate >= startOfDay && f.FeeDate < endOfDay)// filter by date range
            .ToListAsync(); // convert to list to avoid concurrency issues with AddRangeAsync method


    }
    
    // =========================================================================================================
    
    public async Task<decimal> GetOutStandingBalanceAsync(int patientId)
    {
        return await _context.Fees
            .AsNoTracking()
            .Where(f => f.Amount > 0 && f.PatientId == patientId)
            .SumAsync(f => f.Amount);
    }
    // =========================================================================================================
    public async Task<IEnumerable<Fee>> GetBillsByPatientIdAsync(int  PatientId)
    {
        return await _context.Fees
            .AsNoTracking()
            .Where(f => f.PatientId == PatientId)
            .ToListAsync();  
    }
    
    
    // =========================================================================================================
    public async Task<IEnumerable<Fee>> GetBillByPatientNameAsync(string Keyword)
    {
        return await _context.Fees
            .AsNoTracking()
            .Where(f => f.PatientName == Keyword)
            .ToListAsync(); 
    }
    
}

