import { Mail, MapPin, Phone, ShieldPlus, Trash2, UserRound, UserRoundCog } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate, useOutletContext } from "react-router-dom";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

function buildUpdateForm(patient) {
  return {
    firstName: patient.firstName ?? "",
    lastName: patient.lastName ?? "",
    dateOfBirth: patient.dateOfBirth ?? "",
    healthCardNo: patient.healthCardNo ?? "",
    address: patient.address ?? "",
    phoneNumber: patient.phoneNumber ?? "",
    email: patient.email ?? "",
  };
}

export default function PatientOverview() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { patient, refreshPatient } = useOutletContext();
  const [updateForm, setUpdateForm] = useState(() => buildUpdateForm(patient));
  const [doctorDirectory, setDoctorDirectory] = useState([]);
  const [doctorPublicId, setDoctorPublicId] = useState(patient.doctorPublicId ?? "");
  const [isUpdating, setIsUpdating] = useState(false);
  const [isAssigning, setIsAssigning] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const canUpdatePatient = ["Doctor", "Secretary"].includes(user?.role ?? "");
  const canAssignDoctor = ["Manager", "Admin", "Secretary"].includes(user?.role ?? "");
  const canDeletePatient = user?.role === "Doctor";

  useEffect(() => {
    setUpdateForm(buildUpdateForm(patient));
    setDoctorPublicId(patient.doctorPublicId ?? "");
  }, [patient]);

  useEffect(() => {
    let isMounted = true;

    const loadDoctors = async () => {
      if (!canAssignDoctor) {
        return;
      }

      try {
        const response = await apiClient.get("/api/Staff");
        if (!isMounted) {
          return;
        }

        setDoctorDirectory(response.data.filter((staffMember) => staffMember.role === "Doctor"));
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load the doctor directory.",
          );
        }
      }
    };

    loadDoctors();

    return () => {
      isMounted = false;
    };
  }, [canAssignDoctor]);

  const handleUpdateChange = (field, value) => {
    setUpdateForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleUpdatePatient = async (event) => {
    event.preventDefault();
    setIsUpdating(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.put(`/api/Patient/${patient.publicId}`, updateForm);
      refreshPatient();
      setSuccessMessage("Patient demographics updated.");
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to update patient demographics.",
      );
    } finally {
      setIsUpdating(false);
    }
  };

  const handleAssignDoctor = async (event) => {
    event.preventDefault();
    if (!doctorPublicId) {
      return;
    }

    setIsAssigning(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.patch(`/api/Patient/${patient.publicId}/assign-doctor`, {
        doctorPublicId,
      });
      refreshPatient();
      setSuccessMessage("Doctor assignment updated.");
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to assign the selected doctor.",
      );
    } finally {
      setIsAssigning(false);
    }
  };

  const handleDeletePatient = async () => {
    if (!window.confirm(`Delete patient ${patient.publicId}? This cannot be undone.`)) {
      return;
    }

    setIsDeleting(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      await apiClient.delete(`/api/Patient/${patient.publicId}`);
      navigate("/patients", {
        replace: true,
        state: {
          flashMessage: `Patient ${patient.publicId} was deleted.`,
        },
      });
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to delete the patient record.",
      );
      setIsDeleting(false);
    }
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[1.25fr_0.75fr]">
      <section className="space-y-6">
        <div>
          <p className="section-title">Overview</p>
          <h3 className="mt-2 text-2xl font-semibold text-slate-950">
            Demographic Snapshot
          </h3>
          <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
            This chart overview is sourced from the live patient record and anchors the
            patient-specific vitals, prescriptions, visits, and diagnostic result tabs.
          </p>
        </div>

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

        <div className="grid gap-4 sm:grid-cols-2">
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Patient Type</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.type}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Date of Birth</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.dateOfBirth}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Gender</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.gender ?? "---"}</p>
          </article>
          <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-5">
            <p className="text-sm font-medium text-slate-500">Health Card</p>
            <p className="mt-2 text-xl font-semibold text-slate-900">{patient.healthCardNo}</p>
          </article>
        </div>

        {canUpdatePatient ? (
          <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="rounded-2xl bg-slate-900 p-3 text-white">
                <UserRound className="h-4 w-4" />
              </div>
              <div>
                <p className="section-title">Patient Management</p>
                <h4 className="text-xl font-semibold text-slate-950">Update Demographics</h4>
              </div>
            </div>

            <form onSubmit={handleUpdatePatient} className="mt-6 grid gap-4 md:grid-cols-2">
              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">First Name</span>
                <input
                  value={updateForm.firstName}
                  onChange={(event) => handleUpdateChange("firstName", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Last Name</span>
                <input
                  value={updateForm.lastName}
                  onChange={(event) => handleUpdateChange("lastName", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Date of Birth</span>
                <input
                  type="date"
                  value={updateForm.dateOfBirth}
                  onChange={(event) => handleUpdateChange("dateOfBirth", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Health Card</span>
                <input
                  value={updateForm.healthCardNo}
                  onChange={(event) => handleUpdateChange("healthCardNo", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2 md:col-span-2">
                <span className="text-sm font-semibold text-slate-700">Address</span>
                <input
                  value={updateForm.address}
                  onChange={(event) => handleUpdateChange("address", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Phone Number</span>
                <input
                  value={updateForm.phoneNumber}
                  onChange={(event) => handleUpdateChange("phoneNumber", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <label className="space-y-2">
                <span className="text-sm font-semibold text-slate-700">Email</span>
                <input
                  type="email"
                  value={updateForm.email}
                  onChange={(event) => handleUpdateChange("email", event.target.value)}
                  className={fieldClassName}
                  required
                />
              </label>

              <div className="md:col-span-2">
                <button
                  type="submit"
                  disabled={isUpdating}
                  className="rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
                >
                  {isUpdating ? "Saving..." : "Update Patient"}
                </button>
              </div>
            </form>
          </section>
        ) : null}
      </section>

      <aside className="space-y-4">
        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
            <Phone className="h-4 w-4 text-blue-700" />
            Contact
          </div>
          <p className="mt-3 text-sm text-slate-600">{patient.phoneNumber ?? "---"}</p>
          <p className="mt-2 text-sm text-slate-600">{patient.email ?? "---"}</p>
        </article>

        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
            <MapPin className="h-4 w-4 text-blue-700" />
            Address
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">{patient.address ?? "---"}</p>
        </article>

        <article className="rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
            <UserRoundCog className="h-4 w-4 text-blue-700" />
            Assigned Doctor
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-600">{patient.doctorPublicId ?? "Unassigned"}</p>
        </article>

        {canAssignDoctor ? (
          <form onSubmit={handleAssignDoctor} className="rounded-2xl border border-slate-200 bg-white p-5">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
              <UserRoundCog className="h-4 w-4 text-blue-700" />
              Assign / Reassign Doctor
            </div>
            <label className="mt-4 block space-y-2">
              <span className="text-sm font-medium text-slate-600">Doctor</span>
              <select
                value={doctorPublicId}
                onChange={(event) => setDoctorPublicId(event.target.value)}
                className={fieldClassName}
                required
              >
                <option value="" disabled>Select a doctor</option>
                {doctorDirectory.map((doctor) => (
                  <option key={doctor.publicId} value={doctor.publicId}>
                    {doctor.firstName} {doctor.lastName} ({doctor.publicId})
                  </option>
                ))}
              </select>
            </label>
            <button
              type="submit"
              disabled={isAssigning || !doctorPublicId}
              className="mt-4 w-full rounded-2xl bg-blue-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
            >
              {isAssigning ? "Saving..." : "Save Doctor Assignment"}
            </button>
          </form>
        ) : null}

        {canDeletePatient ? (
          <article className="rounded-2xl border border-rose-200 bg-rose-50 p-5">
            <div className="flex items-center gap-2 text-sm font-semibold text-rose-900">
              <Trash2 className="h-4 w-4" />
              Delete Patient
            </div>
            <p className="mt-3 text-sm leading-6 text-rose-800/80">
              This permanently removes the patient record from the system.
            </p>
            <button
              type="button"
              onClick={handleDeletePatient}
              disabled={isDeleting}
              className="mt-4 w-full rounded-2xl bg-rose-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:cursor-not-allowed disabled:bg-rose-300"
            >
              {isDeleting ? "Deleting..." : "Delete Patient"}
            </button>
          </article>
        ) : null}

        <article className="rounded-2xl border border-slate-200 bg-slate-950 p-5 text-white">
          <div className="flex items-center gap-2 text-sm font-semibold">
            <ShieldPlus className="h-4 w-4 text-blue-300" />
            Chart State
          </div>
          <p className="mt-3 text-sm leading-6 text-slate-300">
            Chart modules are live against the backend. Use the chart rail to switch
            between overview, vitals, prescriptions, visits, and diagnostic result history for this patient.
          </p>
        </article>
      </aside>
    </div>
  );
}
