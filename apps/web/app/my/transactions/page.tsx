"use client";

import { useEffect, useState, useMemo } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import { transactionService, Transaction, TransactionType, TransactionStatus } from "@/services/transaction";
import { accountService, AccountTransferObject } from "@/services/account";
import { obligationService, CreditCardDto, CreditCardNetworkLabels } from "@/services/obligation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter, DialogClose, DialogDescription } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ArrowDownCircle, ArrowUpCircle, ArrowRightLeft, AlertCircle, Plus, Calendar, CreditCard as CreditCardIcon, Landmark, Wallet, TrendingDown, TrendingUp } from "lucide-react";
import { cn } from "@/lib/utils";

export default function MyTransactionsPage() {
  const { user } = useAuthStore();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [creditCards, setCreditCards] = useState<CreditCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Form states
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("Food & Dining");
  const [type, setType] = useState<"expense" | "income" | "transfer">("expense");
  
  // Source states
  const [sourceType, setSourceType] = useState<"account" | "card">("account");
  const [accountId, setAccountId] = useState("");
  const [creditCardId, setCreditCardId] = useState("");
  
  // Destination states (for transfers)
  const [toSourceType, setToSourceType] = useState<"account" | "card">("account");
  const [toAccountId, setToAccountId] = useState("");
  const [toCreditCardId, setToCreditCardId] = useState("");
  
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Filter & Search states
  const [filterSourceType, setFilterSourceType] = useState<"all" | "account" | "card">("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCreditCard, setSelectedCreditCard] = useState<CreditCardDto | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const PAGE_SIZE = 15;

  const fetchData = async () => {
    if (!user) return;
    setLoading(true);
    setError(null);
    try {
      const [txRes, accRes, cardRes] = await Promise.allSettled([
        transactionService.getTransactionsByUserId(user.id),
        accountService.getAccountsByUserId(user.id),
        obligationService.getCreditCardsByUserId(),
      ]);

      if (txRes.status === "fulfilled" && txRes.value.success && txRes.value.data) {
        setTransactions(txRes.value.data);
      }

      if (accRes.status === "fulfilled" && accRes.value.success && accRes.value.data) {
        const data = Array.isArray(accRes.value.data) ? accRes.value.data : [accRes.value.data];
        setAccounts(data);
      }

      if (cardRes.status === "fulfilled" && cardRes.value.success && cardRes.value.data) {
        setCreditCards(cardRes.value.data);
      }
    } catch (err: any) {
      setError(err.message || "Failed to fetch data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [user]);

  useEffect(() => {
    if (accounts.length > 0 && !accountId && sourceType === "account") {
      setAccountId(accounts[0].id);
    }
    if (creditCards.length > 0 && !creditCardId && sourceType === "card") {
      setCreditCardId(creditCards[0].id);
    }
  }, [accounts, creditCards, sourceType]);

  const summary = useMemo(() => {
    const totalBank = accounts.reduce((acc, curr) => acc + (curr.balance?.amount || 0), 0);
    const totalCredit = creditCards.reduce((acc, curr) => acc + (curr.outstandingAmount?.amount || 0), 0);
    const recentSpending = transactions
      .filter(t => 
        t.type === TransactionType.Expense && 
        new Date(t.transactionDate).getMonth() === new Date().getMonth() &&
        t.status !== TransactionStatus.Rejected
      )
      .reduce((acc, curr) => acc + (curr.money?.amount || 0), 0);

    return { totalBank, totalCredit, recentSpending };
  }, [accounts, creditCards, transactions]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !amount || !description) return;
    if (sourceType === "account" && !accountId) return;
    if (sourceType === "card" && !creditCardId) return;

    setIsSubmitting(true);
    try {
      const selectedAccount = accounts.find(a => a.id === accountId);
      const selectedCard = creditCards.find(c => c.id === creditCardId);
      
      const payload: any = {
        userId: user.id,
        amount: parseFloat(amount),
        currency: sourceType === "account" ? (selectedAccount?.balance?.currency || "INR") : (selectedCard?.totalLimit?.currency || "INR"),
        description,
        category,
        transactionDate: new Date().toISOString(),
        status: TransactionStatus.Approved,
        type: type === "income" ? TransactionType.Income : (type === "expense" ? TransactionType.Expense : TransactionType.Transfer),
      };

      if (sourceType === "account") {
        payload.accountId = accountId;
      } else {
        payload.creditCardId = creditCardId;
      }

      if (type === "transfer") {
        if (toSourceType === "account") {
          payload.toAccountId = toAccountId;
        } else {
          payload.toCreditCardId = toCreditCardId;
        }
      }

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

  const getSourceName = (tx: Transaction) => {
    const isEmptyGuid = (id?: string) => !id || id === "00000000-0000-0000-0000-000000000000";

    if (!isEmptyGuid(tx.accountId)) {
      const acc = accounts.find(a => a.id === tx.accountId);
      return { name: acc?.name || "Bank Account", type: "account" as const };
    }
    if (!isEmptyGuid(tx.creditCardId)) {
      const card = creditCards.find(c => c.id === tx.creditCardId);
      return { name: card?.cardName || `${card?.bankName} Card` || "Credit Card", type: "card" as const };
    }
    return { name: "Unknown", type: "account" as const };
  };

  const getTypeLabel = (tx: Transaction) => {
    const isCard = !!tx.creditCardId;
    const prefix = isCard ? "Card " : (tx.accountId ? "Bank " : "");
    
    switch (tx.type) {
      case TransactionType.Income: return `${prefix}Income`;
      case TransactionType.Expense: return `${prefix}Expense`;
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

  const filteredTransactions = useMemo(() => {
    return transactions
      .filter(tx => {
        if (selectedCreditCard) {
          if (tx.creditCardId !== selectedCreditCard.id && tx.toCreditCardId !== selectedCreditCard.id) {
            return false;
          }
        } else {
          const matchesType = filterSourceType === "all" || 
            (filterSourceType === "account" && !!tx.accountId) || 
            (filterSourceType === "card" && !!tx.creditCardId);
          if (!matchesType) return false;
        }
        
        const matchesSearch = tx.description?.toLowerCase().includes(searchQuery.toLowerCase()) || 
          tx.category?.toLowerCase().includes(searchQuery.toLowerCase());

        return matchesSearch;
      })
      .sort((a, b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime());
  }, [transactions, filterSourceType, searchQuery, selectedCreditCard]);

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
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-8 pb-20">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">My Transactions</h1>
          <p className="text-muted-foreground mt-1">Manage your cash flow and credit spending</p>
        </div>

        <Dialog open={isAddOpen} onOpenChange={setIsAddOpen}>
          <DialogTrigger asChild>
            <Button className="bg-indigo-600 hover:bg-indigo-700 text-white shadow-lg transition-all hover:scale-105">
              <Plus className="h-4 w-4 mr-2" /> Add Transaction
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md sm:max-w-lg">
            <DialogHeader>
              <DialogTitle>Add New Transaction</DialogTitle>
              <DialogDescription>Record a new income, expense, or transfer.</DialogDescription>
            </DialogHeader>
            <form onSubmit={handleAdd} className="space-y-6 py-2">
              <div className="grid gap-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="type">Transaction Type</Label>
                    <Select value={type} onValueChange={(val) => setType(val as any)}>
                      <SelectTrigger id="type">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="expense">Expense</SelectItem>
                        <SelectItem value="income">Income</SelectItem>
                        <SelectItem value="transfer">Transfer</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="amount">Amount</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground font-medium">₹</span>
                      <Input id="amount" type="number" step="0.01" required value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0.00" className="pl-7" />
                    </div>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Description</Label>
                  <Input id="description" required value={description} onChange={(e) => setDescription(e.target.value)} placeholder="What was this for?" />
                </div>

                <div className="space-y-4 rounded-xl border bg-muted/30 p-4">
                  <div className="flex items-center justify-between mb-2">
                    <Label className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Source</Label>
                    <div className="flex bg-zinc-100 dark:bg-zinc-800 rounded-lg p-0.5">
                      <button 
                        type="button"
                        onClick={() => setSourceType("account")}
                        className={cn("px-3 py-1 text-xs font-medium rounded-md transition-all", sourceType === "account" ? "bg-white dark:bg-zinc-700 text-zinc-900 dark:text-zinc-100 shadow-sm" : "text-zinc-500 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100")}
                      >
                        Bank
                      </button>
                      <button 
                        type="button"
                        onClick={() => setSourceType("card")}
                        className={cn("px-3 py-1 text-xs font-medium rounded-md transition-all", sourceType === "card" ? "bg-white dark:bg-zinc-700 text-zinc-900 dark:text-zinc-100 shadow-sm" : "text-zinc-500 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100")}
                      >
                        Credit Card
                      </button>
                    </div>
                  </div>

                  {sourceType === "account" ? (
                    <div className="space-y-2">
                      <Label className="text-xs">Source Account</Label>
                      <Select value={accountId} onValueChange={setAccountId} required>
                        <SelectTrigger className="bg-background border-zinc-200 dark:border-zinc-800">
                          <SelectValue placeholder="Select account" />
                        </SelectTrigger>
                        <SelectContent>
                          {accounts.map(acc => (
                            <SelectItem key={acc.id} value={acc.id}>
                              <div className="flex items-center gap-2">
                                <Landmark className="h-4 w-4 text-indigo-500" />
                                <span>{acc.name}</span>
                                <span className="text-xs text-muted-foreground ml-1">(₹{acc.balance?.amount?.toLocaleString()})</span>
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  ) : (
                    <div className="space-y-2">
                      <Label className="text-xs">Source Card</Label>
                      <Select value={creditCardId} onValueChange={setCreditCardId} required>
                        <SelectTrigger className="bg-background border-zinc-200 dark:border-zinc-800">
                          <SelectValue placeholder="Select credit card" />
                        </SelectTrigger>
                        <SelectContent>
                          {creditCards.map(card => (
                            <SelectItem key={card.id} value={card.id}>
                              <div className="flex items-center gap-2">
                                <CreditCardIcon className="h-4 w-4 text-orange-500" />
                                <div className="flex flex-col">
                                  <span className="font-medium">{card.cardName || card.bankName}</span>
                                  <span className="text-[10px] text-muted-foreground">
                                    {CreditCardNetworkLabels[card.networkProvider]} • •••• {card.last4Digits}
                                  </span>
                                </div>
                                <span className="text-xs text-muted-foreground ml-auto">(Used: ₹{card.outstandingAmount?.amount?.toLocaleString()})</span>
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  )}
                </div>

                {type === "transfer" && (
                  <div className="space-y-4 rounded-xl border border-indigo-100 bg-indigo-50/10 p-4">
                     <div className="flex items-center justify-between mb-2">
                      <Label className="text-xs font-bold uppercase tracking-wider text-indigo-600/70">Destination</Label>
                      <div className="flex bg-indigo-100/50 dark:bg-indigo-900/30 rounded-lg p-0.5">
                        <button 
                          type="button"
                          onClick={() => setToSourceType("account")}
                          className={cn("px-3 py-1 text-xs font-medium rounded-md transition-all", toSourceType === "account" ? "bg-white dark:bg-indigo-700 text-indigo-700 dark:text-indigo-100 shadow-sm" : "text-indigo-600/60 dark:text-indigo-400/60 hover:text-indigo-600 dark:hover:text-indigo-300")}
                        >
                          Bank
                        </button>
                        <button 
                          type="button"
                          onClick={() => setToSourceType("card")}
                          className={cn("px-3 py-1 text-xs font-medium rounded-md transition-all", toSourceType === "card" ? "bg-white dark:bg-indigo-700 text-indigo-700 dark:text-indigo-100 shadow-sm" : "text-indigo-600/60 dark:text-indigo-400/60 hover:text-indigo-600 dark:hover:text-indigo-300")}
                        >
                          Credit Card
                        </button>
                      </div>
                    </div>

                    {toSourceType === "account" ? (
                      <div className="space-y-2">
                        <Select value={toAccountId} onValueChange={setToAccountId} required>
                          <SelectTrigger className="bg-background border-zinc-200 dark:border-zinc-800">
                            <SelectValue placeholder="Select destination account" />
                          </SelectTrigger>
                          <SelectContent>
                            {accounts.filter(acc => acc.id !== accountId).map(acc => (
                              <SelectItem key={acc.id} value={acc.id}>{acc.name}</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    ) : (
                      <div className="space-y-2">
                        <Select value={toCreditCardId} onValueChange={setToCreditCardId} required>
                          <SelectTrigger className="bg-background border-zinc-200 dark:border-zinc-800">
                            <SelectValue placeholder="Select destination card" />
                          </SelectTrigger>
                          <SelectContent>
                            {creditCards.filter(c => c.id !== creditCardId).map(card => (
                              <SelectItem key={card.id} value={card.id}>{card.cardName || card.bankName}</SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    )}
                  </div>
                )}

                <div className="space-y-2">
                  <Label htmlFor="category">Category</Label>
                  <Select value={category} onValueChange={setCategory}>
                    <SelectTrigger id="category">
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Food & Dining">Food & Dining</SelectItem>
                      <SelectItem value="Housing">Housing</SelectItem>
                      <SelectItem value="Transport">Transport</SelectItem>
                      <SelectItem value="Entertainment">Entertainment</SelectItem>
                      <SelectItem value="Salary">Salary</SelectItem>
                      <SelectItem value="Shopping">Shopping</SelectItem>
                      <SelectItem value="Bills & Utilities">Bills & Utilities</SelectItem>
                      <SelectItem value="Other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <DialogFooter>
                <DialogClose asChild>
                  <Button variant="outline" type="button">Cancel</Button>
                </DialogClose>
                <Button type="submit" disabled={isSubmitting} className="bg-indigo-600 hover:bg-indigo-700">
                  {isSubmitting ? "Saving..." : "Save Transaction"}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card className="overflow-hidden border-none shadow-md bg-gradient-to-br from-indigo-500 to-indigo-600 text-white">
          <CardContent className="p-6">
            <div className="flex justify-between items-start">
              <div>
                <p className="text-zinc-100/80 text-xs font-semibold uppercase tracking-wider">Total Bank Balance</p>
                <h3 className="text-2xl font-bold mt-1">₹{summary.totalBank.toLocaleString()}</h3>
              </div>
              <div className="p-2 bg-white/20 rounded-lg backdrop-blur-sm">
                <Landmark className="h-5 w-5" />
              </div>
            </div>
            <div className="mt-4 flex items-center text-xs text-white/70">
              <TrendingUp className="h-3 w-3 mr-1" />
              <span>Net liquidity across {accounts.length} accounts</span>
            </div>
          </CardContent>
        </Card>

        <Card className="overflow-hidden border-none shadow-md bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800">
          <CardContent className="p-6">
            <div className="flex justify-between items-start">
              <div>
                <p className="text-muted-foreground text-xs font-semibold uppercase tracking-wider">Total Credit Used</p>
                <h3 className="text-2xl font-bold mt-1 text-orange-600 ml-0.5">₹{summary.totalCredit.toLocaleString()}</h3>
              </div>
              <div className="p-2 bg-orange-50 dark:bg-orange-900/20 rounded-lg">
                <CreditCardIcon className="h-5 w-5 text-orange-600" />
              </div>
            </div>
            <div className="mt-4 flex items-center text-xs text-muted-foreground">
              <TrendingDown className="h-3 w-3 mr-1 text-orange-600" />
              <span>Total debt across {creditCards.length} cards</span>
            </div>
          </CardContent>
        </Card>

        <Card className="overflow-hidden border-none shadow-md bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800">
          <CardContent className="p-6">
            <div className="flex justify-between items-start">
              <div>
                <p className="text-muted-foreground text-xs font-semibold uppercase tracking-wider">Spending this Month</p>
                <h3 className="text-2xl font-bold mt-1">₹{summary.recentSpending.toLocaleString()}</h3>
              </div>
              <div className="p-2 bg-zinc-100 dark:bg-zinc-800 rounded-lg">
                <Wallet className="h-5 w-5 text-zinc-600" />
              </div>
            </div>
            <div className="mt-4 flex items-center text-xs text-muted-foreground">
              <Plus className="h-3 w-3 mr-1" />
              <span>Includes bank & card expenses</span>
            </div>
          </CardContent>
        </Card>
      </div>

      {transactions.length === 0 ? (
        <Card className="border-dashed bg-muted/50 p-12 text-center text-muted-foreground">
          <ArrowRightLeft className="w-12 h-12 mx-auto mb-4 opacity-50" />
          <p>No transactions found yet.</p>
          <p className="text-sm mt-2">Start managing your finances by adding your first transaction.</p>
          <Button variant="outline" className="mt-4" onClick={() => setIsAddOpen(true)}>
             Add Transaction
          </Button>
        </Card>
      ) : (
        <Card className="shadow-sm border-zinc-200 dark:border-zinc-800">
          <CardHeader className="pb-3 border-b">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <CardTitle className="text-lg font-semibold">Transaction History</CardTitle>
              <div className="flex flex-wrap items-center gap-3">
                <div className="relative max-w-xs flex-1">
                  <Input 
                    placeholder="Search transactions..." 
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="h-9 text-xs"
                  />
                </div>
                <Select value={filterSourceType} onValueChange={(val) => setFilterSourceType(val as any)}>
                  <SelectTrigger className="h-9 text-xs w-[140px]">
                    <SelectValue placeholder="Source Type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Sources</SelectItem>
                    <SelectItem value="account">Bank Accounts</SelectItem>
                    <SelectItem value="card">Credit Cards</SelectItem>
                  </SelectContent>
                </Select>

                {filterSourceType === "card" && (
                  <Select 
                    value={selectedCreditCard?.id || "all-cards"} 
                    onValueChange={(val) => {
                      if (val === "all-cards") setSelectedCreditCard(null);
                      else {
                        const card = creditCards.find(c => c.id === val);
                        if (card) setSelectedCreditCard(card);
                      }
                    }}
                  >
                    <SelectTrigger className="h-9 text-xs w-[180px] border-orange-200 bg-orange-50/30 text-orange-700">
                      <SelectValue placeholder="Filter by Card" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all-cards">All Cards</SelectItem>
                      {creditCards.map(c => (
                        <SelectItem key={c.id} value={c.id}>{c.cardName || c.bankName}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-muted/50 text-left text-muted-foreground uppercase text-[10px] font-bold tracking-widest">
                    <th className="py-4 px-6">Date</th>
                    <th className="py-4 px-4">Type</th>
                    <th className="py-4 px-4">Description</th>
                    <th className="py-4 px-4">Payment Method</th>
                    <th className="py-4 px-4 text-center">Category</th>
                    <th className="py-4 px-4">Status</th>
                    <th className="py-4 px-6 text-right">Amount</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
                  {filteredTransactions.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE).map(tx => {
                    const source = getSourceName(tx);
                    return (
                      <tr key={tx.id} className="group hover:bg-muted/30 transition-colors">
                        <td className="py-4 px-6 whitespace-nowrap">
                          <div className="flex flex-col">
                            <span className="font-medium">{new Date(tx.transactionDate).toLocaleDateString("en-IN", { day: '2-digit', month: 'short' })}</span>
                            <span className="text-[10px] text-muted-foreground font-mono uppercase">{new Date(tx.transactionDate).toLocaleTimeString("en-IN", { hour: '2-digit', minute: '2-digit' })}</span>
                          </div>
                        </td>
                        <td className="py-4 px-4">
                          <div className="flex items-center gap-2 text-zinc-900 dark:text-zinc-100">
                            {getTypeIcon(tx.type)}
                            <span className="font-medium text-xs whitespace-nowrap">{getTypeLabel(tx)}</span>
                          </div>
                        </td>
                        <td className="py-4 px-4">
                           <p className="font-semibold text-zinc-900 dark:text-zinc-100">{tx.description || "-"}</p>
                        </td>
                        <td className="py-4 px-4">
                          <div className={cn(
                            "flex items-center gap-1.5 px-2 py-1 rounded-md border w-fit min-w-[130px]",
                            source.type === "account" ? "bg-indigo-50/50 border-indigo-100 text-indigo-700" : "bg-orange-50/50 border-orange-100 text-orange-700"
                          )}>
                             {source.type === "account" ? <Landmark className="h-3.5 w-3.5" /> : <CreditCardIcon className="h-3.5 w-3.5" />}
                             <div className="flex flex-col">
                                <span className="text-[10px] font-bold uppercase leading-tight">{source.name}</span>
                                <span className="text-[8px] opacity-70 leading-tight">{source.type === "account" ? "Bank Transfer" : "Card Charge"}</span>
                             </div>
                          </div>
                        </td>
                        <td className="py-4 px-4 text-center">
                          <span className="inline-flex items-center rounded-lg bg-zinc-100 dark:bg-zinc-800 px-2 py-0.5 text-[10px] font-bold text-zinc-600 dark:text-zinc-400 border border-zinc-200 dark:border-zinc-700">
                            {tx.category || "General"}
                          </span>
                        </td>
                        <td className="py-4 px-4">{getStatusBadge(tx.status)}</td>
                        <td className={`py-4 px-6 text-right font-bold text-base ${
                          tx.status === TransactionStatus.Rejected
                          ? "text-muted-foreground line-through opacity-50"
                          : tx.type === TransactionType.Income 
                          ? "text-emerald-600 dark:text-emerald-400" 
                          : tx.type === TransactionType.Expense && tx.creditCardId
                          ? "text-orange-600 dark:text-orange-400"
                          : tx.type === TransactionType.Expense
                          ? "text-zinc-900 dark:text-zinc-100"
                          : "text-indigo-600 dark:text-indigo-400"
                        }`}>
                          {tx.type === TransactionType.Income ? "+" : tx.type === TransactionType.Expense ? "-" : ""}
                          {tx.money?.amount?.toLocaleString("en-IN", { style: "currency", currency: tx.money?.currency || "INR" })}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {filteredTransactions.length > PAGE_SIZE && (
              <div className="p-4 border-t bg-muted/20 flex items-center justify-between">
                <p className="text-xs text-muted-foreground whitespace-nowrap">
                  Showing {Math.min((currentPage - 1) * PAGE_SIZE + 1, filteredTransactions.length)} to {Math.min(currentPage * PAGE_SIZE, filteredTransactions.length)} of {filteredTransactions.length}
                </p>
                <div className="flex items-center gap-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="h-8 text-xs bg-background"
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage(prev => prev - 1)}
                  >
                    Previous
                  </Button>
                  <div className="flex items-center gap-1">
                    {Array.from({ length: Math.ceil(filteredTransactions.length / PAGE_SIZE) }, (_, i) => i + 1)
                      .filter(p => p === 1 || p === Math.ceil(filteredTransactions.length / PAGE_SIZE) || (p >= currentPage - 1 && p <= currentPage + 1))
                      .map((p, i, arr) => (
                        <div key={p} className="flex items-center gap-1">
                          {i > 0 && arr[i-1] !== p - 1 && <span className="text-muted-foreground">...</span>}
                          <Button 
                            variant={currentPage === p ? "default" : "ghost"}
                            size="sm"
                            className={cn("h-8 w-8 text-xs p-0", currentPage === p ? "bg-indigo-600" : "")}
                            onClick={() => setCurrentPage(p)}
                          >
                            {p}
                          </Button>
                        </div>
                      ))
                    }
                  </div>
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="h-8 text-xs bg-background"
                    disabled={currentPage === Math.ceil(filteredTransactions.length / PAGE_SIZE)}
                    onClick={() => setCurrentPage(prev => prev + 1)}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
          {filteredTransactions.length <= PAGE_SIZE && (
            <div className="p-4 border-t bg-muted/20 text-center">
               <p className="text-xs text-muted-foreground">Showing {filteredTransactions.length} recent transactions</p>
            </div>
          )}
        </Card>
      )}
    </div>
  );
}

