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

let isRefreshing = false;
let failedQueue: { resolve: (value: unknown) => void; reject: (reason?: any) => void; }[] = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (typeof window === "undefined") {
        return Promise.reject(error);
      }

      if (isRefreshing) {
        try {
          const token = await new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject });
          });
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        } catch (err) {
          return Promise.reject(err);
        }
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const accessToken = localStorage.getItem("auth_token");
      const refreshToken = localStorage.getItem("refresh_token");

      if (accessToken && refreshToken) {
        try {
          const { authService } = await import("./auth");
          // Use axios directly to avoid interceptor loops if api.ts is reused
          const response = await axios.post(
            `${process.env.NEXT_PUBLIC_API_URL || ""}/gateway-users/api/Auth/refresh`,
            { accessToken, refreshToken },
            { headers: { "Content-Type": "application/json" } }
          );

          if (response.data?.success && response.data?.data) {
            const newAccessToken = response.data.data.accessToken;
            const newRefreshToken = response.data.data.refreshToken;

            authService.storeToken(newAccessToken, newRefreshToken);
            
            // Queue processed securely
            processQueue(null, newAccessToken);

            originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
            isRefreshing = false;
            return api(originalRequest);
          }
        } catch (refreshError) {
          processQueue(refreshError, null);
          console.error("Token refresh failed:", refreshError);
          const { authService } = await import("./auth");
          authService.logout();
        }
      } else {
         processQueue(error, null);
      }

      isRefreshing = false;
      const currentPath = window.location.pathname;
      if (!currentPath.startsWith("/auth") && currentPath !== "/") {
        window.location.href = `/auth?redirect=${encodeURIComponent(
          currentPath
        )}`;
      }
    }
    return Promise.reject(error);
  }
);

export default api;
