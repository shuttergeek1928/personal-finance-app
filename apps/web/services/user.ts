import api from "./api";

const BASE_URL = process.env.NEXT_PUBLIC_GATEWAY_URL || "http://192.168.1.8:5000";
const GATEWAY_BASE_URL = `${BASE_URL}/gateway-users`;

export interface UserProfileTransferObject {
  id: string;
  userId: string;
  dateOfBirth: string | null;
  currency: string | null;
  timeZone: string | null;
  language: string | null;
  avatar: string | null;
  financialGoals: string | null;
  age: number | null;
}

export interface UserTransferObject {
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
  profile: UserProfileTransferObject;
  roles: string[] | null;
}

export interface UserTransferObjectPaginatedResult {
  items: UserTransferObject[] | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UserTransferObjectPaginatedResultApiResponse {
  success: boolean;
  data: UserTransferObjectPaginatedResult;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface UserTransferObjectApiResponse {
  success: boolean;
  data: UserTransferObject;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface BooleanApiResponse {
  success: boolean;
  data: boolean;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface UpdateUserProfileRequest {
  firstName?: string | null;
  lastName?: string | null;
  phoneNumber?: string | null;
  dateOfBirth?: string | null;
  currency?: string | null;
  timeZone?: string | null;
  language?: string | null;
}

export const userService = {
  getUsers: async (page = 1, pageSize = 20): Promise<UserTransferObjectPaginatedResultApiResponse> => {
    const response = await api.get(`/api/Users?page=${page}&pageSize=${pageSize}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getUserById: async (id: string): Promise<UserTransferObjectApiResponse> => {
    const response = await api.get(`/api/Users/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deleteUser: async (id: string): Promise<BooleanApiResponse> => {
    const response = await api.delete(`/api/Users/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getUserByEmail: async (email: string): Promise<UserTransferObjectApiResponse> => {
    const response = await api.get(`/api/Users/by-email/${encodeURIComponent(email)}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  updateProfile: async (id: string, payload: UpdateUserProfileRequest): Promise<UserTransferObjectApiResponse> => {
    const response = await api.put(`/api/Users/${id}/profile`, payload, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  confirmEmail: async (id: string): Promise<BooleanApiResponse> => {
    const response = await api.post(`/api/Users/${id}/confirm-email`, {}, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  }
};
