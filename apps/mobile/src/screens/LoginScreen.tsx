import React, { useState } from "react";
import { View, ScrollView, TouchableOpacity, Image, Text } from "react-native";
import { Eye, EyeOff, ArrowRight, LineChart } from "lucide-react-native";
import { useAuthStore } from "../store/useAuthStore";
import { authService } from "../services/auth";
import { Container } from "../components/Container";
import { H1, H2, P, Muted } from "../components/ui/Typography";
import { Input } from "../components/ui/Input";
import { Button } from "../components/ui/Button";

const LoginScreen = ({ navigation }: { navigation: any }) => {
  const { login: storeLogin } = useAuthStore();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async () => {
    if (!email || !password) {
      setError("Please enter both email and password");
      return;
    }

    setError(null);
    setIsLoading(true);

    try {
      const res = await authService.login({ email, password });
      if (res.success && res.data) {
        await storeLogin(res.data.accessToken, res.data.user);
        // Navigation will automatically update based on auth state in RootNavigator
      } else {
        setError(res.message || "Login failed");
      }
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || "An error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Container className="bg-white dark:bg-zinc-950">
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={{ flexGrow: 1, justifyContent: "center" }}>
        <View className="items-center mb-12">
          <View className="w-16 h-16 rounded-2xl bg-indigo-600 items-center justify-center mb-4 shadow-lg shadow-indigo-600/30">
            <LineChart color="white" size={32} />
          </View>
          <H1 className="text-center">Finance Flow</H1>
          <P className="text-center">Your money, moved smarter.</P>
        </View>

        <View className="space-y-6">
          <View>
            <H2 className="mb-2">Welcome back</H2>
            <Muted>Sign in to continue your journey</Muted>
          </View>

          {error && (
            <View className="bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900/30 p-4 rounded-xl">
              <Text className="text-red-600 dark:text-red-400 text-sm">{error}</Text>
            </View>
          )}

          <Input
            label="Email"
            placeholder="name@example.com"
            keyboardType="email-address"
            autoCapitalize="none"
            value={email}
            onChangeText={setEmail}
          />

          <View className="relative">
            <Input
              label="Password"
              placeholder="••••••••"
              secureTextEntry={!showPassword}
              value={password}
              onChangeText={setPassword}
            />
            <TouchableOpacity 
              onPress={() => setShowPassword(!showPassword)}
              className="absolute right-4 top-[38px]"
            >
              {showPassword ? <EyeOff size={20} color="#71717a" /> : <Eye size={20} color="#71717a" />}
            </TouchableOpacity>
          </View>

          <Button
            label="Sign In"
            onPress={handleLogin}
            isLoading={isLoading}
            size="lg"
            className="mt-4"
          />

          <View className="flex-row justify-center mt-6">
            <Muted>Don't have an account? </Muted>
            <TouchableOpacity onPress={() => console.log("Register pressed")}>
              <Text className="text-indigo-600 dark:text-indigo-400 font-bold">Sign Up</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>
    </Container>
  );
};


export default LoginScreen;
