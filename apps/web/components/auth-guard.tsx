"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthStore } from "@/store/useAuthStore";

export function AuthGuard({
  children,
  requiredRoles,
}: {
  children: React.ReactNode;
  requiredRoles?: string[];
}) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, isLoading, initialize, user } = useAuthStore();

  useEffect(() => {
    initialize();
  }, [initialize]);

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        router.replace(`/auth?redirect=${encodeURIComponent(pathname)}`);
      } else if (requiredRoles && requiredRoles.length > 0) {
        const hasRequiredRole = requiredRoles.some((role) =>
          user?.roles?.includes(role)
        );
        if (!hasRequiredRole) {
          // If not authorized, redirect to a safe page or show error
          router.replace("/my/profile?error=unauthorized");
        }
      }
    }
  }, [isLoading, isAuthenticated, router, pathname, requiredRoles, user]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="flex flex-col items-center gap-4">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
