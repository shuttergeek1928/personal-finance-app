"use client";

import { useEffect, useState, useMemo, useCallback } from "react";
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
  Wallet,
  ChevronLeft,
  ChevronRight,
  MoreVertical,
  Trash2
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Checkbox } from "@/components/ui/checkbox";
import { Skeleton } from "@/components/ui/skeleton";

export default function EmailSyncPage() {
  const { user } = useAuthStore();
  
  // Data State
  const [syncStatus, setSyncStatus] = useState<SyncStatus | null>(null);
  const [transactions, setTransactions] = useState<ParsedTransaction[]>([]);
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  
  // UI/Filter State
  const [selectedAccountId, setSelectedAccountId] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSyncing, setIsSyncing] = useState(false);
  const [isProcessing, setIsProcessing] = useState<string | null>(null);
  const [selectedTxns, setSelectedTxns] = useState<Set<string>>(new Set());
  
  // Pagination & Filtering
  const [currentPage, setCurrentPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [statusFilter, setStatusFilter] = useState<string>("Pending");
  const pageSize = 10;

  // Initialization
  useEffect(() => {
    if (user?.id) {
      loadData();
    }
  }, [user, currentPage, statusFilter]);

  const loadData = useCallback(async () => {
    if (!user?.id) return;
    setIsLoading(true);
    try {
      const [statusRes, txnsRes, accountsRes] = await Promise.all([
        emailIngestionService.getSyncStatus(user.id, true),
        emailIngestionService.getParsedTransactions(user.id, currentPage, pageSize, statusFilter),
        accountService.getAccountsByUserId(user.id)
      ]);

      if (statusRes.success) setSyncStatus(statusRes.data);

      if (txnsRes.success) {
        // Handle both camelCase and PascalCase from .NET
        const items = txnsRes.data.items || txnsRes.data.Items || [];
        const total = txnsRes.data.totalCount || txnsRes.data.TotalCount || 0;
        setTransactions(items);
        setTotalItems(total);
        console.log(`Loaded ${items.length} transactions (Total: ${total}) for status: ${statusFilter}`);
      }

      if (accountsRes.success && accountsRes.data.length > 0) {
        setAccounts(accountsRes.data);
        if (!selectedAccountId) setSelectedAccountId(accountsRes.data[0].id);
      }
      
      // Reset selection when data charges
      setSelectedTxns(new Set());
    } catch (error) {
      console.error("Failed to load email sync data", error);
    } finally {
      setIsLoading(false);
    }
  }, [user?.id, currentPage, statusFilter, selectedAccountId]);

  const handleManualSync = async () => {
    if (!user?.id) return;
    setIsSyncing(true);
    try {
      const res = await emailIngestionService.syncGmail(user.id);
      if (res.success) {
        alert("Sync complete! New emails are being processed.");
        await loadData();
      } else {
        alert(res.message || "Sync failed");
      }
    } catch (error) {
      console.error("Sync failed", error);
    } finally {
      setIsSyncing(false);
    }
  };

  const handleConfirm = async (id: string) => {
    if (!user?.id || !selectedAccountId) {
      alert("Please select a target account first.");
      return;
    }
    setIsProcessing(id);
    try {
      const res = await emailIngestionService.confirmTransaction(user.id, id, selectedAccountId);
      if (res.success) {
        alert("Transaction confirmed!");
        // If we are on the last item of the page, go to previous page
        if (transactions.length === 1 && currentPage > 1) {
          setCurrentPage(prev => prev - 1);
        } else {
          loadData();
        }
      }
    } catch (error) {
      console.error("Confirmation failed", error);
      alert("Failed to confirm transaction.");
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
        alert("Transaction rejected.");
        loadData();
      }
    } catch (error) {
      console.error("Rejection failed", error);
    } finally {
      setIsProcessing(null);
    }
  };

  const handleBulkConfirmHighConfidence = async () => {
    if (!user?.id || !selectedAccountId) {
      alert("Please select a target account.");
      return;
    }
    setIsProcessing("bulk");
    try {
      const res = await emailIngestionService.bulkConfirm(user.id, selectedAccountId, 0.95);
      if (res.success) {
        alert(`Success! High-confidence transactions approved.`);
        setCurrentPage(1);
        await loadData();
      }
    } finally {
      setIsProcessing(null);
    }
  };

  const handleApproveSelected = async () => {
    if (!user?.id || !selectedAccountId || selectedTxns.size === 0) return;
    setIsProcessing("selected");
    try {
      let successCount = 0;
      // We process these sequentially or we could add a bulk confirm by IDs endpoint
      // For now, sequentially to reuse existing logic
      for (const id of Array.from(selectedTxns)) {
        const res = await emailIngestionService.confirmTransaction(user.id, id, selectedAccountId);
        if (res.success) successCount++;
      }
      alert(`${successCount} transactions approved successfully!`);
      setSelectedTxns(new Set());
      loadData();
    } finally {
      setIsProcessing(null);
    }
  };

  const handleRestore = async () => {
    if (!user?.id) return;
    setIsSyncing(true);
    try {
      const res = await emailIngestionService.resetConfirmed(user.id);
      if (res.success) {
        alert(`${res.data || 0} transactions restored to review list.`);
        setCurrentPage(1);
        await loadData();
      }
    } finally {
      setIsSyncing(false);
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

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedTxns(new Set(transactions.map(t => t.id)));
    } else {
      setSelectedTxns(new Set());
    }
  };

  const highConfidenceCount = useMemo(() => 
    transactions.filter(t => t.confidenceScore >= 0.95).length, 
    [transactions]);

  const totalPages = Math.ceil(totalItems / pageSize);

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
              <span className="text-sm font-bold">Processing...</span>
            </div>
          )}
          <Button
            onClick={handleManualSync}
            disabled={isSyncing}
            variant={isSyncing ? "ghost" : "outline"}
            className="h-12 border-indigo-200 dark:border-indigo-800 hover:bg-indigo-50 dark:hover:bg-indigo-900/20"
          >
            <RefreshCcw className={`mr-2 h-4 w-4 ${isSyncing ? 'animate-spin' : ''}`} />
            Scan Gmail
          </Button>

          <Button
            onClick={handleRestore}
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
        <Card 
          className={`cursor-pointer transition-all hover:scale-[1.02] ${statusFilter === "Pending" ? 'ring-2 ring-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/20' : 'bg-indigo-50/20 dark:bg-zinc-900/50'}`}
          onClick={() => { setStatusFilter("Pending"); setCurrentPage(1); }}
        >
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2 font-semibold">
              <Zap className="h-4 w-4 text-indigo-500" /> Pending Review
            </CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.pendingReviewCount || transactions.length}</CardTitle>
          </CardHeader>
        </Card>
        
        <Card 
          className={`cursor-pointer transition-all hover:scale-[1.02] ${statusFilter === "" ? 'ring-2 ring-zinc-500 bg-zinc-50 dark:bg-zinc-800/50' : 'bg-transparent'}`}
          onClick={() => { setStatusFilter(""); setCurrentPage(1); }}
        >
          <CardHeader className="pb-2">
            <CardDescription className="font-semibold">Processed Emails</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalEmailsProcessed || 0}</CardTitle>
          </CardHeader>
        </Card>

        <Card className="bg-transparent opacity-80 border-dashed">
          <CardHeader className="pb-2">
            <CardDescription className="font-semibold">Auto-Detected</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalTransactionsParsed || 0}</CardTitle>
          </CardHeader>
        </Card>

        <Card 
          className={`cursor-pointer transition-all hover:scale-[1.02] ${statusFilter === "Confirmed" ? 'ring-2 ring-green-500 bg-green-50/50 dark:bg-green-950/20' : 'bg-green-50/20 dark:bg-zinc-900/50'}`}
          onClick={() => { setStatusFilter("Confirmed"); setCurrentPage(1); }}
        >
          <CardHeader className="pb-2">
            <CardDescription className="font-semibold text-green-600">Confirmed</CardDescription>
            <CardTitle className="text-3xl font-bold">{syncStatus?.totalTransactionsConfirmed || 0}</CardTitle>
          </CardHeader>
        </Card>
      </div>

      {/* Controls & Bulk Actions */}
      <div className="bg-white dark:bg-zinc-900 p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-4">
        <div className="flex flex-col md:flex-row gap-4 justify-between items-center">
          <div className="flex items-center gap-4 w-full md:w-auto">
            <div className="flex flex-col gap-1.5 min-w-[240px]">
              <label className="text-xs font-semibold uppercase tracking-wider text-zinc-500 flex items-center gap-1.5">
                <Wallet className="h-3 w-3" /> Target Account for Confirmation
              </label>
              <Select value={selectedAccountId} onValueChange={setSelectedAccountId}>
                <SelectTrigger className="bg-zinc-50 dark:bg-zinc-950 border-zinc-200 dark:border-zinc-800 h-11">
                  <SelectValue placeholder="Select Account" />
                </SelectTrigger>
                <SelectContent>
                  {accounts.map(acc => (
                    <SelectItem key={acc.id} value={acc.id}>
                      <span className="font-medium">{acc.name}</span>
                      <span className="ml-2 text-xs text-zinc-500">({acc.balance.currency})</span>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex flex-wrap gap-3 w-full md:w-auto justify-end">
            {selectedTxns.size > 0 && (
               <Button
                variant="default"
                className="bg-indigo-600 hover:bg-indigo-700 text-white animate-in zoom-in duration-300"
                onClick={handleApproveSelected}
                disabled={isProcessing === "selected"}
              >
                <CheckCheck className="mr-2 h-4 w-4" />
                Approve Selected ({selectedTxns.size})
              </Button>
            )}
            
            <Button
              className="bg-green-600 hover:bg-green-700 text-white shadow-lg shadow-green-600/20"
              disabled={isProcessing === 'bulk' || statusFilter !== "Pending"}
              onClick={handleBulkConfirmHighConfidence}
            >
              <Zap className="mr-2 h-4 w-4 fill-white" />
              Approve High Confidence (&gt;95%)
            </Button>

            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="h-11">
                  <Filter className="mr-2 h-4 w-4" /> 
                  Filter: {statusFilter || "All"}
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuItem onClick={() => { setStatusFilter("Pending"); setCurrentPage(1); }}>
                  Pending Only
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => { setStatusFilter("Confirmed"); setCurrentPage(1); }}>
                  Confirmed Only
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => { setStatusFilter("Rejected"); setCurrentPage(1); }}>
                  Rejected Only
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => { setStatusFilter(""); setCurrentPage(1); }}>
                  Show All
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </div>

      {/* Review Table */}
      <div className="rounded-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden bg-white dark:bg-zinc-900 shadow-xl">
        <Table>
          <TableHeader className="bg-zinc-50 dark:bg-zinc-950/50">
            <TableRow>
              <TableHead className="w-[50px]">
                <Checkbox 
                  checked={transactions.length > 0 && selectedTxns.size === transactions.length}
                  onCheckedChange={handleSelectAll}
                />
              </TableHead>
              <TableHead className="w-[50px] text-zinc-400 font-mono text-xs">#</TableHead>
              <TableHead>Transaction</TableHead>
              <TableHead>Category</TableHead>
              <TableHead>Amount</TableHead>
              <TableHead>Source Details</TableHead>
              <TableHead>Confidence</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              [...Array(5)].map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton className="h-4 w-4" /></TableCell>
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
            ) : transactions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} className="h-72 text-center">
                  <div className="flex flex-col items-center gap-4 text-muted-foreground">
                    <div className="p-5 bg-zinc-50 dark:bg-zinc-950 rounded-full border border-zinc-100 dark:border-zinc-900">
                      <Database className="h-12 w-12 opacity-30" />
                    </div>
                    <div className="space-y-1">
                      <p className="text-2xl font-bold text-zinc-900 dark:text-zinc-100 italic">No {statusFilter.toLowerCase()} transactions</p>
                      <p className="text-sm">Try changing your filters or running a manual sync.</p>
                    </div>
                    <div className="flex gap-2">
                       <Button variant="outline" size="sm" onClick={loadData}>
                        <RefreshCcw className="mr-2 h-3.5 w-3.5" /> Refresh
                      </Button>
                      <Button variant="outline" size="sm" onClick={() => setStatusFilter("")}>
                        Clear Filters
                      </Button>
                    </div>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              transactions.map((txn, index) => (
                <TableRow 
                  key={txn.id} 
                  className={`group hover:bg-zinc-50/50 dark:hover:bg-zinc-950/20 transition-colors ${txn.status === "Confirmed" ? 'opacity-60 grayscale-[0.5]' : ''}`}
                >
                  <TableCell>
                    <Checkbox 
                      checked={selectedTxns.has(txn.id)} 
                      onCheckedChange={() => toggleSelectTxn(txn.id)} 
                      disabled={txn.status !== "Pending"}
                    />
                  </TableCell>
                  <TableCell className="text-xs font-mono text-zinc-400">
                    {(currentPage - 1) * pageSize + index + 1}
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col">
                      <div className="flex items-center gap-2">
                         <span className="font-semibold text-zinc-900 dark:text-zinc-100">{txn.description}</span>
                         {txn.status === "Confirmed" && <CheckCircle2 className="h-3 w-3 text-green-500" />}
                      </div>
                      <span className="text-xs text-zinc-500 font-medium uppercase tracking-wider flex items-center gap-1.5 mt-1">
                        <Building2 className="h-3 w-3" /> {txn.merchantName || 'External Merchant'}
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
                      <span className={`font-bold text-lg ${txn.transactionType === 'Income' ? 'text-green-600' : 'text-zinc-900 dark:text-zinc-100'}`}>
                        {txn.transactionType === 'Income' ? '+' : ''} {txn.amount.toLocaleString('en-IN', { style: 'currency', currency: txn.currency })}
                      </span>
                      <span className="text-[10px] text-zinc-400 mt-0.5 flex items-center gap-1">
                        <Calendar className="h-2.5 w-2.5" /> {format(new Date(txn.transactionDate), 'MMM d, p')}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="group relative">
                      <div className="flex flex-col max-w-[180px]">
                        <span className="text-xs truncate text-zinc-600 dark:text-zinc-400 font-medium italic">"{txn.emailSubject}"</span>
                        <span className="text-[10px] text-zinc-500">{txn.emailSender}</span>
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
                      {txn.status === "Pending" ? (
                        <>
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
                            className="h-8 px-4 bg-indigo-600 hover:bg-indigo-700 text-white font-medium shadow-md shadow-indigo-600/10"
                            onClick={() => handleConfirm(txn.id)}
                            disabled={isProcessing === txn.id}
                          >
                            {isProcessing === txn.id ? (
                              <RefreshCcw className="h-3 w-3 animate-spin" />
                            ) : (
                              <>Accept <ArrowRight className="ml-2 h-3 w-3" /></>
                            )}
                          </Button>
                        </>
                      ) : (
                        <div className="flex items-center gap-1.5 text-[11px] font-bold uppercase text-zinc-400 pr-2">
                           {txn.status === "Confirmed" ? <CheckCircle2 className="h-3.5 w-3.5 text-green-500" /> : <XCircle className="h-3.5 w-3.5" />}
                           {txn.status}
                        </div>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        
        {/* Pagination Footer */}
        {totalPages > 0 && (
          <div className="bg-zinc-50 dark:bg-zinc-950/50 border-t border-zinc-200 dark:border-zinc-800 p-4 flex items-center justify-between">
            <p className="text-sm text-muted-foreground">
              Showing <span className="font-medium">{(currentPage - 1) * pageSize + 1}</span> to <span className="font-medium">{Math.min(currentPage * pageSize, totalItems)}</span> of <span className="font-medium">{totalItems}</span> transactions
            </p>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
              >
                <ChevronLeft className="h-4 w-4 mr-1" /> Previous
              </Button>
              <div className="flex items-center gap-1 px-3">
                 <span className="text-sm font-bold">{currentPage}</span>
                 <span className="text-sm text-muted-foreground">/ {totalPages}</span>
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
              >
                Next <ChevronRight className="h-4 w-4 ml-1" />
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* Info Notice */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div className="flex items-start gap-3 p-4 bg-indigo-50/50 dark:bg-indigo-950/10 border border-indigo-100 dark:border-indigo-900/50 rounded-xl text-indigo-700 dark:text-indigo-300 text-sm flex-1">
          <Info className="h-5 w-5 mt-0.5 shrink-0" />
          <p>
            <strong>Review Pro-tip:</strong> Transactions with confidence &gt; 95% have high-precision matches against known merchant patterns. You can bulk approve them to save time.
          </p>
        </div>

        <div className="text-[10px] text-zinc-400 font-mono bg-zinc-100 dark:bg-zinc-900 p-2 rounded border border-zinc-200 dark:border-zinc-800">
          FILTER_MODE: {statusFilter || "NONE"} | TOTAL_RECORDS: {totalItems}
        </div>
      </div>
    </div>
  );
}
