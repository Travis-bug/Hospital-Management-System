using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Utilities; 
using Microsoft.EntityFrameworkCore;
namespace Hospital_Management_System.Services.Scheduling; 

public class AppointmentService : IAppointmentService
{
    private readonly ClinicContext _context;
    private readonly IAuditService _auditService;
    
    public AppointmentService(ClinicContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
    }
    
    public async Task<IEnumerable<AppointmentScheduleItemDto>> GetDoctorScheduleAsync(string doctorPublicId, DateTime date, string role, int currentUserId)
    {
        
        var targetDoctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.PublicId == doctorPublicId);

        if (targetDoctor == null) throw new KeyNotFoundException("Doctor not found.");
        switch (role)
        {
            case "Doctor":
                if (targetDoctor.DoctorId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You are only authorized to view your own schedule.");
                }
                break;
            case "Admin":
            case "Manager":
            case "Secretary":
                break; 
            
            default:
                throw new UnauthorizedAccessException("You are not authorized to view appointment schedules.");
        } 
        
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        // THE STATE MACHINE: We only want to see people who are scheduled or waiting.
        // Once they are "Checked In", they vanish from this list and appear on the "Visits" tab.
        var activeStatuses = new[] { "Booked", "Arrived" }; 

        
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == targetDoctor.DoctorId
                        && a.AppointmentDate >= startOfDay 
                        && a.AppointmentDate < endOfDay
                        && activeStatuses.Contains(a.Status)) // <-- THE NEW FILTER
            
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentScheduleItemDto(
                a.PublicId,
                a.AppointmentDate,
                a.BookedAt,
                a.Status,
                a.Notes,
                a.Patient == null
                    ? null
                    : new AppointmentPatientSummaryDto(
                        a.Patient.PatientPublicId,
                        a.Patient.FirstName,
                        a.Patient.LastName,
                        a.Patient.HealthCardNo)))
            .ToListAsync();
    }


    
    public async Task<AppointmentDetailDto?> GetAppointmentByPublicIdAsync(string appointmentPublicId, string role, int currentUserId, string actorPublicId)
    {
        var query = _context.Appointments.AsNoTracking().AsQueryable();
        
        switch (role)
        {
            case "Doctor":
                query = query.Where(f => f.DoctorId == currentUserId);
                break;
            case "Admin":
            case "Manager":
            case "Secretary":
                break;
            
            default:
                throw new UnauthorizedAccessException("Role not authorized to view appointment details.");
        }

        
        
        var appointment = await query
            .Where(a => a.PublicId == appointmentPublicId)
            .Select(a => new AppointmentDetailDto(
                a.PublicId,
                a.Doctor != null ? a.Doctor.PublicId : null,
                a.Nurse != null ? a.Nurse.PublicId : null,
                a.AppointmentDate,
                a.BookedAt,
                a.Status,
                a.Notes))
            .FirstOrDefaultAsync();
        
        if (appointment != null)
        {
            await _auditService.LogAsync(new AuditLog
            {
                PerformedBy = actorPublicId,
                ActionType = "Read",
                Timestamp = DateTime.UtcNow,
                Details = $"Appointment details viewed by {actorPublicId}."
            });
        }

        return appointment; // REVIEW
    }


    public async Task<Appointment> BookAppointmentAsync(BookAppointmentDto dto, string role, string actorPublicId)
    {
        if (role is not "Secretary" and not "Admin")
        {
            throw new UnauthorizedAccessException("You are not authorized to create an appointment.");
        }

        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientPublicId == dto.PatientPublicId);
        if (patient == null)
        {
            throw new KeyNotFoundException("Patient not found.");
        }

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.PublicId == dto.DoctorPublicId);
        if (doctor == null)
        {
            throw new KeyNotFoundException("Doctor not found.");
        }

        var hasConflict = await _context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctor.DoctorId
            && appointment.AppointmentDate == dto.AppointmentDate
            && appointment.Status != "Cancelled"
            && appointment.Status != "No-Show"
            && appointment.Status != "LWT");

        if (hasConflict)
        {
            throw new InvalidOperationException("The doctor is already booked for that appointment time.");
        }

        var appointment = new Appointment
        {
            PublicId = SecureIdGenerator.GenerateID(8, "APT"),
            PatientId = patient.PatientId,
            DoctorId = doctor.DoctorId,
            AppointmentDate = dto.AppointmentDate,
            Notes = dto.Notes,
            BookedAt = DateTime.UtcNow,
            Status = "Booked"
        };
        
        var log = new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Create",
            Timestamp = DateTime.UtcNow,
            Details = $"Appointment created for patient {patient.PatientPublicId}."
        };
        
        _context.Appointments.Add(appointment);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task CancelAppointmentAsync(string appointmentPublicId, string role, string actorPublicId, int currentUserId)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(f => f.PublicId == appointmentPublicId); 
        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found.");

        // Block access to non-secretary and non-doctor actors
        switch (role)
        {
            case "Doctor":
                if (appointment.DoctorId != currentUserId)
                    throw new UnauthorizedAccessException("Doctors can only cancel their own appointments.");
                break;
            
            case "Admin":
            case "Manager":
            case "Secretary": // deny access to anyone that's not the secretary
                break;
       
            default:
                throw new UnauthorizedAccessException("Role not authorized to cancel appointments.");
        }

        
        
        switch (appointment.Status)
        {
            case "LWT":
            case "No-Show":
            case "Booked":
                appointment.Status = "Cancelled";
                break;
            
            default: 
                throw new InvalidOperationException($"Cannot cancel an appointment that is already {appointment.Status}.");
        }
        
        
        await _context.SaveChangesAsync();
        
        
        var log = new AuditLog
        {
            PerformedBy = actorPublicId,
            ActionType = "Cancelled",
            EntityName = "Appointment",
            EntityPublicId = appointmentPublicId, 
            Timestamp = DateTime.UtcNow,
            Details = $"Appointment {appointmentPublicId} was cancelled."
        };
        
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        
    }
    
    
      
    }
    
