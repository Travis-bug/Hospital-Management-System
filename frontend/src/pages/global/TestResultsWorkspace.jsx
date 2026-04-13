import { FlaskConical } from "lucide-react";
import { useEffect, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

function normalizeResult(result) {
  return {
    publicTestId: result.publicTestId,
    findings: result.findings,
    resultDate: result.resultDate,
    testName: result.test?.testName ?? "Unknown test",
    visitPublicId: result.visit?.visitPublicId ?? result.visit?.publicId,
    nurseName: result.nurse ? `${result.nurse.firstName} ${result.nurse.lastName}` : "Unknown nurse",
  };
}

export default function TestResultsWorkspace() {
  const { user } = useAuth();
  const [results, setResults] = useState([]);
  const [selectedResult, setSelectedResult] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadResults = async () => {
      try {
        const response = await apiClient.get("/api/TestResults");
        if (!isMounted) {
          return;
        }

        const normalizedResults = response.data.map(normalizeResult);
        setResults(normalizedResults);
        setSelectedResult(normalizedResults[0] ?? null);
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load test results.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    loadResults();
    return () => {
      isMounted = false;
    };
  }, []);

  const handleSelectResult = async (publicTestId) => {
    setDetailLoading(true);

    try {
      const response = await apiClient.get(`/api/TestResults/${publicTestId}`);
      setSelectedResult(normalizeResult(response.data));
    } catch (error) {
      setErrorMessage(error?.response?.data?.detail ?? "Unable to load test result detail.");
    } finally {
      setDetailLoading(false);
    }
  };

  if (!["Doctor", "Nurse"].includes(user?.role ?? "")) {
    return (
      <section className="panel-shell p-8">
        <p className="section-title">Tests</p>
        <h2 className="mt-2 text-2xl font-semibold text-slate-950">Test results unavailable</h2>
        <p className="mt-3 text-sm leading-6 text-slate-500">
          The backend currently scopes test-result access to Doctor and Nurse sessions only.
        </p>
      </section>
    );
  }

  return (
    <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 px-6 py-5">
          <p className="section-title">Tests</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-950">Diagnostic Results</h2>
        </div>

        {errorMessage ? (
          <div className="p-4">
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
              {errorMessage}
            </div>
          </div>
        ) : null}

        {isLoading ? (
          <div className="p-6 text-sm text-slate-500">Loading test results...</div>
        ) : (
          <div className="space-y-3 p-4">
            {results.map((result) => (
              <button
                key={result.publicTestId}
                type="button"
                onClick={() => handleSelectResult(result.publicTestId)}
                className={[
                  "w-full rounded-2xl border px-4 py-4 text-left transition",
                  selectedResult?.publicTestId === result.publicTestId
                    ? "border-blue-200 bg-blue-50"
                    : "border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50",
                ].join(" ")}
              >
                <p className="font-semibold text-slate-900">{result.testName}</p>
                <p className="mt-1 text-sm text-slate-500">{result.publicTestId}</p>
              </button>
            ))}
          </div>
        )}
      </section>

      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
          <div className="flex items-center gap-3">
            <FlaskConical className="h-5 w-5" />
            <div>
              <p className="section-title text-slate-300">Result Detail</p>
              <h2 className="mt-2 text-2xl font-semibold">
                {selectedResult?.publicTestId ?? "Select a result"}
              </h2>
            </div>
          </div>
        </div>

        <div className="space-y-4 p-6">
          {detailLoading ? (
            <div className="text-sm text-slate-500">Loading result detail...</div>
          ) : selectedResult ? (
            <>
              <article className="rounded-2xl border border-slate-200 bg-slate-50/80 p-4">
                <p className="text-sm font-semibold text-slate-500">Test</p>
                <p className="mt-2 text-lg font-semibold text-slate-900">{selectedResult.testName}</p>
              </article>
              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Findings</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedResult.findings}</p>
              </article>
              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Visit</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">{selectedResult.visitPublicId ?? "Unlinked visit"}</p>
              </article>
              <article className="rounded-2xl border border-slate-200 bg-white p-4">
                <p className="text-sm font-semibold text-slate-500">Nurse / Result Date</p>
                <p className="mt-2 text-sm leading-6 text-slate-700">
                  {selectedResult.nurseName} • {selectedResult.resultDate ? new Date(selectedResult.resultDate).toLocaleString() : "Unknown"}
                </p>
              </article>
            </>
          ) : (
            <div className="text-sm text-slate-500">Select a test result from the left to inspect it.</div>
          )}
        </div>
      </section>
    </div>
  );
}
