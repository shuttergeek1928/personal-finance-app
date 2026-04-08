"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { LineChart, ArrowRight, Eye, EyeOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ThemeToggle } from "@/components/theme-toggle";
import { RegisterModal } from "@/components/register-modal";
import { authService } from "@/services/auth";
import { useAuthStore } from "@/store/useAuthStore";

export default function AuthClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectUrl = searchParams.get("redirect") || "/my/dashboard";

  const { login } = useAuthStore();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const res = await authService.login({ email, password });

      if (res.success && res.data) {
        login(res.data.accessToken, res.data.user);

        const target =
          redirectUrl ||
          (res.data.user.roles?.includes("Admin")
            ? "/dashboard"
            : "/my/dashboard");

        router.push(target);
      } else {
        if (res.errors && res.errors.length > 0) {
          setError(res.errors[0]);
        } else {
          setError(res.message || "Login failed");
        }
      }
    } catch (err: any) {
      const data = err.response?.data;
      if (data?.errors && data.errors.length > 0) {
        setError(data.errors[0]);
      } else {
        setError(data?.message || err.message || "An error occurred");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-indigo-600 via-indigo-700 to-purple-800 relative overflow-hidden">
        <div className="absolute inset-0 opacity-30" />
        <div className="relative z-10 flex flex-col justify-center px-16">
          <div className="flex items-center gap-3 mb-12">
            <div className="h-12 w-12 rounded-xl bg-white/20 flex items-center justify-center">
              <LineChart className="h-7 w-7 text-white" />
            </div>
            <span className="text-3xl font-bold text-white">Finance Flow</span>
          </div>
        </div>
      </div>

      <div className="flex-1 flex items-center justify-center p-8">
        <div className="absolute top-4 right-4">
          <ThemeToggle />
        </div>

        <div className="w-full max-w-md space-y-8">
          <h2 className="text-3xl font-bold">Welcome back</h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            {error && <div className="text-red-500">{error}</div>}

            <Input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Email"
            />

            <div className="relative">
              <Input
                type={showPassword ? "text" : "password"}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Password"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-2 top-2"
              >
                {showPassword ? <EyeOff /> : <Eye />}
              </button>
            </div>

            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Loading..." : "Sign In"}
            </Button>
          </form>

          <RegisterModal>
            <button>Sign up</button>
          </RegisterModal>

          <Link href="/">Back to home</Link>
        </div>
      </div>
    </div>
  );
}