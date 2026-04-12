import api from "./api";

// Use environment variable to set the API Gateway URL.
// Defaults to relative path if NEXT_PUBLIC_API_URL is empty (ideal for Nginx proxy).
const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "";
const GATEWAY_BASE_URL = `${BASE_URL}/gateway-transactions`;

export enum TransactionType {
  Income = 0,
  Expense = 1,
  Transfer = 2,
}

export enum TransactionStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

export interface Money {
  amount: number;
  currency: string;
}

export interface Transaction {
  id: string;
  userId: string;
  accountId?: string;
  creditCardId?: string;
  toAccountId?: string;
  toCreditCardId?: string;
  money: Money;
  type: TransactionType;
  description: string;
  category: string;
  transactionDate: string;
  status: TransactionStatus;
  rejectionReason?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
  timestamp: string;
}

export const transactionService = {
  getTransactionsByUserId: async (
    userId: string
  ): Promise<ApiResponse<Transaction[]>> => {
    const response = await api.get<ApiResponse<Transaction[]>>(
      `/api/Transaction/user/${userId}`,
      { baseURL: GATEWAY_BASE_URL }
    );
    return response.data;
  },

  createIncome: async (data: any): Promise<ApiResponse<Transaction>> => {
    const response = await api.post<ApiResponse<Transaction>>(
      "/api/Transaction/income/deposit",
      data,
      { baseURL: GATEWAY_BASE_URL }
    );
    return response.data;
  },

  createExpense: async (data: any): Promise<ApiResponse<Transaction>> => {
    const response = await api.post<ApiResponse<Transaction>>(
      "/api/Transaction/expense/withdraw",
      data,
      { baseURL: GATEWAY_BASE_URL }
    );
    return response.data;
  },

  createTransfer: async (data: any): Promise<ApiResponse<Transaction>> => {
    const response = await api.post<ApiResponse<Transaction>>(
      "/api/Transaction/transfer",
      data,
      { baseURL: GATEWAY_BASE_URL }
    );
    return response.data;
  },
};
