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
        
    //======================================== THREE GATE RULE =========================================================    
        
 // get all patients
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(string role, int currentUserId)
        {
            var query = _context.Patients.AsNoTracking().AsQueryable(); 
               
               switch (role) 
               {
                   case "Doctor":
                       
                         query = query.Where(f => f.DoctorId == currentUserId);
                         break;
                   case "Admin":
                   case "Secretary":
                   case "Manager":
                   case "Nurse":
                        break;
                   default:
                   throw new UnauthorizedAccessException("Role not authorized to view Patient list.");
               }
               
              return await query.OrderBy(p => p.LastName)
                .ToListAsync();
        }
        
        
        
        public async Task<Patient?> GetByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
        {
            var query = _context.Patients.AsNoTracking().AsQueryable();

            switch (role)
            {
                case "Doctor":
                    query = query.Where(p => p.DoctorId == currentUserId);
                    break;
                case "Admin":
                case "Secretary":
                case "Manager":
                case "Nurse":
                    break;
                default:
                    throw new UnauthorizedAccessException("Role not authorized to view Patient details.");
            }
            
            var patient = await query.FirstOrDefaultAsync(p => p.PatientPublicId == publicId);
            if (patient != null)
            {
                await _auditService.LogAsync(new AuditLog
                {
                    PerformedBy = actorPublicId,
                    ActionType = "Read",
                    Timestamp = DateTime.UtcNow,
                    Details = $"Patient {publicId} details were viewed by {actorPublicId}."
                });
            }
            return patient;
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
                    throw new UnauthorizedAccessException("Role not authorized to search patients.");
            }

            return await query
                .Where(p =>
                    (p.FirstName != null && p.FirstName.ToLower().Contains(keyword)) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(keyword)) ||
                    (p.Email != null && p.Email.ToLower().Contains(keyword)))
                    .ToListAsync();
        }
    }
}
