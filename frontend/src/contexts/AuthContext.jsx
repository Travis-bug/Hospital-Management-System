import { createContext, useContext, useEffect, useState } from "react";
import apiClient from "../api/apiClient";

const AuthContext = createContext(null);

function toDisplayName(email) {
  if (!email) {
    return "Staff Member";
  }

  return email
    .split("@")[0]
    .split(/[._-]/)
    .filter(Boolean)
    .map((segment) => segment[0].toUpperCase() + segment.slice(1))
    .join(" ");
}

function toSessionUser(payload) {
  return {
    role: payload.role,
    email: payload.email,
    publicId: payload.publicId,
    name: payload.displayName ?? toDisplayName(payload.email),
  };
}

function getAuthErrorMessage(error, fallbackMessage) {
  return (
    error?.response?.data?.message
    ?? error?.response?.data?.detail
    ?? fallbackMessage
  );
}

export function AuthProvider({ children }) {
  const [currentUser, setCurrentUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  // On first load we ask the backend whether the ASP.NET Identity cookie
  // already represents an authenticated session. This keeps refreshes from
  // dumping the user back to the login page when the cookie is still valid.
  useEffect(() => {
    let isMounted = true;

    const hydrateSession = async () => {
      try {
        const response = await apiClient.get("/api/auth/me");

        if (isMounted) {
          setCurrentUser(toSessionUser(response.data));
        }
      } catch (error) {
        if (error?.response?.status !== 401 && isMounted) {
          console.error("Session bootstrap failed", error);
        }

        if (isMounted) {
          setCurrentUser(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    hydrateSession();

    return () => {
      isMounted = false;
    };
  }, []);

  const loginStep1 = async (email, password) => {
    try {
      const response = await apiClient.post("/api/auth/login", { email, password });

      if (response.data.requiresTwoFactor) {
        return response.data;
      }

      const sessionUser = toSessionUser(response.data);
      setCurrentUser(sessionUser);
      return response.data;
    } catch (error) {
      throw new Error(getAuthErrorMessage(error, "Invalid login attempt."));
    }
  };

  const loginStep2 = async (email, code, rememberMachine = false) => {
    try {
      const response = await apiClient.post("/api/auth/login-2fa", {
        email,
        code,
        rememberMachine,
      });

      const sessionUser = toSessionUser(response.data);
      setCurrentUser(sessionUser);
      return response.data;
    } catch (error) {
      throw new Error(getAuthErrorMessage(error, "Invalid 2FA code."));
    }
  };

  const logout = async () => {
    try {
      await apiClient.post("/api/auth/logout");
    } finally {
      setCurrentUser(null);
    }
  };

  const changePassword = async (currentPassword, newPassword) => {
    try {
      await apiClient.post("/api/auth/change-password", {
        currentPassword,
        newPassword,
      });
    } catch (error) {
      throw new Error(getAuthErrorMessage(error, "Unable to change password."));
    }
  };

  return (
    <AuthContext.Provider
      value={{
        currentUser,
        user: currentUser,
        isAuthenticated: Boolean(currentUser),
        isLoading,
        loginStep1,
        loginStep2,
        changePassword,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
