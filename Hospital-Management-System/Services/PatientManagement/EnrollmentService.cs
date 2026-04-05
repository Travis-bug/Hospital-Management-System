using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.PatientManagement;


public class EnrollmentService : IEnrollmentService
{
    private readonly ClinicContext _context;

    public EnrollmentService(ClinicContext context)
    {
        _context = context;
    }


    private async Task ValidateEnrollmentAsync(Patient patient)
    {
        patient.Type = "Enrolled";
        patient.CreatedAt = DateTime.UtcNow;
        patient.LastModified = DateTime.UtcNow;
        patient.HealthCardNo = patient.HealthCardNo.Trim();

        if (string.IsNullOrWhiteSpace(patient.HealthCardNo))
        {
            throw new ArgumentException("Health card number is required.");
        }

        var exists = await _context.Patients.AnyAsync(p => p.HealthCardNo == patient.HealthCardNo);
        if (exists)
        {
            throw new InvalidOperationException("Patient already enrolled.");
        }


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
    
   


    public async Task<Patient> EnrollAsync(EnrollPatientDto dto)
    
        {
                var patient = new Patient
                {
                    PatientPublicId = Utilities.SecureIdGenerator.GenerateID(10, "PA"),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    PhoneNumber = dto.PhoneNumber,
                    HealthCardNo = dto.HealthCardNo,
                    Email = dto.Email,
                    Type = "Enrolled",
                    Address = dto.Address,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

            await ValidateEnrollmentAsync(patient);
            await  _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }
    }
