import { CheckCircle2, KeyRound, LockKeyhole, Mail } from "lucide-react";
import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

export default function ActivateAccount() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { completeOnboarding } = useAuth();

  const email = searchParams.get("email") ?? "";
  const emailToken = searchParams.get("emailToken") ?? "";
  const passwordToken = searchParams.get("passwordToken") ?? "";

  const hasRequiredTokens = useMemo(
    () => Boolean(email && emailToken && passwordToken),
    [email, emailToken, passwordToken],
  );

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const handleSubmit = async (event) => {
    event.preventDefault();
    setErrorMessage("");

    if (!hasRequiredTokens) {
      setErrorMessage("Activation link is missing required token data.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setErrorMessage("New password and confirmation do not match.");
      return;
    }

    setIsSubmitting(true);

    try {
      await completeOnboarding({
        email,
        emailConfirmationToken: emailToken,
        passwordResetToken: passwordToken,
        newPassword,
      });

      setSuccessMessage("Account activated. Sign in with your new password.");
      setTimeout(() => navigate("/login", { replace: true }), 1200);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top_right,rgba(30,58,138,0.16),transparent_22rem),linear-gradient(180deg,#f8fafc_0%,#e2e8f0_100%)]">
      <div className="mx-auto flex min-h-screen max-w-7xl items-center justify-center px-4 py-8 lg:px-8">
        <section className="panel-shell w-full max-w-2xl overflow-hidden">
          <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
            <p className="section-title text-slate-300">Account Activation</p>
            <h2 className="mt-2 text-2xl font-semibold">Set your permanent password</h2>
            <p className="mt-3 text-sm leading-6 text-slate-300">
              This step confirms the email address for the provisioned staff account and
              replaces the temporary password on the existing Identity user.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6 p-6">
            {errorMessage ? (
              <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {errorMessage}
              </div>
            ) : null}

            {successMessage ? (
              <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                {successMessage}
              </div>
            ) : null}

            <div className="grid gap-5 md:grid-cols-2">
              <label className="space-y-2 md:col-span-2">
                <span className="text-sm font-semibold text-slate-700">Provisioned Email</span>
                <div className="relative">
                  <Mail className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <input
                    type="email"
                    value={email}
                    readOnly
                    className={`${fieldClassName} pl-11 text-slate-500`}
                  />
                </div>
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">New Password</span>
                <div className="relative">
                  <LockKeyhole className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <input
                    type="password"
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    className={`${fieldClassName} pl-11`}
                    placeholder="Enter your permanent password"
                    autoComplete="new-password"
                    disabled={!hasRequiredTokens || isSubmitting}
                    required
                  />
                </div>
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Confirm Password</span>
                <div className="relative">
                  <KeyRound className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <input
                    type="password"
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                    className={`${fieldClassName} pl-11`}
                    placeholder="Confirm your permanent password"
                    autoComplete="new-password"
                    disabled={!hasRequiredTokens || isSubmitting}
                    required
                  />
                </div>
              </label>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
              <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                <CheckCircle2 className="h-4 w-4 text-blue-700" />
                Activation Outcome
              </div>
              <ul className="mt-4 space-y-2 text-sm leading-6 text-slate-600">
                <li>Email confirmation is completed on the existing staff-linked account.</li>
                <li>The temporary password is replaced and rehashed through ASP.NET Identity.</li>
                <li>After activation, sign in normally and complete email 2FA when prompted.</li>
              </ul>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <button
                type="submit"
                disabled={!hasRequiredTokens || isSubmitting}
                className="rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
              >
                {isSubmitting ? "Activating..." : "Activate Account"}
              </button>

              <Link
                to="/login"
                className="text-sm font-medium text-slate-600 transition hover:text-slate-900"
              >
                Back to sign-in
              </Link>
            </div>
          </form>
        </section>
      </div>
    </div>
  );
}
