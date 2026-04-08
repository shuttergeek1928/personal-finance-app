"use client"

import { usePathname, useRouter } from "next/navigation"
import { Bell, Search, LogOut } from "lucide-react"
import Link from "next/link"
import { ThemeToggle } from "@/components/theme-toggle"
import { MobileNav } from "./MobileNav"
import { useAuthStore } from "@/store/useAuthStore"
import { useEffect } from "react"

export function Header() {
  const pathname = usePathname()
  const router = useRouter()
  const { user, isAuthenticated, initialize, logout } = useAuthStore()

  useEffect(() => {
    initialize()
  }, [initialize])

  if (pathname === "/" || pathname.startsWith("/auth")) return null;

  const handleLogout = () => {
    logout()
    router.push("/auth")
  }

  return (
    <header className="sticky top-0 z-10 flex h-16 shrink-0 items-center justify-between border-b border-zinc-200 bg-white/80 px-6 backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-950/80">
      <div className="flex items-center gap-4">
        <MobileNav />
        <div className="hidden lg:flex relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-zinc-500 dark:text-zinc-400" />
          <input
            type="search"
            placeholder="Search transactions..."
            className="h-9 w-64 rounded-md border border-zinc-200 bg-zinc-50 pl-9 pr-4 text-sm outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 dark:border-zinc-800 dark:bg-zinc-900"
          />
        </div>
      </div>
      <div className="flex items-center gap-4">
        <ThemeToggle />
        <button className="relative text-zinc-500 hover:text-zinc-900 dark:text-zinc-400 dark:hover:text-zinc-50">
          <Bell className="h-5 w-5" />
          <span className="absolute top-0 right-0 h-2 w-2 rounded-full bg-red-500 border-2 border-white dark:border-zinc-950"></span>
        </button>
        {isAuthenticated && user && (
          <div className="hidden md:flex items-center gap-3">
            <Link
              href="/my/profile"
              title="View Profile"
              className="hover:ring-2 hover:ring-indigo-500 hover:ring-offset-2 rounded-full transition-all"
            >
              <div className="h-8 w-8 bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 rounded-full flex items-center justify-center text-xs font-bold">
                {`${(user.firstName || "")[0] || ""}${(user.lastName || "")[0] || ""}`.toUpperCase() || "U"}
              </div>
            </Link>
            <button
              onClick={handleLogout}
              className="text-zinc-500 hover:text-red-600 dark:text-zinc-400 dark:hover:text-red-400 transition"
              title="Sign out"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        )}
      </div>
    </header>
  )
}
