import api from "./api";

// Use environment variable to set the API Gateway URL.
// Defaults to relative path if NEXT_PUBLIC_API_URL is empty (ideal for Nginx proxy).
const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "";
const GATEWAY_BASE_URL = `${BASE_URL}/gateway-users`;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponseUser {
  id: string;
  email: string | null;
  userName: string | null;
  firstName: string | null;
  lastName: string | null;
  phoneNumber: string | null;
  isEmailConfirmed: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
  fullName: string | null;
  roles: string[] | null;
}

export interface LoginResponse {
  user: LoginResponseUser;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  permissions: string[];
}

export interface LoginApiResponse {
  success: boolean;
  data: LoginResponse;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface RegisterUserRequest {
  email?: string | null;
  userName?: string | null;
  password?: string | null;
  confirmPassword?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  phoneNumber?: string | null;
  acceptTerms: boolean;
}

export interface RegisterApiResponse {
  success: boolean;
  data: LoginResponseUser;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export const authService = {
  login: async (payload: LoginRequest): Promise<LoginApiResponse> => {
    const response = await api.post("/api/Auth/login", payload, {
      baseURL: GATEWAY_BASE_URL,
    });
    return response.data;
  },

  register: async (
    payload: RegisterUserRequest
  ): Promise<RegisterApiResponse> => {
    const response = await api.post("/api/Auth/register", payload, {
      baseURL: GATEWAY_BASE_URL,
    });
    return response.data;
  },

  googleLogin: async (idToken: string): Promise<LoginApiResponse> => {
    const response = await api.post(
      "/api/Auth/google-login",
      { idToken },
      {
        baseURL: GATEWAY_BASE_URL,
      }
    );
    return response.data;
  },

  refreshToken: async (
    accessToken: string,
    refreshToken: string
  ): Promise<LoginApiResponse> => {
    const response = await api.post(
      "/api/Auth/refresh",
      { accessToken, refreshToken },
      {
        baseURL: GATEWAY_BASE_URL,
      }
    );
    return response.data;
  },

  storeToken: (accessToken: string, refreshToken?: string) => {
    if (typeof window !== "undefined") {
      localStorage.setItem("auth_token", accessToken);
      if (refreshToken) {
        localStorage.setItem("refresh_token", refreshToken);
      }
    }
  },

  storeUser: (user: LoginResponseUser) => {
    if (typeof window !== "undefined") {
      localStorage.setItem("auth_user", JSON.stringify(user));
    }
  },

  getToken: (): string | null => {
    if (typeof window !== "undefined") {
      return localStorage.getItem("auth_token");
    }
    return null;
  },

  getRefreshToken: (): string | null => {
    if (typeof window !== "undefined") {
      return localStorage.getItem("refresh_token");
    }
    return null;
  },

  getStoredUser: (): LoginResponseUser | null => {
    if (typeof window !== "undefined") {
      const userStr = localStorage.getItem("auth_user");
      if (userStr) {
        try {
          return JSON.parse(userStr);
        } catch {
          return null;
        }
      }
    }
    return null;
  },

  isAuthenticated: (): boolean => {
    return !!authService.getToken();
  },

  logout: () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("auth_token");
      localStorage.removeItem("refresh_token");
      localStorage.removeItem("auth_user");
    }
  },
};
