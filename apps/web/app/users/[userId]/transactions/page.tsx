"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { userService, UserTransferObject } from "../../../../services/user";
import {
  accountService,
  AccountTransferObject,
} from "../../../../services/account";
import {
  transactionService,
  Transaction,
  TransactionType,
  TransactionStatus,
} from "../../../../services/transaction";
import {
  AlertCircle,
  ArrowLeft,
  Search,
  RefreshCw,
  FileText,
} from "lucide-react";
import { Input } from "@/components/ui/input";
import Link from "next/link";

export default function UserTransactionsPage() {
  const params = useParams<{ userId: string }>();

  const [currentUser, setCurrentUser] = useState<UserTransferObject | null>(
    null
  );
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      if (!params || !params.userId) {
        setError("User ID not provided in route.");
        return;
      }

      // 1. Fetch User
      const userRes = await userService.getUserById(params.userId);
      if (!userRes.success || !userRes.data) {
        setError(userRes.message || "Failed to find the specified user.");
        return;
      }
      setCurrentUser(userRes.data);

      // 2. Fetch Accounts (to map names)
      try {
        const accRes = await accountService.getAccountsByUserId(params.userId);
        if (accRes.success && accRes.data) {
          setAccounts(accRes.data);
        }
      } catch (accErr) {
        console.warn("Failed to fetch accounts for mapping", accErr);
      }

      // 3. Fetch Transactions
      const transRes = await transactionService.getTransactionsByUserId(
        params.userId
      );
      if (transRes.success && transRes.data) {
        setTransactions(transRes.data);
      } else {
        setTransactions([]);
        if (!transRes.success && transRes.message) {
          console.warn("Transaction fetch message:", transRes.message);
        }
      }
    } catch (err: any) {
      console.error(err);
      setError(err.message || "Failed to fetch data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (params?.userId) {
      fetchData();
    }
  }, [params?.userId]);

  const getAccountName = (accountId?: string) => {
    if (!accountId) return "N/A";
    const account = accounts.find((a) => a.id === accountId);
    return account?.name || "Unknown Account";
  };

  const getTransactionTypeName = (type: TransactionType) => {
    switch (type) {
      case TransactionType.Income:
        return "Income";
      case TransactionType.Expense:
        return "Expense";
      case TransactionType.Transfer:
        return "Transfer";
      default:
        return "Other";
    }
  };

  const filteredTransactions = transactions.filter(
    (t) =>
      t.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      t.category?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      getAccountName(t.accountId)
        .toLowerCase()
        .includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="p-8 text-center text-muted-foreground">
        Loading transactions...
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2">
          <AlertCircle className="w-5 h-5" />
          {error}
        </div>
        <div className="mt-4">
          <Link href="/users">
            <Button variant="outline" size="sm">
              Back to Users
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      <div className="flex items-center gap-4">
        <Link href={`/users/${params.userId}`} passHref>
          <Button variant="outline" size="icon" className="shrink-0">
            <ArrowLeft className="w-4 h-4" />
          </Button>
        </Link>
        <div className="flex-1">
          <h1 className="text-3xl font-bold tracking-tight">Transactions</h1>
          <p className="text-muted-foreground mt-1">
            Showing transaction history for{" "}
            <span className="font-semibold text-foreground">
              {currentUser?.fullName || currentUser?.email || "Unknown User"}
            </span>
          </p>
        </div>
        <Button
          variant="outline"
          size="sm"
          className="gap-2"
          onClick={fetchData}
        >
          <RefreshCw className="w-4 h-4" /> Refresh
        </Button>
      </div>

      <div className="flex items-center gap-2 max-w-sm relative">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Filter by description, category or account..."
          className="pl-9"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      <div className="rounded-md border border-border overflow-hidden bg-card">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-muted border-b border-border">
              <tr>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  User Name
                </th>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  Date
                </th>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  Description
                </th>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  Category
                </th>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  Account Name
                </th>
                <th className="h-10 px-4 text-left font-medium text-muted-foreground">
                  Status
                </th>
                <th className="h-10 px-4 text-right font-medium text-muted-foreground">
                  Amount
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {filteredTransactions.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="h-32 text-center text-muted-foreground italic"
                  >
                    No transactions found for this user.
                  </td>
                </tr>
              ) : (
                filteredTransactions.map((t) => {
                  return (
                    <tr
                      key={t.id}
                      className="hover:bg-muted/50 transition-colors"
                    >
                      <td className="p-4 align-middle font-medium whitespace-nowrap">
                        {currentUser?.fullName ||
                          currentUser?.userName ||
                          currentUser?.email ||
                          "N/A"}
                      </td>
                      <td className="p-4 align-middle text-muted-foreground whitespace-nowrap">
                        {new Date(t.transactionDate).toLocaleDateString()}
                      </td>
                      <td className="p-4 align-middle">
                        <div className="flex flex-col gap-0.5">
                          <span className="font-medium">
                            {t.description || "No description"}
                          </span>
                          <span className="text-[10px] text-muted-foreground font-mono uppercase tracking-wider">
                            {getTransactionTypeName(t.type)}
                          </span>
                        </div>
                      </td>
                      <td className="p-4 align-middle">
                        <span className="inline-flex items-center rounded-full border border-border px-2.5 py-0.5 text-xs font-semibold bg-background">
                          {t.category || "General"}
                        </span>
                      </td>
                      <td className="p-4 align-middle text-muted-foreground italic">
                        {getAccountName(t.accountId)}
                      </td>
                      <td className="p-4 align-middle">
                        {t.status === 2 /* TransactionStatus.Rejected */ ? (
                          <div className="flex flex-col">
                            <span className="inline-flex items-center rounded-full bg-destructive/10 px-2.5 py-0.5 text-xs font-semibold text-destructive w-fit">
                              Rejected
                            </span>
                            {t.rejectionReason && (
                              <span
                                className="text-[10px] text-destructive/80 mt-1 max-w-[120px] truncate"
                                title={t.rejectionReason}
                              >
                                {t.rejectionReason}
                              </span>
                            )}
                          </div>
                        ) : t.status === 0 /* TransactionStatus.Pending */ ? (
                          <span className="inline-flex items-center rounded-full bg-amber-100/50 px-2.5 py-0.5 text-xs font-semibold text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                            Pending
                          </span>
                        ) : (
                          <span className="inline-flex items-center rounded-full bg-emerald-100/50 px-2.5 py-0.5 text-xs font-semibold text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                            Approved
                          </span>
                        )}
                      </td>
                      <td
                        className={`p-4 align-middle text-right font-bold text-base ${
                          t.status === 2
                            ? "text-muted-foreground line-through"
                            : t.type === TransactionType.Expense
                            ? "text-foreground"
                            : "text-emerald-600"
                        }`}
                      >
                        {t.type === TransactionType.Expense ? "-" : "+"}
                        {t.money.amount.toLocaleString("en-US", {
                          style: "currency",
                          currency: t.money.currency || "INR",
                        })}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
