using Hospital_Management_System.Models;
using Hospital_Management_System.Utilities; // Where your IdGenerator lives
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Data;
using Microsoft.AspNetCore.Mvc;

namespace Hospital_Management_System.Services.ClinicalRecording;

public class VisitService : IVisitService
{
    private readonly ClinicContext _context;

    public VisitService(ClinicContext context)
    {
        _context = context;
    }

    
    public async Task<Doctor?> GetAvailableTriageDoctorAsync()
    {
        return await _context.Doctors
            .Where(d => d.IsTriageQualified == true)
            .FirstOrDefaultAsync()
             ?? throw new InvalidOperationException("No triage doctor available.");
    }



    public async Task<Visit> CreateVisitAsync(Visit visit)
    {
        visit.PublicId = SecureIdGenerator.GenerateID(); // Using the entropy method
        visit.Status = "Active"; // Set the status to Active for appointments
        visit.CheckinTime = DateTime.Now; // Log the start of their stay



        if (visit.AppointmentId != null)
        {
            visit.ArrivalSource = "Appointment";

            /////////////////////////////////////////////   
            if (visit.PatientClass == "OutPatient")
            {
                visit.AdmissionStatus = "Not-Admitted";
            }
            /////////////////////////////////////////////////


            //////////////////////////////////////////////////
            else if (visit.PatientClass == "InPatient")
            {
                visit.AdmissionStatus = "Admitted";
                //  visit.AdmissionDate = DateTime.Now; // Log the start of their stay
            }


            //////////////////////////////////////////////////


        }

        
        else 
        {
            visit.ArrivalSource = "Walk-In";
            
            if (visit.PatientClass == "ER Referral")
            {
                visit.AdmissionStatus = "Priority Triage";
                visit.VisitNotes = "ER Referral detected: Patient set to Priority Triage.";
            }
            
            else // Default to Emergency / Self-Referral
            {
                visit.PatientClass = "Emergency";
                visit.AdmissionStatus = "Triage Pending";
            }


            
            try
            {
                var triageDoctor = await GetAvailableTriageDoctorAsync();
                visit.DoctorId = triageDoctor.DoctorId;
            }
            catch
            {
                throw new InvalidOperationException("No triage doctor available for walk-in.");
            }
        }
        
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }


    public async Task<IEnumerable<Visit>> GetVisitsById(int Id)
    {
        return await _context.Visits
            .AsNoTracking ()
            .Where(v => v.VisitsId == Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId)
    {
        return await _context.Visits.AsNoTracking().Where(v => v.PatientId == patientId).ToListAsync(); 
    }
    
    public async Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
        {
            throw new Exception("Visit not found");
        }
        visit.Symptoms = symptoms;
        visit.Diagnosis = diagnosis;
        visit.Treatment = treatment;
    }
    
    public async Task<bool> CompleteVisitAsync(int visitId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
        {
            throw new Exception("Visit not found");
        }
        visit.Status = "Completed";
        visit.CheckoutTime = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;  // this will Return true if the visit is successfully completed (Test)
    }
    
}