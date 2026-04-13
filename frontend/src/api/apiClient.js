import axios from "axios";

function normalizeBaseUrl(value) {
  if (!value) {
    return "";
  }

  return value.replace(/\/+$/, "");
}

const apiClient = axios.create({
  // In development Vite proxies /api to the ASP.NET backend.
  // In production the frontend is served by ASP.NET Core, so relative /api calls
  // stay same-origin and cookie-authenticated without extra CORS work.
  baseURL: normalizeBaseUrl(import.meta.env.VITE_API_BASE_URL),
  withCredentials: true,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
});

export default apiClient;
