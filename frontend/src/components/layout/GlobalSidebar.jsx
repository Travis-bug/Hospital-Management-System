import {
  CalendarDays,
  CalendarRange,
  ClipboardList,
  FlaskConical,
  LayoutDashboard,
  ShieldCheck,
  UsersRound,
} from "lucide-react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import {
  APPOINTMENT_WORKSPACE_ROLES,
  PATIENT_REGISTRY_ROLES,
  SCHEDULE_WORKSPACE_ROLES,
  STAFF_MANAGEMENT_ROLES,
  TEST_RESULTS_WORKSPACE_ROLES,
  VISIT_WORKSPACE_ROLES,
  hasRoleAccess,
} from "../../data/roleAccess";

const globalNavItems = [
  {
    label: "Patients",
    to: "/patients",
    icon: UsersRound,
    description: "Browse the active patient roster.",
    roles: PATIENT_REGISTRY_ROLES,
  },
  {
    label: "Visits",
    to: "/visits",
    icon: ClipboardList,
    description: "Track open and historical encounters.",
    roles: VISIT_WORKSPACE_ROLES,
  },
  {
    label: "Appointments",
    to: "/appointments",
    icon: CalendarDays,
    description: "Review booked and upcoming schedules.",
    roles: APPOINTMENT_WORKSPACE_ROLES,
  },
  {
    label: "Schedule",
    to: "/schedule",
    icon: CalendarRange,
    description: "Manage personal shifts and daily staffing rosters.",
    roles: SCHEDULE_WORKSPACE_ROLES,
  },
  {
    label: "Tests",
    to: "/tests",
    icon: FlaskConical,
    description: "Access diagnostic workups and orders.",
    roles: TEST_RESULTS_WORKSPACE_ROLES,
  },
];

export default function GlobalSidebar() {
  const { user } = useAuth();
  const role = user?.role ?? "";
  const canManageStaff = hasRoleAccess(role, STAFF_MANAGEMENT_ROLES);
  const visibleNavItems = globalNavItems.filter((item) => hasRoleAccess(role, item.roles));

  if (canManageStaff) {
    visibleNavItems.push({
      label: "Staff Management",
      to: "/staff-management",
      icon: ShieldCheck,
      description: "Provision internal staff accounts and roles.",
    });
  }

  return (
    <aside className="panel-shell h-fit w-full shrink-0 p-4 lg:w-[300px]">
      <div className="flex items-center gap-3 border-b border-slate-200 pb-4">
        <div className="rounded-2xl bg-slate-900 p-3 text-white">
          <LayoutDashboard className="h-5 w-5" />
        </div>
        <div>
          <p className="section-title">Workspace</p>
          <h2 className="text-lg font-semibold text-slate-900">Clinical Dashboard</h2>
        </div>
      </div>

      {/* On mobile this becomes a stacked scroller; on desktop it behaves like
          the persistent left rail requested in the prototype. */}
      <nav className="mt-4 flex gap-3 overflow-x-auto pb-1 lg:flex-col lg:overflow-visible">
        {visibleNavItems.map(({ label, to, icon: Icon, description }) => (
          <NavLink
            key={label}
            to={to}
            className={({ isActive }) =>
              [
                "group min-w-[220px] rounded-2xl border px-4 py-4 transition-all duration-200 lg:min-w-0",
                isActive
                  ? "border-blue-200 bg-blue-50 text-blue-900 shadow-sm"
                  : "border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50",
              ].join(" ")
            }
          >
            <div className="flex items-start gap-3">
              <div className="rounded-xl bg-slate-900/90 p-2 text-white group-[.active]:bg-blue-900">
                <Icon className="h-4 w-4" />
              </div>
              <div className="space-y-1">
                <p className="font-medium">{label}</p>
                <p className="text-sm leading-5 text-slate-500">{description}</p>
              </div>
            </div>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
