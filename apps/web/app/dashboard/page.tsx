"use client";

import { useEffect, useMemo } from "react";
import { useFinanceStore } from "@/store/useFinanceStore";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  ArrowDownIcon,
  ArrowUpIcon,
  CreditCard,
  DollarSign,
  Activity,
} from "lucide-react";
import {
  Bar,
  BarChart,
  ResponsiveContainer,
  XAxis,
  YAxis,
  Tooltip,
} from "recharts";

import { AuthGuard } from "@/components/auth-guard";

function DashboardContent() {
  const { accounts, transactions, isLoading, fetchDashboardData } =
    useFinanceStore();

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);

  const totalBalance = useMemo(
    () => accounts.reduce((acc, curr) => acc + curr.balance, 0),
    [accounts]
  );

  const income = useMemo(
    () =>
      transactions
        .filter((t) => t.type === "income")
        .reduce((acc, curr) => acc + curr.amount, 0),
    [transactions]
  );
  const expenses = useMemo(
    () =>
      transactions
        .filter((t) => t.type === "expense")
        .reduce((acc, curr) => acc + curr.amount, 0),
    [transactions]
  );

  const chartData = useMemo(() => {
    const dataMap: Record<string, number> = {};
    transactions.forEach((t) => {
      const date = new Date(t.date).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });
      if (!dataMap[date]) dataMap[date] = 0;
      if (t.type === "expense") dataMap[date] += t.amount;
    });
    return Object.keys(dataMap)
      .map((date) => ({ date, amount: dataMap[date] }))
      .reverse();
  }, [transactions]);

  if (isLoading) {
    return (
      <div className="flex-1 p-8 space-y-6">
        <div className="h-8 w-48 bg-zinc-200 dark:bg-zinc-800 rounded animate-pulse"></div>
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <div
              key={i}
              className="h-32 bg-zinc-200 dark:bg-zinc-800 rounded-xl animate-pulse"
            />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 space-y-8 p-8 max-w-7xl mx-auto">
      <div className="flex items-center justify-between space-y-2">
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Balance</CardTitle>
            <DollarSign className="h-4 w-4 text-zinc-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              $
              {totalBalance.toLocaleString("en-US", {
                minimumFractionDigits: 2,
              })}
            </div>
            <p className="text-xs text-zinc-500 mt-1">
              Across all {accounts.length} accounts
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Income</CardTitle>
            <ArrowUpIcon className="h-4 w-4 text-emerald-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
              +${income.toLocaleString("en-US", { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-zinc-500 mt-1">This month</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Total Expenses
            </CardTitle>
            <ArrowDownIcon className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600 dark:text-red-400">
              -${expenses.toLocaleString("en-US", { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-zinc-500 mt-1">This month</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Active Accounts
            </CardTitle>
            <CreditCard className="h-4 w-4 text-zinc-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{accounts.length}</div>
            <p className="text-xs text-zinc-500 mt-1">Synced successfully</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
        <Card className="col-span-4">
          <CardHeader>
            <CardTitle>Spending Overview</CardTitle>
            <CardDescription>
              Your expenses over the last 30 days.
            </CardDescription>
          </CardHeader>
          <CardContent className="pl-2">
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={350}>
                <BarChart data={chartData}>
                  <XAxis
                    dataKey="date"
                    stroke="#888888"
                    fontSize={12}
                    tickLine={false}
                    axisLine={false}
                  />
                  <YAxis
                    stroke="#888888"
                    fontSize={12}
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(value) => `$${value}`}
                  />
                  <Tooltip cursor={{ fill: "rgba(0,0,0,0.05)" }} />
                  <Bar
                    dataKey="amount"
                    fill="currentColor"
                    className="fill-indigo-600 dark:fill-indigo-400"
                    radius={[4, 4, 0, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[350px] flex items-center justify-center text-zinc-500">
                <div className="flex flex-col items-center">
                  <Activity className="h-10 w-10 mb-4 opacity-20" />
                  <p>No expense data available</p>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="col-span-3">
          <CardHeader>
            <CardTitle>Recent Transactions</CardTitle>
            <CardDescription>
              You have {transactions.length} transactions total finding your
              recent activity.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-6">
              {transactions.slice(0, 5).map((t) => (
                <div key={t.id} className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-sm font-medium leading-none">
                      {t.description}
                    </p>
                    <p className="text-sm text-zinc-500 dark:text-zinc-400">
                      {t.category}
                    </p>
                  </div>
                  <div
                    className={`font-medium ${
                      t.type === "expense"
                        ? "text-zinc-900 dark:text-zinc-100"
                        : "text-emerald-600 dark:text-emerald-400"
                    }`}
                  >
                    {t.type === "expense" ? "-" : "+"}$
                    {t.amount.toLocaleString("en-US", {
                      minimumFractionDigits: 2,
                    })}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function DashboardPage() {
  return (
    <AuthGuard requiredRoles={["Admin"]}>
      <DashboardContent />
    </AuthGuard>
  );
}
