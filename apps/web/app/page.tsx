"use client"

import Link from "next/link";
import { Bot, LineChart, Shield, Smartphone, ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/theme-toggle";
import { RegisterModal } from "@/components/register-modal";

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      <header className="fixed top-0 w-full border-b border-zinc-200 bg-white/80 backdrop-blur-md dark:border-zinc-800 dark:bg-zinc-950/80 z-50">
        <div className="container mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-2 font-bold text-xl tracking-tighter">
            <div className="h-8 w-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white">
              <LineChart className="h-5 w-5" />
            </div>
            Finance Flow
          </div>
          <nav className="hidden md:flex gap-6 text-sm font-medium text-zinc-600 dark:text-zinc-400">
            <Link href="#features" className="hover:text-zinc-900 dark:hover:text-zinc-50 transition">Features</Link>
            <Link href="#how-it-works" className="hover:text-zinc-900 dark:hover:text-zinc-50 transition">How it Works</Link>
            <Link href="#pricing" className="hover:text-zinc-900 dark:hover:text-zinc-50 transition">Pricing</Link>
          </nav>
          <div className="flex items-center gap-4">
            <ThemeToggle />
            <Link href="/auth" className="text-sm font-medium hover:underline underline-offset-4">
              Log in
            </Link>
            <RegisterModal>
              <button className="text-sm font-medium hover:underline underline-offset-4">
                Sign Up
              </button>
            </RegisterModal>
            <Button asChild className="bg-indigo-600 hover:bg-indigo-700 text-white rounded-full">
              <Link href="/auth">Get Started</Link>
            </Button>
          </div>
        </div>
      </header>

      <main className="pt-32 pb-16">
        <section className="container mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 text-center">
          <h1 className="text-5xl md:text-7xl font-bold tracking-tight mb-6 bg-gradient-to-r from-zinc-900 to-zinc-500 bg-clip-text text-transparent dark:from-white dark:to-zinc-500">
            AI-Powered Personal <br className="hidden md:block" /> Finance Optimization
          </h1>
          <p className="max-w-2xl mx-auto text-lg md:text-xl text-zinc-600 dark:text-zinc-400 mb-10">
            Track your wealth across bank accounts, set automated budgets, and get AI recommendations to improve your financial situation.
          </p>
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Button size="lg" asChild className="h-12 px-8 rounded-full bg-indigo-600 text-white hover:bg-indigo-700 text-base">
              <Link href="/auth">
                Start for Free <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
            <Button size="lg" variant="outline" className="h-12 px-8 rounded-full text-base">
              View Demo
            </Button>
          </div>

          <div className="mt-20 relative mx-auto max-w-4xl rounded-xl border border-zinc-200 bg-white p-2 shadow-2xl dark:border-zinc-800 dark:bg-zinc-900">
            <div className="aspect-[16/9] bg-zinc-100 dark:bg-zinc-950 rounded-lg flex items-center justify-center border border-zinc-200 dark:border-zinc-800 overflow-hidden">
              {/* Concept placeholder for an image/app screenshot */}
              <div className="absolute inset-0 bg-gradient-to-tr from-indigo-500/10 to-purple-500/10" />
              <LineChart className="h-24 w-24 text-zinc-300 dark:text-zinc-700" />
              <p className="absolute bottom-4 font-medium text-zinc-500">Dashboard Preview</p>
            </div>
          </div>
        </section>

        <section id="features" className="container mx-auto max-w-6xl px-4 sm:px-6 lg:px-8 mt-32">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold tracking-tight mb-4">Everything you need to grow your wealth</h2>
            <p className="text-zinc-600 dark:text-zinc-400 max-w-2xl mx-auto">
              Our platform syncs seamlessly with all your financial institutions to give you a complete picture of your net worth and spending.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            <div className="p-6 rounded-2xl bg-white border border-zinc-200 shadow-sm dark:bg-zinc-900 dark:border-zinc-800">
              <div className="h-12 w-12 rounded-lg bg-indigo-100 text-indigo-600 dark:bg-indigo-900/30 dark:text-indigo-400 flex items-center justify-center mb-6">
                <Bot className="h-6 w-6" />
              </div>
              <h3 className="text-xl font-semibold mb-2">AI Insights</h3>
              <p className="text-zinc-600 dark:text-zinc-400">
                Get personalized recommendations on how to cut unnecessary subscriptions, pay down debt faster, and optimize savings.
              </p>
            </div>
            <div className="p-6 rounded-2xl bg-white border border-zinc-200 shadow-sm dark:bg-zinc-900 dark:border-zinc-800">
              <div className="h-12 w-12 rounded-lg bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400 flex items-center justify-center mb-6">
                <Shield className="h-6 w-6" />
              </div>
              <h3 className="text-xl font-semibold mb-2">Bank-grade Security</h3>
              <p className="text-zinc-600 dark:text-zinc-400">
                Your data is protected with 256-bit encryption. We never store your banking credentials or sell your personal data.
              </p>
            </div>
            <div className="p-6 rounded-2xl bg-white border border-zinc-200 shadow-sm dark:bg-zinc-900 dark:border-zinc-800">
              <div className="h-12 w-12 rounded-lg bg-pink-100 text-pink-600 dark:bg-pink-900/30 dark:text-pink-400 flex items-center justify-center mb-6">
                <Smartphone className="h-6 w-6" />
              </div>
              <h3 className="text-xl font-semibold mb-2">Omnichannel Access</h3>
              <p className="text-zinc-600 dark:text-zinc-400">
                Track your finances on the go with our beautifully designed mobile applications for iOS and Android.
              </p>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-950 py-12 text-center text-sm text-zinc-500">
        <div className="container mx-auto px-4 max-w-6xl flex flex-col md:flex-row justify-between items-center">
          <div className="flex items-center gap-2 font-semibold tracking-tight text-zinc-900 dark:text-zinc-100 mb-4 md:mb-0">
            <LineChart className="h-5 w-5 text-indigo-600" />
            Finance Flow
          </div>
          <div className="flex gap-6">
            <Link href="#" className="hover:text-zinc-900 dark:hover:text-zinc-200">Privacy</Link>
            <Link href="#" className="hover:text-zinc-900 dark:hover:text-zinc-200">Terms</Link>
            <Link href="#" className="hover:text-zinc-900 dark:hover:text-zinc-200">Contact</Link>
          </div>
        </div>
      </footer>
    </div>
  )
}
