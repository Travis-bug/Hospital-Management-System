using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Services.ClinicalRecording;

public class VisitService : IVisitService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;

    public VisitService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Doctor?> GetAvailableTriageDoctorAsync()
    {
        return await _context.Doctors
                   .Where(d => d.IsTriageQualified)
                   .FirstOrDefaultAsync()
               ?? throw new InvalidOperationException("No triage doctor available.");
    }

    public async Task<Visit> CreateVisitAsync(CreateVisitDto dto, string actorPublicId, string role, int currentUserId)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(existingPatient => existingPatient.PatientPublicId == dto.PatientPublicId)
            ?? throw new KeyNotFoundException("Patient not found.");

        Appointment? appointment = null;
        if (!string.IsNullOrWhiteSpace(dto.AppointmentPublicId))
        {
            appointment = await _context.Appointments
                .FirstOrDefaultAsync(existingAppointment => existingAppointment.PublicId == dto.AppointmentPublicId)
                ?? throw new KeyNotFoundException("Appointment not found.");

            if (appointment.PatientId != patient.PatientId)
            {
                throw new InvalidOperationException("The appointment does not belong to the supplied patient.");
            }
        }

        var visit = new Visit
        {
            VisitPublicId = SecureIdGenerator.GenerateID(8, "VIS"),
            PatientId = patient.PatientId,
            AppointmentId = appointment?.AppointmentId,
            PatientClass = dto.PatientClass,
            AdmissionStatus = dto.AdmissionStatus,
            ArrivalSource = appointment == null ? "Walk-in" : "Appointment",
            Symptoms = dto.Symptoms,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Status = "Active",
            CheckinTime = DateTime.UtcNow,
            VisitNotes = string.Empty
        };

        if (appointment != null)
        {
            visit.ArrivalSource = "Appointment";
            visit.DoctorId = appointment.DoctorId;

            if (visit.PatientClass == "Outpatient")
            {
                visit.AdmissionStatus = "Not Admitted";
            }
            else if (visit.PatientClass == "Inpatient")
            {
                visit.AdmissionStatus = "Admitted";
            }
            else if (string.IsNullOrWhiteSpace(visit.AdmissionStatus))
            {
                visit.AdmissionStatus = "Not Admitted";
            }

            appointment.Status = "Checked In";
        }
        else
        {
            visit.ArrivalSource = "Walk-in";

            if (visit.PatientClass == "ER Referral")
            {
                visit.AdmissionStatus = "Priority Triage";
                visit.VisitNotes = "ER Referral detected: Patient set to Priority Triage.";
            }
            else
            {
                visit.PatientClass = string.IsNullOrWhiteSpace(visit.PatientClass) ? "Emergency" : visit.PatientClass;
                visit.AdmissionStatus = "Triage Pending";
            }

            var triageDoctor = await GetAvailableTriageDoctorAsync()
                               ?? throw new InvalidOperationException("No triage doctor available.");
            visit.DoctorId = triageDoctor.DoctorId;
        }

        if (role == "Doctor" && visit.DoctorId != currentUserId)
        {
            throw new UnauthorizedAccessException("Doctors can only create visits assigned to themselves.");
        }

        _context.Visits.Add(visit);

        _context.AuditLogs.Add(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Visit created: {visit.VisitPublicId}."
        });

        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task<IEnumerable<Visit>> GetVisitsAsync(string role, int currentUserId)
    {
        var query = _context.Visits
            .AsNoTracking()
            .AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to view visits.");
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<PatientVisitListItemDto>> GetVisitsByPatientPublicIdAsync(string patientPublicId, string role, int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(patientPublicId))
        {
            throw new ArgumentException("Patient public ID is required.");
        }

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(existingPatient => existingPatient.PatientPublicId == patientPublicId)
            ?? throw new KeyNotFoundException("Patient not found.");

        var query = _context.Visits
            .AsNoTracking()
            .Include(visit => visit.Doctor)
            .Include(visit => visit.Nurse)
            .Where(visit => visit.PatientId == patient.PatientId);

        switch (role)
        {
            case "Doctor":
                query = query.Where(visit => visit.DoctorId == currentUserId);
                break;

            case "Nurse":
            case "Admin":
            case "Secretary":
            case "Manager":
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to view patient visits.");
        }

        return await query
            .OrderByDescending(visit => visit.CheckinTime)
            .Select(visit => new PatientVisitListItemDto(
                visit.VisitPublicId,
                visit.Status,
                visit.PatientClass,
                visit.AdmissionStatus,
                visit.ArrivalSource,
                visit.CheckinTime,
                visit.CheckoutTime,
                visit.Symptoms,
                visit.Diagnosis,
                visit.Treatment,
                visit.VisitNotes,
                visit.Doctor != null ? visit.Doctor.PublicId : null,
                visit.Doctor != null ? $"{visit.Doctor.FirstName} {visit.Doctor.LastName}" : null,
                visit.Nurse != null ? visit.Nurse.PublicId : null,
                visit.Nurse != null ? $"{visit.Nurse.FirstName} {visit.Nurse.LastName}" : null
            ))
            .ToListAsync();
    }

    public async Task<Visit?> GetVisitByPublicIdAsync(string publicId, string role, int currentUserId, string actorPublicId)
    {
        var query = _context.Visits
            .AsNoTracking()
            .AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to view clinical notes.");
        }

        var visit = await query.FirstOrDefaultAsync(v => v.VisitPublicId == publicId);

        if (visit != null)
        {
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId,
                ActionType = "Read",
                Timestamp = DateTime.UtcNow,
                Details = $"Visit details viewed by {actorPublicId}."
            });
        }

        return visit;
    }

    public async Task<Visit?> GetVisitsByIdAsync(int id)
    {
        return await _context.Visits.FindAsync(id);
    }

    public async Task<IEnumerable<Visit>> SearchVisitsAsync(string keyword, string role, string actorPublicId, int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        keyword = keyword.ToLower();

        var query = _context.Visits
            .AsNoTracking()
            .Include(v => v.Patient)
            .AsQueryable();

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("You are not authorized to perform this action");
        }

        var results = await query
            .Where(v => v.Patient != null &&
                        ((v.Patient.FirstName != null && v.Patient.FirstName.ToLower().Contains(keyword)) ||
                         (v.Patient.LastName != null && v.Patient.LastName.ToLower().Contains(keyword))))
            .ToListAsync();

        await _auditService.LogAsync(new AuditLog
        {
            PerformedBy = actorPublicId,
            EntityName = "Visit",
            ActionType = "Search",
            Timestamp = DateTime.UtcNow,
            Details = $"Visit search executed. Matches: {results.Count}."
        });

        return results;
    }

    public async Task<IEnumerable<Visit>> GetVisitsByDateAsync(DateTime date, string role, int currentUserId)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var query = _context.Visits
            .AsNoTracking()
            .Where(v => v.CheckinTime >= startOfDay && v.CheckinTime < endOfDay);

        switch (role)
        {
            case "Doctor":
                query = query.Where(v => v.DoctorId == currentUserId);
                break;

            case "Nurse":
                query = query.Where(v => v.AdmissionStatus != "Discharged");
                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to filter visits by date.");
        }

        return await query.ToListAsync();
    }

    public async Task UpdateClinicalNotesAsync(string visitPublicId, UpdateClinicalNotesDto dto, int currentUserId, string role, string actorPublicId)
    {
        var visit = await _context.Visits
            .FirstOrDefaultAsync(v => v.VisitPublicId == visitPublicId);

        if (visit == null)
        {
            throw new KeyNotFoundException("Visit not found");
        }

        switch (role)
        {
            case "Doctor":
                if (visit.DoctorId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Doctors can only update their own visits.");
                }

                break;

            case "Nurse":
                if (visit.AdmissionStatus == "Discharged")
                {
                    throw new InvalidOperationException("Visit is already discharged.");
                }

                break;

            default:
                throw new UnauthorizedAccessException("Role not authorized to update clinical notes.");
        }

        visit.Symptoms = dto.Symptoms;
        visit.Diagnosis = dto.Diagnosis;
        visit.Treatment = dto.Treatment;
        visit.AdmissionStatus = dto.AdmissionStatus;
        visit.VisitNotes = dto.VisitNotes;

        _context.AuditLogs.Add(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Update",
            Timestamp = DateTime.UtcNow,
            Details = $"Clinical notes updated for visit {visit.VisitPublicId}."
        });

        await _context.SaveChangesAsync();
    }

    public async Task<bool> CompleteVisitAsync(string visitPublicId, string role, int currentUserId, string actorPublicId)
    {
        var visit = await _context.Visits
            .FirstOrDefaultAsync(existingVisit => existingVisit.VisitPublicId == visitPublicId);
        if (visit == null)
        {
            throw new KeyNotFoundException("Visit not found");
        }

        switch (role)
        {
            case "Doctor":
                if (visit.DoctorId != currentUserId)
                {
                    throw new UnauthorizedAccessException("Doctors can only complete their own visits.");
                }
                break;

            case "Nurse":
                break; 
            
            default:
                throw new UnauthorizedAccessException("Role not authorized to complete visits.");
        }

        if (visit.Status == "Completed")
        {
            throw new InvalidOperationException("Visit is already marked as completed.");
        }
        
        switch (visit.AdmissionStatus)
        {
            case "Admitted": 
                throw new InvalidOperationException("Cannot complete visit. Patient is currently admitted to a bed. Please discharge them first.");
            
            case "Triage Pending": 
                throw new InvalidOperationException("Cannot complete visit. Patient has not been triaged yet.");
        }

        visit.Status = "Completed";
        visit.AdmissionStatus = "Discharged";
        visit.CheckoutTime = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Complete",
            Timestamp = DateTime.UtcNow,
            Details = $"Visit {visit.VisitPublicId} completed and discharged."
        });

        await _context.SaveChangesAsync();
        return true;
    }
    
    
    // Experimental (might delete) 
    public async Task UpdateVisitClassificationsAsync(string visitPublicId, UpdateVisitEnumsDto dto, string role, int currentUserId, string actorPublicId) 
{
    var visit = await _context.Visits
        .FirstOrDefaultAsync(v => v.VisitPublicId == visitPublicId);

    if (visit == null)
        throw new KeyNotFoundException("Visit not found.");

    // ==========================================
    //  SECURITY CHECKPOINT
    // ==========================================
    switch (role)
    {
        case "Doctor":
            if (visit.DoctorId != currentUserId)
                throw new UnauthorizedAccessException("Doctors can only update classifications for their own visits.");
            break;
        case "Nurse":
            // Nurses control patient flow and bed management
            break;
        default:
            throw new UnauthorizedAccessException("Your role is not authorized to modify visit classifications.");
    }

    // ==========================================
    // 🚦 ENUM HARD-VALIDATION (Protects MySQL)
    // ==========================================
    var validStatuses = new[] { "Active", "Completed" };
    var validPatientClasses = new[] { "Inpatient", "Outpatient", "Emergency", "ER Referral" };
    var validAdmissionStatuses = new[] { "Admitted", "Not Admitted", "Discharged", "Triage Pending" };

    // ==========================================
    //  SAFE UPDATE LOGIC
    // ==========================================
    if (!string.IsNullOrWhiteSpace(dto.Status))
    {
        if (!validStatuses.Contains(dto.Status))
            throw new ArgumentException($"Invalid Status. Allowed: {string.Join(", ", validStatuses)}");
            
        visit.Status = dto.Status;
        
        // Auto-stamp the checkout time if they use the dropdown to complete the visit
        if (dto.Status == "Completed")
            visit.CheckoutTime = DateTime.UtcNow;
    }

    if (!string.IsNullOrWhiteSpace(dto.PatientClass))
    {
        if (!validPatientClasses.Contains(dto.PatientClass))
            throw new ArgumentException($"Invalid Patient Class. Allowed: {string.Join(", ", validPatientClasses)}");
            
        visit.PatientClass = dto.PatientClass;
    }

    if (!string.IsNullOrWhiteSpace(dto.AdmissionStatus))
    {
        if (!validAdmissionStatuses.Contains(dto.AdmissionStatus))
            throw new ArgumentException($"Invalid Admission Status. Allowed: {string.Join(", ", validAdmissionStatuses)}");
            
        visit.AdmissionStatus = dto.AdmissionStatus;
    }

    await _context.SaveChangesAsync();

    // ==========================================
    //  AUDIT LOG
    // ==========================================
    await _auditService.LogAsync(new AuditLog
    {
        PerformedBy = actorPublicId,
        ActionType = "Update",
        EntityName = "Visit",
        EntityPublicId = visit.VisitPublicId,
        Timestamp = DateTime.UtcNow,
        Details = $"Visit classifications updated via panel. Status: {visit.Status}, Class: {visit.PatientClass}, Admission: {visit.AdmissionStatus}."
    });
}
    
    
}
