using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.PatientManagement

{
    public class PatientService : IPatientService
    {
        private readonly ClinicContext _context;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IAuditService _auditService;

        public PatientService(ClinicContext context, IEnrollmentService enrollmentService, IAuditService auditService)
        
        {
            _context = context;
            _enrollmentService = enrollmentService;
            _auditService = auditService;
           
        }

        private IQueryable<Patient> BuildScopedPatientQuery(string role, int currentUserId)
        {
            var query = _context.Patients.AsQueryable();

            switch (role)
            {
                case "Doctor":
                    query = query.Where(patient => patient.DoctorId == currentUserId);
                    break;
                case "Admin":
                case "Secretary":
                case "Manager":
                case "Nurse":
                    break;
                default:
                    throw new UnauthorizedAccessException("Role not authorized to access patient records.");
            }

            return query;
        }

        private Task<Patient?> ResolveScopedPatientAsync(string publicId, string role, int currentUserId)
        {
            return BuildScopedPatientQuery(role, currentUserId)
                .FirstOrDefaultAsync(patient => patient.PatientPublicId == publicId);
        }
        
    //======================================== THREE GATE RULE =========================================================    
        
 // get all patients
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(string role, int currentUserId)
        {
            var query = BuildScopedPatientQuery(role, currentUserId)
                .AsNoTracking();
               
              return await query.OrderBy(p => p.LastName)
                .ToListAsync();
        }
        
        
        
        public async Task<Patient?> GetByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
        {
            var patient = await ResolveScopedPatientAsync(publicId, role, currentUserId);
            if (patient != null)
            {
                await _auditService.LogAsync(new AuditLog
                {
                    PerformedBy = actorPublicId,
                    EntityName = "Patient",
                    EntityPublicId = publicId,
                    ActionType = "Read",
                    Timestamp = DateTime.UtcNow,
                    // Keep this short because some local MySQL schemas may still be on
                    // the legacy varchar(50) AuditLog.Details definition.
                    Details = "Viewed patient record."
                });
            }
            return patient;
        }

        public async Task<PatientDetailDto?> GetDetailByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
        {
            var patient = await GetByPublicIdAsync(publicId, role, currentUserId, actorPublicId);
            if (patient == null)
            {
                return null;
            }

            var doctorPublicId = patient.DoctorId == null
                ? null
                : await _context.Doctors
                    .AsNoTracking()
                    .Where(doctor => doctor.DoctorId == patient.DoctorId)
                    .Select(doctor => doctor.PublicId)
                    .FirstOrDefaultAsync();

            return new PatientDetailDto(
                patient.PatientPublicId,
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.Address,
                patient.CreatedAt,
                patient.LastModified,
                patient.PhoneNumber,
                patient.Email,
                patient.Gender,
                patient.HealthCardNo,
                patient.Type,
                doctorPublicId);
        }
        
        
// get patient info by id
        public async Task<Patient?> GetByIdAsync(int patientid)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientid);
        }
        
        
//================================================COMPLETE =============================================================
        
        
        // TODO: this method might need to be removed since it is virtually redundant 
        // create a single patient at a single time 
        public async Task CreateAsync(EnrollPatientDto dto)
        {
            await _enrollmentService.EnrollAsync(dto);
        }

        public async Task AssignDoctorAsync(string patientpublicId, AssignPatientDoctorDto dto, string role, string actorPublicId)
        {
            if (role is not "Manager" and not "Admin" and not "Secretary")
            {
                throw new UnauthorizedAccessException("Role not authorized to assign doctors.");
            }

            if (string.IsNullOrWhiteSpace(dto.DoctorPublicId))
            {
                throw new ArgumentException("Doctor public ID is required.");
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientPublicId == patientpublicId);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient not found.");
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.PublicId == dto.DoctorPublicId);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            patient.DoctorId = doctor.DoctorId;
            patient.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId,
                EntityName = "Patient",
                EntityPublicId = patient.PatientPublicId,
                ActionType = "Update",
                Timestamp = DateTime.UtcNow,
                Details = $"Assigned doctor {doctor.PublicId} to patient {patient.PatientPublicId}."
            });
        }
        
        
        
        public async Task<IEnumerable<Patient>> GetPatientsByDoctorId(int doctorId)
        {
            return await _context.Patients
                                 .Where(p => p.DoctorId == doctorId)
                                 .ToListAsync();
        }
        
        
// update patient info
        public async Task UpdateAsync(string patientpublicId, UpdatePatientDto dto,  string role, int currentUserId, string actorPublicId)
        {
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientPublicId == patientpublicId);  // REVIEW (this is the only way to get the patient id from the public id)
            

            if (existingPatient == null)
                throw new KeyNotFoundException("Patient not found");

            switch (role)
            {
                case "Doctor":
                    if (existingPatient.DoctorId != currentUserId)
                    {
                        throw new UnauthorizedAccessException("You are only authorized to update your own patients.");
                    }
                    break;
                
                case "Secretary":
                    break; 
                
                default: 
                    throw new UnauthorizedAccessException("Role not authorized to update patients.");
            }
            
            var isDuplicate = await _context.Patients  // NOTE:this uses Implicit Typing as the variable "isDuplicate" can be written as a bool 
                .AnyAsync(p => p.HealthCardNo == dto.HealthCardNo && p.PatientId != existingPatient.PatientId); // REVIEW
                
            
            if (isDuplicate)
                throw new Exception("This Health card number already assigned to another patient.");
            
            
            existingPatient.LastModified = DateTime.UtcNow;
            existingPatient.FirstName = dto.FirstName;
            existingPatient.LastName = dto.LastName;
            existingPatient.DateOfBirth = dto.DateOfBirth;
            existingPatient.PhoneNumber = dto.PhoneNumber;
            existingPatient.Address = dto.Address;
            existingPatient.Email = dto.Email;

            await _context.SaveChangesAsync(); // might need to change this since logger already does an internal save

            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId, 
                EntityName = "Patient",
                EntityPublicId = existingPatient.PatientPublicId,
                ActionType = "Update",
                Timestamp = DateTime.UtcNow,
                Details = $"Patient {existingPatient.PatientPublicId} was updated."
            });

        }
        
        
        // delete patient info 
        public async Task DeleteAsync(string patientpublicId, string role, string actorPublicId, int currentUserId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientPublicId == patientpublicId);

            if (patient == null)
                throw new KeyNotFoundException ("Patient not found");

            switch (role)
            {
                case "Doctor":
                    if (patient.DoctorId != currentUserId)
                    {
                        throw new UnauthorizedAccessException("You are only authorized to delete your own patients.");
                    }
                    break;
                case "Secretary":
                case "Manager":
                case "Nurse":
                    break;
                default:
                    throw new UnauthorizedAccessException("Role not authorized to delete patients.");
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId,
                EntityName = "Patient",
                EntityPublicId = patient.PatientPublicId,
                ActionType = "Delete",
                Timestamp = DateTime.UtcNow,
                Details = $"Patient {patient.PatientPublicId} was deleted."
            }); 
           
        }

        
        // search patient info by keyword, name, or email
        public async Task<IEnumerable<Patient>> SearchAsync(string keyword, string role, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return []; // this should return an empty list if the keyword is null or whitespace {TEST} 

            keyword = keyword.ToLower();

            var query = BuildScopedPatientQuery(role, currentUserId);

            return await query
                .Where(p =>
                    (p.FirstName != null && p.FirstName.ToLower().Contains(keyword)) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(keyword)) ||
                    (p.Email != null && p.Email.ToLower().Contains(keyword)))
                    .ToListAsync();
        }

        public async Task<IEnumerable<PatientVitalListItemDto>> GetVitalsByPatientPublicIdAsync(string patientPublicId, string role, int currentUserId)
        {
            var patient = await ResolveScopedPatientAsync(patientPublicId, role, currentUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            return await _context.PatientVitals
                .AsNoTracking()
                .Include(vital => vital.Nurse)
                .Include(vital => vital.Visits)
                .Where(vital => vital.Visits.PatientId == patient.PatientId)
                .OrderByDescending(vital => vital.RecordedAt)
                .Select(vital => new PatientVitalListItemDto(
                    vital.Visits.VisitPublicId,
                    vital.RecordedAt,
                    vital.Weight,
                    vital.Height,
                    vital.BloodPressure,
                    vital.Temperature,
                    vital.Nurse.PublicId,
                    $"{vital.Nurse.FirstName} {vital.Nurse.LastName}"))
                .ToListAsync();
        }

        public async Task<IEnumerable<PrescriptionListItemDto>> GetPrescriptionsByPatientPublicIdAsync(string patientPublicId, string role, int currentUserId)
        {
            var patient = await ResolveScopedPatientAsync(patientPublicId, role, currentUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            return await _context.Prescriptions
                .AsNoTracking()
                .Include(prescription => prescription.Doctor)
                .Include(prescription => prescription.Result)
                .Include(prescription => prescription.Visits)
                .Where(prescription => prescription.Visits != null && prescription.Visits.PatientId == patient.PatientId)
                .OrderByDescending(prescription => prescription.PrescriptionId)
                .Select(prescription => new PrescriptionListItemDto(
                    prescription.PublicId,
                    prescription.MedicineName,
                    prescription.Dosage,
                    prescription.Visits != null ? prescription.Visits.VisitPublicId : null,
                    prescription.Doctor != null ? prescription.Doctor.PublicId : null,
                    prescription.Doctor != null ? $"{prescription.Doctor.FirstName} {prescription.Doctor.LastName}" : null,
                    prescription.Result != null ? prescription.Result.PublicTestId : null))
                .ToListAsync();
        }

        public async Task<IEnumerable<TestResultListItemDto>> GetTestResultsByPatientPublicIdAsync(string patientPublicId, string role, int currentUserId)
        {
            var patient = await ResolveScopedPatientAsync(patientPublicId, role, currentUserId)
                ?? throw new KeyNotFoundException("Patient not found.");

            return await _context.TestResults
                .AsNoTracking()
                .Include(result => result.Nurse)
                .Include(result => result.Test)
                .Include(result => result.Visit)
                .Where(result => result.Visit.PatientId == patient.PatientId)
                .OrderByDescending(result => result.ResultDate)
                .Select(result => new TestResultListItemDto(
                    result.PublicTestId,
                    result.ResultDate,
                    result.Findings,
                    result.Test.TestName,
                    result.Visit.VisitPublicId,
                    result.Nurse.PublicId,
                    $"{result.Nurse.FirstName} {result.Nurse.LastName}"))
                .ToListAsync();
        }
    }
}
