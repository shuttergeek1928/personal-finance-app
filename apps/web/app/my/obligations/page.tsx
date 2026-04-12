"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import {
  obligationService,
  ObligationDashboardDto,
  LiabilityDto,
  SubscriptionDto,
  LiabilityTypeLabels,
  SubscriptionTypeLabels,
  BillingCycleLabels,
  AmortizationScheduleDto,
  LiabilityType,
  BillingCycle,
  getLiabilityProgress,
} from "@/services/obligation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Landmark,
  TrendingDown,
  CalendarClock,
  Receipt,
  Plus,
  ChevronRight,
  BarChart3,
  RefreshCw,
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";

export default function ObligationsPage() {
  const { user } = useAuthStore();
  const router = useRouter();
  const [dashboard, setDashboard] = useState<ObligationDashboardDto | null>(
    null
  );
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;
    const fetchData = async () => {
      setLoading(true);
      try {
        const res = await obligationService.getDashboard(user.id);
        if (res.success && res.data) {
          setDashboard(res.data);
        }
      } catch {
        // silent
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [user]);

  const fmt = (amount: number) =>
    amount.toLocaleString("en-IN", { style: "currency", currency: "INR" });

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">
            Loading your obligations...
          </p>
        </div>
      </div>
    );
  }

  const d = dashboard;
  const liabilities = d?.liabilities ?? [];
  const subscriptions = d?.subscriptions ?? [];

  return (
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Obligations</h1>
          <p className="text-muted-foreground mt-1">
            Track your loans, EMIs, and recurring subscriptions.
          </p>
        </div>
        <div className="flex gap-2">
          <Link
            href="/my/obligations/liabilities"
            className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 transition-colors shadow-md hover:shadow-lg"
          >
            <Plus className="h-4 w-4" /> Add Loan
          </Link>
          <Link
            href="/my/obligations/subscriptions"
            className="inline-flex items-center gap-2 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 px-4 py-2.5 text-sm font-medium hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors"
          >
            <Plus className="h-4 w-4" /> Add Subscription
          </Link>
          <Link
            href="/my/obligations/credit-cards"
            className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-sky-700 transition-colors shadow-md hover:shadow-lg"
          >
            <Plus className="h-4 w-4" /> Manage Cards
          </Link>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="bg-gradient-to-br from-rose-500 to-rose-700 text-white border-0 shadow-lg">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-rose-100">
              Outstanding Balance
            </CardTitle>
            <TrendingDown className="h-5 w-5 text-rose-200" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {fmt(d?.totalOutstandingBalance ?? 0)}
            </div>
            <p className="text-xs text-rose-200 mt-1">
              {d?.totalActiveLiabilities ?? 0} active loan
              {(d?.totalActiveLiabilities ?? 0) !== 1 ? "s" : ""}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Monthly EMI
            </CardTitle>
            <Landmark className="h-5 w-5 text-amber-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-amber-600 dark:text-amber-400">
              {fmt(d?.totalMonthlyEmi ?? 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Across all loans
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Subscriptions / mo
            </CardTitle>
            <Receipt className="h-5 w-5 text-violet-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-violet-600 dark:text-violet-400">
              {fmt(d?.totalMonthlySubscriptionCost ?? 0)}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {d?.totalActiveSubscriptions ?? 0} active subscription
              {(d?.totalActiveSubscriptions ?? 0) !== 1 ? "s" : ""}
            </p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-to-br from-indigo-500 to-indigo-700 text-white border-0 shadow-lg">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-indigo-100">
              Total / Month
            </CardTitle>
            <BarChart3 className="h-5 w-5 text-indigo-200" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {fmt(d?.totalMonthlyObligations ?? 0)}
            </div>
            <p className="text-xs text-indigo-200 mt-1">EMIs + Subscriptions</p>
          </CardContent>
        </Card>
      </div>

      {/* Liabilities Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold tracking-tight">Loans & EMIs</h2>
          <Link
            href="/my/obligations/liabilities"
            className="text-sm text-indigo-600 dark:text-indigo-400 hover:underline inline-flex items-center gap-1"
          >
            View all <ChevronRight className="h-3 w-3" />
          </Link>
        </div>

        {liabilities.length === 0 ? (
          <Card className="border-dashed">
            <CardContent className="flex flex-col items-center justify-center py-12 text-center">
              <Landmark className="h-10 w-10 text-muted-foreground/40 mb-3" />
              <p className="text-muted-foreground font-medium">
                No active loans
              </p>
              <p className="text-sm text-muted-foreground/70 mt-1">
                Add a loan or EMI to start tracking.
              </p>
              <Link
                href="/my/obligations/liabilities"
                className="mt-4 inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors"
              >
                <Plus className="h-4 w-4" /> Add Loan
              </Link>
            </CardContent>
          </Card>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {liabilities.slice(0, 6).map((l) => {
              const { paidPercent, effectiveOutstanding } =
                getLiabilityProgress(l);
              return (
                <Card
                  key={l.id}
                  className="group hover:shadow-lg transition-all cursor-pointer hover:border-indigo-300 dark:hover:border-indigo-700"
                  onClick={() => router.push("/my/obligations/liabilities")}
                >
                  <CardContent className="p-5 space-y-4">
                    <div className="flex items-start justify-between">
                      <div>
                        <p className="font-semibold text-base">{l.name}</p>
                        <p className="text-xs text-muted-foreground">
                          {l.lenderName} · {LiabilityTypeLabels[l.type]}
                        </p>
                      </div>
                      <span className="text-xs px-2 py-1 rounded-full bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 font-medium">
                        {l.interestRate}% p.a.
                      </span>
                    </div>

                    {/* Progress bar */}
                    <div>
                      <div className="flex justify-between text-xs text-muted-foreground mb-1.5">
                        <span>Paid: {paidPercent}%</span>
                        <span>Remaining: {fmt(effectiveOutstanding)}</span>
                      </div>
                      <div className="h-2 rounded-full bg-zinc-200 dark:bg-zinc-700 overflow-hidden">
                        <div
                          className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400 transition-all"
                          style={{ width: `${paidPercent}%` }}
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-3 text-sm">
                      <div>
                        <p className="text-muted-foreground text-xs">
                          EMI / Month
                        </p>
                        <p className="font-semibold">
                          {fmt(l.emiAmount.amount)}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground text-xs">Tenure</p>
                        <p className="font-semibold">{l.tenureMonths} months</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        )}
      </div>

      {/* Subscriptions Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold tracking-tight">
            Subscriptions
          </h2>
          <Link
            href="/my/obligations/subscriptions"
            className="text-sm text-indigo-600 dark:text-indigo-400 hover:underline inline-flex items-center gap-1"
          >
            View all <ChevronRight className="h-3 w-3" />
          </Link>
        </div>

        {subscriptions.length === 0 ? (
          <Card className="border-dashed">
            <CardContent className="flex flex-col items-center justify-center py-12 text-center">
              <Receipt className="h-10 w-10 text-muted-foreground/40 mb-3" />
              <p className="text-muted-foreground font-medium">
                No active subscriptions
              </p>
              <p className="text-sm text-muted-foreground/70 mt-1">
                Add a subscription to start tracking.
              </p>
              <Link
                href="/my/obligations/subscriptions"
                className="mt-4 inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors"
              >
                <Plus className="h-4 w-4" /> Add Subscription
              </Link>
            </CardContent>
          </Card>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {subscriptions.slice(0, 6).map((s) => (
              <Card
                key={s.id}
                className="group hover:shadow-lg transition-all cursor-pointer hover:border-indigo-300 dark:hover:border-indigo-700"
                onClick={() => router.push("/my/obligations/subscriptions")}
              >
                <CardContent className="p-5 space-y-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="font-semibold text-base">{s.name}</p>
                      <p className="text-xs text-muted-foreground">
                        {s.provider} · {SubscriptionTypeLabels[s.type]}
                      </p>
                    </div>
                    <span
                      className={`text-xs px-2 py-1 rounded-full font-medium ${
                        s.autoRenew
                          ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400"
                          : "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400"
                      }`}
                    >
                      {s.autoRenew ? (
                        <span className="inline-flex items-center gap-1">
                          <RefreshCw className="h-3 w-3" /> Auto
                        </span>
                      ) : (
                        "Manual"
                      )}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div>
                      <p className="text-muted-foreground text-xs">Amount</p>
                      <p className="font-semibold">{fmt(s.amount.amount)}</p>
                    </div>
                    <div>
                      <p className="text-muted-foreground text-xs">Cycle</p>
                      <p className="font-semibold">
                        {BillingCycleLabels[s.billingCycle]}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 text-xs text-muted-foreground">
                    <CalendarClock className="h-3.5 w-3.5" />
                    Next:{" "}
                    {new Date(s.nextBillingDate).toLocaleDateString("en-IN", {
                      day: "numeric",
                      month: "short",
                      year: "numeric",
                    })}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
