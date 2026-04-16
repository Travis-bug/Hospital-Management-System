# Hospital Management System (HMS) –Architectural Guidelines & API Documentation

## System Objective
To build a production-grade, PHIPA-compliant (Ontario) backend system. The codebase must prioritize strict security, high performance, decoupling, and comprehensive
auditability over quick "vibe coding" shortcuts. This document ensures consistent backend design, clean controller implementation, scalable UI integration, and secure API exposure.

### 1. The "Security Sandwich" (ID Strategy)
The system uses a Dual-ID System to completely decouple internal database
architecture from external APIs, preventing enumeration attacks and securing patient
data.

- **PublicId (External / Polymorphic):** Secure, non-sequential alphanumeric
  strings (e.g., DOC_xyz123, VIS_abc890, NUR_w63b). Used exclusively in API routes,
  the UI, and Audit Logs.

-  **Internal ID (int):** Fast, indexed, relational integers. Used strictly inside the
   backend database and internal service methods.
---

### 2. Authentication workflow 
The client sends public IDs only for externally addressable resources such as patients, visits, appointments, doctors, or shifts.
After authentication, DomainUserClaimsTransformation maps the authenticated Identity user to the linked clinic-side staff record and adds:
DomainUserId
PublicId

Controllers no longer parse the raw Identity GUID. They use ClaimsPrincipalExtensions to read:
- the current role
- current domain staff ID
- and the current actor public ID
Services then use that resolved actor context plus any incoming target PublicId values to perform authorization, lookup, and business login


#### The Flow:
```Client (PublicId) -> ASP.NET Identity Auth -> DomainUserClaimsTransformation -> Controller(ClaimsPrincipalExtensions extracts DomainUserId / Role / ActorPublicId) -> Service (resolves target PublicIds and executes scoped business logic)```

---

### 3. Separation of Concerns (Controller vs. Service)
- The Controller (The Guard): The Controller: Acts as the API gateway layer. It receives public request identifiers. 
After ASP.NET Identity authenticates the request, DomainUserClaimsTransformation enriches the ClaimsPrincipal with the
linked clinic-side DomainUserId and PublicId. The controller then uses ClaimsPrincipalExtensions to retrieve the actor's 
role and domain identity and passes that context into the service layer. Controllers never touch ClinicContext directly 
and only accept public IDs at the HTTP boundary.

- The Service (The Librarian): Responsible for Authorization and Business Logic.
It does not know about HTTP requests. It takes the IDs provided by the
controller, converts them if necessary, and enforces the "Circle of Care" directly
in the database query (e.g., ```.Where(v ⇒ v.EntityId == currentUserId)``` ).
---

### 4. The "Three Gets" Service Design Pattern
Every major service (Visits, Tests, Patients) must implement exactly three types of
retrieval methods to avoid method bloat, duplicate logic, and IDOR attacks:
1. **The "Get All" Decision Tree (List/Filter):** ```Task<IEnumerable<Entity>> GetAllEntitiesAsync(string role, int CurrentUserId)```
   - Stacks .Where() clauses dynamically.
   - Used in Dashboards and Patient Profiles.
   - for getting all data belonging 

2. **The API Entry Point (External):** ```Task<Entity?> GetEntityByPublicIdAsync(string publicId)```
   - "The Bouncer." Controllers use this to fetch *specific* records requested by
   the frontend.

3. **The Internal Workhorse (Internal):** ```Task<Entity?> GetEntityByIdAsync(int id)```
   - Used purely inside the service layer for updates and business logic.
   Uses .FindAsync(id) for maximum speed.
---

### 5. Search & Filter Implementation Rules
- **Mental Model: Search** Find data (Entry point, user keyword input). Get =
  Retrieve structured data (IDs). **Filter** = Refine results (Date, Status).

- **graceful Degradation:** NEVER throw an ArgumentException if a search keyword is
  null or whitespace. Return an empty C# 12 collection expression ```(return [];)```.
  Throwing exceptions breaks the UI on initial page loads.

- **Scoped Searching:** Searches must ALWAYS be bounded by the user's role and
  ID to prevent cross-department snooping.
---

### 6. PHIPA-Compliant Audit Logging (CRITICAL)
The system uses a decoupled, generic AuditLog table using Polymorphic Public IDs
(The Stripe ID Pattern). We do not use hard Foreign Keys for the AuditLog to prevent
schema spiderwebs and cascade-delete issues. The system also ensures that nothing is 
deleted from the database to prevent orphaned audit logs.The system has been
designed to "cancel" or "deactivate" the records, which preserves the audit authenticity

#### AuditLog Schema Standard:
- PerformedBy: string (e.g., DOC_xyz3 or NUR_w63b)
- ActionType: string/enum (e.g., CREATE, UPDATE, READ, SEARCH)
- EntityName: string (e.g., nameof(Visit))
- EntityPublicId: string (e.g., VIS_cwy6. Use "Multiple" for bulk searches)
- Timestamp: UTC DateTime

#### Ui Display Strategy: 
Raw audit logs are translated into LogDisplayDTOs before hitting the clinical
UI (translating DOC_xyz3 to "Dr. John Smith"). Raw codes are strictly reserved 
for the Admin/Privacy Officer UI.


### 7. System Workflow

1. #### Doctor Dashboard
- **Tabs:** Patients, Visits, Appointments, Schedule, Tests
- **Behaviour:** Loads all records related and belonging to the logged-in doctor, ```(GetAllEntityAsync(currentUserId)).```
- **Search:** Keyword-based, strictly scoped to the doctor's assigned patients```(SearchEntitysAsync("John", currentUserId)).```

2. #### Patient Profile
- **Flow:** Search Patient ➔ Select Patient ➔ Open Profile.
- **Tabs:** Overview, Vitals, Prescriptions, Visits, Tests 
- **Behavior:** Shows data related to BOTH the selected patient and the logged-in doctor ```(GetEntityByPatientPublicAsync(currentUserId, requestedPatientId)).```


### 8. API endpoints Design Examples (Visits)
- **Get all visits for a doctor:** 
- **get Visits for a specific patient:** ```GET /api/patients/{patientPublicId}/visits```
- **Get single visit:** GET ```GET /api/visits/{publicId}```
- **Search visits:** GET ```/api/visits/search?keyword=john```
- **Filter visits by data: GET ```/api/visits?date=2026-03-18```


### 9. Anti-Patterns & Rules to NEVER Break
- **Never query the database using names.**
- **Never expose internal IDs (ints) in the API.**
- **Never mix Search and Get logic.**
- **Never use internal IDs for Audit logs** 
- **Never allow non-role-based access** 
- **Never create duplicate query methods (e.g., ```GetDoctorVisits(), GetPatientVisits())``` Use the "Three Gets" pattern**
- **Never blindly trust auth data from the front end, enforce a zero trust policy**

### Note for future contributors
- **N+1 Audit Log Nightmare:** A "Search" is NOT a "Read". Log EXACTLY
  ONE event for a search (```ActionType: "SEARCH", EntityPublicId: "Multiple"```). Do not loop
  through 500 search results to log individual reads. Only log a "READ" when a
  user explicitly opens a specific record.

- **Using Naked Integers for Generic Logging:** ALWAYS use the Polymorphic ID
  (VIS_cwy6) for audit logs so they remain human-readable and decoupled.

- **Controller DB Logic:** Controllers NEVER touch _context or
  write ```.FirstOrDefaultAsync().``` They call the Service layer.

- **Missing SaveChangesAsync:** Every mutation service method (Create, Update,
  Delete) must culminate in a single **await _context.SaveChangesAsync()** to ensure
  atomic transactions.



