"use client";

import { useEffect, useState, useMemo } from "react";
import {
  Mail,
  RefreshCcw,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Info,
  CheckCheck,
  Zap,
  Filter,
  ArrowRight,
  Database,
  Building2,
  Calendar,
  Wallet
} from "lucide-react";
import { format } from "date-fns";
import { useAuthStore } from "@/store/useAuthStore";
import { emailIngestionService, SyncStatus, ParsedTransaction } from "@/services/emailIngestion";
import { accountService, AccountTransferObject } from "@/services/account";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Skeleton } from "@/components/ui/skeleton";

export default function EmailSyncPage() {
  const { user } = useAuthStore();
  const [syncStatus, setSyncStatus] = useState<SyncStatus | null>(null);
  const [pendingTransactions, setPendingTransactions] = useState<ParsedTransaction[]>([]);
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [selectedAccountId, setSelectedAccountId] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSyncing, setIsSyncing] = useState(false);
  const [isProcessing, setIsProcessing] = useState<string | null>(null);
  const [selectedTxns, setSelectedTxns] = useState<Set<string>>(new Set());

  // Initialization
  useEffect(() => {
    if (user?.id) {
      loadData();
    }
  }, [user]);

  const loadData = async () => {
    if (!user?.id) return;
    setIsLoading(true);
    try {
      const [statusRes, txnsRes, accountsRes] = await Promise.all([
        emailIngestionService.getSyncStatus(user.id, true),
        emailIngestionService.getParsedTransactions(user.id),
        accountService.getAccountsByUserId(user.id)
      ]);

      if (statusRes.success) setSyncStatus(statusRes.data);

      if (txnsRes.success) {
        // Handle both camelCase and PascalCase from .NET
        const items = txnsRes.data.items || txnsRes.data.Items || [];
        setPendingTransactions(items);
        console.log("Loaded transactions:", items.length);
      }

      if (accountsRes.success && accountsRes.data.length > 0) {
        setAccounts(accountsRes.data);
        setSelectedAccountId(accountsRes.data[0].id);
      }
    } catch (error) {
      console.error("Failed to load email sync data", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleManualSync = async () => {
    if (!user?.id) return;
    setIsSyncing(true);
    try {
      const res = await emailIngestionService.syncGmail(user.id);
      if (res.success) {
        await loadData();
      }
    } catch (error) {
      console.error("Sync failed", error);
    } finally {
      setIsSyncing(false);
    }
  };

  const handleConfirm = async (id: string) => {
    if (!user?.id || !selectedAccountId) return;
    setIsProcessing(id);
    try {
      const res = await emailIngestionService.confirmTransaction(user.id, id, selectedAccountId);
      if (res.success) {
        setPendingTransactions(prev => prev.filter(t => t.id !== id));
        // Refresh status to update counters
        const statusRes = await emailIngestionService.getSyncStatus(user.id, true);
        if (statusRes.success) setSyncStatus(statusRes.data);
      }
    } catch (error) {
      console.error("Confirmation failed", error);
    } finally {
      setIsProcessing(null);
    }
  };

  const handleReject = async (id: string) => {
    if (!user?.id) return;
    setIsProcessing(id);
    try {
      const res = await emailIngestionService.rejectTransaction(user.id, id);
      if (res.success) {
        setPendingTransactions(prev => prev.filter(t => t.id !== id));
      }
    } catch (error) {
      console.error("Rejection failed", error);
    } finally {
      setIsProcessing(null);
    }
  };

  const handleBulkConfirmHighConfidence = async () => {
    if (!user?.id || !selectedAccountId) return;
    setIsProcessing("bulk");
    try {
      const res = await emailIngestionService.bulkConfirm(user.id, selectedAccountId, 0.9);
      if (res.success) {
        await loadData();
      }
    } finally {
      setIsProcessing(null);
    }
  };

  const toggleSelectTxn = (id: string) => {
    setSelectedTxns(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const highConfidenceCount = useMemo(() =>
    pendingTransactions.filter(t => t.confidenceScore >= 0.9).length,
    [pendingTransactions]);

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8 animate-in fade-in duration-700">
      {/* Header & Sync Status */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
        <div>
          <h1 className="text-4xl font-extrabold tracking-tight flex items-center gap-3">
            <Mail className="h-10 w-10 text-indigo-600" />
            Email Review Queue
          </h1>
          <p className="text-muted-foreground mt-2 text-lg">
            Review and finalize transactions detected in your Gmail inbox.
          </p>
        </div>

        <div className="flex items-center gap-3">
          {isSyncing && (
            <div className="flex items-center gap-2 px-3 py-1.5 bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400 rounded-full animate-pulse border border-indigo-100 dark:border-indigo-800">
              <RefreshCcw className="h-4 w-4 animate-spin" />
              <span className="text-sm font-bold">Syncing Gmail...</span>
            </div>
          )}
          <Button
            onClick={handleManualSync}
            disabled={isSyncing}
            variant={isSyncing ? "ghost" : "outline"}
            className="h-12 border-indigo-200 dark:border-indigo-800 hover:bg-indigo-50 dark:hover:bg-indigo-900/20"
          >
            <RefreshCcw className={`mr-2 h-4 w-4 ${isSyncing ? 'animate-spin' : ''}`} />
            {isSyncing ? 'Processing...' : 'Manual Sync'}
          </Button>

          <Button
            onClick={async () => {
              if (!user?.id) return;
              setIsSyncing(true);
              try {
                const res = await fetch(`/gateway-email-ingestion/api/EmailIngestion/reset-confirmed/${user.id}`, { method: 'POST' });
                const data = await res.json();
                if (data.success) {
                  await loadData();
                  alert(`Success: ${data.data} transactions were restored to your review list!`);
                }
              } finally {
                setIsSyncing(false);
              }
            }}
            disabled={isSyncing}
            variant="ghost"
            className="h-12 text-zinc-400 hover:text-amber-600 hover:bg-amber-50"
            title="Restore transactions that were confirmed but didn't appear in history"
          >
            <Zap className="h-4 w-4 mr-2" /> Restore All
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card className="bg-indigo-50/50 dark:bg-indigo-950/10 border-indigo-100 dark:border-indigo-900/50">
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2">
              <Zap className="h-4 w-4 text-indigo-500" /> Pending Review
            </CardDescription>
            <CardTitle className="text-3xl font-bold">{pendingTransactions.length}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Processed Emails</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalEmailsProcessed || 0}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Auto-Detected</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalTransactionsParsed || 0}</CardTitle>
          </CardHeader>
        </Card>
        <Card className="bg-green-50/50 dark:bg-green-950/10 border-green-100 dark:border-green-900/50">
          <CardHeader className="pb-2">
            <CardDescription>Confirmed</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalTransactionsConfirmed || 0}</CardTitle>
          </CardHeader>
        </Card>
      </div>

      {/* Controls & Bulk Actions */}
      <div className="bg-white dark:bg-zinc-900 p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-4">
        <div className="flex flex-col md:flex-row gap-4 justify-between items-center">
          <div className="flex items-center gap-4 w-full md:w-auto">
            <div className="flex flex-col gap-1.5 min-w-[200px]">
              <label className="text-xs font-semibold uppercase tracking-wider text-zinc-500 flex items-center gap-1.5">
                <Wallet className="h-3 w-3" /> Target Account
              </label>
              <Select value={selectedAccountId} onValueChange={setSelectedAccountId}>
                <SelectTrigger className="bg-zinc-50 dark:bg-zinc-950 border-zinc-200 dark:border-zinc-800 h-10">
                  <SelectValue placeholder="Select Account" />
                </SelectTrigger>
                <SelectContent>
                  {accounts.map(acc => (
                    <SelectItem key={acc.id} value={acc.id}>{acc.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex gap-3 w-full md:w-auto">
            <Button
              className="flex-1 md:flex-none bg-green-600 hover:bg-green-700 text-white shadow-lg shadow-green-600/20"
              disabled={highConfidenceCount === 0 || isProcessing === 'bulk'}
              onClick={handleBulkConfirmHighConfidence}
            >
              <CheckCheck className="mr-2 h-4 w-4" />
              Accept {highConfidenceCount} High Confidence
            </Button>
            <Button variant="outline" className="flex-1 md:flex-none">
              <Filter className="mr-2 h-4 w-4" /> Filters
            </Button>
          </div>
        </div>
      </div>

      {/* Review Table */}
      <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden bg-white dark:bg-zinc-900 shadow-xl">
        <Table>
          <TableHeader className="bg-zinc-50 dark:bg-zinc-950/50">
            <TableRow>
              <TableHead className="w-[50px]">
                <Checkbox />
              </TableHead>
              <TableHead>Transaction</TableHead>
              <TableHead>Category</TableHead>
              <TableHead>Amount</TableHead>
              <TableHead>Source Email</TableHead>
              <TableHead>Confidence</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              [...Array(5)].map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton className="h-4 w-4" /></TableCell>
                  <TableCell>
                    <Skeleton className="h-4 w-32 mb-2" />
                    <Skeleton className="h-3 w-20" />
                  </TableCell>
                  <TableCell><Skeleton className="h-5 w-16 rounded-full" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-40" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                  <TableCell className="text-right"><Skeleton className="h-9 w-24 ml-auto" /></TableCell>
                </TableRow>
              ))
            ) : pendingTransactions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="h-64 text-center">
                  <div className="flex flex-col items-center gap-4 text-muted-foreground">
                    <div className="p-4 bg-zinc-50 dark:bg-zinc-950 rounded-full border border-zinc-100 dark:border-zinc-900">
                      <Database className="h-10 w-10 opacity-40" />
                    </div>
                    <div className="space-y-1">
                      <p className="text-xl font-semibold text-zinc-900 dark:text-zinc-100">No transactions found</p>
                      <p className="text-sm">Click "Manual Sync" to fetch new transactions from Gmail.</p>
                    </div>
                    <Button variant="outline" size="sm" onClick={loadData} className="mt-2">
                      <RefreshCcw className="mr-2 h-3 w-3" /> Refresh View
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              pendingTransactions.map((txn) => (
                <TableRow key={txn.id} className="group hover:bg-zinc-50/50 dark:hover:bg-zinc-950/20 transition-colors">
                  <TableCell>
                    <Checkbox checked={selectedTxns.has(txn.id)} onCheckedChange={() => toggleSelectTxn(txn.id)} />
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col">
                      <span className="font-semibold text-zinc-900 dark:text-zinc-100">{txn.description}</span>
                      <span className="text-xs text-zinc-500 font-medium uppercase tracking-wider flex items-center gap-1.5 mt-1">
                        <Building2 className="h-3 w-3" /> {txn.merchantName || 'Unknown Merchant'}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="secondary" className="bg-zinc-100 dark:bg-zinc-800 hover:bg-zinc-200 dark:hover:bg-zinc-700 font-medium">
                      {txn.category}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col">
                      <span className={`font-bold ${txn.transactionType === 'Income' ? 'text-green-600' : 'text-zinc-900 dark:text-zinc-100'}`}>
                        {txn.transactionType === 'Income' ? '+' : ''} {txn.amount.toLocaleString('en-IN', { style: 'currency', currency: txn.currency })}
                      </span>
                      <span className="text-[10px] text-zinc-400 mt-0.5 flex items-center gap-1">
                        <Calendar className="h-2.5 w-2.5" /> {format(new Date(txn.transactionDate), 'MMM d, h:mm a')}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="group relative">
                      <div className="flex flex-col max-w-[200px]">
                        <span className="text-sm truncate text-zinc-600 dark:text-zinc-400 font-medium">{txn.emailSubject}</span>
                        <span className="text-[11px] text-zinc-500">{txn.emailSender}</span>
                      </div>
                      <div className="hidden group-hover:block absolute left-0 -bottom-8 z-50 bg-zinc-900 text-white text-[10px] p-1.5 rounded shadow-xl whitespace-nowrap">
                        {txn.emailSubject}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <div className="w-12 bg-zinc-100 dark:bg-zinc-800 h-1.5 rounded-full overflow-hidden">
                        <div
                          className={`h-full rounded-full ${txn.confidenceScore >= 0.9 ? 'bg-green-500' : txn.confidenceScore >= 0.7 ? 'bg-amber-500' : 'bg-red-500'}`}
                          style={{ width: `${txn.confidenceScore * 100}%` }}
                        />
                      </div>
                      <span className={`text-[11px] font-bold ${txn.confidenceScore >= 0.9 ? 'text-green-600' : 'text-zinc-500'}`}>
                        {(txn.confidenceScore * 100).toFixed(0)}%
                      </span>
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                      <Button
                        size="sm"
                        variant="ghost"
                        className="h-8 w-8 p-0 text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/20"
                        onClick={() => handleReject(txn.id)}
                        disabled={isProcessing === txn.id}
                      >
                        <XCircle className="h-4 w-4" />
                      </Button>
                      <Button
                        size="sm"
                        className="h-8 px-3 bg-indigo-600 hover:bg-indigo-700 text-white font-medium"
                        onClick={() => handleConfirm(txn.id)}
                        disabled={isProcessing === txn.id}
                      >
                        {isProcessing === txn.id ? (
                          <RefreshCcw className="h-3 w-3 animate-spin" />
                        ) : (
                          <>Accept <ArrowRight className="ml-2 h-3 w-3" /></>
                        )}
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Info Notice */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div className="flex items-start gap-3 p-4 bg-indigo-50/50 dark:bg-indigo-950/10 border border-indigo-100 dark:border-indigo-900/50 rounded-xl text-indigo-700 dark:text-indigo-300 text-sm flex-1">
          <Info className="h-5 w-5 mt-0.5 shrink-0" />
          <p>
            Transactions are staged here after being parsed from your emails. Once you click <strong>Accept</strong>, they will be persisted to your main transaction history.
          </p>
        </div>

        {/* Debug ID - Hidden in production usually, but helpful for us now */}
        <div className="text-[10px] text-zinc-400 font-mono bg-zinc-100 dark:bg-zinc-900 p-2 rounded border border-zinc-200 dark:border-zinc-800">
          DEBUG_UID: {user?.id}
        </div>
      </div>
    </div>
  );
}
