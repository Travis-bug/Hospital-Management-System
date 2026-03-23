using Hospital_Management_System.Models;

namespace Hospital_Management_System.Services.ClinicalRecording;

public interface IVisitService
{

    // Creates a new visit record.
    // Handles business logic such as triage assignment and PublicId generation.
    Task<Visit> CreateVisitAsync(Visit visit, string actorPublicId, string role, int currentUserId, int assignedDoctorId);


    // ================= GET (STRUCTURED DATA) =================

    // Retrieves a single visit by its primary key.
    Task<Visit?> GetVisitsById(int Id);


    // Retrieves a single visit by its PublicId.
    Task<Visit?> GetVisitByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId);


    // Retrieves visits based on context.
    // - doctorId → returns all visits related to the doctor (Dashboard view)
    // - patientId → returns all visits for a specific patient (Profile view)
    // - both → returns visits scoped to BOTH doctor and patient
    Task<IEnumerable<Visit>> GetVisitsAsync(string role, int currentUserId);







    // ================= SEARCH (USER INPUT) =================

    // Searches visits using a keyword.
    // Supports matching against:
    // - PublicId (secure external identifier)
    // - Patient name (for usability)
    // Used in search bars within UI tabs.
    Task<IEnumerable<Visit>> SearchVisitsAsync(string keyword, string role, string actorPublicId, int currentUserId);



    // ================= FILTER (REFINEMENT) =================

    // Filters visits by a specific date.
    // Typically used to refine results within a tab view.
    // Results are scoped by role: Doctor sees own visits, Nurse sees non-discharged only.
    Task<IEnumerable<Visit>> GetVisitsByDateAsync(DateTime date, string role, int currentUserId);



    // ================= BUSINESS LOGIC =================

    // Updates clinical notes for a visit without modifying other fields.
    // Used by doctors during or after consultation.
    Task UpdateClinicalNotesAsync(int visitId, string symptoms, string diagnosis, string treatment, int currentUserId, string actorPublicId);



    // Retrieves an available triage-qualified doctor.
    // Used during walk-in visit creation.
    Task<Doctor?> GetAvailableTriageDoctorAsync();



    // Marks a visit as completed (discharge workflow).
    // Updates status and checkout time.
    // Authorization: Doctor can complete own visits only, Nurse can complete any non-discharged visit.
    Task<bool> CompleteVisitAsync(int visitId, string role, int currentUserId, string actorPublicId);

}