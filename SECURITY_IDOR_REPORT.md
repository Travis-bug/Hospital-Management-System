# IDOR Security Report

Date: 2026-04-04

## Scope

This review focused on the current web app under `Hospital-Management-System/`, with emphasis on:

- API controllers in `Hospital-Management-System/Controllers/Api`
- authorization and claim resolution in `Hospital-Management-System/Program.cs` and `Hospital-Management-System/Services/StaffManagement`
- object lookup and ownership checks in the service layer
- places where client input could reference internal database keys directly

## Review Standard

For this report, a path was treated as an IDOR or IDOR-adjacent vulnerability when one of these patterns existed:

- the API exposed internal database IDs instead of public identifiers
- the client could supply foreign keys or entity IDs directly for objects it should not control
- authorization decisions were made using the wrong identifier source
- a lookup by external identifier lacked role-based ownership scoping

## Executive Summary

After the latest remediation pass, I did not find a remaining IDOR vulnerability in the currently exposed API controllers.

The exposed API now consistently resolves the authenticated user into a domain staff identity, uses role-scoped queries, and uses public IDs for externally addressable patient, visit, appointment, and shift actions.

I did find latent service-layer risks in non-exposed billing and test-result services. They are not currently reachable through a controller in this solution, but they should be treated as unsafe to expose without an API contract refactor first.

## Fixed Vulnerabilities

### 1. Identity claim parsing broke object-level authorization

Severity: High

Previous state:

- controllers parsed `ClaimTypes.NameIdentifier` as an `int`
- ASP.NET Identity stores that claim as a string user ID
- object scoping logic for doctor/nurse/secretary access depended on that incorrect parse

Risk:

- authenticated requests could fail with `FormatException`
- object ownership checks were being performed against the wrong identity source
- this undermined authorization decisions across multiple endpoints

Fix:

- added a claims transformation to resolve the logged-in Identity user into:
  - `DomainUserId`
  - `PublicId`
- updated controllers to use the resolved domain staff ID instead of the raw Identity GUID/string

Changed files:

- `Hospital-Management-System/Services/StaffManagement/DomainUserClaimsTransformation.cs`
- `Hospital-Management-System/Services/StaffManagement/ClaimsPrincipalExtensions.cs`
- `Hospital-Management-System/Program.cs`
- `Hospital-Management-System/Controllers/Api/AppointmentController.cs`
- `Hospital-Management-System/Controllers/Api/PatientController.cs`
- `Hospital-Management-System/Controllers/Api/SchedulingController.cs`
- `Hospital-Management-System/Controllers/Api/VisitController.cs`

### 2. Patient deletion used internal `PatientId` in the public route

Severity: High

Previous state:

- `DELETE /api/Patient/{id}` exposed the internal auto-incrementing patient key
- this made patient records enumerable from the outside

Risk:

- even with role checks, the route leaked internal object identifiers
- this increased the attack surface for object enumeration and authorization probing

Fix:

- changed patient deletion to use `patientPublicId`
- updated the service layer to resolve patients by public ID instead of internal `PatientId`

Changed files:

- `Hospital-Management-System/Controllers/Api/PatientController.cs`
- `Hospital-Management-System/Services/PatientManagement/IPatientService.cs`
- `Hospital-Management-System/Services/PatientManagement/PatientService.cs`

### 3. Visit completion used internal `VisitsId` in the public route

Severity: High

Previous state:

- `POST /api/Visit/{visitId}/complete` used the internal visit primary key

Risk:

- this exposed internal visit IDs externally
- it allowed predictable identifier probing against the completion endpoint

Fix:

- changed visit completion to use `visitPublicId`
- updated completion logic to resolve the visit by public ID before applying role checks

Changed files:

- `Hospital-Management-System/Controllers/Api/VisitController.cs`
- `Hospital-Management-System/Services/ClinicalRecording/IVistsService.cs`
- `Hospital-Management-System/Services/ClinicalRecording/VisitService.cs`

### 4. Appointment creation accepted the EF entity directly

Severity: High

Previous state:

- appointment booking accepted an `Appointment` entity from the client
- the entity shape included direct foreign-key properties such as `PatientId` and `DoctorId`

Risk:

- callers could attempt to create appointments by guessing or supplying internal references
- callers could also send fields that should be server-controlled, including `PublicId`

Fix:

- changed booking to use `BookAppointmentDto`
- the service now resolves:
  - patient by `PatientPublicId`
  - doctor by `DoctorPublicId`
- `PublicId` is server-generated only
- added conflict validation before save

Changed files:

- `Hospital-Management-System/Controllers/Api/AppointmentController.cs`
- `Hospital-Management-System/Services/Scheduling/IAppointmentService.cs`
- `Hospital-Management-System/Services/Scheduling/AppointmentService.cs`
- `Hospital-Management-System/Models/ViewModels/ControllersDtos.cs`
- `Hospital-Management-System/Models/Appointment.cs`

### 5. Visit creation relied on an unsafe object reference model

Severity: High

Previous state:

- visit creation used a DTO that did not carry the right external references
- the endpoint also accepted doctor assignment through a direct parameter path in earlier behavior
- the service created visits without safely resolving patient and appointment ownership from public IDs

Risk:

- object creation could be attached to the wrong record set
- future callers could have been forced toward unsafe use of internal keys to make the endpoint work

Fix:

- changed visit creation to accept:
  - `PatientPublicId`
  - optional `AppointmentPublicId`
- the service now resolves both objects server-side
- appointment-to-patient ownership is validated before creating the visit
- visit `PublicId` is server-generated only

Changed files:

- `Hospital-Management-System/Controllers/Api/VisitController.cs`
- `Hospital-Management-System/Services/ClinicalRecording/IVistsService.cs`
- `Hospital-Management-System/Services/ClinicalRecording/VisitService.cs`
- `Hospital-Management-System/Models/ViewModels/ControllersDtos.cs`
- `Hospital-Management-System/Models/Visit.cs`

### 6. Patient enrollment incorrectly pushed doctor assignment into create-patient

Severity: Medium

Previous state:

- patient enrollment expected doctor assignment during creation
- the workflow requirement is that patients can be created first and assigned later

Risk:

- this was not a pure IDOR on its own, but it pushed the API toward taking object references too early
- it also created pressure to trust caller-supplied doctor references during patient creation

Fix:

- removed doctor assignment from enrollment
- added an explicit doctor assignment action after patient creation
- doctor assignment now resolves the doctor by public ID on the server

Changed files:

- `Hospital-Management-System/Services/PatientManagement/EnrollmentService.cs`
- `Hospital-Management-System/Services/PatientManagement/IPatientService.cs`
- `Hospital-Management-System/Services/PatientManagement/PatientService.cs`
- `Hospital-Management-System/Controllers/Api/PatientController.cs`
- `Hospital-Management-System/Models/ViewModels/ControllersDtos.cs`
- `Hospital-Management-System/Models/Patient.cs`

### 7. Error handling leaked authorization failures as generic 500s

Severity: Medium

Previous state:

- object lookup and authorization failures could bubble up as internal server errors

Risk:

- not a direct IDOR, but it obscured object-level security behavior
- it made it harder to distinguish between bad identifiers, forbidden access, and true server faults

Fix:

- added centralized exception mapping:
  - `404` for missing objects
  - `403` for forbidden access
  - `400` for invalid requests

Changed files:

- `Hospital-Management-System/Program.cs`
- `Hospital-Management-System/Controllers/ErrorController.cs`

## Public ID Hardening

While not a standalone IDOR category, several entities had public ID generators that could exceed the existing database column limits. That creates inconsistent behavior and can break create flows that are supposed to be secured by public IDs.

Adjusted model-side generation to fit current schema limits for:

- appointments
- visits
- doctor shifts
- nurse shifts
- secretary shifts
- admin assistant shifts
- manager records

Changed files:

- `Hospital-Management-System/Models/Appointment.cs`
- `Hospital-Management-System/Models/Visit.cs`
- `Hospital-Management-System/Models/DoctorsShift.cs`
- `Hospital-Management-System/Models/NurseShift.cs`
- `Hospital-Management-System/Models/SecretaryShift.cs`
- `Hospital-Management-System/Models/AdminAssistantShift.cs`
- `Hospital-Management-System/Models/Manager.cs`

## Remaining Latent Risks

These were not found on an exposed controller route in this solution, so they are not current API IDOR findings. They are still important because they would become vulnerable quickly if an API controller were added on top of them without redesign.

### 1. Billing service still uses internal IDs in write and lookup contracts

Risk level: Medium

Observed patterns in `Hospital-Management-System/Services/ClinicalRecording/BillingService.cs`:

- `CreateBillingAsync(Fee fee, ...)` accepts a full entity and trusts internal `VisitId`
- `DeleteFeeAsync(int feeId, ...)` uses internal `FeeId`
- `MarkAsPaidAsync(int feeId)` uses internal `FeeId`
- `GetUnpaidBillsAsync(int patientId, ...)` uses internal `PatientId`
- `GetOutstandingBalanceAsync(int patientId, ...)` uses internal `PatientId`

Assessment:

- safe enough as internal-only service methods for now
- not safe to expose directly as external API contracts

Recommended future fix:

- replace entity-bound inputs with DTOs using `VisitPublicId`, `FeePublicId`, and `PatientPublicId`

### 2. Test result service still uses internal IDs in creation

Risk level: Medium

Observed patterns in `Hospital-Management-System/Services/ClinicalRecording/TestResultService.cs`:

- `AddTestResultAsync(TestResult result, ...)` accepts a full entity
- it relies on internal `TestId` and `VisitId`

Assessment:

- currently latent because there is no matching controller in the scanned API surface
- should not be exposed directly through Swagger or a public API as-is

Recommended future fix:

- move to DTOs that resolve ordered test and visit by public identifiers on the server

## Current Exposed API Status

After the remediation in this pass:

- no exposed controller route in `Controllers/Api` is using `int.Parse` on the raw Identity `NameIdentifier`
- no exposed patient, visit, appointment, or shift action currently requires a client to send an internal database key
- object retrieval paths are role-scoped before returning records
- public IDs are used as the external reference for the main exposed object types

## Verification

Verification performed:

- static scan across controllers and services for direct object reference patterns
- solution build verification with:
  - `dotnet build Hospital-Management.sln`

Build result:

- success
- `0` warnings
- `0` errors

## Changed Files Summary

- `Hospital-Management-System/Program.cs`
- `Hospital-Management-System/Controllers/ErrorController.cs`
- `Hospital-Management-System/Controllers/Api/AppointmentController.cs`
- `Hospital-Management-System/Controllers/Api/PatientController.cs`
- `Hospital-Management-System/Controllers/Api/SchedulingController.cs`
- `Hospital-Management-System/Controllers/Api/VisitController.cs`
- `Hospital-Management-System/Services/StaffManagement/DomainUserClaimsTransformation.cs`
- `Hospital-Management-System/Services/StaffManagement/ClaimsPrincipalExtensions.cs`
- `Hospital-Management-System/Services/PatientManagement/EnrollmentService.cs`
- `Hospital-Management-System/Services/PatientManagement/IPatientService.cs`
- `Hospital-Management-System/Services/PatientManagement/PatientService.cs`
- `Hospital-Management-System/Services/Scheduling/IAppointmentService.cs`
- `Hospital-Management-System/Services/Scheduling/AppointmentService.cs`
- `Hospital-Management-System/Services/Scheduling/AvailabilityService.cs`
- `Hospital-Management-System/Services/Scheduling/SchedulingQueryService.cs`
- `Hospital-Management-System/Services/ClinicalRecording/IVistsService.cs`
- `Hospital-Management-System/Services/ClinicalRecording/VisitService.cs`
- `Hospital-Management-System/Models/ViewModels/ControllersDtos.cs`
- `Hospital-Management-System/Models/Patient.cs`
- `Hospital-Management-System/Models/Appointment.cs`
- `Hospital-Management-System/Models/Visit.cs`
- `Hospital-Management-System/Models/DoctorsShift.cs`
- `Hospital-Management-System/Models/NurseShift.cs`
- `Hospital-Management-System/Models/SecretaryShift.cs`
- `Hospital-Management-System/Models/AdminAssistantShift.cs`
- `Hospital-Management-System/Models/Manager.cs`
- `ISSUES.md`
