import { ShieldAlert, ShieldCheck, Users } from "lucide-react";
import { useEffect, useState } from "react";
import apiClient from "../../api/apiClient";
import CreateStaffForm from "../../components/admin/CreateStaffForm";
import { useAuth } from "../../contexts/AuthContext";

function canManageStaff(role) {
  return role === "Manager" || role === "Admin";
}

export default function StaffProvisioning() {
  const { user } = useAuth();
  const [staffMembers, setStaffMembers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const loadStaff = async () => {
    const response = await apiClient.get("/api/Staff");
    setStaffMembers(response.data);
  };

  useEffect(() => {
    let isMounted = true;

    const hydrateStaff = async () => {
      try {
        if (isMounted) {
          await loadStaff();
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load the staff directory.",
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    if (canManageStaff(user?.role)) {
      hydrateStaff();
    }

    return () => {
      isMounted = false;
    };
  }, [user?.role]);

  // The page itself is guarded so a direct URL hit still respects RBAC
  // even when the sidebar link is hidden.
  if (!canManageStaff(user?.role)) {
    return (
      <section className="panel-shell mx-auto max-w-3xl p-10 text-center">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-50 text-rose-700">
          <ShieldAlert className="h-6 w-6" />
        </div>
        <p className="section-title mt-5">Restricted Workspace</p>
        <h2 className="mt-3 text-2xl font-semibold text-slate-950">Staff management is blocked</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          Only `Admin` and `Manager` sessions can provision internal staff accounts.
          The current demo user is `{user?.role ?? "Guest"}`.
        </p>
      </section>
    );
  }

  const handleCreateStaff = async (payload) => {
    setIsCreating(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      const response = await apiClient.post("/api/Staff", payload);
      await loadStaff();
      setSuccessMessage(
        `Provisioned ${response.data.role} account ${response.data.publicId} for ${response.data.displayName}.`,
      );
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.message
          ?? error?.response?.data?.detail
          ?? "Unable to provision the staff account.",
      );
      throw error;
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[380px_1fr]">
      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <div className="flex items-center gap-3">
            <div className="rounded-2xl bg-slate-900 p-3 text-white">
              <Users className="h-5 w-5" />
            </div>
            <div>
              <p className="section-title">Staff Directory</p>
              <h2 className="text-2xl font-semibold text-slate-950">Current Internal Users</h2>
            </div>
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-500">
            This list represents provisioned staff profiles. Patients are intentionally
            excluded because they never receive application logins.
          </p>
        </div>

        {errorMessage ? (
          <div className="px-4 pt-4">
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
              {errorMessage}
            </div>
          </div>
        ) : null}

        <div className="space-y-3 p-4">
          {isLoading ? (
            <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
              Loading staff directory...
            </div>
          ) : staffMembers.length === 0 ? (
            <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
              No staff records are available.
            </div>
          ) : (
            staffMembers.map((staffMember) => (
              <article
                key={`${staffMember.publicId}-${staffMember.email}`}
                className="rounded-2xl border border-slate-200 bg-white p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-semibold text-slate-900">
                      {staffMember.firstName} {staffMember.lastName}
                    </p>
                    <p className="mt-1 text-sm text-slate-500">{staffMember.publicId}</p>
                  </div>
                  <span className="rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700 ring-1 ring-blue-200">
                    {staffMember.role}
                  </span>
                </div>

                <dl className="mt-4 space-y-2 text-sm text-slate-600">
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Email</dt>
                    <dd className="truncate font-medium text-slate-800">
                      {staffMember.email ?? "No linked login"}
                    </dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Department</dt>
                    <dd className="font-medium text-slate-800">{staffMember.department}</dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Status</dt>
                    <dd className="font-medium text-slate-800">{staffMember.status}</dd>
                  </div>
                </dl>
              </article>
            ))
          )}
        </div>
      </section>

      <div className="space-y-6">
        <div className="panel-shell border-blue-100 bg-blue-50/70 p-5">
          <div className="flex items-start gap-3">
            <ShieldCheck className="mt-0.5 h-5 w-5 text-blue-700" />
            <div>
              <p className="text-sm font-semibold text-blue-900">Identity Split</p>
              <p className="mt-2 text-sm leading-6 text-blue-900/80">
                This form submits one creation payload. The backend is responsible for
                creating the clinic staff row, creating the linked `IdentityUser`, assigning
                the correct role, and forcing a password change on first login.
              </p>
            </div>
          </div>
        </div>

        {successMessage ? (
          <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
            {successMessage}
          </div>
        ) : null}

        <CreateStaffForm
          currentUserRole={user.role}
          isSubmitting={isCreating}
          onCreateStaff={handleCreateStaff}
        />
      </div>
    </div>
  );
}
