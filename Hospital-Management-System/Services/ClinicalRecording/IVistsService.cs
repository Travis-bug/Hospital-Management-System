using Hospital_Management_System.Models;

namespace Hospital_Management_System.Services.ClinicalRecording;

public interface IVisitService
{
    
    // Creates a new visit record.
    // Handles business logic such as triage assignment and PublicId generation.
    Task <Visit> CreateVisitAsync (Visit visit);
    
    
    // ================= GET (STRUCTURED DATA) =================
    
    // Retrieves a single visit by its primary key.
    Task <Visit?> GetVisitsById(int Id);

    
    // Retrieves a single visit by its PublicId.
    Task<Visit?> GetVisitByPublicIdAsync(string publicId); 
    
    
    // Retrieves visits based on context.
    // - doctorId → returns all visits related to the doctor (Dashboard view)
    // - patientId → returns all visits for a specific patient (Profile view)
    // - both → returns visits scoped to BOTH doctor and patient
    Task<IEnumerable<Visit>> GetVisitsAsync(int? doctorId = null, int? patientId = null); 
    
    
    
    
    
    
    
    // ================= SEARCH (USER INPUT) =================
    
    // Searches visits using a keyword.
    // Supports matching against:
    // - PublicId (secure external identifier)
    // - Patient name (for usability)
    // Used in search bars within UI tabs.
    Task<IEnumerable<Visit>> SearchVisitsAsync(string keyword, int doctorId );

    
    
    // ================= FILTER (REFINEMENT) =================
    
    // Filters visits by a specific date.
    // Typically used to refine results within a tab view.
    Task<IEnumerable<Visit>> GetVisitsByDateAsync(DateTime date);
    
    
    
    // ================= BUSINESS LOGIC =================
    
    // Updates clinical notes for a visit without modifying other fields.
    // Used by doctors during or after consultation.
    Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment);
    

    
    // Retrieves an available triage-qualified doctor.
    // Used during walk-in visit creation.
    Task <Doctor?> GetAvailableTriageDoctorAsync () ;
    
    
     
    // Marks a visit as completed (discharge workflow).
    // Updates status and checkout time.
    Task<bool> CompleteVisitAsync(int visitId); 
    
}