import api from "./api";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "";
const GATEWAY_URL = `${BASE_URL}/gateway-email-ingestion`;

export interface SyncStatus {
  userId: string;
  isGmailConnected: boolean;
  lastSyncAt: string | null;
  totalEmailsProcessed: number;
  totalTransactionsParsed: number;
  totalTransactionsConfirmed: number;
  pendingReviewCount: number;
  categorySyncInfo: {
    category: string;
    lastSyncAt: string;
    emailsProcessed: number;
    transactionsParsed: number;
  }[];
}

export interface ParsedTransaction {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  transactionType: string;
  category: string;
  description: string;
  transactionDate: string;
  merchantName: string | null;
  referenceNumber: string | null;
  status: string;
  confidenceScore: number;
  emailSubject: string | null;
  emailSender: string | null;
  emailDate: string | null;
  createdAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const emailIngestionService = {
  getSyncStatus: async (userId: string, hasGmailAccess: boolean = true) => {
    const response = await api.get(`/api/EmailIngestion/sync-status/${userId}?hasGmailAccess=${hasGmailAccess}`, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  },

  getParsedTransactions: async (userId: string, page = 1, pageSize = 20, status = "Pending") => {
    const response = await api.get(`/api/EmailIngestion/parsed-transactions/${userId}`, {
      baseURL: GATEWAY_URL,
      params: { page, pageSize, status }
    });
    return response.data;
  },

  syncGmail: async (userId: string, accessToken?: string, refreshToken?: string) => {
    const response = await api.post(`/api/EmailIngestion/sync`, {
      userId,
      gmailAccessToken: accessToken || "",
      gmailRefreshToken: refreshToken || ""
    }, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  },

  confirmTransaction: async (userId: string, transactionId: string, accountId: string) => {
    const response = await api.post(`/api/EmailIngestion/confirm/${transactionId}?userId=${userId}`, {
      accountId
    }, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  },

  rejectTransaction: async (userId: string, transactionId: string) => {
    const response = await api.post(`/api/EmailIngestion/reject/${transactionId}?userId=${userId}`, {}, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  },

  bulkConfirm: async (userId: string, accountId: string, minConfidence = 0.9) => {
    const response = await api.post(`/api/EmailIngestion/bulk-confirm?userId=${userId}`, {
      accountId,
      minConfidenceScore: minConfidence
    }, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  },

  resetConfirmed: async (userId: string) => {
    const response = await api.post(`/api/EmailIngestion/reset-confirmed/${userId}`, {}, {
      baseURL: GATEWAY_URL,
    });
    return response.data;
  }
};
