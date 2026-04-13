import { ClipboardCheck, Save } from "lucide-react";

const statusOptions = ["Active", "Completed"];
const patientClassOptions = ["Inpatient", "Outpatient", "Emergency", "ER Referral"];
const admissionStatusOptions = [
  "Admitted",
  "Not Admitted",
  "Discharged",
  "Triage Pending",
];

export default function VisitDetails({
  patient,
  visit,
  classificationDraft,
  onClassificationChange,
  onSave,
  isSaving,
}) {
  if (!visit) {
    return (
      <section className="panel-shell flex min-h-[420px] items-center justify-center p-8">
        <div className="max-w-md text-center">
          <p className="section-title">Visit Details</p>
          <h3 className="mt-3 text-2xl font-semibold text-slate-950">Select a visit to classify</h3>
          <p className="mt-3 text-sm leading-6 text-slate-500">
            The right panel becomes the clinical classification controller once a patient-specific visit is selected from the chart timeline.
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="panel-shell overflow-hidden">
      <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
        <div className="flex flex-col gap-3 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <p className="section-title text-slate-300">Visit Controller</p>
            <h3 className="mt-2 text-2xl font-semibold">{visit.publicId}</h3>
            <p className="mt-2 text-sm text-slate-300">
              {patient.firstName} {patient.lastName} • {visit.location}
            </p>
          </div>
          <div className="rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm text-slate-200">
            <p>Arrival Source: {visit.arrivalSource}</p>
            <p className="mt-1">Attending: {visit.attendingLabel}</p>
          </div>
        </div>
      </div>

      <div className="grid gap-6 p-6 xl:grid-cols-[1fr_320px]">
        <div className="space-y-6">
          <div>
            <p className="section-title">Clinical Edit Panel</p>
            <h4 className="mt-2 text-xl font-semibold text-slate-950">
              Medical Classification Controls
            </h4>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
              This panel updates the live enum-backed visit classification fields through the backend API.
            </p>
          </div>

          {/* The draft state lives in the parent visits page so selecting a different
              visit can rehydrate the form from that visit's current classifications. */}
          <div className="grid gap-5 lg:grid-cols-3">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Status</span>
              <select
                value={classificationDraft.status}
                onChange={(event) => onClassificationChange("status", event.target.value)}
                className="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
              >
                {statusOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Patient Class</span>
              <select
                value={classificationDraft.patientClass}
                onChange={(event) => onClassificationChange("patientClass", event.target.value)}
                className="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
              >
                {patientClassOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Admission Status</span>
              <select
                value={classificationDraft.admissionStatus}
                onChange={(event) =>
                  onClassificationChange("admissionStatus", event.target.value)
                }
                className="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
              >
                {admissionStatusOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
              <ClipboardCheck className="h-4 w-4 text-blue-700" />
              Draft Payload Preview
            </div>
            <pre className="mt-4 overflow-x-auto rounded-2xl bg-slate-950 p-4 text-sm text-slate-100">
{JSON.stringify(
  {
    visitPublicId: visit.publicId,
    patientPublicId: patient.publicId,
    ...classificationDraft,
  },
  null,
  2,
)}
            </pre>
          </div>
        </div>

        <aside className="space-y-4">
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-semibold text-slate-500">Visit Date</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">
              {new Date(visit.visitDate).toLocaleString()}
            </p>
          </article>

          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-semibold text-slate-500">Chief Complaint</p>
            <p className="mt-2 text-sm leading-6 text-slate-700">{visit.primaryComplaint}</p>
          </article>

          <button
            type="button"
            onClick={onSave}
            disabled={isSaving}
            className="flex w-full items-center justify-center gap-2 rounded-2xl bg-blue-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
          >
            <Save className="h-4 w-4" />
            {isSaving ? "Saving..." : "Save Classifications"}
          </button>
        </aside>
      </div>
    </section>
  );
}
