import { Activity } from "lucide-react";
import { useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import apiClient from "../../api/apiClient";

export default function PatientVitals() {
  const { patient } = useOutletContext();
  const [vitals, setVitals] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadVitals = async () => {
      try {
        const response = await apiClient.get(`/api/Patient/${patient.publicId}/vitals`);
        if (isMounted) {
          setVitals(response.data);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load patient vitals.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadVitals();
    return () => {
      isMounted = false;
    };
  }, [patient.publicId]);

  return (
    <section className="space-y-6">
      <div>
        <p className="section-title">Vitals</p>
        <h3 className="mt-2 text-2xl font-semibold text-slate-950">Recorded Patient Vitals</h3>
      </div>

      {errorMessage ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      {isLoading ? (
        <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">Loading vitals...</div>
      ) : (
        <div className="grid gap-4">
          {vitals.length === 0 ? (
            <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
              No recorded vitals for this patient.
            </div>
          ) : (
            vitals.map((vital) => (
              <article key={`${vital.visitPublicId}-${vital.recordedAt}`} className="rounded-2xl border border-slate-200 bg-white p-5">
                <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                  <Activity className="h-4 w-4 text-blue-700" />
                  {vital.visitPublicId}
                </div>
                <div className="mt-4 grid gap-4 md:grid-cols-4 text-sm text-slate-600">
                  <div><span className="text-slate-400">Weight</span><p className="mt-1 font-medium text-slate-800">{vital.weight ?? "N/A"}</p></div>
                  <div><span className="text-slate-400">Height</span><p className="mt-1 font-medium text-slate-800">{vital.height ?? "N/A"}</p></div>
                  <div><span className="text-slate-400">BP</span><p className="mt-1 font-medium text-slate-800">{vital.bloodPressure ?? "N/A"}</p></div>
                  <div><span className="text-slate-400">Temp</span><p className="mt-1 font-medium text-slate-800">{vital.temperature ?? "N/A"}</p></div>
                </div>
                <p className="mt-4 text-sm text-slate-500">
                  {vital.nurseName ?? "Unknown nurse"} • {vital.recordedAt ? new Date(vital.recordedAt).toLocaleString() : "Unknown time"}
                </p>
              </article>
            ))
          )}
        </div>
      )}
    </section>
  );
}
