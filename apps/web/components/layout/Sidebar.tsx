"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { Home, ArrowLeftRight, Wallet, Users, PieChart, LogOut, User } from "lucide-react"
import { cn } from "@/lib/utils"
import { useAuthStore } from "@/store/useAuthStore"
import { useEffect } from "react"

const adminNavigation = [
  { name: "Dashboard", href: "/dashboard", icon: Home },
  { name: "Transactions", href: "/transactions", icon: ArrowLeftRight },
  { name: "Users/Accounts", href: "/users", icon: Users },
]

const userNavigation = [
  { name: "My Dashboard", href: "/my/dashboard", icon: Home },
  { name: "My Transactions", href: "/my/transactions", icon: ArrowLeftRight },
  { name: "My Accounts", href: "/my/accounts", icon: Wallet },
  { name: "My Profile", href: "/my/profile", icon: User },
]

export { adminNavigation, userNavigation }

export function Sidebar() {
  const pathname = usePathname()
  const router = useRouter()
  const { user, isAuthenticated, initialize, logout, isAdmin } = useAuthStore()

  useEffect(() => {
    initialize()
  }, [initialize])

  // Hide sidebar on landing page or auth
  if (pathname === "/" || pathname.startsWith("/auth")) return null;

  const isUserRoute = pathname.startsWith("/my")
  const navigation = isUserRoute ? userNavigation : [...userNavigation, ...adminNavigation]

  const handleLogout = () => {
    logout()
    router.push("/auth")
  }

  const initials = user
    ? `${(user.firstName || "")[0] || ""}${(user.lastName || "")[0] || ""}`.toUpperCase() || (user.userName || "U")[0].toUpperCase()
    : "?"

  return (
    <div className="hidden border-r border-zinc-200 bg-zinc-50/50 dark:border-zinc-800 dark:bg-zinc-900/50 md:flex md:w-64 md:flex-col h-full">
      <div className="flex h-16 items-center border-b border-zinc-200 px-6 dark:border-zinc-800">
        <Link href="/" className="flex items-center gap-2 font-semibold tracking-tight text-lg">
          <div className="h-8 w-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white">
            <Wallet className="h-5 w-5" />
          </div>
          Finance Flow
        </Link>
      </div>
      <div className="flex-1 overflow-auto py-4">
        {/* User section nav */}
        {isAuthenticated && (
          <div className="px-4 mb-2">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider px-3 mb-2">Personal</p>
          </div>
        )}
        <nav className="grid gap-1 px-4">
          {isAuthenticated && userNavigation.map((item) => {
            const isActive = pathname.startsWith(item.href)
            return (
              <Link
                key={item.name}
                href={item.href}
                className={cn(
                  "flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-zinc-200/50 text-zinc-900 dark:bg-zinc-800 dark:text-zinc-50"
                    : "text-zinc-500 hover:bg-zinc-200/30 hover:text-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800/50 dark:hover:text-zinc-50"
                )}
              >
                <item.icon className={cn("h-4 w-4", isActive ? "text-indigo-600 dark:text-indigo-400" : "")} />
                {item.name}
              </Link>
            )
          })}
        </nav>

        {/* Admin section nav - only shown to Admins */}
        {isAdmin() && (
          <>
            <div className="px-4 mt-6 mb-2">
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider px-3 mb-2">Admin</p>
            </div>
            <nav className="grid gap-1 px-4">
              {adminNavigation.map((item) => {
                const isActive = pathname.startsWith(item.href)
                return (
                  <Link
                    key={item.name}
                    href={item.href}
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors",
                      isActive
                        ? "bg-zinc-200/50 text-zinc-900 dark:bg-zinc-800 dark:text-zinc-50"
                        : "text-zinc-500 hover:bg-zinc-200/30 hover:text-zinc-900 dark:text-zinc-400 dark:hover:bg-zinc-800/50 dark:hover:text-zinc-50"
                    )}
                  >
                    <item.icon className={cn("h-4 w-4", isActive ? "text-indigo-600 dark:text-indigo-400" : "")} />
                    {item.name}
                  </Link>
                )
              })}
            </nav>
          </>
        )}
      </div>

      {/* User footer */}
      <div className="p-4 border-t border-zinc-200 dark:border-zinc-800">
        {isAuthenticated && user ? (
          <div className="space-y-3">
            <Link 
              href="/my/profile"
              className="flex items-center gap-3 rounded-md p-2 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors group"
            >
              <div className="h-9 w-9 bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 rounded-full flex items-center justify-center text-sm font-bold group-hover:bg-indigo-200 dark:group-hover:bg-indigo-900/50 transition-colors">
                {initials}
              </div>
              <div className="flex flex-col min-w-0">
                <span className="text-sm font-medium truncate group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">{user.fullName || user.userName || "User"}</span>
                <span className="text-xs text-zinc-500 truncate">{user.email}</span>
              </div>
            </Link>
            <button
              onClick={handleLogout}
              className="w-full flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-50 dark:text-red-400 dark:hover:bg-red-900/20 transition-colors"
            >
              <LogOut className="h-4 w-4" /> Sign Out
            </button>
          </div>
        ) : (
          <Link
            href="/auth"
            className="flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-indigo-600 hover:bg-indigo-50 dark:text-indigo-400 dark:hover:bg-indigo-900/20 transition-colors"
          >
            Sign In
          </Link>
        )}
      </div>
    </div>
  )
}
