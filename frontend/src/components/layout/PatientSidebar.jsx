import {
  Activity,
  ArrowLeft,
  ClipboardPlus,
  FlaskConical,
  Pill,
  UserRound,
} from "lucide-react";
import { NavLink, useParams } from "react-router-dom";

const patientNavItems = [
  { label: "Overview", to: "overview", icon: UserRound },
  { label: "Vitals", to: "vitals", icon: Activity },
  { label: "Prescriptions", to: "prescriptions", icon: Pill },
  { label: "Visits", to: "visits", icon: ClipboardPlus },
  { label: "Tests", to: "tests", icon: FlaskConical },
];

export default function PatientSidebar() {
  const { patientId } = useParams();

  return (
    <aside className="panel-shell h-fit w-full shrink-0 p-4 lg:w-[280px]">
      <NavLink
        to="/patients"
        className="inline-flex items-center gap-2 text-sm font-medium text-slate-600 transition hover:text-slate-900"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Main
      </NavLink>

      <div className="mt-5 rounded-2xl bg-slate-950 px-4 py-5 text-white">
        <p className="section-title text-slate-300">Chart Context</p>
        <h2 className="mt-2 text-lg font-semibold">Patient Chart</h2>
        <p className="mt-1 text-sm text-slate-300">{patientId}</p>
      </div>

      <nav className="mt-4 space-y-2">
        {patientNavItems.map(({ label, to, icon: Icon }) => (
          <NavLink
            key={label}
            to={to}
            className={({ isActive }) =>
              [
                "flex items-center gap-3 rounded-2xl border px-4 py-3 text-sm font-medium transition",
                isActive
                  ? "border-blue-200 bg-blue-50 text-blue-900"
                  : "border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50",
              ].join(" ")
            }
          >
            <Icon className="h-4 w-4" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
