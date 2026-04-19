import { Search, UserPlus, UsersRound } from "lucide-react";
import { useDeferredValue, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";
import { PATIENT_ENROLLMENT_ROLES, hasRoleAccess } from "../../data/roleAccess";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-100";

function normalizePatient(patient) {
  return {
    publicId: patient.patientPublicId,
    firstName: patient.firstName,
    lastName: patient.lastName,
    dateOfBirth: patient.dateOfBirth,
    address: patient.address,
    phoneNumber: patient.phoneNumber,
    email: patient.email,
    gender: patient.gender,
    healthCardNo: patient.healthCardNo,
    type: patient.type,
    lastModified: patient.lastModified,
  };
}

function buildEnrollmentForm() {
  return {
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    gender: "Female",
    phoneNumber: "",
    healthCardNo: "",
    email: "",
    address: "",
  };
}

export default function PatientList() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchQuery, setSearchQuery] = useState("");
  const [visibleCount, setVisibleCount] = useState(8);
  const [patients, setPatients] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState(location.state?.flashMessage ?? "");
  const [enrollmentForm, setEnrollmentForm] = useState(buildEnrollmentForm());
  const deferredSearchQuery = useDeferredValue(searchQuery);
  const canEnrollPatients = hasRoleAccess(user?.role, PATIENT_ENROLLMENT_ROLES);

  const loadPatients = async () => {
    const response = await apiClient.get("/api/Patient");
    setPatients(response.data.map(normalizePatient));
  };

  useEffect(() => {
    let isMounted = true;

    const hydratePatients = async () => {
      try {
        if (isMounted) {
          await loadPatients();
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load the patient registry.",
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    hydratePatients();

    return () => {
      isMounted = false;
    };
  }, []);

  useEffect(() => {
    if (location.state?.flashMessage) {
      navigate(location.pathname, { replace: true, state: null });
    }
  }, [location.pathname, location.state, navigate]);

  const filteredPatients = patients.filter((patient) => {
    const haystack = `${patient.firstName} ${patient.lastName} ${patient.publicId} ${patient.healthCardNo}`.toLowerCase();
    return haystack.includes(deferredSearchQuery.trim().toLowerCase());
  });

  const visiblePatients = filteredPatients.slice(0, visibleCount);

  const handleEnrollmentChange = (field, value) => {
    setEnrollmentForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleEnrollPatient = async (event) => {
    event.preventDefault();
    setIsCreating(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.post("/api/Patient/enroll", enrollmentForm);
      await loadPatients();
      setEnrollmentForm(buildEnrollmentForm());
      setSuccessMessage("Patient enrolled successfully.");
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to enroll the new patient.",
      );
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <section className="space-y-6">
      <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
        <section className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 px-6 py-5">
            <p className="section-title">Patients</p>
            <div className="mt-3 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
              <div>
                <h2 className="text-2xl font-semibold tracking-tight text-slate-950">
                  Owned Patient Registry
                </h2>
                <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">
                  This registry is live against the backend and remains scoped by the caller&apos;s role permissions.
                </p>
              </div>

              <label className="relative block w-full max-w-md">
                <Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                <input
                  type="search"
                  value={searchQuery}
                  onChange={(event) => setSearchQuery(event.target.value)}
                  placeholder="Search by name, patient public ID, or health card"
                  className="w-full rounded-2xl border border-slate-200 bg-slate-50 py-3 pl-11 pr-4 text-sm text-slate-900 outline-none transition focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-100"
                />
              </label>
            </div>
          </div>

          {errorMessage ? (
            <div className="px-6 pt-6">
              <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {errorMessage}
              </div>
            </div>
          ) : null}

          {successMessage ? (
            <div className="px-6 pt-6">
              <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                {successMessage}
              </div>
            </div>
          ) : null}

          {isLoading ? (
            <div className="p-6">
              <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
                Loading patients...
              </div>
            </div>
          ) : (
            <div className="grid gap-4 p-6 sm:grid-cols-2 xl:grid-cols-2">
              {visiblePatients.map((patient) => (
                <button
                  key={patient.publicId}
                  type="button"
                  onClick={() => navigate(`/patients/${patient.publicId}`)}
                  className="rounded-2xl border border-slate-200 bg-white p-5 text-left transition hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-panel"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-lg font-semibold text-slate-900">
                        {patient.firstName} {patient.lastName}
                      </p>
                      <p className="mt-1 text-sm text-slate-500">{patient.publicId}</p>
                    </div>
                    <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700 ring-1 ring-slate-200">
                      {patient.type}
                    </span>
                  </div>

                  <dl className="mt-5 grid grid-cols-2 gap-3 text-sm text-slate-600">
                    <div>
                      <dt className="text-slate-400">Health Card</dt>
                      <dd className="mt-1 font-medium text-slate-800">{patient.healthCardNo}</dd>
                    </div>
                    <div>
                      <dt className="text-slate-400">Gender</dt>
                      <dd className="mt-1 font-medium text-slate-800">{patient.gender ?? "Unspecified"}</dd>
                    </div>
                    <div>
                      <dt className="text-slate-400">Email</dt>
                      <dd className="mt-1 truncate font-medium text-slate-800">{patient.email ?? "Not provided"}</dd>
                    </div>
                    <div>
                      <dt className="text-slate-400">Last Modified</dt>
                      <dd className="mt-1 font-medium text-slate-800">
                        {patient.lastModified ? new Date(patient.lastModified).toLocaleDateString() : "Unknown"}
                      </dd>
                    </div>
                  </dl>
                </button>
              ))}
            </div>
          )}

          <div className="flex flex-col items-center gap-3 border-t border-slate-200 px-6 py-5 text-center">
            {!isLoading && visiblePatients.length === 0 ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                No patients matched the current search.
              </div>
            ) : (
              <>
                <div className="flex items-center gap-2 text-sm text-slate-500">
                  <UsersRound className="h-4 w-4" />
                  Showing {visiblePatients.length} of {filteredPatients.length} available patients
                </div>
                {visibleCount < filteredPatients.length ? (
                  <button
                    type="button"
                    onClick={() => setVisibleCount((current) => current + 4)}
                    className="rounded-2xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800"
                  >
                    Load More Patients
                  </button>
                ) : null}
              </>
            )}
          </div>
        </section>

        {canEnrollPatients ? (
          <section className="panel-shell overflow-hidden">
            <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
              <div className="flex items-center gap-3">
                <div className="rounded-2xl bg-white/10 p-3 text-white">
                  <UserPlus className="h-5 w-5" />
                </div>
                <div>
                  <p className="section-title text-slate-300">Patient Intake</p>
                  <h2 className="text-2xl font-semibold">Enroll New Patient</h2>
                </div>
              </div>
              <p className="mt-3 text-sm text-slate-300">
                Create a brand-new patient record before assigning a doctor later in the workflow.
              </p>
            </div>

            <form onSubmit={handleEnrollPatient} className="grid gap-4 p-6 md:grid-cols-2">
              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">First Name</span>
                <input
                  value={enrollmentForm.firstName}
                  onChange={(event) => handleEnrollmentChange("firstName", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Last Name</span>
                <input
                  value={enrollmentForm.lastName}
                  onChange={(event) => handleEnrollmentChange("lastName", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Date of Birth</span>
                <input
                  type="date"
                  value={enrollmentForm.dateOfBirth}
                  onChange={(event) => handleEnrollmentChange("dateOfBirth", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Gender</span>
                <select
                  value={enrollmentForm.gender}
                  onChange={(event) => handleEnrollmentChange("gender", event.target.value)}
                  className={fieldClassName}
                  required
                >
                  <option value="Female">Female</option>
                  <option value="Male">Male</option>
                </select>
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Phone Number</span>
                <input
                  value={enrollmentForm.phoneNumber}
                  onChange={(event) => handleEnrollmentChange("phoneNumber", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Health Card</span>
                <input
                  value={enrollmentForm.healthCardNo}
                  onChange={(event) => handleEnrollmentChange("healthCardNo", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2 md:col-span-2">
                <span className="text-sm font-semibold text-slate-700">Email</span>
                <input
                  type="email"
                  value={enrollmentForm.email}
                  onChange={(event) => handleEnrollmentChange("email", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2 md:col-span-2">
                <span className="text-sm font-semibold text-slate-700">Address</span>
                <input
                  value={enrollmentForm.address}
                  onChange={(event) => handleEnrollmentChange("address", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <div className="md:col-span-2">
                <button
                  type="submit"
                  disabled={isCreating}
                  className="rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
                >
                  {isCreating ? "Enrolling..." : "Enroll Patient"}
                </button>
              </div>
            </form>
          </section>
        ) : null}
      </div>
    </section>
  );
}
