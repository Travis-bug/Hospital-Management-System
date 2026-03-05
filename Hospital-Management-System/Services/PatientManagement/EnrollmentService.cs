using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.PatientManagement;


public class EnrollmentService : IEnrollmentService
{
    private readonly ClinicContext _context;

    public EnrollmentService(ClinicContext context)
    {
        _context = context;
    }


    private async Task PatientperimeterValidation(Patient patient)
    {
        // this enforces patient types to be enrolled 
        patient.Type = "Enrolled";
        
        // this enforces the patient to be created at the current time
        patient.CreatedAt = DateTime.UtcNow;
        
        // this validates and trims the extra spaces
        patient.HealthCardNo = patient.HealthCardNo.Trim();

        
        // this validates health card number is unique
        var exists = await _context.Patients.AnyAsync(p => p.HealthCardNo == patient.HealthCardNo);
        if (exists)
        {
            throw new Exception("Patient already enrolled");
        }



        // this validates if the doctor id is not null
        if (patient.DoctorId == null)
        {
            throw new Exception("DoctorId is null");
        }
        
// this validates the doctor exists during enrollement
        var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == patient.DoctorId);
        if (!doctorExists)
        {
            throw new Exception("Assigned doctor does not exist");
        }


        //This validates the family relationships for the patient 
        if (patient.PrimaryMemberId != null)
        {
            var primaryExists = await _context.Patients
                .AnyAsync(p => p.PatientId == patient.PrimaryMemberId);

            if (!primaryExists)
                throw new InvalidOperationException("Primary member does not exist.");

            if (string.IsNullOrEmpty(patient.Relationship))
                throw new InvalidOperationException("Relationship required when PrimaryMemberId is set.");
        }
    }

    public async Task<Patient> EnrollAsync(Patient patient)
        {
            await PatientperimeterValidation(patient);
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }
    }
