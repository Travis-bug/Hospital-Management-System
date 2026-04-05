namespace Hospital_Management_System.Models.ViewModels;

// ─────────────────────────────────────────────────────────────────
// PATIENT DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Fields a caller supplies when enrolling a new patient.
/// PatientId, PatientPublicId, CreatedAt are generated server-side.
/// </summary>
public record EnrollPatientDto(
    string  FirstName,
    string  LastName,
    DateOnly DateOfBirth,
    string  Gender,
    string  PhoneNumber,
    string  HealthCardNo,
    string  Email,
    string  Address
);

/// <summary>
/// Used to assign or reassign a patient's doctor after enrollment.
/// </summary>
public record AssignPatientDoctorDto(
    string DoctorPublicId
);


/// <summary>
/// Fields a caller is allowed to update on an existing patient.
/// Immutable fields (PublicId, HealthCardNo, Type) are excluded.
/// </summary>
public record UpdatePatientDto(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string  HealthCardNo, // I passed it here because I want it to be an updatable field in case of accidental entry
    string Address, 
    string PhoneNumber,
    string Email
);

// ─────────────────────────────────────────────────────────────────
// VISIT DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Fields required to open a new visit record.
/// Patient and appointment references are resolved server-side from
/// PublicIds so the API never exposes internal database keys.
/// </summary>
public record CreateVisitDto(
    string  PatientPublicId,
    string? AppointmentPublicId,
    string  PatientClass, 
    string? AdmissionStatus,
    string? ArrivalSource,
    string? Symptoms,
    string? Diagnosis,
    string? Treatment
);

/// <summary>
/// Clinical note fields for a PATCH update.
/// Moved to [FromBody] because clinical text easily exceeds the
/// ~2048 character URL length limit when sent as query params.
/// </summary>
public record UpdateClinicalNotesDto(
    string? Symptoms,
    string? Diagnosis,
    string? Treatment, 
    string? AdmissionStatus, 
    string VisitNotes
    
);

// ─────────────────────────────────────────────────────────────────
// APPOINTMENT DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Fields required to book an appointment.
/// Uses PublicIds so the service fetches real entities from DB —
/// prevents EF from trying to INSERT the navigation objects
/// (Doctor, Patient) that the deserializer partially reconstructed.
/// </summary>
public record BookAppointmentDto(
    string   PatientPublicId,
    string   DoctorPublicId,
    DateTime AppointmentDate,
    string?  Notes
);

// ─────────────────────────────────────────────────────────────────
// SCHEDULING DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Body payload for POST /api/scheduling/schedule-staff.
/// Was incorrectly [FromQuery] — body is the correct transport for POST data.
/// </summary>
public record ScheduleStaffDto(
    DateTime ShiftDate,
    string   ShiftRulePublicId,
    string   StaffPublicId,
    string   StaffType           // "Doctor" | "Nurse" | "Secretary" | etc.
);


public record UpdateBillingDto(
    decimal Amount,
    string ServiceName,
    string PatientName

);
