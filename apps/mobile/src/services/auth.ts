import api from "./api";
import AsyncStorage from "@react-native-async-storage/async-storage";

// Use your Local Network IP (found from expo start output)
const BASE_URL = process.env.EXPO_PUBLIC_API_URL || "http://192.168.1.8:5000";
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
  profile?: {
    avatar?: string | null;
    bio?: string | null;
  } | null;
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

  storeToken: async (token: string) => {
    await AsyncStorage.setItem("auth_token", token);
  },

  storeUser: async (user: LoginResponseUser) => {
    await AsyncStorage.setItem("auth_user", JSON.stringify(user));
  },

  getToken: async (): Promise<string | null> => {
    return await AsyncStorage.getItem("auth_token");
  },

  getStoredUser: async (): Promise<LoginResponseUser | null> => {
    const userStr = await AsyncStorage.getItem("auth_user");
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  },

  isAuthenticated: async (): Promise<boolean> => {
    const token = await authService.getToken();
    return !!token;
  },

  logout: async () => {
    await AsyncStorage.removeItem("auth_token");
    await AsyncStorage.removeItem("auth_user");
  },
};
