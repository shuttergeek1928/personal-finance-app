"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { Home, PieChart, ArrowLeftRight, Settings, Wallet } from "lucide-react"

import { cn } from "@/lib/utils"

export const navigation = [
  { name: "Dashboard", href: "/dashboard", icon: Home },
  { name: "Transactions", href: "/transactions", icon: ArrowLeftRight },
  { name: "Users/Accounts", href: "/users", icon: Wallet },
  // { name: "Budgets", href: "/budgets", icon: PieChart },
  // { name: "Settings", href: "/settings", icon: Settings },
]

export function Sidebar() {
  const pathname = usePathname()

  // Hide sidebar on landing page or auth
  if (pathname === "/" || pathname === "/auth") return null;

  return (
    <div className="hidden border-r border-zinc-200 bg-zinc-50/50 dark:border-zinc-800 dark:bg-zinc-900/50 md:flex md:w-64 md:flex-col min-h-screen">
      <div className="flex h-16 items-center border-b border-zinc-200 px-6 dark:border-zinc-800">
        <Link href="/" className="flex items-center gap-2 font-semibold tracking-tight text-lg">
          <div className="h-8 w-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white">
            <Wallet className="h-5 w-5" />
          </div>
          FinanceFlow
        </Link>
      </div>
      <div className="flex-1 overflow-auto py-4">
        <nav className="grid gap-1 px-4">
          {navigation.map((item) => {
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
      </div>
      <div className="p-4 border-t border-zinc-200 dark:border-zinc-800">
        <div className="flex items-center gap-3 rounded-md p-2 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition cursor-pointer">
          <div className="h-9 w-9 bg-zinc-200 dark:bg-zinc-800 rounded-full flex items-center justify-center text-sm font-bold">
            JD
          </div>
          <div className="flex flex-col">
            <span className="text-sm font-medium">John Doe</span>
            <span className="text-xs text-zinc-500">Premium Plan</span>
          </div>
        </div>
      </div>
    </div>
  )
}
