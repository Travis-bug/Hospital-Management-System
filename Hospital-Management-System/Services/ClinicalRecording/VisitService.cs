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
        visit.PublicId = SecureIdGenerator.GenerateID(10); // Using the entropy method
        visit.Status = "Active"; // Set the status to Active for appointments
        visit.CheckinTime = DateTime.Now; // Log the start of their stay


//======Decision tree for visit arrival source=================// 
        if (visit.AppointmentId != null)
        {
            
            visit.ArrivalSource = "Appointment";

            ///////////// sub branch one ////////////////////////////////   
            if (visit.PatientClass == "OutPatient")
            {
                visit.AdmissionStatus = "Not-Admitted";
            }
            /////////////////////////////////////////////////


            /////////////// sub branch two ///////////////////////////////////
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

    
    
//====================== Get Structed Data (3 Layers)=======================================


     // 1. THE "MANY" (The Decision Tree)
    // Handles Dashboard (DoctorId only) and Profile (Both IDs)
    public async Task<IEnumerable<Visit>> GetVisitsAsync(int? doctorId = null, int? patientId = null)
    {
        var query = _context.Visits
            .AsNoTracking(); 
        
        //============================= Decision Tree for visit filtering ================================//
        if (patientId.HasValue)
        {
            query = query.Where(v => v.PatientId == patientId); 
        }

        if (doctorId.HasValue)
        {
            query = query.Where(v => v.DoctorId == doctorId);
        }
       
        return await query.ToListAsync(); 
    }
    
    
    // 2. THE "API ENTRY" (Security Layer)
    // Used by Controllers to Protect the real Database PKs.
    public async Task<Visit?> GetVisitByPublicIdAsync(string publicId)
    {
        return await _context.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.PublicId == publicId);
    }
    
    
    // 3. THE "WORKHORSE" (Internal Speed)
    // Used inside the service for Updates/Business Logic.
    public async Task <Visit?> GetVisitsById(int Id)
    {
        return await _context.Visits.FindAsync(Id);
            
    } 
    
//===============================================================================================//




    public async Task<IEnumerable<Visit>> SearchVisitsAsync(string keyword, int doctorId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return []; // this should return an empty list if the keyword is null or whitespace {TEST} or change to Enumerable.Empty<Visit>()

        keyword = keyword.ToLower();

            return await _context.Visits
                    
                .Include(v => v.Patient)
                
                .Where(v =>
                    v.DoctorId == doctorId ||
                    v.Patient.FirstName.ToLower().Contains(keyword) ||
                    v.Patient.LastName.ToLower().Contains(keyword))
                .ToListAsync();
        
    }



    public async Task<IEnumerable<Visit>> GetVisitsByDateAsync (DateTime date)
    {
        var startOfDay = date.Date; // get the start of the day
        var endOfDay = startOfDay.AddDays(1); // get the end of the day
        
        return await _context.Visits 
            .AsNoTracking()
            .Where(v => v.CheckinTime >= startOfDay && v.CheckinTime < endOfDay) 
            .ToListAsync(); 
    }
    
    
    
    public async Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment)
    {
        var visit = await GetVisitsById(visitId); // get the visit by id (this is the "WORKHORSE"````)
        
        if (visit == null)
        {
            throw new Exception("Visit not found");
        }
        visit.Symptoms = symptoms;
        visit.Diagnosis = diagnosis;
        visit.Treatment = treatment;

        await _context.SaveChangesAsync();
    }
    
    
    
    public async Task<bool> CompleteVisitAsync(int visitId)
    {
        var visit = await GetVisitsById(visitId);
        
        
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