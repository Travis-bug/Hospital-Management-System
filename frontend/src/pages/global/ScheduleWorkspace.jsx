import { CalendarRange, Clock3, ShieldCheck, Users } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

function formatTime(value) {
  return value ? value.slice(0, 5) : "--:--";
}

function formatDateRange(daysFromToday) {
  const date = new Date();
  date.setDate(date.getDate() + daysFromToday);
  return date.toISOString().slice(0, 10);
}

export default function ScheduleWorkspace() {
  const { user } = useAuth();
  const [myShifts, setMyShifts] = useState([]);
  const [roster, setRoster] = useState([]);
  const [staffMembers, setStaffMembers] = useState([]);
  const [shiftRules, setShiftRules] = useState([]);
  const [isLoadingShifts, setIsLoadingShifts] = useState(true);
  const [isLoadingRoster, setIsLoadingRoster] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [range, setRange] = useState({
    startDate: formatDateRange(0),
    endDate: formatDateRange(7),
  });
  const [rosterDate, setRosterDate] = useState(formatDateRange(0));
  const [scheduleForm, setScheduleForm] = useState({
    shiftDate: formatDateRange(0),
    shiftRulePublicId: "",
    staffType: "Doctor",
    staffPublicId: "",
  });

  const canManageRoster = ["Manager", "Admin"].includes(user?.role ?? "");
  const canViewOwnShifts = ["Doctor", "Nurse", "Secretary", "Admin"].includes(user?.role ?? "");

  const assignableStaff = useMemo(
    () => staffMembers.filter((staffMember) => staffMember.role === scheduleForm.staffType),
    [scheduleForm.staffType, staffMembers],
  );

  useEffect(() => {
    let isMounted = true;

    const loadMyShifts = async () => {
      if (!canViewOwnShifts) {
        setMyShifts([]);
        setIsLoadingShifts(false);
        return;
      }

      setIsLoadingShifts(true);

      try {
        const response = await apiClient.get("/api/Scheduling/my-shifts", {
          params: range,
        });

        if (isMounted) {
          setMyShifts(response.data);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load your scheduled shifts.",
          );
        }
      } finally {
        if (isMounted) {
          setIsLoadingShifts(false);
        }
      }
    };

    loadMyShifts();
    return () => {
      isMounted = false;
    };
  }, [canViewOwnShifts, range]);

  useEffect(() => {
    let isMounted = true;

    const loadRosterData = async () => {
      if (!canManageRoster) {
        setRoster([]);
        setIsLoadingRoster(false);
        return;
      }

      setIsLoadingRoster(true);

      try {
        const [rosterResponse, shiftRulesResponse, staffResponse] = await Promise.all([
          apiClient.get("/api/Scheduling/daily-roster", {
            params: {
              date: new Date(`${rosterDate}T00:00:00`).toISOString(),
            },
          }),
          apiClient.get("/api/Scheduling/shift-rules"),
          apiClient.get("/api/Staff"),
        ]);

        if (!isMounted) {
          return;
        }

        setRoster(rosterResponse.data);
        setShiftRules(shiftRulesResponse.data);
        setStaffMembers(staffResponse.data.filter((staffMember) => staffMember.role !== "Manager"));
        setScheduleForm((current) => ({
          ...current,
          shiftRulePublicId: current.shiftRulePublicId || shiftRulesResponse.data[0]?.publicId || "",
          staffPublicId:
            current.staffPublicId
            || staffResponse.data.find((staffMember) => staffMember.role === current.staffType)?.publicId
            || "",
        }));
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load roster management data.",
          );
        }
      } finally {
        if (isMounted) {
          setIsLoadingRoster(false);
        }
      }
    };

    loadRosterData();
    return () => {
      isMounted = false;
    };
  }, [canManageRoster, rosterDate]);

  useEffect(() => {
    if (!assignableStaff.length) {
      return;
    }

    setScheduleForm((current) => ({
      ...current,
      staffPublicId: assignableStaff.some((staffMember) => staffMember.publicId === current.staffPublicId)
        ? current.staffPublicId
        : assignableStaff[0].publicId,
    }));
  }, [assignableStaff]);

  const handleRangeChange = (field, value) => {
    setRange((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleScheduleChange = (field, value) => {
    setScheduleForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleAssignShift = async (event) => {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      const response = await apiClient.post("/api/Scheduling/schedule-staff", null, {
        params: {
          shiftDate: new Date(`${scheduleForm.shiftDate}T00:00:00`).toISOString(),
          shiftRulePublicId: scheduleForm.shiftRulePublicId,
          staffPublicId: scheduleForm.staffPublicId,
          staffType: scheduleForm.staffType,
        },
      });

      setSuccessMessage(`Scheduled shift ${response.data.shiftId ?? response.data.ShiftId}.`);
      setRosterDate(scheduleForm.shiftDate);
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to schedule the staff member.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelShift = async (shiftPublicId) => {
    if (!window.confirm(`Cancel shift ${shiftPublicId}?`)) {
      return;
    }

    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.delete(`/api/Scheduling/cancel-shift/${shiftPublicId}`);
      setRoster((current) => current.filter((shift) => shift.shiftPublicId !== shiftPublicId));
      setSuccessMessage(`Shift ${shiftPublicId} cancelled.`);
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to cancel the shift.",
      );
    }
  };

  return (
    <div className="space-y-6">
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

      <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
        <section className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 px-6 py-5">
            <div className="flex items-center gap-3">
              <CalendarRange className="h-5 w-5 text-blue-700" />
              <div>
                <p className="section-title">My Schedule</p>
                <h2 className="text-2xl font-semibold text-slate-950">Shift Range</h2>
              </div>
            </div>
            <div className="mt-4 grid gap-4">
              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Start Date</span>
                <input
                  type="date"
                  value={range.startDate}
                  onChange={(event) => handleRangeChange("startDate", event.target.value)}
                  className={fieldClassName}
                />
              </label>
              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">End Date</span>
                <input
                  type="date"
                  value={range.endDate}
                  onChange={(event) => handleRangeChange("endDate", event.target.value)}
                  className={fieldClassName}
                />
              </label>
            </div>
          </div>

          <div className="space-y-3 p-4">
            {!canViewOwnShifts ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                The backend does not expose personal-shift retrieval for the current role.
              </div>
            ) : isLoadingShifts ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                Loading personal shifts...
              </div>
            ) : myShifts.length === 0 ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                No shifts were returned for the selected range.
              </div>
            ) : (
              myShifts.map((shift) => (
                <article key={`${shift.date}-${shift.shiftType}-${shift.startTime}`} className="rounded-2xl border border-slate-200 bg-white p-4">
                  <p className="font-semibold text-slate-900">{shift.shiftType}</p>
                  <p className="mt-1 text-sm text-slate-500">{shift.date}</p>
                  <p className="mt-3 text-sm text-slate-700">
                    {formatTime(shift.startTime)} - {formatTime(shift.endTime)}
                  </p>
                </article>
              ))
            )}
          </div>
        </section>

        <section className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
            <div className="flex items-center gap-3">
              <Clock3 className="h-5 w-5" />
              <div>
                <p className="section-title text-slate-300">Daily Roster</p>
                <h2 className="text-2xl font-semibold">Hospital Staffing</h2>
              </div>
            </div>
          </div>

          <div className="space-y-4 p-6">
            {!canManageRoster ? (
              <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                Daily roster management is restricted to Admin and Manager sessions.
              </div>
            ) : (
              <>
                <label className="block space-y-2">
                  <span className="text-sm font-semibold text-slate-700">Roster Date</span>
                  <input
                    type="date"
                    value={rosterDate}
                    onChange={(event) => setRosterDate(event.target.value)}
                    className={fieldClassName}
                  />
                </label>

                {isLoadingRoster ? (
                  <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                    Loading daily roster...
                  </div>
                ) : roster.length === 0 ? (
                  <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
                    No staff are scheduled for the selected date.
                  </div>
                ) : (
                  roster.map((entry) => (
                    <article key={entry.shiftPublicId} className="rounded-2xl border border-slate-200 bg-white p-4">
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <p className="font-semibold text-slate-900">{entry.staffName}</p>
                          <p className="mt-1 text-sm text-slate-500">{entry.role}</p>
                        </div>
                        <button
                          type="button"
                          onClick={() => handleCancelShift(entry.shiftPublicId)}
                          className="rounded-xl border border-rose-200 px-3 py-2 text-xs font-semibold text-rose-700 transition hover:bg-rose-50"
                        >
                          Cancel Shift
                        </button>
                      </div>
                      <p className="mt-3 text-sm text-slate-700">
                        {entry.shiftType} • {formatTime(entry.startTime)} - {formatTime(entry.endTime)}
                      </p>
                    </article>
                  ))
                )}
              </>
            )}
          </div>
        </section>
      </div>

      {canManageRoster ? (
        <section className="panel-shell overflow-hidden">
          <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
            <div className="flex items-center gap-3">
              <ShieldCheck className="h-5 w-5" />
              <div>
                <p className="section-title text-slate-300">Scheduling Controls</p>
                <h2 className="text-2xl font-semibold">Assign Staff to Shift</h2>
              </div>
            </div>
          </div>

          <form onSubmit={handleAssignShift} className="grid gap-4 p-6 md:grid-cols-2 xl:grid-cols-4">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Shift Date</span>
              <input
                type="date"
                value={scheduleForm.shiftDate}
                onChange={(event) => handleScheduleChange("shiftDate", event.target.value)}
                className={fieldClassName}
                required
              />
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Staff Type</span>
              <select
                value={scheduleForm.staffType}
                onChange={(event) => handleScheduleChange("staffType", event.target.value)}
                className={fieldClassName}
              >
                {["Doctor", "Nurse", "Secretary", "Admin"].map((role) => (
                  <option key={role} value={role}>{role}</option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Staff Member</span>
              <select
                value={scheduleForm.staffPublicId}
                onChange={(event) => handleScheduleChange("staffPublicId", event.target.value)}
                className={fieldClassName}
                required
              >
                <option value="" disabled>Select a staff member</option>
                {assignableStaff.map((staffMember) => (
                  <option key={staffMember.publicId} value={staffMember.publicId}>
                    {staffMember.firstName} {staffMember.lastName} ({staffMember.publicId})
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-slate-700">Shift Block</span>
              <select
                value={scheduleForm.shiftRulePublicId}
                onChange={(event) => handleScheduleChange("shiftRulePublicId", event.target.value)}
                className={fieldClassName}
                required
              >
                <option value="" disabled>Select a shift block</option>
                {shiftRules.map((rule) => (
                  <option key={rule.publicId} value={rule.publicId}>
                    {rule.shiftType} ({formatTime(rule.startTime)} - {formatTime(rule.endTime)})
                  </option>
                ))}
              </select>
            </label>

            <div className="xl:col-span-4">
              <button
                type="submit"
                disabled={isSubmitting}
                className="inline-flex items-center gap-2 rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
              >
                <Users className="h-4 w-4" />
                {isSubmitting ? "Scheduling..." : "Assign Shift"}
              </button>
            </div>
          </form>
        </section>
      ) : null}
    </div>
  );
}
