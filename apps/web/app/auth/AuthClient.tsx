"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import {
  LineChart as LineChartIcon,
  ArrowRight as ArrowRightIcon,
  Eye as EyeIcon,
  EyeOff as EyeOffIcon,
  ShieldCheck as ShieldCheckIcon,
  Zap as ZapIcon,
} from "lucide-react";

const LineChart = LineChartIcon as any;
const ArrowRight = ArrowRightIcon as any;
const Eye = EyeIcon as any;
const EyeOff = EyeOffIcon as any;
const ShieldCheck = ShieldCheckIcon as any;
const Zap = ZapIcon as any;
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ThemeToggle } from "@/components/theme-toggle";
import { RegisterModal } from "@/components/register-modal";
import { authService } from "@/services/auth";
import { useAuthStore } from "@/store/useAuthStore";
import { useEffect } from "react";

const GOOGLE_CLIENT_ID = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID || "";

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

  const handleGoogleLogin = async (response: any) => {
    setError(null);
    setIsLoading(true);

    try {
      const res = await authService.googleLogin(response.credential);

      if (res.success && res.data) {
        login(res.data.accessToken, res.data.user);
        authService.storeToken(res.data.accessToken, res.data.refreshToken);

        const target =
          redirectUrl ||
          (res.data.user.roles?.includes("Admin")
            ? "/dashboard"
            : "/my/dashboard");

        router.push(target);
      } else {
        setError(res.message || "Google login failed");
      }
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || "Google login failed");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    /* global google */
    if (typeof window !== "undefined" && (window as any).google && GOOGLE_CLIENT_ID) {
      const google = (window as any).google;
      google.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: handleGoogleLogin,
      });

      google.accounts.id.renderButton(
        document.getElementById("google-button"),
        {
          theme: "outline",
          size: "large",
          width: "100%",
          shape: "rectangular",
          text: "continue_with",
          logo_alignment: "left"
        }
      );
    }
  }, [GOOGLE_CLIENT_ID]);

  return (
    <div className="min-h-screen flex selection:bg-indigo-500 selection:text-white">
      {/* Left side: Premium Graphics and Branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-zinc-950 relative overflow-hidden">
        {/* Background Image with Overlay */}
        <div className="absolute inset-0 z-0">
          <Image
            src="/auth-bg.png"
            alt="Finance Visualization"
            fill
            className="object-cover opacity-60"
            priority
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-indigo-900/80 via-transparent to-purple-950/40" />
        </div>

        {/* Floating Decorative Elements */}
        <div className="absolute top-1/4 -left-12 h-64 w-64 rounded-full bg-indigo-600/20 blur-[100px]" />
        <div className="absolute bottom-1/4 -right-12 h-64 w-64 rounded-full bg-purple-600/20 blur-[100px]" />

        <div className="relative z-10 flex flex-col justify-between p-16 w-full text-white">
          <div className="flex items-center gap-3">
            <div className="h-12 w-12 rounded-xl bg-gradient-to-br from-indigo-500 to-indigo-700 flex items-center justify-center shadow-xl shadow-indigo-500/20 border border-white/20">
              <LineChart className="h-7 w-7 text-white" />
            </div>
            <span className="text-3xl font-extrabold tracking-tighter">
              Finance Flow
            </span>
          </div>

          <div className="max-w-md space-y-6">
            <h1 className="text-5xl font-bold leading-tight tracking-tight">
              Master your wealth with{" "}
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-indigo-400 to-purple-400">
                AI Intelligence.
              </span>
            </h1>
            <p className="text-xl text-zinc-300 font-light">
              Experience the next generation of personal finance tracking.
              Secure, automated, and beautifully designed.
            </p>
            <div className="flex gap-6 pt-4">
              <div className="flex items-center gap-2">
                <ShieldCheck className="text-indigo-400 h-5 w-5" />
                <span className="text-sm font-medium text-zinc-400">
                  Bank-level Security
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Zap className="text-amber-400 h-5 w-5" />
                <span className="text-sm font-medium text-zinc-400">
                  Real-time Sync
                </span>
              </div>
            </div>
          </div>

          <div className="text-zinc-500 text-sm italic">
            &copy; 2026 Personal Finance Pro. Empowering financial freedom.
          </div>
        </div>
      </div>

      {/* Right side: Login Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-zinc-50 dark:bg-zinc-950 relative">
        <div className="absolute top-6 right-6">
          <ThemeToggle />
        </div>

        <div className="w-full max-w-md space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-1000">
          <div className="space-y-2">
            <h2 className="text-4xl font-bold tracking-tight">Welcome back</h2>
            <p className="text-muted-foreground">
              Please enter your credentials to access your account.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-500 text-sm font-medium animate-shake">
                {error}
              </div>
            )}

            <div className="space-y-2">
              <Label htmlFor="email">Email Address</Label>
              <Input
                id="email"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="admin@personalfinance.com"
                className="h-12 border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-sm"
              />
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Password</Label>
                <Link
                  href="#"
                  className="text-xs text-indigo-600 hover:underline"
                >
                  Forgot password?
                </Link>
              </div>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="h-12 pr-10 border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 shadow-sm"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-3.5 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-300 transition-colors"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <Button
              type="submit"
              disabled={isLoading}
              className="w-full h-12 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold text-lg shadow-lg shadow-indigo-600/20 transition-all active:scale-[0.98]"
            >
              {isLoading ? (
                <div className="flex items-center gap-2">
                  <div className="h-4 w-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  <span>Signing in...</span>
                </div>
              ) : (
                <div className="flex items-center gap-2">
                  <span>Sign In</span>
                  <ArrowRight size={18} />
                </div>
              )}
            </Button>
          </form>

          <div className="relative pt-4">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t border-zinc-200 dark:border-zinc-800" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-zinc-50 dark:bg-zinc-950 px-2 text-muted-foreground">
                Or continue with
              </span>
            </div>
          </div>

          <div id="google-button" className="w-full flex justify-center" />

          <div className="flex flex-col gap-4 text-center">
            <RegisterModal>
              <button className="text-sm font-medium hover:text-indigo-600 transition-colors">
                Don't have an account?{" "}
                <span className="text-indigo-600">Sign up</span>
              </button>
            </RegisterModal>

            <Link
              href="/"
              className="text-xs text-muted-foreground hover:text-zinc-900 dark:hover:text-zinc-100 transition-colors"
            >
              &larr; Back to home
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
