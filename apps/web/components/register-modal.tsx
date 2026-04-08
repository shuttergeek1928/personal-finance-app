"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogTrigger } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { authService } from "@/services/auth";
import { useAuthStore } from "@/store/useAuthStore";
import { ArrowRight } from "lucide-react";

export function RegisterModal({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { login } = useAuthStore();
  const [open, setOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    email: "",
    userName: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
    acceptTerms: false,
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleCheckedChange = (checked: boolean) => {
    setFormData((prev) => ({
      ...prev,
      acceptTerms: checked,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    if (!formData.acceptTerms) {
      setError("Please accept the terms and conditions");
      return;
    }

    try {
      setIsLoading(true);
      const payload = {
        email: formData.email,
        userName: formData.userName,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phoneNumber: formData.phoneNumber,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
        acceptTerms: formData.acceptTerms,
      };

      const res = await authService.register(payload);

      if (res.success) {
        // Auto-login after registration
        try {
          const loginRes = await authService.login({
            email: formData.email,
            password: formData.password,
          });

          if (loginRes.success && loginRes.data) {
            login(loginRes.data.accessToken, loginRes.data.user);
            setOpen(false);
            router.push("/my/dashboard");
            return;
          }
        } catch {
          // If auto-login fails, redirect to login page
        }

        setOpen(false);
        router.push("/auth");
      } else {
        if (res.errors && res.errors.length > 0) {
          setError(res.errors[0]);
        } else {
          setError(res.message || "Registration failed");
        }
      }
    } catch (err: any) {
      const data = err.response?.data;
      if (data?.errors && data.errors.length > 0) {
        setError(data.errors[0]);
      } else {
        const errorMessage = data?.message || err.message || "An error occurred";
        setError(errorMessage);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        {children}
      </DialogTrigger>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle className="text-2xl">Create your account</DialogTitle>
          <DialogDescription>
            Join Finance Flow to start analyzing your wealth instantly.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4 py-4">
          {error && <div className="p-3 text-sm text-red-500 bg-red-100 dark:bg-red-900/30 dark:text-red-400 rounded-md">{error}</div>}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name</Label>
              <Input id="firstName" name="firstName" required value={formData.firstName} onChange={handleChange} placeholder="John" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name</Label>
              <Input id="lastName" name="lastName" required value={formData.lastName} onChange={handleChange} placeholder="Doe" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="userName">Username</Label>
              <Input id="userName" name="userName" required value={formData.userName} onChange={handleChange} placeholder="johndoe123" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="phoneNumber">Phone Number</Label>
              <Input id="phoneNumber" name="phoneNumber" required type="tel" value={formData.phoneNumber} onChange={handleChange} placeholder="+1 (555) 000-0000" />
            </div>
            <div className="space-y-2 md:col-span-2">
              <Label htmlFor="email">Email</Label>
              <Input id="email" name="email" required type="email" value={formData.email} onChange={handleChange} placeholder="john@example.com" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input id="password" name="password" required type="password" value={formData.password} onChange={handleChange} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirm Password</Label>
              <Input id="confirmPassword" name="confirmPassword" required type="password" value={formData.confirmPassword} onChange={handleChange} />
            </div>
          </div>

          <div className="flex items-center space-x-2 pt-2">
            <Checkbox id="terms" checked={formData.acceptTerms} onCheckedChange={handleCheckedChange} />
            <Label htmlFor="terms" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
              I accept the terms and conditions
            </Label>
          </div>

          <div className="pt-4 flex justify-end">
            <Button type="submit" disabled={isLoading} className="w-full md:w-auto bg-indigo-600 hover:bg-indigo-700">
              {isLoading ? "Creating account..." : "Sign Up"} <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
