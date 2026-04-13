import { FileText, LayoutList, TimerReset } from "lucide-react";
import { startTransition, useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import apiClient from "../../api/apiClient";
import VisitDetails from "./VisitDetails";

function createClassificationDraft(visit) {
  return {
    status: visit.status,
    patientClass: visit.patientClass,
    admissionStatus: visit.admissionStatus,
  };
}

const visitStatusTone = {
  Active: "bg-blue-50 text-blue-700 ring-1 ring-blue-200",
  Completed: "bg-slate-100 text-slate-700 ring-1 ring-slate-200",
};

function normalizeVisit(visit) {
  return {
    publicId: visit.publicId,
    status: visit.status,
    patientClass: visit.patientClass,
    admissionStatus: visit.admissionStatus,
    arrivalSource: visit.arrivalSource,
    visitDate: visit.checkInTime,
    checkInTime: visit.checkInTime ? new Date(visit.checkInTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : "Unknown",
    primaryComplaint: visit.symptoms ?? "No symptoms recorded",
    attendingLabel: visit.doctorName ?? visit.nurseName ?? "Unassigned clinician",
    location: visit.arrivalSource ?? "Clinical intake",
    diagnosis: visit.diagnosis,
    treatment: visit.treatment,
    visitNotes: visit.visitNotes,
  };
}

export default function ContextualVisits() {
  const { patient } = useOutletContext();
  const [visits, setVisits] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // The selected visit drives the detail panel on the right. Keeping this state here
  // lets the chart route behave like a contextual workspace without introducing a
  // deeper child router for Prompt 2.
  const [selectedVisitId, setSelectedVisitId] = useState(null);

  // The editable enum values live independently from the source visit row so the user
  // can make several classification changes before pressing Save.
  const [classificationDraft, setClassificationDraft] = useState(null);

  const selectedVisit =
    visits.find((visit) => visit.publicId === selectedVisitId) ?? null;

  useEffect(() => {
    let isMounted = true;

    const loadVisits = async () => {
      try {
        const response = await apiClient.get(`/api/Patient/${patient.publicId}/visits`);

        if (isMounted) {
          const normalizedVisits = response.data.map(normalizeVisit);
          setVisits(normalizedVisits);
          setSelectedVisitId(normalizedVisits[0]?.publicId ?? null);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            error?.response?.data?.detail ?? "Unable to load patient visits.",
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadVisits();

    return () => {
      isMounted = false;
    };
  }, [patient.publicId]);

  useEffect(() => {
    if (!selectedVisit) {
      setClassificationDraft(null);
      return;
    }

    setClassificationDraft(createClassificationDraft(selectedVisit));
  }, [selectedVisit]);

  const handleVisitSelection = (visitPublicId) => {
    // startTransition keeps the list interaction responsive if the detail panel becomes
    // significantly more expensive in later prompts.
    startTransition(() => {
      setSelectedVisitId(visitPublicId);
    });
  };

  const handleClassificationChange = (field, value) => {
    setClassificationDraft((currentDraft) => {
      if (!currentDraft) {
        return currentDraft;
      }

      return {
        ...currentDraft,
        [field]: value,
      };
    });
  };

  const saveClassifications = async () => {
    if (!selectedVisit || !classificationDraft) {
      return;
    }

    setErrorMessage("");
    setIsSaving(true);

    try {
      await apiClient.patch(`/api/Visit/${selectedVisit.publicId}/classifications`, classificationDraft);

      setVisits((currentVisits) =>
        currentVisits.map((visit) =>
          visit.publicId === selectedVisit.publicId
            ? {
                ...visit,
                ...classificationDraft,
              }
            : visit,
        ),
      );
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to update visit classifications.",
      );
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[420px_1fr]">
      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <p className="section-title">Contextual Visits</p>
          <h3 className="mt-2 text-2xl font-semibold text-slate-950">
            Patient-Specific Encounter Timeline
          </h3>
          <p className="mt-3 text-sm leading-6 text-slate-500">
            This list is scoped to the charted patient only, which keeps it distinct from the future global visits workspace in the main rail.
          </p>
        </div>

        {errorMessage ? (
          <div className="p-6">
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-5 py-4 text-sm text-rose-700">
              {errorMessage}
            </div>
          </div>
        ) : isLoading ? (
          <div className="p-6">
            <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
              Loading visits...
            </div>
          </div>
        ) : visits.length === 0 ? (
          <div className="p-6">
            <div className="rounded-2xl bg-slate-50 px-5 py-10 text-center text-sm text-slate-500">
              No contextual visits exist for this patient.
            </div>
          </div>
        ) : (
          <div className="space-y-3 p-4">
            {visits.map((visit) => {
              const isSelected = visit.publicId === selectedVisitId;

              return (
                <button
                  key={visit.publicId}
                  type="button"
                  onClick={() => handleVisitSelection(visit.publicId)}
                  className={[
                    "w-full rounded-2xl border px-4 py-4 text-left transition",
                    isSelected
                      ? "border-blue-200 bg-blue-50 shadow-sm"
                      : "border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50",
                  ].join(" ")}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-semibold text-slate-900">{visit.publicId}</p>
                      <p className="mt-1 text-sm text-slate-500">
                        {new Date(visit.visitDate).toLocaleString()}
                      </p>
                    </div>
                    <span
                      className={`rounded-full px-3 py-1 text-xs font-semibold ${visitStatusTone[visit.status]}`}
                    >
                      {visit.status}
                    </span>
                  </div>

                  <div className="mt-4 grid gap-3 text-sm text-slate-600 sm:grid-cols-2">
                    <div className="flex items-start gap-2">
                      <FileText className="mt-0.5 h-4 w-4 text-slate-400" />
                      <div>
                        <p className="font-medium text-slate-800">Complaint</p>
                        <p className="mt-1 text-slate-500">{visit.primaryComplaint}</p>
                      </div>
                    </div>

                    <div className="flex items-start gap-2">
                      <LayoutList className="mt-0.5 h-4 w-4 text-slate-400" />
                      <div>
                        <p className="font-medium text-slate-800">Classification</p>
                        <p className="mt-1 text-slate-500">
                          {visit.patientClass} • {visit.admissionStatus}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-start gap-2 sm:col-span-2">
                      <TimerReset className="mt-0.5 h-4 w-4 text-slate-400" />
                      <div>
                        <p className="font-medium text-slate-800">Location</p>
                        <p className="mt-1 text-slate-500">
                          {visit.location} • Check-in {visit.checkInTime}
                        </p>
                      </div>
                    </div>
                  </div>
                </button>
              );
            })}
          </div>
        )}
      </section>

      <VisitDetails
        patient={patient}
        visit={selectedVisit}
        classificationDraft={classificationDraft}
        onClassificationChange={handleClassificationChange}
        onSave={saveClassifications}
        isSaving={isSaving}
      />
    </div>
  );
}
