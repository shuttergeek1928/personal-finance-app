"use client"

import { usePathname } from "next/navigation"
import Link from "next/link"
import { Menu, Wallet } from "lucide-react"
import { navigation } from "./Sidebar"
import { cn } from "@/lib/utils"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose,
} from "@/components/ui/dialog"

export function MobileNav() {
  const pathname = usePathname()

  return (
    <Dialog>
      <DialogTrigger asChild>
        <button className="md:hidden p-2 text-zinc-500 hover:text-zinc-900 dark:text-zinc-400 dark:hover:text-zinc-50 focus:outline-none transition-colors">
          <Menu className="h-6 w-6" />
          <span className="sr-only">Toggle menu</span>
        </button>
      </DialogTrigger>
      <DialogContent className="fixed inset-y-0 left-0 z-50 w-[80%] max-w-sm border-r border-zinc-200 bg-white p-0 shadow-2xl duration-300 transform translate-x-0 dark:border-zinc-800 dark:bg-zinc-950 sm:rounded-none h-full translate-y-0 translate-x-0 top-0 overflow-y-auto">
        <div className="flex h-full flex-col">
          <DialogHeader className="p-6 border-b border-zinc-100 dark:border-zinc-900 text-left">
            <DialogTitle asChild>
              <Link href="/" className="flex items-center gap-2 font-semibold tracking-tight text-xl no-underline">
                 <div className="h-9 w-9 rounded-lg bg-indigo-600 flex items-center justify-center text-white">
                    <Wallet className="h-5 w-5" />
                 </div>
                 <span>FinanceFlow</span>
              </Link>
            </DialogTitle>
          </DialogHeader>
          
          <div className="flex-1 px-4 py-6">
            <nav className="grid gap-2">
              {navigation.map((item) => {
                const isActive = pathname.startsWith(item.href)
                return (
                  <DialogClose asChild key={item.name}>
                    <Link
                      href={item.href}
                      className={cn(
                        "flex items-center gap-4 rounded-xl px-4 py-3.5 text-base font-medium transition-all duration-200",
                        isActive
                          ? "bg-indigo-50 text-indigo-700 dark:bg-indigo-900/20 dark:text-indigo-400"
                          : "text-zinc-600 hover:bg-zinc-100 dark:text-zinc-400 dark:hover:bg-zinc-900 shadow-none border-0"
                      )}
                    >
                      <item.icon className={cn("h-5 w-5", isActive ? "text-indigo-600 dark:text-indigo-400" : "")} />
                      {item.name}
                    </Link>
                  </DialogClose>
                )
              })}
            </nav>
          </div>
          
          <div className="p-6 bg-zinc-50 dark:bg-zinc-900/50 border-t border-zinc-100 dark:border-zinc-900">
             <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="h-10 w-10 bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 rounded-full flex items-center justify-center text-sm font-bold">
                    JD
                  </div>
                  <div>
                    <p className="text-sm font-semibold">John Doe</p>
                    <p className="text-xs text-zinc-500">Premium Plan</p>
                  </div>
                </div>
             </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
