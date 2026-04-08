"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import { accountService, AccountTransferObject } from "@/services/account";
import { transactionService, Transaction } from "@/services/transaction";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Wallet, ArrowUpCircle, ArrowDownCircle, TrendingUp, ArrowRightLeft } from "lucide-react";
import Link from "next/link";

export default function MyDashboardPage() {
  const { user } = useAuthStore();
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;

    const fetchData = async () => {
      setLoading(true);
      try {
        const [accRes, txRes] = await Promise.allSettled([
          accountService.getAccountsByUserId(user.id),
          transactionService.getTransactionsByUserId(user.id),
        ]);

        if (accRes.status === "fulfilled" && accRes.value.success && accRes.value.data) {
          const data = Array.isArray(accRes.value.data) ? accRes.value.data : [accRes.value.data];
          setAccounts(data);
        }

        if (txRes.status === "fulfilled" && txRes.value.success && txRes.value.data) {
          setTransactions(txRes.value.data);
        }
      } catch {
        // silent
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [user]);

  const totalBalance = accounts.reduce((sum, acc) => sum + (acc.balance?.amount || 0), 0);
  const totalIncome = transactions
    .filter((t) => t.type === 0)
    .reduce((sum, t) => sum + (t.money?.amount || 0), 0);
  const totalExpenses = transactions
    .filter((t) => t.type === 1)
    .reduce((sum, t) => sum + (t.money?.amount || 0), 0);

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading your dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-7xl mx-auto space-y-8">
      {/* Greeting */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Welcome back, {user?.firstName || user?.userName || "User"} 👋
        </h1>
        <p className="text-muted-foreground mt-1">Here&apos;s an overview of your financial activity.</p>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="bg-gradient-to-br from-indigo-500 to-indigo-700 text-white border-0 shadow-lg">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-indigo-100">Total Balance</CardTitle>
            <Wallet className="h-5 w-5 text-indigo-200" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {totalBalance.toLocaleString("en-IN", { style: "currency", currency: "INR" })}
            </div>
            <p className="text-xs text-indigo-200 mt-1">{accounts.length} account{accounts.length !== 1 ? "s" : ""}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Income</CardTitle>
            <ArrowDownCircle className="h-5 w-5 text-emerald-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
              +{totalIncome.toLocaleString("en-IN", { style: "currency", currency: "INR" })}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {transactions.filter((t) => t.type === 0).length} transaction{transactions.filter((t) => t.type === 0).length !== 1 ? "s" : ""}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Expenses</CardTitle>
            <ArrowUpCircle className="h-5 w-5 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600 dark:text-red-400">
              -{totalExpenses.toLocaleString("en-IN", { style: "currency", currency: "INR" })}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {transactions.filter((t) => t.type === 1).length} transaction{transactions.filter((t) => t.type === 1).length !== 1 ? "s" : ""}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Net Flow</CardTitle>
            <TrendingUp className="h-5 w-5 text-indigo-500" />
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${totalIncome - totalExpenses >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-red-600 dark:text-red-400"}`}>
              {(totalIncome - totalExpenses).toLocaleString("en-IN", { style: "currency", currency: "INR" })}
            </div>
            <p className="text-xs text-muted-foreground mt-1">Income minus expenses</p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Links */}
      <div className="grid gap-4 md:grid-cols-2">
        <Link href="/my/accounts">
          <Card className="hover:shadow-md transition-shadow cursor-pointer group">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="h-12 w-12 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center group-hover:bg-indigo-200 dark:group-hover:bg-indigo-900/50 transition">
                <Wallet className="h-6 w-6 text-indigo-600 dark:text-indigo-400" />
              </div>
              <div>
                <p className="font-semibold">My Accounts</p>
                <p className="text-sm text-muted-foreground">Manage your bank accounts, deposits, and withdrawals</p>
              </div>
            </CardContent>
          </Card>
        </Link>

        <Link href="/my/transactions">
          <Card className="hover:shadow-md transition-shadow cursor-pointer group">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="h-12 w-12 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center group-hover:bg-emerald-200 dark:group-hover:bg-emerald-900/50 transition">
                <ArrowRightLeft className="h-6 w-6 text-emerald-600 dark:text-emerald-400" />
              </div>
              <div>
                <p className="font-semibold">My Transactions</p>
                <p className="text-sm text-muted-foreground">View your income, expenses, and transfer history</p>
              </div>
            </CardContent>
          </Card>
        </Link>
      </div>

      {/* Recent Transactions */}
      {transactions.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Recent Transactions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {transactions.slice(0, 5).map((tx) => (
                <div key={tx.id} className="flex items-center justify-between py-2 border-b last:border-0">
                  <div className="flex items-center gap-3">
                    <div className={`h-8 w-8 rounded-full flex items-center justify-center ${tx.type === 0 ? "bg-emerald-100 dark:bg-emerald-900/30" : "bg-red-100 dark:bg-red-900/30"}`}>
                      {tx.type === 0 ? (
                        <ArrowDownCircle className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                      ) : (
                        <ArrowUpCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
                      )}
                    </div>
                    <div>
                      <p className="text-sm font-medium">{tx.description || tx.category}</p>
                      <p className="text-xs text-muted-foreground">{new Date(tx.transactionDate).toLocaleDateString()}</p>
                    </div>
                  </div>
                  <span className={`text-sm font-semibold ${tx.type === 0 ? "text-emerald-600 dark:text-emerald-400" : "text-red-600 dark:text-red-400"}`}>
                    {tx.type === 0 ? "+" : "-"}{tx.money?.amount?.toLocaleString("en-IN", { style: "currency", currency: tx.money?.currency || "INR" })}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
