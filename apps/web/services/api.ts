import axios from "axios";

// This is a base Axios instance. We connect to the .NET REST API eventually.
// For now, it might be relative or point to a local dev server.
const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "",
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  (config) => {
    // Inject token if available
    const token =
      typeof window !== "undefined" ? localStorage.getItem("auth_token") : null;
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (typeof window !== "undefined") {
        const accessToken = localStorage.getItem("auth_token");
        const refreshToken = localStorage.getItem("refresh_token");

        if (accessToken && refreshToken) {
          try {
            // Import authService dynamically to avoid circular dependency
            const { authService } = await import("./auth");
            const response = await authService.refreshToken(
              accessToken,
              refreshToken
            );

            if (response.success && response.data) {
              const newAccessToken = response.data.accessToken;
              const newRefreshToken = response.data.refreshToken;

              authService.storeToken(newAccessToken, newRefreshToken);

              // Update header and retry
              originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
              return api(originalRequest);
            }
          } catch (refreshError) {
            console.error("Token refresh failed:", refreshError);
            // If refresh fails, log out
            const { authService } = await import("./auth");
            authService.logout();
          }
        }

        // Handle unauthorized - clear token and redirect to login
        const currentPath = window.location.pathname;
        if (!currentPath.startsWith("/auth") && currentPath !== "/") {
          window.location.href = `/auth?redirect=${encodeURIComponent(
            currentPath
          )}`;
        }
      }
    }
    return Promise.reject(error);
  }
);

export default api;
