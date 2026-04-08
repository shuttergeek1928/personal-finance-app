import { create } from "zustand";
import { authService, LoginResponseUser } from "@/services/auth";

interface AuthState {
  user: LoginResponseUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  initialize: () => void;
  login: (token: string, user: LoginResponseUser) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,

  initialize: () => {
    const token = authService.getToken();
    const user = authService.getStoredUser();

    if (token && user) {
      set({
        token,
        user,
        isAuthenticated: true,
        isLoading: false,
      });
    } else {
      // Clear any partial state
      authService.logout();
      set({
        token: null,
        user: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  },

  login: (token: string, user: LoginResponseUser) => {
    authService.storeToken(token);
    authService.storeUser(user);
    set({
      token,
      user,
      isAuthenticated: true,
      isLoading: false,
    });
  },

  logout: () => {
    authService.logout();
    set({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
    });
  },
}));
