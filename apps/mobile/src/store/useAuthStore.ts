import { create } from "zustand";
import { authService, LoginResponseUser } from "../services/auth";

interface AuthState {
  user: LoginResponseUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  initialize: () => Promise<void>;
  login: (token: string, user: LoginResponseUser) => Promise<void>;
  logout: () => Promise<void>;
  isAdmin: () => boolean;
  hasRole: (role: string) => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,

  initialize: async () => {
    try {
      const token = await authService.getToken();
      const user = await authService.getStoredUser();

      if (token && user) {
        set({
          token,
          user,
          isAuthenticated: true,
          isLoading: false,
        });
      } else {
        await authService.logout();
        set({
          token: null,
          user: null,
          isAuthenticated: false,
          isLoading: false,
        });
      }
    } catch (error) {
      console.error("Auth initialization failed:", error);
      set({ isLoading: false });
    }
  },

  login: async (token: string, user: LoginResponseUser) => {
    await authService.storeToken(token);
    await authService.storeUser(user);
    set({
      token,
      user,
      isAuthenticated: true,
      isLoading: false,
    });
  },

  logout: async () => {
    await authService.logout();
    set({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
    });
  },

  isAdmin: () => {
    const { user } = get();
    return user?.roles?.includes("Admin") ?? false;
  },

  hasRole: (role: string) => {
    const { user } = get();
    return user?.roles?.includes(role) ?? false;
  },
}));
