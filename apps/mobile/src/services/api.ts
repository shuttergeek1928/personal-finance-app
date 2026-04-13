import axios from "axios";
import AsyncStorage from "@react-native-async-storage/async-storage";

// In React Native, we use environment variables prefixed with EXPO_PUBLIC_
const api = axios.create({
  baseURL: process.env.EXPO_PUBLIC_API_URL || "http://192.168.1.8:5000",
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  async (config) => {
    try {
      const token = await AsyncStorage.getItem("auth_token");
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch (error) {
      console.error("Error retrieving token from storage:", error);
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      try {
        await AsyncStorage.removeItem("auth_token");
        await AsyncStorage.removeItem("auth_user");
        // Navigation-based redirect should be handled via store state change
      } catch (storageError) {
        console.error("Error clearing storage on 401:", storageError);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
