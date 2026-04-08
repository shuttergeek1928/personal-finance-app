"use client";

import { AuthGuard } from "@/components/auth-guard";

export default function MyLayout({ children }: { children: React.ReactNode }) {
  return <AuthGuard>{children}</AuthGuard>;
}
