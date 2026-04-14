import { CalendarDays, CalendarPlus2, CircleX } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

function formatDateForApi(date) {
  return date.toISOString();
}

function createBookingForm(defaultDoctorPublicId = "") {
  const defaultDate = new Date();
  defaultDate.setHours(defaultDate.getHours() + 1, 0, 0, 0);

  return {
    patientPublicId: "",
    doctorPublicId: defaultDoctorPublicId,
    appointmentDate: defaultDate.toISOString().slice(0, 16),
    notes: "",
  };
}

export default function AppointmentsWorkspace() {
  const { user } = useAuth();
  const [staffMembers, setStaffMembers] = useState([]);
  const [patients, setPatients] = useState([]);
  const [selectedDoctorPublicId, setSelectedDoctorPublicId] = useState("");
  const [selectedDate, setSelectedDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [appointments, setAppointments] = useState([]);
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [bookingForm, setBookingForm] = useState(() => createBookingForm());
  const [isLoading, setIsLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [isBooking, setIsBooking] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const canChooseDoctor = ["Admin", "Manager", "Secretary"].includes(user?.role ?? "");
  const canBookAppointment = ["Admin", "Secretary"].includes(user?.role ?? "");
  const canCancelAppointment = ["Doctor", "Admin", "Manager", "Secretary"].includes(user?.role ?? "");

  useEffect(() => {
    let isMounted = true;

    const loadReferenceData = async () => {
      try {
        const [staffResponse, patientsResponse] = await Promise.all([
          apiClient.get("/api/Staff"),
          canBookAppointment ? apiClient.get("/api/Patient") : Promise.resolve({ data: [] }),
        ]);

        if (!isMounted) {
          return;
        }

        const doctors = staffResponse.data.filter((staffMember) => staffMember.role === "Doctor");
        setStaffMembers(doctors);

        if (canChooseDoctor) {
          setSelectedDoctorPublicId((current) => current || doctors[0]?.publicId || "");
        } else {
          setSelectedDoctorPublicId(user?.publicId ?? "");
        }

        setPatients(patientsResponse.data);
        setBookingForm((current) => ({
          ...current,
          doctorPublicId: current.doctorPublicId || doctors[0]?.publicId || user?.publicId || "",
          patientPublicId: current.patientPublicId || patientsResponse.data[0]?.patientPublicId || "",
        }));
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load appointment workspace reference data.",
          );
        }
      }
    };

    loadReferenceData();
    return () => {
      isMounted = false;
    };
  }, [canBookAppointment, canChooseDoctor, user?.publicId]);

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
        setSelectedAppointment((current) => {
          if (!current) {
            return response.data[0] ?? null;
          }

          return response.data.find((appointment) => appointment.publicId === current.publicId)
            ?? response.data[0]
            ?? null;
        });
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
    setErrorMessage("");

    try {
      const response = await apiClient.get(`/api/Appointment/${appointmentPublicId}`);
      setSelectedAppointment(response.data);
    } catch (error) {
      setErrorMessage(error?.response?.data?.detail ?? "Unable to load appointment details.");
    } finally {
      setDetailLoading(false);
    }
  };

  const handleBookingChange = (field, value) => {
    setBookingForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleBookAppointment = async (event) => {
    event.preventDefault();
    setIsBooking(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      const response = await apiClient.post("/api/Appointment/book", {
        patientPublicId: bookingForm.patientPublicId,
        doctorPublicId: bookingForm.doctorPublicId,
        appointmentDate: new Date(bookingForm.appointmentDate).toISOString(),
        notes: bookingForm.notes || null,
      });

      const appointmentDate = new Date(bookingForm.appointmentDate);
      setSelectedDoctorPublicId(bookingForm.doctorPublicId);
      setSelectedDate(appointmentDate.toISOString().slice(0, 10));
      setSelectedAppointment(response.data);
      setBookingForm(createBookingForm(bookingForm.doctorPublicId));
      setSuccessMessage(`Appointment ${response.data.publicId} booked successfully.`);
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to book the appointment.",
      );
    } finally {
      setIsBooking(false);
    }
  };

  const handleCancelAppointment = async () => {
    if (!selectedAppointment) {
      return;
    }

    if (!window.confirm(`Cancel appointment ${selectedAppointment.publicId}?`)) {
      return;
    }

    setIsCancelling(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.delete(`/api/Appointment/cancel/${selectedAppointment.publicId}`);
      setSuccessMessage(`Appointment ${selectedAppointment.publicId} cancelled.`);
      setAppointments((currentAppointments) =>
        currentAppointments.filter((appointment) => appointment.publicId !== selectedAppointment.publicId),
      );
      setSelectedAppointment(null);
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to cancel the appointment.",
      );
    } finally {
      setIsCancelling(false);
    }
  };

  return (
    <div className="space-y-6">
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
                    className={fieldClassName}
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
                  className={fieldClassName}
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

          {successMessage ? (
            <div className="px-4 pt-4">
              <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                {successMessage}
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

                {canCancelAppointment ? (
                  <button
                    type="button"
                    onClick={handleCancelAppointment}
                    disabled={isCancelling}
                    className="inline-flex items-center gap-2 rounded-2xl bg-rose-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:cursor-not-allowed disabled:bg-rose-300"
                  >
                    <CircleX className="h-4 w-4" />
                    {isCancelling ? "Cancelling..." : "Cancel Appointment"}
                  </button>
                ) : null}
              </>
            ) : (
              <div className="text-sm text-slate-500">Select an appointment from the left to inspect it.</div>
            )}
          </div>
        </section>
      </div>

      {canBookAppointment ? (
        <section className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
            <div className="flex items-center gap-3">
              <CalendarPlus2 className="h-5 w-5" />
              <div>
                <p className="section-title text-slate-300">Booking Controls</p>
                <h2 className="text-2xl font-semibold">Schedule New Appointment</h2>
              </div>
            </div>
          </div>

          <form onSubmit={handleBookAppointment} className="grid gap-4 p-6 md:grid-cols-2 xl:grid-cols-4">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Patient</span>
              <select
                value={bookingForm.patientPublicId}
                onChange={(event) => handleBookingChange("patientPublicId", event.target.value)}
                className={fieldClassName}
                required
              >
                <option value="" disabled>Select a patient</option>
                {patients.map((patient) => (
                  <option key={patient.patientPublicId} value={patient.patientPublicId}>
                    {patient.firstName} {patient.lastName} ({patient.patientPublicId})
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Doctor</span>
              <select
                value={bookingForm.doctorPublicId}
                onChange={(event) => handleBookingChange("doctorPublicId", event.target.value)}
                className={fieldClassName}
                required
              >
                <option value="" disabled>Select a doctor</option>
                {staffMembers.map((doctor) => (
                  <option key={doctor.publicId} value={doctor.publicId}>
                    {doctor.firstName} {doctor.lastName} ({doctor.publicId})
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Appointment Time</span>
              <input
                type="datetime-local"
                value={bookingForm.appointmentDate}
                onChange={(event) => handleBookingChange("appointmentDate", event.target.value)}
                className={fieldClassName}
                required
              />
            </label>

            <label className="space-y-2 xl:col-span-1">
              <span className="text-sm font-semibold text-slate-700">Notes</span>
              <textarea
                value={bookingForm.notes}
                onChange={(event) => handleBookingChange("notes", event.target.value)}
                className={`${fieldClassName} min-h-[52px] resize-y`}
                placeholder="Reason for visit, prep notes, or scheduling comments"
              />
            </label>

            <div className="md:col-span-2 xl:col-span-4">
              <button
                type="submit"
                disabled={isBooking}
                className="rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
              >
                {isBooking ? "Booking..." : "Book Appointment"}
              </button>
            </div>
          </form>
        </section>
      ) : null}
    </div>
  );
}
