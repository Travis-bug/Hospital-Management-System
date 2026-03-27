using Hospital_Management_System.Models;

namespace Hospital_Management_System.Services.ClinicalRecording;

public interface IBillingService
{
    Task<Fee> CreateBillingAsync(Fee fee, int currentUserId, string role, string actorPublicId);
    Task<IEnumerable<Fee>> CreateBillingRangeAsync(IEnumerable<Fee> fees);

    Task UpdateFeeAsync(Fee fee, string role, int currentUserId);
    Task DeleteFeeAsync(int feeId, string role, int currentUserId);
    Task MarkAsPaidAsync(int feeId);

    Task<Fee?> GetBillByIdAsync(int feeId); // work horse 
    Task<IEnumerable<Fee>> GetBillsAsync(string role, int currentUserId); // get all bills in system 
    Task<Fee?> GetBillByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId); // get details for one bill 

    Task<IEnumerable<Fee>> SearchBillsAsync(string keyword, string role, int currentUserId, string actorPublicId);

    Task<IEnumerable<Fee>> GetUnpaidBillsAsync(int patientId, string role, int currentUserId); // get unpaid bills 
    Task<decimal> GetOutstandingBalanceAsync(int patientId, string role, int currentUserId); // get partially paid bills 
    Task<IEnumerable<Fee>> GetBillByDateAsync(DateTime date, string role, int currentUserId);
}
