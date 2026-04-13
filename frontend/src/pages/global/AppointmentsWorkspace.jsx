import { CalendarDays } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

function formatDateForApi(date) {
  return date.toISOString();
}

export default function AppointmentsWorkspace() {
  const { user } = useAuth();
  const [staffMembers, setStaffMembers] = useState([]);
  const [selectedDoctorPublicId, setSelectedDoctorPublicId] = useState("");
  const [selectedDate, setSelectedDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [appointments, setAppointments] = useState([]);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const canChooseDoctor = ["Admin", "Manager", "Secretary"].includes(user?.role ?? "");

  useEffect(() => {
    let isMounted = true;

    const loadDoctors = async () => {
      if (!canChooseDoctor) {
        setSelectedDoctorPublicId(user?.publicId ?? "");
        return;
      }

      try {
        const response = await apiClient.get("/api/Staff");
        if (!isMounted) {
          return;
        }

        const doctors = response.data.filter((staffMember) => staffMember.role === "Doctor");
        setStaffMembers(doctors);
        setSelectedDoctorPublicId(doctors[0]?.publicId ?? "");
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load the doctor directory.");
        }
      }
    };

    loadDoctors();
    return () => {
      isMounted = false;
    };
  }, [canChooseDoctor, user?.publicId]);

  useEffect(() => {
    let isMounted = true;

    const loadAppointments = async () => {
      if (!selectedDoctorPublicId || !selectedDate) {
        setAppointments([]);
        setSelectedAppointment(null);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);

      try {
        const response = await apiClient.get("/api/Appointment/doctor-schedule", {
          params: {
            doctorPublicId: selectedDoctorPublicId,
            date: formatDateForApi(new Date(`${selectedDate}T00:00:00`)),
          },
        });

        if (!isMounted) {
          return;
        }

        setAppointments(response.data);
        setSelectedAppointment(response.data[0] ?? null);
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load appointments.");
          setAppointments([]);
          setSelectedAppointment(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadAppointments();
    return () => {
      isMounted = false;
    };
  }, [selectedDoctorPublicId, selectedDate]);

  const selectedDoctor = useMemo(
    () => staffMembers.find((staffMember) => staffMember.publicId === selectedDoctorPublicId) ?? null,
    [staffMembers, selectedDoctorPublicId],
  );

  const handleSelectAppointment = async (appointmentPublicId) => {
    setDetailLoading(true);

    try {
      const response = await apiClient.get(`/api/Appointment/${appointmentPublicId}`);
      setSelectedAppointment(response.data);
    } catch (error) {
      setErrorMessage(error?.response?.data?.detail ?? "Unable to load appointment details.");
    } finally {
      setDetailLoading(false);
    }
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <p className="section-title">Appointments</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-950">Daily Schedule</h2>
          <div className="mt-4 grid gap-4">
            {canChooseDoctor ? (
              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Doctor</span>
                <select
                  value={selectedDoctorPublicId}
                  onChange={(event) => setSelectedDoctorPublicId(event.target.value)}
                  className="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
                >
                  {staffMembers.map((doctor) => (
                    <option key={doctor.publicId} value={doctor.publicId}>
                      {doctor.firstName} {doctor.lastName} ({doctor.publicId})
                    </option>
                  ))}
                </select>
              </label>
            ) : (
              <p className="text-sm text-slate-500">Schedule scope: {user?.publicId ?? "Current doctor"}</p>
            )}

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Date</span>
              <input
                type="date"
                value={selectedDate}
                onChange={(event) => setSelectedDate(event.target.value)}
                className="w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100"
              />
            </label>
          </div>
        </div>

        {errorMessage ? (
          <div className="p-4">
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
              {errorMessage}
            </div>
          </div>
        ) : null}

        {isLoading ? (
          <div className="p-6 text-sm text-slate-500">Loading appointments...</div>
        ) : (
          <div className="space-y-3 p-4">
            {appointments.length === 0 ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                No booked or arrived appointments were returned for the selected day.
              </div>
            ) : (
              appointments.map((appointment) => (
                <button
                  key={appointment.publicId}
                  type="button"
                  onClick={() => handleSelectAppointment(appointment.publicId)}
                  className={[
                    "w-full rounded-2xl border px-4 py-4 text-left transition",
                    selectedAppointment?.publicId === appointment.publicId
                      ? "border-blue-200 bg-blue-50"
                      : "border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50",
                  ].join(" ")}
                >
                  <p className="font-semibold text-slate-900">{appointment.publicId}</p>
                  <p className="mt-1 text-sm text-slate-500">
                    {new Date(appointment.appointmentDate).toLocaleString()}
                  </p>
                  {appointment.patient ? (
                    <p className="mt-2 text-sm text-slate-700">
                      {appointment.patient.firstName} {appointment.patient.lastName} • {appointment.patient.publicId}
                    </p>
                  ) : null}
                </button>
              ))
            )}
          </div>
        )}
      </section>

      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
          <div className="flex items-center gap-3">
            <CalendarDays className="h-5 w-5" />
            <div>
              <p className="section-title text-slate-300">Appointment Detail</p>
              <h2 className="mt-2 text-2xl font-semibold">
                {selectedAppointment?.publicId ?? "Select an appointment"}
              </h2>
            </div>
          </div>
          <p className="mt-3 text-sm text-slate-300">
            {selectedDoctor ? `${selectedDoctor.firstName} ${selectedDoctor.lastName}` : user?.name ?? "Current clinician"}
          </p>
        </div>

        <div className="space-y-4 p-6">
          {detailLoading ? (
            <div className="text-sm text-slate-500">Loading appointment detail...</div>
          ) : selectedAppointment ? (
            <>
              <div className="grid gap-4 md:grid-cols-2">
                <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4">
                  <p className="text-sm font-semibold text-slate-500">Status</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">{selectedAppointment.status}</p>
                </article>
                <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4">
                  <p className="text-sm font-semibold text-slate-500">Booked At</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">
                    {selectedAppointment.bookedAt ? new Date(selectedAppointment.bookedAt).toLocaleString() : "Unknown"}
                  </p>
                </article>
              </div>
              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Appointment Date</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">
                  {new Date(selectedAppointment.appointmentDate).toLocaleString()}
                </p>
              </article>
              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Notes</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedAppointment.notes ?? "No notes recorded."}</p>
              </article>
            </>
          ) : (
            <div className="text-sm text-slate-500">Select an appointment from the left to inspect it.</div>
          )}
        </div>
      </section>
    </div>
  );
}
