using Hospital_Management_System.Models;
namespace Hospital_Management_System.Services.ClinicalRecording;

public interface IBillingService
{
    Task<Fee> CreateBillingAsync(Fee fee); 
    Task<IEnumerable<Fee>> CreateBillingRangeAsync (IEnumerable<Fee> fees);
    Task UpdateFeeAsync(Fee fee);
    Task DeleteFeeAsync(int FeeId);
    Task MarkAsPaidAsync(int FeeId);
    Task <Fee?> GetBillByIdAsync(int FeedId);
    Task <IEnumerable<Fee>> GetUnpaidBillsAsync(int FeeId);
    Task<IEnumerable<Fee>> GetBillByDateAsync(DateTime date); 
    Task <decimal> GetOutStandingBalanceAsync(int patientId);
    Task<IEnumerable<Fee>> GetBillsByPatientIdAsync(int patientId); 
    Task<IEnumerable<Fee>> GetBillByPatientNameAsync(string keyword);
   
    
}