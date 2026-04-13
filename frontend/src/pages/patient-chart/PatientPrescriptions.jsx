import { Pill } from "lucide-react";
import { useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import apiClient from "../../api/apiClient";

export default function PatientPrescriptions() {
  const { patient } = useOutletContext();
  const [prescriptions, setPrescriptions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadPrescriptions = async () => {
      try {
        const response = await apiClient.get(`/api/Patient/${patient.publicId}/prescriptions`);
        if (isMounted) {
          setPrescriptions(response.data);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load prescriptions.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadPrescriptions();
    return () => {
      isMounted = false;
    };
  }, [patient.publicId]);

  return (
    <section className="space-y-6">
      <div>
        <p className="section-title">Prescriptions</p>
        <h3 className="mt-2 text-2xl font-semibold text-slate-950">Medication Orders</h3>
      </div>

      {errorMessage ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      {isLoading ? (
        <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">Loading prescriptions...</div>
      ) : (
        <div className="grid gap-4">
          {prescriptions.length === 0 ? (
            <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
              No prescriptions recorded for this patient.
            </div>
          ) : (
            prescriptions.map((prescription) => (
              <article key={prescription.publicId} className="rounded-2xl border border-slate-200 bg-white p-5">
                <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                  <Pill className="h-4 w-4 text-blue-700" />
                  {prescription.medicineName}
                </div>
                <p className="mt-2 text-sm text-slate-500">{prescription.publicId}</p>
                <div className="mt-4 grid gap-4 md:grid-cols-3 text-sm text-slate-600">
                  <div><span className="text-slate-400">Dosage</span><p className="mt-1 font-medium text-slate-800">{prescription.dosage ?? "N/A"}</p></div>
                  <div><span className="text-slate-400">Visit</span><p className="mt-1 font-medium text-slate-800">{prescription.visitPublicId ?? "N/A"}</p></div>
                  <div><span className="text-slate-400">Doctor</span><p className="mt-1 font-medium text-slate-800">{prescription.doctorName ?? prescription.doctorPublicId ?? "N/A"}</p></div>
                </div>
              </article>
            ))
          )}
        </div>
      )}
    </section>
  );
}
