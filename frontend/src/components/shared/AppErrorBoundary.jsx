import React from "react";
import { Link } from "react-router-dom";

function ErrorShell({ title, message, showHomeLink = true }) {
  return (
    <div className="flex min-h-screen items-center justify-center px-4 py-10">
      <section className="panel-shell w-full max-w-2xl p-10">
        <p className="section-title">Application Error</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-950">{title}</h1>
        <p className="mt-4 text-sm leading-7 text-slate-600">{message}</p>

        <div className="mt-8 flex flex-wrap gap-3">
          {showHomeLink ? (
            <Link
              to="/patients"
              className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white no-underline transition hover:bg-slate-800"
            >
              Return to dashboard
            </Link>
          ) : null}

          <button
            type="button"
            onClick={() => window.location.reload()}
            className="rounded-full border border-slate-300 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:text-slate-950"
          >
            Reload application
          </button>
        </div>
      </section>
    </div>
  );
}

export function AppErrorFallback({ title, message, showHomeLink }) {
  return <ErrorShell title={title} message={message} showHomeLink={showHomeLink} />;
}

export default class AppErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    console.error("Unhandled React error", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <ErrorShell
          title="The dashboard hit an unexpected runtime error."
          message="A frontend component failed while rendering. Reload the application, and if the problem persists, check the latest API response or application logs."
          showHomeLink
        />
      );
    }

    return this.props.children;
  }
}
