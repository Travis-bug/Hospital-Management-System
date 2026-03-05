using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.PatientManagement

{
    public class PatientService : IPatientService
    {
        private readonly ClinicContext _context;
        private readonly IEnrollmentService _enrollmentService;

        public PatientService(ClinicContext context, IEnrollmentService enrollmentService)
        
        {
            _context = context;
            _enrollmentService = enrollmentService;
           
        }
        
 // get all patients
        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .OrderBy(p => p.LastName)
                .ToListAsync();
        }
        
// get patient info by id
        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);
        }

        
        // create a single patient at a single time 
        public async Task CreateAsync(Patient patients)
        {
            await _enrollmentService.EnrollAsync(patients);
        }
        
        
         //create a range of patients at a single time (in case of bulk insert)
        public async Task CreateRangeAsync( IEnumerable <Patient> patients)
        {
            var list = patients.ToList(); // convert to list to avoid concurrency issues with AddRangeAsync method
            foreach (var p in list)
            {
                await _enrollmentService.EnrollAsync(p);
            }
        }
        
        public async Task<IEnumerable<Patient>> GetPatientsByDoctorId(int doctorId)
        {
            return await _context.Patients
                                 .Where(p => p.DoctorId == doctorId)
                                 .ToListAsync();
        }
// update patient info
        public async Task UpdateAsync(Patient patient)
        {
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

            if (existingPatient == null)
                throw new Exception("Patient not found");
            
            var isDuplicate = await _context.Patients
                .AnyAsync(p => p.HealthCardNo == patient.HealthCardNo && p.PatientId != patient.PatientId);
                
            if (isDuplicate)
                throw new Exception("This Health card number already assigned to another patient.");
            
            
            existingPatient.LastModified = DateTime.UtcNow;
            existingPatient.FirstName = patient.FirstName;
            existingPatient.LastName = patient.LastName;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.PhoneNumber = patient.PhoneNumber;
            existingPatient.Address = patient.Address;
            existingPatient.Email = patient.Email;

            await _context.SaveChangesAsync();
        }

        
        // delete patient info 
        public async Task DeleteAsync(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                throw new Exception("Patient not found");

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }

        
        // search patient info by keyword, name, or email
        public async Task<IEnumerable<Patient>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllAsync();

            keyword = keyword.ToLower();

            return await _context.Patients
                .Where(p =>
                    p.FirstName.ToLower().Contains(keyword) ||
                    p.LastName.ToLower().Contains(keyword) ||
                    p.Email!.ToLower().Contains(keyword))
                    .ToListAsync();
        }
    }
}