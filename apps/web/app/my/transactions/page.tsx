"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import { transactionService, Transaction, TransactionType, TransactionStatus } from "@/services/transaction";
import { accountService, AccountTransferObject } from "@/services/account";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter, DialogClose, DialogDescription } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ArrowDownCircle, ArrowUpCircle, ArrowRightLeft, AlertCircle, Plus, Calendar } from "lucide-react";

export default function MyTransactionsPage() {
  const { user } = useAuthStore();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Form states
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("Food & Dining");
  const [type, setType] = useState<"expense" | "income" | "transfer">("expense");
  const [accountId, setAccountId] = useState("");
  const [toAccountId, setToAccountId] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchData = async () => {
    if (!user) return;
    setLoading(true);
    setError(null);
    try {
      const [txRes, accRes] = await Promise.allSettled([
        transactionService.getTransactionsByUserId(user.id),
        accountService.getAccountsByUserId(user.id),
      ]);

      if (txRes.status === "fulfilled" && txRes.value.success && txRes.value.data) {
        setTransactions(txRes.value.data);
      }

      if (accRes.status === "fulfilled" && accRes.value.success && accRes.value.data) {
        const data = Array.isArray(accRes.value.data) ? accRes.value.data : [accRes.value.data];
        setAccounts(data);
        if (data.length > 0 && !accountId) {
          setAccountId(data[0].id);
        }
      }
    } catch (err: any) {
      setError(err.message || "Failed to fetch transactions.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [user]);

  useEffect(() => {
    if (accounts.length > 0 && !accountId) {
      setAccountId(accounts[0].id);
    }
  }, [accounts]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !amount || !description || !accountId) return;

    setIsSubmitting(true);
    try {
      const selectedAccount = accounts.find(a => a.id === accountId);
      const payload = {
        userId: user.id,
        accountId: accountId,
        amount: parseFloat(amount),
        currency: selectedAccount?.balance?.currency || "INR",
        description,
        category,
        transactionDate: new Date().toISOString(),
        status: TransactionStatus.Approved,
        type: type === "income" ? TransactionType.Income : (type === "expense" ? TransactionType.Expense : TransactionType.Transfer),
        toAccountId: type === "transfer" ? toAccountId : null
      };

      let res;
      if (type === "income") {
        res = await transactionService.createIncome(payload);
      } else if (type === "expense") {
        res = await transactionService.createExpense(payload);
      } else {
        res = await transactionService.createTransfer(payload);
      }

      if (res.success) {
        setIsAddOpen(false);
        setAmount("");
        setDescription("");
        fetchData(); // Refresh
      } else {
        alert(res.message || "Failed to add transaction");
      }
    } catch (err: any) {
      alert(err.message || "Failed to add transaction");
    } finally {
      setIsSubmitting(false);
    }
  };

  const getAccountName = (accountId: string) => {
    const acc = accounts.find(a => a.id === accountId);
    return acc?.name || "Unknown Account";
  };

  const getTypeLabel = (type: TransactionType) => {
    switch (type) {
      case TransactionType.Income: return "Income";
      case TransactionType.Expense: return "Expense";
      case TransactionType.Transfer: return "Transfer";
      default: return "Unknown";
    }
  };

  const getTypeIcon = (type: TransactionType) => {
    switch (type) {
      case TransactionType.Income: return <ArrowDownCircle className="h-5 w-5 text-emerald-500" />;
      case TransactionType.Expense: return <ArrowUpCircle className="h-5 w-5 text-red-500" />;
      case TransactionType.Transfer: return <ArrowRightLeft className="h-5 w-5 text-indigo-500" />;
    }
  };

  const getStatusBadge = (status: TransactionStatus) => {
    switch (status) {
      case TransactionStatus.Pending:
        return <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400">Pending</span>;
      case TransactionStatus.Approved:
        return <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">Approved</span>;
      case TransactionStatus.Rejected:
        return <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400">Rejected</span>;
    }
  };

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading your transactions...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <div className="bg-destructive/10 text-destructive p-4 rounded-md flex items-center gap-2">
          <AlertCircle className="w-5 h-5" /> {error}
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">My Transactions</h1>
          <p className="text-muted-foreground mt-1">View and manage your financial transaction history</p>
        </div>

        <Dialog open={isAddOpen} onOpenChange={setIsAddOpen}>
          <DialogTrigger asChild>
            <Button className="bg-indigo-600 hover:bg-indigo-700 text-white shadow-md">
              <Plus className="h-4 w-4 mr-2" /> Add Transaction
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Add New Transaction</DialogTitle>
              <DialogDescription>Create a new transaction for your account.</DialogDescription>
            </DialogHeader>
            <form onSubmit={handleAdd} className="space-y-4">
              {accounts.length === 0 ? (
                <div className="py-6 text-center space-y-4">
                  <div className="mx-auto w-12 h-12 rounded-full bg-yellow-100 flex items-center justify-center">
                    <AlertCircle className="h-6 w-6 text-yellow-600" />
                  </div>
                  <div className="space-y-1">
                    <p className="font-medium text-zinc-900">No accounts found</p>
                    <p className="text-sm text-zinc-500">You need to create an account before you can add transactions.</p>
                  </div>
                  <Button variant="outline" asChild className="w-full">
                    <a href="/my/accounts">Go to My Accounts</a>
                  </Button>
                </div>
              ) : (
                <>
                  <div className="grid gap-4 py-2">
                    <div className="space-y-2">
                      <Label htmlFor="amount">Amount</Label>
                      <Input id="amount" type="number" step="0.01" required value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0.00" />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="description">Description</Label>
                      <Input id="description" required value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Grocery, Rent, etc." />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Type</Label>
                        <Select value={type} onValueChange={(val) => setType(val as any)}>
                          <SelectTrigger>
                            <SelectValue placeholder="Select type" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="expense">Expense</SelectItem>
                            <SelectItem value="income">Income</SelectItem>
                            <SelectItem value="transfer">Transfer</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Category</Label>
                        <Select value={category} onValueChange={setCategory}>
                          <SelectTrigger>
                            <SelectValue placeholder="Select category" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Food & Dining">Food & Dining</SelectItem>
                            <SelectItem value="Housing">Housing</SelectItem>
                            <SelectItem value="Transport">Transport</SelectItem>
                            <SelectItem value="Entertainment">Entertainment</SelectItem>
                            <SelectItem value="Salary">Salary</SelectItem>
                            <SelectItem value="Other">Other</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                    <div className="space-y-2">
                      <Label>Source Account</Label>
                      <Select value={accountId} onValueChange={setAccountId} required>
                        <SelectTrigger>
                          <SelectValue placeholder="Select account" />
                        </SelectTrigger>
                        <SelectContent>
                          {accounts.map(acc => (
                            <SelectItem key={acc.id} value={acc.id}>{acc.name} ({acc.balance?.amount?.toLocaleString()} {acc.balance?.currency})</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    {type === "transfer" && (
                      <div className="space-y-2">
                        <Label>Destination Account</Label>
                        <Select value={toAccountId} onValueChange={setToAccountId} required>
                          <SelectTrigger>
                            <SelectValue placeholder="Select destination account" />
                          </SelectTrigger>
                          <SelectContent>
                            {accounts.filter(acc => acc.id !== accountId).map(acc => (
                              <SelectItem key={acc.id} value={acc.id}>{acc.name} ({acc.balance?.amount?.toLocaleString()} {acc.balance?.currency})</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    )}
                  </div>
                  <DialogFooter>
                    <DialogClose asChild>
                      <Button variant="outline" type="button">Cancel</Button>
                    </DialogClose>
                    <Button type="submit" disabled={isSubmitting}>
                      {isSubmitting ? "Saving..." : "Save Transaction"}
                    </Button>
                  </DialogFooter>
                </>
              )}
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {transactions.length === 0 ? (
        <Card className="border-dashed bg-muted/50 p-12 text-center text-muted-foreground">
          <ArrowRightLeft className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p>No transactions found yet.</p>
          <p className="text-sm mt-2">Transactions will appear here once you make deposits, withdrawals, or transfers.</p>
          <Button variant="outline" className="mt-4" onClick={() => setIsAddOpen(true)}>
            Add Your First Transaction
          </Button>
        </Card>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">All Transactions ({transactions.length})</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-muted-foreground">
                    <th className="py-3 px-2 font-medium">Date</th>
                    <th className="py-3 px-2 font-medium">Type</th>
                    <th className="py-3 px-2 font-medium">Description</th>
                    <th className="py-3 px-2 font-medium">Category</th>
                    <th className="py-3 px-2 font-medium">Account</th>
                    <th className="py-3 px-2 font-medium">Status</th>
                    <th className="py-3 px-2 font-medium text-right">Amount</th>
                  </tr>
                </thead>
                <tbody>
                  {transactions.map(tx => (
                    <tr key={tx.id} className="border-b last:border-0 hover:bg-muted/30 transition">
                      <td className="py-3 px-2 whitespace-nowrap">
                        <div className="flex items-center gap-1.5">
                          <Calendar className="h-3.5 w-3.5 text-muted-foreground" />
                          {new Date(tx.transactionDate).toLocaleDateString()}
                        </div>
                      </td>
                      <td className="py-3 px-2">
                        <div className="flex items-center gap-2">
                          {getTypeIcon(tx.type)}
                          <span>{getTypeLabel(tx.type)}</span>
                        </div>
                      </td>
                      <td className="py-3 px-2 font-medium">{tx.description || "-"}</td>
                      <td className="py-3 px-2">
                        <span className="inline-flex items-center rounded-full bg-secondary px-2.5 py-0.5 text-xs font-semibold text-secondary-foreground">
                          {tx.category || "Uncategorized"}
                        </span>
                      </td>
                      <td className="py-3 px-2 text-muted-foreground italic">{getAccountName(tx.accountId)}</td>
                      <td className="py-3 px-2">{getStatusBadge(tx.status)}</td>
                      <td className={`py-3 px-2 text-right font-bold text-base ${tx.type === TransactionType.Income ? "text-emerald-600 dark:text-emerald-400" : tx.type === TransactionType.Expense ? "text-foreground" : ""}`}>
                        {tx.type === TransactionType.Income ? "+" : tx.type === TransactionType.Expense ? "-" : ""}
                        {tx.money?.amount?.toLocaleString("en-IN", { style: "currency", currency: tx.money?.currency || "INR" })}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
