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

/// <summary>
/// Public-safe patient chart summary used by the SPA chart shell.
/// Keeps the endpoint off the raw EF Patient entity graph.
/// </summary>
public record PatientDetailDto(
    string PatientPublicId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Address,
    DateTime CreatedAt,
    DateTime LastModified,
    string? PhoneNumber,
    string? Email,
    string? Gender,
    string HealthCardNo,
    string Type,
    string? DoctorPublicId
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

/// <summary>
/// Public-safe shape for appointment detail responses.
/// Excludes internal database keys and EF navigation graphs.
/// </summary>
public record AppointmentDetailDto(
    string   PublicId,
    string?  DoctorPublicId,
    string?  NursePublicId,
    DateTime AppointmentDate,
    DateTime? BookedAt,
    string?  Status,
    string?  Notes
);

/// <summary>
/// Minimal patient data needed in appointment schedule responses.
/// Keeps the API useful for staff without exposing the full patient entity graph.
/// shows at the small box before clicking for more details
/// </summary>
public record AppointmentPatientSummaryDto(
    string PublicId,
    string FirstName,
    string LastName,
    string HealthCardNo
);

/// <summary>
/// Public-safe shape for appointment schedule items.
/// Excludes internal database IDs and EF navigation collections.
/// </summary>
public record AppointmentScheduleItemDto(
    string PublicId,
    DateTime AppointmentDate,
    DateTime? BookedAt,
    string? Status,
    string? Notes,
    AppointmentPatientSummaryDto? Patient
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

public record ShiftRuleDto(
    string PublicId,
    string ShiftType,
    TimeOnly StartTime,
    TimeOnly EndTime
);



public class UpdateVisitEnumsDto
{
    public string? Status { get; set; }
    public string? PatientClass { get; set; }
    public string? AdmissionStatus { get; set; }
}



public record UpdateBillingDto(
    decimal Amount,
    string ServiceName,
    string PatientName

);

// ─────────────────────────────────────────────────────────────────
// AUTH DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// First-step login payload. The backend authenticates with ASP.NET Identity
/// and may either issue the app cookie or request a second factor.
/// </summary>
public record LoginRequestDto(
    string Email,
    string Password
);

/// <summary>
/// Second-step 2FA verification payload. The user is already in a pending
/// two-factor flow after a successful password check.
/// </summary>
public record TwoFactorLoginDto(
    string Email,
    string Code,
    bool RememberMachine
);

/// <summary>
/// Self-service password change payload for an already authenticated user.
/// </summary>
public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

/// <summary>
/// Public-safe auth session payload returned to the React frontend.
/// The cookie itself remains HTTP-only and is never exposed to JavaScript.
/// </summary>
public record AuthSessionDto(
    bool RequiresTwoFactor,
    string? Role,
    string? Email,
    string? PublicId,
    string? DisplayName
);

// ─────────────────────────────────────────────────────────────────
// FRONTEND DATA DTOs
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Lightweight visit payload for the patient chart contextual visits tab.
/// Keeps the frontend off the raw EF Visit graph.
/// </summary>
public record PatientVisitListItemDto(
    string PublicId,
    string? Status,
    string? PatientClass,
    string? AdmissionStatus,
    string? ArrivalSource,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    string? Symptoms,
    string? Diagnosis,
    string? Treatment,
    string? VisitNotes,
    string? DoctorPublicId,
    string? DoctorName,
    string? NursePublicId,
    string? NurseName
);

/// <summary>
/// Unified staff directory row used by the admin staff management workspace.
/// It merges the clinic-side staff profile with the linked Identity account email when present.
/// </summary>
public record StaffDirectoryItemDto(
    string PublicId,
    string FirstName,
    string LastName,
    string Role,
    string? Email,
    string Department,
    string Status
);

/// <summary>
/// Unified admin/manager provisioning payload for creating a clinic-side staff row
/// and the linked ASP.NET Identity account in one request.
/// </summary>
public record CreateStaffAccountDto(
    string FirstName,
    string LastName,
    string Email,
    string TemporaryPassword,
    string Role
);

/// <summary>
/// Confirmation payload returned after a staff account is provisioned.
/// </summary>
public record ProvisionedStaffAccountDto(
    string PublicId,
    string Role,
    string Email,
    string DisplayName
);

public record PatientVitalListItemDto(
    string VisitPublicId,
    DateTime? RecordedAt,
    decimal? Weight,
    decimal? Height,
    string? BloodPressure,
    decimal? Temperature,
    string? NursePublicId,
    string? NurseName
);

public record PrescriptionListItemDto(
    string PublicId,
    string MedicineName,
    string? Dosage,
    string? VisitPublicId,
    string? DoctorPublicId,
    string? DoctorName,
    string? RelatedTestResultPublicId
);

public record TestResultListItemDto(
    string PublicTestId,
    DateTime? ResultDate,
    string Findings,
    string TestName,
    string VisitPublicId,
    string? NursePublicId,
    string? NurseName
);

public record PendingDiagnosticTestDto(
    string TestPublicId,
    string TestName,
    string VisitPublicId,
    string PatientPublicId,
    string PatientName,
    DateTime? OrderedAt
);

public record AddTestResultDto(
    string TestPublicId,
    string Findings
);
