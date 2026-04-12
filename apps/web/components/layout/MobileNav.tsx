"use client";

import { usePathname, useRouter } from "next/navigation";
import Link from "next/link";
import { Menu, Wallet, LogOut } from "lucide-react";
import { adminNavigation, userNavigation } from "./Sidebar";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/store/useAuthStore";
import { useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose,
} from "@/components/ui/dialog";

export function MobileNav() {
  const pathname = usePathname();
  const router = useRouter();
  const { user, isAuthenticated, initialize, logout, isAdmin } = useAuthStore();

  useEffect(() => {
    initialize();
  }, [initialize]);

  const isUserRoute = pathname.startsWith("/my");
  const navigation = isUserRoute
    ? userNavigation
    : [...userNavigation, ...adminNavigation];

  const handleLogout = () => {
    logout();
    router.push("/auth");
  };

  const initials = user
    ? `${(user.firstName || "")[0] || ""}${
        (user.lastName || "")[0] || ""
      }`.toUpperCase() || (user.userName || "U")[0].toUpperCase()
    : "?";

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
              <Link
                href="/"
                className="flex items-center gap-2 font-semibold tracking-tight text-xl no-underline"
              >
                <div className="h-9 w-9 rounded-lg bg-indigo-600 flex items-center justify-center text-white">
                  <Wallet className="h-5 w-5" />
                </div>
                <span>Finance Flow</span>
              </Link>
            </DialogTitle>
          </DialogHeader>

          <div className="flex-1 px-4 py-6">
            {isAuthenticated && (
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider px-4 mb-3">
                Personal
              </p>
            )}
            <nav className="grid gap-2">
              {isAuthenticated &&
                userNavigation.map((item) => {
                  const isActive = pathname.startsWith(item.href);
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
                        <item.icon
                          className={cn(
                            "h-5 w-5",
                            isActive
                              ? "text-indigo-600 dark:text-indigo-400"
                              : ""
                          )}
                        />
                        {item.name}
                      </Link>
                    </DialogClose>
                  );
                })}
            </nav>

            {isAdmin() && !isUserRoute && (
              <>
                <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider px-4 mt-6 mb-3">
                  Admin
                </p>
                <nav className="grid gap-2">
                  {adminNavigation.map((item) => {
                    const isActive = pathname.startsWith(item.href);
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
                          <item.icon
                            className={cn(
                              "h-5 w-5",
                              isActive
                                ? "text-indigo-600 dark:text-indigo-400"
                                : ""
                            )}
                          />
                          {item.name}
                        </Link>
                      </DialogClose>
                    );
                  })}
                </nav>
              </>
            )}
          </div>

          <div className="p-6 bg-zinc-50 dark:bg-zinc-900/50 border-t border-zinc-100 dark:border-zinc-900">
            {isAuthenticated && user ? (
              <div className="space-y-3">
                <DialogClose asChild>
                  <Link
                    href="/my/profile"
                    className="flex items-center gap-3 p-1 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
                  >
                    <div className="h-10 w-10 bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 rounded-full flex items-center justify-center text-sm font-bold">
                      {initials}
                    </div>
                    <div>
                      <p className="text-sm font-semibold">
                        {user.fullName || user.userName || "User"}
                      </p>
                      <p className="text-xs text-zinc-500 truncate max-w-[150px]">
                        {user.email}
                      </p>
                    </div>
                  </Link>
                </DialogClose>
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
                className="flex items-center justify-center gap-2 rounded-md px-3 py-2.5 text-sm font-medium bg-indigo-600 text-white hover:bg-indigo-700 transition-colors"
              >
                Sign In
              </Link>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
