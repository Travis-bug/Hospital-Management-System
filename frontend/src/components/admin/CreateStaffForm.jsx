import { LockKeyhole, ShieldCheck, UserPlus } from "lucide-react";
import { useMemo, useState } from "react";

const allRoles = ["Doctor", "Nurse", "Secretary", "Admin", "Manager"];

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

function getAssignableRoles(currentUserRole) {
  if (currentUserRole === "Manager") {
    return allRoles;
  }

  if (currentUserRole === "Admin") {
    return ["Doctor", "Nurse", "Secretary"];
  }

  return [];
}

export default function CreateStaffForm({ currentUserRole, onCreateStaff, isSubmitting = false }) {
  const assignableRoles = useMemo(
    () => getAssignableRoles(currentUserRole),
    [currentUserRole],
  );

  // The frontend only builds the unified provisioning payload.
  // The backend remains responsible for:
  // 1. creating the correct clinic staff row
  // 2. creating the linked IdentityUser
  // 3. assigning the final Identity role
  const [formState, setFormState] = useState({
    firstName: "",
    lastName: "",
    email: "",
    temporaryPassword: "",
    role: assignableRoles[0] ?? "",
  });
  const [errorMessage, setErrorMessage] = useState("");

  const handleChange = (field, value) => {
    setFormState((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setErrorMessage("");

    const payload = {
      firstName: formState.firstName.trim(),
      lastName: formState.lastName.trim(),
      email: formState.email.trim(),
      temporaryPassword: formState.temporaryPassword,
      role: formState.role,
    };

    try {
      await onCreateStaff?.(payload);

      setFormState({
        firstName: "",
        lastName: "",
        email: "",
        temporaryPassword: "",
        role: assignableRoles[0] ?? "",
      });
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.message
          ?? error?.response?.data?.detail
          ?? error?.message
          ?? "Unable to provision the staff account.",
      );
    }
  };

  return (
    <section className="panel-shell overflow-hidden">
      <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
        <p className="section-title text-slate-300">Provisioning Controls</p>
        <h2 className="mt-2 text-2xl font-semibold">Create Internal Staff Account</h2>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-300">
          Public sign-up is disabled. This tool submits a single payload that the backend
          will use to create both the staff record and the linked login account.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 p-6">
        {errorMessage ? (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {errorMessage}
          </div>
        ) : null}

        <div className="grid gap-5 md:grid-cols-2">
          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">First Name</span>
            <input
              type="text"
              value={formState.firstName}
              onChange={(event) => handleChange("firstName", event.target.value)}
              className={fieldClassName}
              placeholder="Amina"
              disabled={isSubmitting}
              required
            />
          </label>

          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">Last Name</span>
            <input
              type="text"
              value={formState.lastName}
              onChange={(event) => handleChange("lastName", event.target.value)}
              className={fieldClassName}
              placeholder="Said"
              disabled={isSubmitting}
              required
            />
          </label>

          <label className="space-y-2 md:col-span-2">
            <span className="text-sm font-semibold text-slate-700">Email</span>
            <input
              type="email"
              value={formState.email}
              onChange={(event) => handleChange("email", event.target.value)}
              className={fieldClassName}
              placeholder="nurse.intake@hospital.com"
              disabled={isSubmitting}
              required
            />
          </label>

          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">Temporary Password</span>
            <input
              type="password"
              value={formState.temporaryPassword}
              onChange={(event) =>
                handleChange("temporaryPassword", event.target.value)
              }
              className={fieldClassName}
              placeholder="Temporary password"
              disabled={isSubmitting}
              required
            />
          </label>

          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">Role</span>
            <select
              value={formState.role}
              onChange={(event) => handleChange("role", event.target.value)}
              className={fieldClassName}
              disabled={isSubmitting}
              required
            >
              {assignableRoles.map((role) => (
                <option key={role} value={role}>
                  {role}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="grid gap-4 xl:grid-cols-[1fr_320px]">
          <div className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
              <ShieldCheck className="h-4 w-4 text-blue-700" />
              RBAC Rules
            </div>
            <ul className="mt-4 space-y-2 text-sm leading-6 text-slate-600">
              <li>`Admin` can provision `Doctor`, `Nurse`, and `Secretary` only.</li>
              <li>`Manager` can provision all internal staff roles.</li>
              <li>Patients are excluded because they do not receive login accounts.</li>
            </ul>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-5">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
              <LockKeyhole className="h-4 w-4 text-blue-700" />
              Security Flags
            </div>
            <div className="mt-4 space-y-2 text-sm text-slate-600">
              <p>2FA delivery: Email</p>
              <p>Activation link: Email-delivered</p>
              <p>Permanent password setup: Required before first sign-in</p>
              <p>Self-registration: Disabled</p>
            </div>
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="inline-flex items-center gap-2 rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800"
        >
          <UserPlus className="h-4 w-4" />
          {isSubmitting ? "Provisioning..." : "Provision Staff Member"}
        </button>
      </form>
    </section>
  );
}
