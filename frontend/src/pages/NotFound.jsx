import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div className="flex min-h-[70vh] items-center justify-center px-4 py-10">
      <section className="panel-shell w-full max-w-2xl p-10">
        <p className="section-title">Not Found</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-950">The requested workspace does not exist.</h1>
        <p className="mt-4 text-sm leading-7 text-slate-600">
          The route loaded successfully, but no screen matches the current path. Use the dashboard navigation
          to return to a supported area of the application.
        </p>

        <div className="mt-8">
          <Link
            to="/patients"
            className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white no-underline transition hover:bg-slate-800"
          >
            Return to dashboard
          </Link>
        </div>
      </section>
    </div>
  );
}
