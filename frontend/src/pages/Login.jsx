import { KeyRound, LockKeyhole, Mail, ShieldCheck } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

export default function Login() {
  const navigate = useNavigate();
  const { loginStep1, loginStep2 } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [code, setCode] = useState("");
  const [rememberMachine, setRememberMachine] = useState(false);
  const [is2FAState, setIs2FAState] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const handlePrimaryLogin = async (event) => {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      const result = await loginStep1(email, password);

      if (result.requiresTwoFactor) {
        setIs2FAState(true);
        setPassword("");
        return;
      }

      navigate("/patients", { replace: true });
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleTwoFactorLogin = async (event) => {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    try {
      await loginStep2(email, code, rememberMachine);
      navigate("/patients", { replace: true });
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top_right,rgba(30,58,138,0.16),transparent_22rem),linear-gradient(180deg,#f8fafc_0%,#e2e8f0_100%)]">
      <div className="mx-auto grid min-h-screen max-w-7xl gap-8 px-4 py-8 lg:grid-cols-[1.05fr_0.95fr] lg:px-8">
        <section className="hidden rounded-[2rem] bg-slate-950 p-10 text-white shadow-panel lg:flex lg:flex-col lg:justify-between">
          <div>
            <p className="section-title text-slate-300">Hospital Management System</p>
            <h1 className="mt-4 max-w-xl text-4xl font-semibold tracking-tight">
              Secure clinical access for internal staff only
            </h1>
            <p className="mt-5 max-w-xl text-sm leading-7 text-slate-300">
              This login flow uses ASP.NET Identity cookies, role-based provisioning,
              email verification, and optional two-factor authentication. Patients are
              excluded because they do not receive platform accounts.
            </p>
          </div>

          <div className="grid gap-4">
            <article className="rounded-2xl border border-white/10 bg-white/5 p-5">
              <div className="flex items-center gap-2 text-sm font-semibold">
                <ShieldCheck className="h-4 w-4 text-blue-300" />
                Internal Access Policy
              </div>
              <p className="mt-3 text-sm leading-6 text-slate-300">
                No public sign-up. Staff identities are provisioned by Admin or Manager
                workflows and linked to clinic-side staff records.
              </p>
            </article>
          </div>
        </section>

        <section className="flex items-center justify-center">
          <div className="panel-shell w-full max-w-xl overflow-hidden">
            <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
              <p className="section-title text-slate-300">Secure Sign-In</p>
              <h2 className="mt-2 text-2xl font-semibold">
                {is2FAState ? "Verify authentication code" : "Sign in to the clinical dashboard"}
              </h2>
              <p className="mt-3 text-sm leading-6 text-slate-300">
                {is2FAState
                  ? "Your password has been accepted. Enter the email-delivered authentication code to complete sign-in."
                  : "Use your provisioned staff credentials. The backend issues an encrypted HTTP-only cookie session on success."}
              </p>
            </div>

            <form
              onSubmit={is2FAState ? handleTwoFactorLogin : handlePrimaryLogin}
              className="space-y-6 p-6"
            >
              {errorMessage ? (
                <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                  {errorMessage}
                </div>
              ) : null}

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Email</span>
                <div className="relative">
                  <Mail className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                  <input
                    type="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    className={`${fieldClassName} pl-11`}
                    placeholder="doctor@hospital.com"
                    autoComplete="username"
                    disabled={is2FAState}
                    required
                  />
                </div>
              </label>

              {!is2FAState ? (
                <label className="space-y-2">
                  <span className="text-sm font-semibold text-slate-700">Password</span>
                  <div className="relative">
                    <LockKeyhole className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                    <input
                      type="password"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      className={`${fieldClassName} pl-11`}
                      placeholder="Enter your password"
                      autoComplete="current-password"
                      required
                    />
                  </div>
                </label>
              ) : (
                <div className="space-y-5">
                  <label className="space-y-2">
                    <span className="text-sm font-semibold text-slate-700">Authentication Code</span>
                    <div className="relative">
                      <KeyRound className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                      <input
                        type="text"
                        value={code}
                        onChange={(event) => setCode(event.target.value)}
                        className={`${fieldClassName} pl-11`}
                        placeholder="Enter your 2FA code"
                        autoComplete="one-time-code"
                        required
                      />
                    </div>
                  </label>

                  <label className="flex items-center gap-3 text-sm text-slate-600">
                    <input
                      type="checkbox"
                      checked={rememberMachine}
                      onChange={(event) => setRememberMachine(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-300 text-blue-700 focus:ring-blue-500"
                    />
                    Remember this machine
                  </label>

                  <button
                    type="button"
                    onClick={() => {
                      setIs2FAState(false);
                      setCode("");
                      setRememberMachine(false);
                      setErrorMessage("");
                    }}
                    className="text-sm font-medium text-slate-600 transition hover:text-slate-900"
                  >
                    Back to password sign-in
                  </button>
                </div>
              )}

              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
              >
                {isSubmitting
                  ? "Submitting..."
                  : is2FAState
                    ? "Verify and Sign In"
                    : "Sign In"}
              </button>
            </form>
          </div>
        </section>
      </div>
    </div>
  );
}

