import { ClipboardList } from "lucide-react";
import { useEffect, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

function normalizeVisit(visit) {
  return {
    publicId: visit.visitPublicId,
    status: visit.status,
    patientClass: visit.patientClass,
    admissionStatus: visit.admissionStatus,
    arrivalSource: visit.arrivalSource,
    checkinTime: visit.checkinTime,
    checkoutTime: visit.checkoutTime,
    symptoms: visit.symptoms,
    diagnosis: visit.diagnosis,
    treatment: visit.treatment,
    visitNotes: visit.visitNotes,
  };
}

export default function VisitsWorkspace() {
  const { user } = useAuth();
  const [visits, setVisits] = useState([]);
  const [selectedVisit, setSelectedVisit] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadVisits = async () => {
      try {
        const response = await apiClient.get("/api/Visit/active");
        if (!isMounted) {
          return;
        }

        const normalizedVisits = response.data.map(normalizeVisit);
        setVisits(normalizedVisits);
        if (normalizedVisits[0]) {
          setSelectedVisit(normalizedVisits[0]);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load active visits.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadVisits();
    return () => {
      isMounted = false;
    };
  }, []);

  const handleSelectVisit = async (visitPublicId) => {
    setDetailLoading(true);
    setErrorMessage("");

    try {
      const response = await apiClient.get(`/api/Visit/${visitPublicId}`);
      setSelectedVisit(normalizeVisit(response.data));
    } catch (error) {
      setErrorMessage(error?.response?.data?.detail ?? "Unable to load visit details.");
    } finally {
      setDetailLoading(false);
    }
  };

  if (!["Doctor", "Nurse"].includes(user?.role ?? "")) {
    return (
      <section className="panel-shell p-8">
        <p className="section-title">Visits</p>
        <h2 className="mt-2 text-2xl font-semibold text-slate-950">Visit workspace unavailable</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          The backend currently scopes global visit access to Doctor and Nurse sessions only.
        </p>
      </section>
    );
  }

  return (
    <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <p className="section-title">Visits</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-950">Active Visit Workspace</h2>
        </div>

        {errorMessage ? (
          <div className="p-4">
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
              {errorMessage}
            </div>
          </div>
        ) : null}

        {isLoading ? (
          <div className="p-6 text-sm text-slate-500">Loading visits...</div>
        ) : (
          <div className="space-y-3 p-4">
            {visits.map((visit) => (
              <button
                key={visit.publicId}
                type="button"
                onClick={() => handleSelectVisit(visit.publicId)}
                className={[
                  "w-full rounded-2xl border px-4 py-4 text-left transition",
                  selectedVisit?.publicId === visit.publicId
                    ? "border-blue-200 bg-blue-50"
                    : "border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50",
                ].join(" ")}
              >
                <p className="font-semibold text-slate-900">{visit.publicId}</p>
                <p className="mt-1 text-sm text-slate-500">
                  {visit.patientClass} • {visit.admissionStatus}
                </p>
              </button>
            ))}
          </div>
        )}
      </section>

      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
          <div className="flex items-center gap-3">
            <ClipboardList className="h-5 w-5" />
            <div>
              <p className="section-title text-slate-300">Visit Detail</p>
              <h2 className="mt-2 text-2xl font-semibold">
                {selectedVisit?.publicId ?? "Select a visit"}
              </h2>
            </div>
          </div>
        </div>

        <div className="space-y-4 p-6">
          {detailLoading ? (
            <div className="text-sm text-slate-500">Loading visit details...</div>
          ) : selectedVisit ? (
            <>
              <div className="grid gap-4 md:grid-cols-2">
                <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4">
                  <p className="text-sm font-semibold text-slate-500">Status</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">{selectedVisit.status}</p>
                </article>
                <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4">
                  <p className="text-sm font-semibold text-slate-500">Arrival Source</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">{selectedVisit.arrivalSource}</p>
                </article>
              </div>

              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Symptoms</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedVisit.symptoms ?? "No symptoms recorded."}</p>
              </article>

              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Diagnosis</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedVisit.diagnosis ?? "No diagnosis recorded."}</p>
              </article>

              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Treatment & Notes</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedVisit.treatment ?? "No treatment recorded."}</p>
                <p className="mt-3 text-sm leading-6 text-slate-500">{selectedVisit.visitNotes ?? "No visit notes recorded."}</p>
              </article>
            </>
          ) : (
            <div className="text-sm text-slate-500">Select a visit from the left to inspect it.</div>
          )}
        </div>
      </section>
    </div>
  );
}
