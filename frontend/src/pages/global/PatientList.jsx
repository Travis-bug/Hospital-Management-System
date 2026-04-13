import { Search, UsersRound } from "lucide-react";
import { useDeferredValue, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import apiClient from "../../api/apiClient";

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

export default function PatientList() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [visibleCount, setVisibleCount] = useState(8);
  const [patients, setPatients] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");
  const deferredSearchQuery = useDeferredValue(searchQuery);

  useEffect(() => {
    let isMounted = true;

    const loadPatients = async () => {
      try {
        const response = await apiClient.get("/api/Patient");

        if (isMounted) {
          setPatients(response.data.map(normalizePatient));
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

    loadPatients();

    return () => {
      isMounted = false;
    };
  }, []);

  const filteredPatients = patients.filter((patient) => {
    const haystack = `${patient.firstName} ${patient.lastName} ${patient.publicId} ${patient.healthCardNo}`.toLowerCase();
    return haystack.includes(deferredSearchQuery.trim().toLowerCase());
  });

  const visiblePatients = filteredPatients.slice(0, visibleCount);

  return (
    <section className="space-y-6">
      <div className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <p className="section-title">Patients</p>
          <div className="mt-3 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <h2 className="text-2xl font-semibold tracking-tight text-slate-950">
                Owned Patient Registry
              </h2>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">
                This registry is now sourced from the live backend and remains scoped by the caller's role permissions.
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

        {isLoading ? (
          <div className="p-6">
            <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
              Loading patients...
            </div>
          </div>
        ) : (
        <div className="grid gap-4 p-6 sm:grid-cols-2 xl:grid-cols-3">
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
      </div>
    </section>
  );
}
