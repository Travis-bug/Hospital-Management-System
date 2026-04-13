import { Outlet } from "react-router-dom";
import TopNavbar from "../shared/TopNavbar";

export default function MainLayout() {
  return (
    <div className="min-h-screen">
      <TopNavbar />

      {/* Main application canvas. Nested layouts below this point swap between
          the wide global workspace and the patient chart workspace. */}
      <main className="mx-auto max-w-[1600px] px-4 pb-8 pt-6 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  );
}
