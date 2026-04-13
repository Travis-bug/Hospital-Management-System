import { Mail, MapPin, Phone, ShieldPlus } from "lucide-react";
import { useOutletContext } from "react-router-dom";

export default function PatientOverview() {
  const { patient } = useOutletContext();

  return (
    <div className="grid gap-6 xl:grid-cols-[1.3fr_0.7fr]">
      <section className="space-y-6">
        <div>
          <p className="section-title">Overview</p>
          <h3 className="mt-2 text-2xl font-semibold text-slate-950">
            Demographic Snapshot
          </h3>
          <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
            This chart overview is sourced from the live patient record and acts as the
            anchor context for the patient-specific vitals, prescriptions, visits, and test results tabs.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Patient Type</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.type}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Date of Birth</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.dateOfBirth}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Gender</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.gender}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Health Card</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.healthCardNo}</p>
          </article>
        </div>
      </section>

      <aside className="space-y-4">
        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
            <Phone className="h-4 w-4 text-blue-700" />
            Contact
          </div>
          <p className="mt-3 text-sm text-slate-600">{patient.phoneNumber}</p>
          <p className="mt-2 text-sm text-slate-600">{patient.email}</p>
        </article>

        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
            <MapPin className="h-4 w-4 text-blue-700" />
            Address
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">{patient.address}</p>
        </article>

        <article className="rounded-2xl border border-slate-200 bg-slate-950 p-5 text-white">
          <div className="flex items-center gap-2 text-sm font-semibold">
            <ShieldPlus className="h-4 w-4 text-blue-300" />
            Chart State
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-300">
            Chart modules are live against the backend. Use the chart rail to switch
            between overview, vitals, prescriptions, visits, and diagnostic result history for this patient.
          </p>
        </article>
      </aside>
    </div>
  );
}
