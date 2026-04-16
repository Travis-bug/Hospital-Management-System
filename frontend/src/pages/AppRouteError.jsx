import { isRouteErrorResponse, useRouteError } from "react-router-dom";
import { AppErrorFallback } from "../components/shared/AppErrorBoundary";

export default function AppRouteError() {
  const error = useRouteError();

  if (isRouteErrorResponse(error)) {
    return (
      <AppErrorFallback
        title={error.status === 404 ? "The requested page was not found." : "The application could not complete this request."}
        message={typeof error.data === "string" ? error.data : error.data?.message ?? error.statusText}
        showHomeLink={error.status !== 404}
      />
    );
  }

  return (
    <AppErrorFallback
      title="The application hit an unexpected route error."
      message={error instanceof Error ? error.message : "An unexpected route-level failure occurred while loading this view."}
      showHomeLink
    />
  );
}
