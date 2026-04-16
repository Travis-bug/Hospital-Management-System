import { createBrowserRouter, Navigate, Outlet, RouterProvider, useLocation } from "react-router-dom";
import GlobalSidebar from "./components/layout/GlobalSidebar";
import MainLayout from "./components/layout/MainLayout";
import AppRouteError from "./pages/AppRouteError";
import { useAuth } from "./contexts/AuthContext";
import Login from "./pages/Login";
import NotFound from "./pages/NotFound";
import StaffProvisioning from "./pages/admin/StaffProvisioning";
import AppointmentsWorkspace from "./pages/global/AppointmentsWorkspace";
import PatientList from "./pages/global/PatientList";
import ScheduleWorkspace from "./pages/global/ScheduleWorkspace";
import TestResultsWorkspace from "./pages/global/TestResultsWorkspace";
import VisitsWorkspace from "./pages/global/VisitsWorkspace";
import ChartLayout from "./pages/patient-chart/ChartLayout";
import ContextualVisits from "./pages/patient-chart/ContextualVisits";
import PatientOverview from "./pages/patient-chart/PatientOverview";
import PatientPrescriptions from "./pages/patient-chart/PatientPrescriptions";
import PatientTestResults from "./pages/patient-chart/PatientTestResults";
import PatientVitals from "./pages/patient-chart/PatientVitals";

function AuthBootstrapScreen() {
  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <section className="panel-shell w-full max-w-lg p-10 text-center">
        <p className="section-title">Session Bootstrap</p>
        <h2 className="mt-3 text-2xl font-semibold text-slate-950">Checking secure session</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          The frontend is asking the ASP.NET Identity backend whether an authenticated
          cookie-backed session already exists.
        </p>
      </section>
    </div>
  );
}

function RequireAuth() {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <AuthBootstrapScreen />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}

function RequireGuest() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <AuthBootstrapScreen />;
  }

  if (isAuthenticated) {
    return <Navigate to="/patients" replace />;
  }

  return <Outlet />;
}

function GlobalModeShell() {
  return (
    <div className="flex flex-col gap-6 lg:flex-row">
      <GlobalSidebar />
      <section className="min-w-0 flex-1">
        <Outlet />
      </section>
    </div>
  );
}

const router = createBrowserRouter([
  {
    errorElement: <AppRouteError />,
    element: <RequireGuest />,
    children: [
      {
        path: "/login",
        element: <Login />,
      },
      {
        path: "*",
        element: <Navigate to="/login" replace />,
      },
    ],
  },
  {
    errorElement: <AppRouteError />,
    element: <RequireAuth />,
    children: [
      {
        path: "/",
        element: <MainLayout />,
        errorElement: <AppRouteError />,
        children: [
          {
            index: true,
            element: <Navigate to="/patients" replace />,
          },
          {
            element: <GlobalModeShell />,
            children: [
              {
                path: "patients",
                element: <PatientList />,
              },
              {
                path: "visits",
                element: <VisitsWorkspace />,
              },
              {
                path: "appointments",
                element: <AppointmentsWorkspace />,
              },
              {
                path: "schedule",
                element: <ScheduleWorkspace />,
              },
              {
                path: "tests",
                element: <TestResultsWorkspace />,
              },
              {
                path: "staff-management",
                element: <StaffProvisioning />,
              },
              {
                path: "*",
                element: <NotFound />,
              },
            ],
          },
          {
            path: "patients/:patientId",
            element: <ChartLayout />,
            errorElement: <AppRouteError />,
            children: [
              {
                index: true,
                element: <Navigate to="overview" replace />,
              },
              {
                path: "overview",
                element: <PatientOverview />,
              },
              {
                path: "vitals",
                element: <PatientVitals />,
              },
              {
                path: "prescriptions",
                element: <PatientPrescriptions />,
              },
              {
                path: "visits",
                element: <ContextualVisits />,
              },
              {
                path: "tests",
                element: <PatientTestResults />,
              },
            ],
          },
          {
            path: "*",
            element: <NotFound />,
          },
        ],
      },
    ],
  },
]);

export default function App() {
  return <RouterProvider router={router} />;
}
