import { FlaskConical, Microscope } from "lucide-react";
import { useEffect, useState } from "react";
import apiClient from "../../api/apiClient";
import { useAuth } from "../../contexts/AuthContext";

const fieldClassName =
  "w-full rounded-2xl border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-blue-500 focus:ring-4 focus:ring-blue-100";

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
  const [pendingTests, setPendingTests] = useState([]);
  const [selectedResult, setSelectedResult] = useState(null);
  const [resultForm, setResultForm] = useState({
    testPublicId: "",
    findings: "",
  });
  const [isLoading, setIsLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const canUseWorkspace = ["Doctor", "Nurse"].includes(user?.role ?? "");

  const loadResults = async () => {
    const response = await apiClient.get("/api/TestResults");
    const normalizedResults = response.data.map(normalizeResult);
    setResults(normalizedResults);
    setSelectedResult((current) => current ?? normalizedResults[0] ?? null);
  };

  const loadPendingTests = async () => {
    const response = await apiClient.get("/api/TestResults/pending-tests");
    setPendingTests(response.data);
    setResultForm((current) => ({
      ...current,
      testPublicId: current.testPublicId || response.data[0]?.testPublicId || "",
    }));
  };

  useEffect(() => {
    let isMounted = true;

    const hydrateResults = async () => {
      try {
        if (!canUseWorkspace) {
          return;
        }

        const [resultsResponse, pendingTestsResponse] = await Promise.all([
          apiClient.get("/api/TestResults"),
          apiClient.get("/api/TestResults/pending-tests"),
        ]);

        if (!isMounted) {
          return;
        }

        const normalizedResults = resultsResponse.data.map(normalizeResult);
        setResults(normalizedResults);
        setSelectedResult(normalizedResults[0] ?? null);
        setPendingTests(pendingTestsResponse.data);
        setResultForm((current) => ({
          ...current,
          testPublicId: pendingTestsResponse.data[0]?.testPublicId || "",
        }));
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

    hydrateResults();
    return () => {
      isMounted = false;
    };
  }, [canUseWorkspace]);

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

  const handleAddResult = async (event) => {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage("");
    setSuccessMessage("");

    try {
      const response = await apiClient.post("/api/TestResults", resultForm);
      setSuccessMessage(`Recorded result ${response.data.publicTestId}.`);
      setResultForm({
        testPublicId: "",
        findings: "",
      });
      await Promise.all([loadResults(), loadPendingTests()]);
    } catch (error) {
      setErrorMessage(
        error?.response?.data?.detail ?? "Unable to record the test result.",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!canUseWorkspace) {
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
            <p className="section-title">Tests</p>
            <h2 className="mt-2 text-2xl font-semibold text-slate-950">Diagnostic Results</h2>
          </div>

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

      <section className="panel-shell overflow-hidden">
        <div className="border-b border-slate-200 bg-slate-950 px-6 py-5 text-white">
          <div className="flex items-center gap-3">
            <Microscope className="h-5 w-5" />
            <div>
              <p className="section-title text-slate-300">Result Entry</p>
              <h2 className="text-2xl font-semibold">Add Test Result</h2>
            </div>
          </div>
        </div>

        <form onSubmit={handleAddResult} className="grid gap-4 p-6 xl:grid-cols-[1fr_1.5fr_auto]">
          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">Ordered Test</span>
            <select
              value={resultForm.testPublicId}
              onChange={(event) => setResultForm((current) => ({ ...current, testPublicId: event.target.value }))}
              className={fieldClassName}
              required
            >
              <option value="" disabled>Select a pending test</option>
              {pendingTests.map((test) => (
                <option key={test.testPublicId} value={test.testPublicId}>
                  {test.testName} • {test.patientName} ({test.visitPublicId})
                </option>
              ))}
            </select>
          </label>

          <label className="space-y-2">
            <span className="text-sm font-semibold text-slate-700">Findings</span>
            <textarea
              value={resultForm.findings}
              onChange={(event) => setResultForm((current) => ({ ...current, findings: event.target.value }))}
              className={`${fieldClassName} min-h-[54px] resize-y`}
              placeholder="Document the result findings"
              required
            />
          </label>

          <div className="flex items-end">
            <button
              type="submit"
              disabled={isSubmitting || !pendingTests.length}
              className="w-full rounded-2xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-800 disabled:cursor-not-allowed disabled:bg-slate-400"
            >
              {isSubmitting ? "Saving..." : "Add Result"}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
