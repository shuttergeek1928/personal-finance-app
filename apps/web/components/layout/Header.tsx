"use client"

import { usePathname } from "next/navigation"
import { Bell, Search, Menu } from "lucide-react"
import { ThemeToggle } from "@/components/theme-toggle"

export function Header() {
  const pathname = usePathname()
  
  if (pathname === "/" || pathname === "/auth") return null;

  return (
    <header className="sticky top-0 z-10 flex h-16 shrink-0 items-center justify-between border-b border-zinc-200 bg-white/80 px-6 backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-950/80">
      <div className="flex items-center gap-4">
        <button className="md:hidden text-zinc-500 hover:text-zinc-900 dark:text-zinc-400 dark:hover:text-zinc-50">
          <Menu className="h-6 w-6" />
        </button>
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
      </div>
    </header>
  )
}
