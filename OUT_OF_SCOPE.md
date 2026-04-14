## Out Of Scope

These flows are intentionally not wired into the frontend yet because the backend service/controller layer is still missing or placeholder-only.

### Diagnostic Test Ordering
- `IDiagnosticTestService` exists as an interface stub in [IDignosticsService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/IDignosticsService.cs)
- [DiagnosticsService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/DiagnosticsService.cs) is empty
- there is no API controller for ordering a diagnostic test

### Prescription Management
- [PrescriptionService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/PrescriptionService.cs) is placeholder-only
- the frontend can read prescriptions through the patient chart, but there is no real create/update/delete prescription service flow to wire

### Referral Management
- [ReferralService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/ReferralService.cs) is placeholder-only
- there is no referral API workflow to expose in the UI

### Standalone Vitals Management
- [VitalsService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/VitalsService.cs) is placeholder-only
- the frontend can read vitals through patient-chart queries, but there is no dedicated vitals create/update service surface

### MedAssist
- [MedAssistService.cs](/Users/traviseweka/Documents/Personal%20Project/Hospital-Management-System/Hospital-Management-System/Services/ClinicalRecording/MedAssistService.cs) is not implemented
- there is no controller surface to wire
