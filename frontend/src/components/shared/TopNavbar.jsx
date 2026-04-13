import { Bell, ChevronDown, KeyRound, LogOut, ShieldCheck, UserRound } from "lucide-react";
import { useState } from "react";
import { useAuth } from "../../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

export default function TopNavbar() {
  const { user, logout, changePassword } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const initials = user?.name
    ?.split(" ")
    .map((segment) => segment[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  const handlePasswordSubmit = async (event) => {
    event.preventDefault();
    setErrorMessage("");
    setStatusMessage("");
    setIsSubmitting(true);

    try {
      await changePassword(currentPassword, newPassword);
      setStatusMessage("Password updated.");
      setCurrentPassword("");
      setNewPassword("");
      setShowPasswordForm(false);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <header className="sticky top-0 z-30 border-b border-blue-950/50 bg-slate-900 text-white shadow-lg">
      <div className="mx-auto flex max-w-[1600px] items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
        <div>
          <p className="section-title text-slate-300">Hospital Management System</p>
          <h1 className="mt-1 text-xl font-semibold tracking-tight">
            Welcome, {user?.role ?? "Guest"} {user?.name ?? ""}
          </h1>
        </div>

        <div className="flex items-center gap-3">
          <button
            type="button"
            className="rounded-2xl border border-white/10 bg-white/5 p-3 transition hover:bg-white/10"
            aria-label="Notifications"
          >
            <Bell className="h-5 w-5" />
          </button>

          <div className="relative">
            <button
              type="button"
              onClick={() => setMenuOpen((current) => !current)}
              className="flex items-center gap-3 rounded-2xl border border-white/10 bg-white/5 px-3 py-2 text-left transition hover:bg-white/10"
            >
              <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-blue-700 font-semibold">
                {initials || "SM"}
              </div>
              <div className="hidden text-sm sm:block">
                <p className="font-medium">{user?.name ?? "No active user"}</p>
                <p className="text-slate-300">{user?.email ?? "Awaiting session"}</p>
              </div>
              <ChevronDown className="h-4 w-4 text-slate-300" />
            </button>

            {menuOpen ? (
              <div className="absolute right-0 mt-3 w-[22rem] rounded-2xl border border-slate-200 bg-white p-2 text-slate-800 shadow-panel">
                <div className="rounded-xl bg-slate-50 p-3">
                  <div className="flex items-center gap-2 text-sm font-medium text-slate-900">
                    <UserRound className="h-4 w-4 text-blue-700" />
                    Staff Profile
                  </div>
                  <dl className="mt-3 space-y-2 text-sm text-slate-600">
                    <div className="flex items-center justify-between gap-3">
                      <dt className="text-slate-400">Name</dt>
                      <dd className="font-medium text-slate-800">{user?.name ?? "Unknown"}</dd>
                    </div>
                    <div className="flex items-center justify-between gap-3">
                      <dt className="text-slate-400">Role</dt>
                      <dd className="font-medium text-slate-800">{user?.role ?? "Unknown"}</dd>
                    </div>
                    <div className="flex items-center justify-between gap-3">
                      <dt className="text-slate-400">Public ID</dt>
                      <dd className="font-medium text-slate-800">{user?.publicId ?? "Unlinked"}</dd>
                    </div>
                  </dl>
                </div>

                <button
                  type="button"
                  onClick={() => {
                    setShowPasswordForm((current) => !current);
                    setErrorMessage("");
                    setStatusMessage("");
                  }}
                  className="mt-2 flex w-full items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100"
                >
                  <KeyRound className="h-4 w-4" />
                  Change Password
                </button>

                {showPasswordForm ? (
                  <form onSubmit={handlePasswordSubmit} className="mt-2 space-y-3 rounded-xl border border-slate-200 bg-slate-50 p-3">
                    {errorMessage ? (
                      <div className="rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                        {errorMessage}
                      </div>
                    ) : null}

                    {statusMessage ? (
                      <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
                        {statusMessage}
                      </div>
                    ) : null}

                    <label className="block space-y-1">
                      <span className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                        Current Password
                      </span>
                      <input
                        type="password"
                        value={currentPassword}
                        onChange={(event) => setCurrentPassword(event.target.value)}
                        className={fieldClassName}
                        required
                      />
                    </label>

                    <label className="block space-y-1">
                      <span className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                        New Password
                      </span>
                      <input
                        type="password"
                        value={newPassword}
                        onChange={(event) => setNewPassword(event.target.value)}
                        className={fieldClassName}
                        required
                      />
                    </label>

                    <button
                      type="submit"
                      disabled={isSubmitting}
                      className="flex w-full items-center justify-center gap-2 rounded-xl bg-blue-900 px-3 py-2 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
                    >
                      <ShieldCheck className="h-4 w-4" />
                      {isSubmitting ? "Saving..." : "Update Password"}
                    </button>
                  </form>
                ) : null}

                <button
                  type="button"
                  onClick={() => {
                    logout();
                    setMenuOpen(false);
                  }}
                  className="mt-2 flex w-full items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100"
                >
                  <LogOut className="h-4 w-4" />
                  Logout
                </button>
              </div>
            ) : null}
          </div>
        </div>
      </div>
    </header>
  );
}
