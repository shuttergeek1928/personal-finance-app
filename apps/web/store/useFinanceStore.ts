import { create } from "zustand";

export interface Transaction {
  id: string;
  amount: number;
  category: string;
  date: string;
  type: "income" | "expense";
  description: string;
  accountId: string;
}

export interface Account {
  id: string;
  name: string;
  balance: number;
  type: "checking" | "savings" | "credit" | "investment" | "loan";
}

export interface FinanceState {
  accounts: Account[];
  transactions: Transaction[];
  isLoading: boolean;
  error: string | null;
  fetchDashboardData: () => Promise<void>;
  addTransaction: (transaction: Omit<Transaction, "id">) => Promise<void>;
  deleteTransaction: (id: string) => Promise<void>;
}

// Mock Data
const MOCK_ACCOUNTS: Account[] = [
  { id: "a1", name: "Main Checking", balance: 5420.50, type: "checking" },
  { id: "a2", name: "High Yield Savings", balance: 12500.00, type: "savings" },
  { id: "a3", name: "Credit Card", balance: -840.20, type: "credit" },
];

const MOCK_TRANSACTIONS: Transaction[] = [
  { id: "t1", amount: 3200, category: "Salary", date: new Date().toISOString(), type: "income", description: "Monthly Salary", accountId: "a1" },
  { id: "t2", amount: 45.99, category: "Food & Dining", date: new Date(Date.now() - 86400000).toISOString(), type: "expense", description: "Uber Eats", accountId: "a3" },
  { id: "t3", amount: 1200, category: "Housing", date: new Date(Date.now() - 86400000 * 2).toISOString(), type: "expense", description: "Rent", accountId: "a1" },
  { id: "t4", amount: 15.99, category: "Entertainment", date: new Date(Date.now() - 86400000 * 3).toISOString(), type: "expense", description: "Netflix", accountId: "a3" },
];

export const useFinanceStore = create<FinanceState>((set) => ({
  accounts: [],
  transactions: [],
  isLoading: false,
  error: null,

  fetchDashboardData: async () => {
    set({ isLoading: true, error: null });
    try {
      // TODO: Connect to real .NET API using axios
      // const res = await api.get('/dashboard');
      
      // Simulating network request
      await new Promise(resolve => setTimeout(resolve, 800));
      set({ accounts: MOCK_ACCOUNTS, transactions: MOCK_TRANSACTIONS, isLoading: false });
    } catch (error: any) {
      set({ error: error.message || "Failed to load dashboard data", isLoading: false });
    }
  },

  addTransaction: async (newTx) => {
    set({ isLoading: true, error: null });
    try {
      // Simulating API call
      await new Promise(resolve => setTimeout(resolve, 500));
      const transaction: Transaction = {
        ...newTx,
        id: Math.random().toString(36).substring(7),
      };
      
      set((state) => ({
        transactions: [transaction, ...state.transactions],
        isLoading: false
      }));
    } catch (error: any) {
      set({ error: error.message || "Failed to add transaction", isLoading: false });
    }
  },

  deleteTransaction: async (id) => {
    set({ isLoading: true, error: null });
    try {
      // Simulating API call
      await new Promise(resolve => setTimeout(resolve, 500));
      set((state) => ({
        transactions: state.transactions.filter(t => t.id !== id),
        isLoading: false
      }));
    } catch (error: any) {
      set({ error: error.message || "Failed to delete transaction", isLoading: false });
    }
  }
}));
