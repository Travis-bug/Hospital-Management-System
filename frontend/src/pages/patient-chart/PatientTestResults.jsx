import { FlaskConical } from "lucide-react";
import { useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import apiClient from "../../api/apiClient";

export default function PatientTestResults() {
  const { patient } = useOutletContext();
  const [results, setResults] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    const loadResults = async () => {
      try {
        const response = await apiClient.get(`/api/Patient/${patient.publicId}/test-results`);
        if (isMounted) {
          setResults(response.data);
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error?.response?.data?.detail ?? "Unable to load patient test results.");
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
  }, [patient.publicId]);

  return (
    <section className="space-y-6">
      <div>
        <p className="section-title">Test Results</p>
        <h3 className="mt-2 text-2xl font-semibold text-slate-950">Patient Diagnostic Results</h3>
      </div>

      {errorMessage ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      {isLoading ? (
        <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">Loading test results...</div>
      ) : (
        <div className="grid gap-4">
          {results.length === 0 ? (
            <div className="rounded-2xl bg-slate-50 px-5 py-8 text-sm text-slate-500">
              No test results recorded for this patient.
            </div>
          ) : (
            results.map((result) => (
              <article key={result.publicTestId} className="rounded-2xl border border-slate-200 bg-white p-5">
                <div className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                  <FlaskConical className="h-4 w-4 text-blue-700" />
                  {result.testName}
                </div>
                <p className="mt-2 text-sm text-slate-500">{result.publicTestId}</p>
                <p className="mt-4 text-sm leading-6 text-slate-700">{result.findings}</p>
                <p className="mt-4 text-sm text-slate-500">
                  {result.nurseName ?? "Unknown nurse"} • {result.resultDate ? new Date(result.resultDate).toLocaleString() : "Unknown time"}
                </p>
              </article>
            ))
          )}
        </div>
      )}
    </section>
  );
}
