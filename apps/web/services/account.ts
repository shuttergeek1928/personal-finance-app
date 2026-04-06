import api from "./api";

const GATEWAY_BASE_URL = process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:5000";

export enum AccountType {
  Checking = 0,
  Savings = 1,
  Credit = 2,
  Loan = 3,
}

export interface Money {
  amount: number;
  currency: string;
}

export interface AccountTransferObject {
  id: string;
  userId: string;
  name: string | null;
  type: AccountType;
  balance: Money;
  accountNumber: string | null;
  description: string | null;
  createdAt: string;
  updatedAt: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface AccountTransferObjectApiResponse {
  success: boolean;
  data: AccountTransferObject;
  message: string | null;
  errors: string[] | null;
  timestamp: string;
}

export interface CreateAccountRequest {
  name: string | null;
  type: AccountType;
  balance: Money;
  userId: string;
  accountNumber: string | null;
  description: string | null;
  isDefault: boolean;
}

export interface UpdateBalanceRequest {
  id: string;
  balance: Money;
  accountNumber: string | null;
  isDeposit: boolean;
}

export interface TransferMoneyRequest {
  id: string;
  balance: Money;
  accountNumber: string | null;
  isDeposit: boolean;
  toAccountId: string;
}

export const accountService = {
  createAccount: async (data: CreateAccountRequest): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.post("/gateway-accounts/api/Accounts/create", data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getAccountById: async (id: string): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.get(`/gateway-accounts/api/Accounts/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getAccountByNumber: async (number: string): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.get(`/gateway-accounts/api/Accounts/${number}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getAccountsByUserId: async (userId: string): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.get(`/gateway-accounts/api/Accounts/userid/${userId}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deposit: async (id: string, data: UpdateBalanceRequest): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.put(`/gateway-accounts/api/Accounts/${id}/deposit`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  withdraw: async (id: string, data: UpdateBalanceRequest): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.put(`/gateway-accounts/api/Accounts/${id}/withdraw`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  transfer: async (data: TransferMoneyRequest): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.put("/gateway-accounts/api/Accounts/transfer", data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  setDefault: async (userId: string, accountNumber: string): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.put(`/gateway-accounts/api/Accounts/${userId}/set-default?accountNumber=${accountNumber}`, {}, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deleteAccount: async (userId: string, accountId: string): Promise<AccountTransferObjectApiResponse> => {
    const response = await api.delete(`/gateway-accounts/api/Accounts/${userId}/${accountId}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },
};
