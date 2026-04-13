import { AlertTriangle, CalendarClock, ShieldCheck, Stethoscope } from "lucide-react";
import { Outlet, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import apiClient from "../../api/apiClient";
import PatientSidebar from "../../components/layout/PatientSidebar";

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

export default function ChartLayout() {
  const { patientId } = useParams();
  const [patient, setPatient] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadPatient = async () => {
      try {
        const response = await apiClient.get(`/api/Patient/${patientId}`);

        if (isMounted) {
          setPatient(normalizePatient(response.data));
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load the requested patient chart.",
          );
          setPatient(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    if (patientId) {
      loadPatient();
    }

    return () => {
      isMounted = false;
    };
  }, [patientId]);

  if (isLoading) {
    return (
      <section className="panel-shell mx-auto max-w-3xl p-10 text-center">
        <p className="section-title">Patient Chart</p>
        <h2 className="mt-3 text-2xl font-semibold text-slate-950">Loading chart...</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          Retrieving the patient record from the live API.
        </p>
      </section>
    );
  }

  if (!patient) {
    return (
      <section className="panel-shell mx-auto max-w-3xl p-10 text-center">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-50 text-rose-700">
          <AlertTriangle className="h-6 w-6" />
        </div>
        <p className="section-title">Patient Chart</p>
        <h2 className="mt-3 text-2xl font-semibold text-slate-950">Chart not found</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          {errorMessage || "The requested patient could not be loaded."}
        </p>
      </section>
    );
  }

  return (
    <div className="flex flex-col gap-6 lg:flex-row">
      <PatientSidebar />

      <section className="min-w-0 flex-1 space-y-6">
        <div className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 bg-slate-50/80 px-6 py-5">
            <p className="section-title">Active Chart</p>
            <div className="mt-3 flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
              <div>
                <h2 className="text-2xl font-semibold tracking-tight text-slate-950">
                  {patient.firstName} {patient.lastName}
                </h2>
                <p className="mt-2 text-sm text-slate-500">
                  {patient.publicId} • {patient.healthCardNo}
                </p>
              </div>

              <div className="grid gap-3 sm:grid-cols-3">
                <div className="rounded-2xl bg-white px-4 py-3">
                  <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    <ShieldCheck className="h-4 w-4" />
                    Patient Type
                  </div>
                  <p className="mt-2 font-semibold text-slate-900">{patient.type}</p>
                </div>
                <div className="rounded-2xl bg-white px-4 py-3">
                  <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    <CalendarClock className="h-4 w-4" />
                    Last Modified
                  </div>
                  <p className="mt-2 font-semibold text-slate-900">
                    {patient.lastModified ? new Date(patient.lastModified).toLocaleDateString() : "Unknown"}
                  </p>
                </div>
                <div className="rounded-2xl bg-white px-4 py-3">
                  <div className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    <Stethoscope className="h-4 w-4" />
                    Gender
                  </div>
                  <p className="mt-2 font-semibold text-slate-900">{patient.gender ?? "Unspecified"}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Outlet context keeps the selected patient in one place so the chart
              pages do not each need to refetch or re-derive it in phase one. */}
          <div className="p-6">
            <Outlet context={{ patient }} />
          </div>
        </div>
      </section>
    </div>
  );
}
