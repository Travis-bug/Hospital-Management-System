# Hospital Management System

**Web App system for managing patients, enrollment, visits, and billing inside a clinic environment.**

This project focuses on **backend architecture**, **database design**, and **service-layer development** using ASP.NET Core and Entity Framework Core.

---

## Architecture

The project follows a Clean Architecture approach with a strictly defined **Service Layer** to ensure separation of concerns.

---

## Star Featured Component: IMedAssistService (AI Assist)

The `IMedAssistService` is a specialized backend layer designed for **AI-powered diagnostic support** and clinical decision assistance.

### Core Capabilities

* **Clinical Decision Support:** Analyzes combinations of symptoms, vitals, and medical history to suggest potential conditions (e.g., identifying patterns for Streptococcal infection vs. Influenza).
* **Data Retrieval & Summarization:** Provides instant summaries of a patient’s medical activity, reducing the time doctors spend manually reviewing records.
* **System Navigation:** Assists administrative staff by guiding them through complex workflows like record creation or scheduling.
* **Safety Guardrail:** The service acts as a supportive tool; all final medical decisions remain the responsibility of licensed healthcare professionals.

---

## Database Design (ER Diagram)

The relational schema tracks the full patient lifecycle, ensuring data integrity for the AI service to analyze.

---

## Full Project Structure

*As implemented in the current source code:*

```files
Hospital-Management-System
│
├── 📁 Controllers               # API Endpoints (Handling HTTP Requests)
├── 📁 Data                      # DBContext (ClinicContext) and Migrations
├── 📁 Models                    # Entity Definitions & Data Transfer Objects
│   ├── 📁 ViewModels            # Specific models for UI/API responses
│   ├── AdminAssistantShift.cs
│   ├── AdministrativeAssistant.cs
│   ├── Appointment.cs
│   ├── DiagnosticTest.cs
│   ├── Doctor.cs
│   ├── DoctorsShift.cs
│   ├── Fee.cs
│   ├── Manager.cs
│   ├── Nurse.cs
│   ├── NurseShift.cs
│   ├── Patient.cs
│   ├── PatientVital.cs
│   ├── Prescription.cs
│   ├── Referral.cs
│   ├── Secretary.cs
│   ├── SecretaryShift.cs
│   ├── Shift.cs
│   ├── TestResult.cs
│   └── Visit.cs
│
├── 📁 Services                  # Business Logic Layer
│   ├── 📁 ClinicalRecording     # Core Medical Services
│   │   ├── IBillingService.cs
│   │   ├── IDiagnosticsService.cs
│   │   ├── IMedAssistService.cs (AI Assist Service)
│   │   ├── IPrescriptionService.cs
│   │   ├── IReferralService.cs
│   │   ├── ITestResultsService.cs
│   │   ├── IVistsService.cs
│   │   └── IVitalsService.cs
│   ├── 📁 PatientManagement     # Patient Lifecycle Services
│   │   ├── IEnrollmentService.cs
│   │   ├── IPatientSearchEngineService.cs
│   │   └── IPatientService.cs
│   └── 📁 Scheduling            # Staff & Appointment Services
│       ├── IAppointmentService.cs
│       ├── IAvailabilityService.cs
│       └── ISchedulingQueryService.cs
│
├── 📁 Views                     # Frontend Razor views
├── appsettings.json             # Configuration & Connection Strings
├── Dockerfile                   # Containerization settings
└── Program.cs                   # App entry point & Dependency Injection

```

---

## Sample API Requests

### 1. AI Diagnostic Suggestion

**Endpoint:** `POST /api/assist/diagnose`

**Purpose:** Analyzes symptoms to provide clinical considerations.

**Request Body:**

```json
{
  "patientId": 502,
  "symptoms": ["fever", "sore throat", "swollen lymph nodes"],
  "includeHistory": true
}

```

### 2. Patient Enrollment

**Endpoint:** `POST /api/enrollment`

**Request Body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "healthCardNo": "1234-567-890-AB",
  "assignedDoctorId": 7
}

```

---

## Technology Stack

* **Framework:** ASP.NET Core / C#
* **ORM:** Entity Framework Core (MySQL)
* **AI Integration:** Conceptual Service Layer for Diagnostic Analysis
* **DevOps:** Docker

---

## Author

**Eseosa Travis Eweka** Computer Programming & Analysis (T177)

George Brown College

---
