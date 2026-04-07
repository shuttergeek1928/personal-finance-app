"use client"

import { useState, useEffect } from "react"
import { useSearchParams } from "next/navigation"
import { userService, UserTransferObject } from "@/services/user"
import { accountService, AccountTransferObject } from "@/services/account"
import { transactionService, Transaction, TransactionType, TransactionStatus } from "@/services/transaction"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter, DialogClose, DialogDescription } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2, Search, User as UserIcon, RefreshCw, AlertCircle, Calendar, FileText } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function TransactionsPage() {
  const searchParams = useSearchParams()
  const initialUserId = searchParams.get("userId")

  // State for user selection
  const [userInput, setUserInput] = useState(initialUserId || "")
  const [selectedUser, setSelectedUser] = useState<UserTransferObject | null>(null)
  const [isFetchingUser, setIsFetchingUser] = useState(false)
  
  // Data states
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (initialUserId) {
        handleUserSearchDirect(initialUserId)
    }
  }, [initialUserId])
  
  // UI states
  const [searchTerm, setSearchTerm] = useState("")
  const [isAddOpen, setIsAddOpen] = useState(false)

  // Form states for adding transaction
  const [amount, setAmount] = useState("")
  const [description, setDescription] = useState("")
  const [category, setCategory] = useState("Food & Dining")
  const [type, setType] = useState<"expense" | "income" | "transfer">("expense")
  const [accountId, setAccountId] = useState("")
  const [toAccountId, setToAccountId] = useState("")

  const fetchUserData = async (userId: string) => {
    setIsLoading(true)
    setError(null)
    try {
      // 1. Fetch User Details
      const userRes = await userService.getUserById(userId)
      if (!userRes.success || !userRes.data) {
        setError(userRes.message || "User not found")
        setSelectedUser(null)
        setTransactions([])
        setAccounts([])
        return
      }
      setSelectedUser(userRes.data)

      // 2. Fetch Accounts
      const accRes = await accountService.getAccountsByUserId(userId)
      if (accRes.success && accRes.data) {
        setAccounts(Array.isArray(accRes.data) ? accRes.data : [accRes.data])
      } else {
        setAccounts([])
      }

      // 3. Fetch Transactions
      const transRes = await transactionService.getTransactionsByUserId(userId)
      if (transRes.success && transRes.data) {
        setTransactions(transRes.data)
      } else {
        setTransactions([])
      }
    } catch (err: any) {
      console.error(err)
      setError(err.message || "An error occurred while fetching data")
    } finally {
      setIsLoading(false)
    }
  }

  const handleUserSearchDirect = async (input: string) => {
    if (!input.trim()) return

    setIsFetchingUser(true)
    setError(null)

    // Check if it's a UUID/Guid format
    const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(input.trim())

    if (isGuid) {
      await fetchUserData(input.trim())
    } else {
      // Try to find user by name or email in the list
      try {
        const usersRes = await userService.getUsers(1, 100)
        if (usersRes.success && usersRes.data.items) {
          const matchedUser = usersRes.data.items.find(u => 
            u.userName?.toLowerCase() === input.toLowerCase() || 
            u.email?.toLowerCase() === input.toLowerCase() ||
            u.fullName?.toLowerCase() === input.toLowerCase()
          )
          if (matchedUser) {
            await fetchUserData(matchedUser.id)
          } else {
            setError("No user found with that name or email. Try entering a User ID.")
            setSelectedUser(null)
            setTransactions([])
          }
        } else {
          setError("Failed to fetch users list for searching.")
        }
      } catch (err: any) {
        setError("Search failed: " + (err.message || "Unknown error"))
      }
    }
    setIsFetchingUser(false)
  }

  const handleUserSearchForm = async (e?: React.FormEvent) => {
    if (e) e.preventDefault()
    await handleUserSearchDirect(userInput)
  }

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUser || !amount || !description || !accountId) return;

    setIsLoading(true);
    try {
      const payload = {
        userId: selectedUser.id,
        accountId: accountId,
        amount: parseFloat(amount),
        currency: accounts.find(a => a.id === accountId)?.balance?.currency || "INR",
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
        // Refresh transactions
        const transRes = await transactionService.getTransactionsByUserId(selectedUser.id);
        if (transRes.success && transRes.data) setTransactions(transRes.data);
      } else {
        alert(res.message || "Failed to add transaction");
      }
    } catch (err: any) {
      alert(err.message || "Failed to add transaction");
    } finally {
      setIsLoading(false);
    }
  };

  const getAccountName = (id: string) => {
    const acc = accounts.find(a => a.id === id);
    return acc?.name || "Unknown Account";
  };

  const filteredTransactions = transactions.filter(t => 
    t.description?.toLowerCase().includes(searchTerm.toLowerCase()) || 
    t.category?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const getTransactionTypeName = (type: TransactionType) => {
    switch (type) {
      case TransactionType.Income: return "Income";
      case TransactionType.Expense: return "Expense";
      case TransactionType.Transfer: return "Transfer";
      default: return "Other";
    }
  };

  return (
    <div className="flex-1 space-y-6 p-8 max-w-7xl mx-auto">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Global Transactions</h2>
          <p className="text-muted-foreground mt-1">Manage and view transactions by entering a User ID or User Name.</p>
        </div>
      </div>

      {/* User Selection Bar */}
      <Card className="bg-card shadow-sm border-border">
        <CardContent className="pt-6">
          <form onSubmit={handleUserSearchForm} className="flex items-end gap-4">
            <div className="flex-1 space-y-2">
              <Label htmlFor="userId">User Identifier</Label>
              <div className="relative">
                <UserIcon className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input 
                  id="userId"
                  placeholder="Enter User ID (Guid), User Name or Email..." 
                  className="pl-9" 
                  value={userInput}
                  onChange={(e) => setUserInput(e.target.value)}
                />
              </div>
            </div>
            <Button type="submit" disabled={isFetchingUser || !userInput.trim()} className="gap-2">
              {isFetchingUser ? <RefreshCw className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
              Fetch Transactions
            </Button>
          </form>
          {error && (
            <div className="mt-4 bg-destructive/10 text-destructive p-3 rounded-md text-sm flex items-center gap-2">
              <AlertCircle className="h-4 w-4" />
              {error}
            </div>
          )}
        </CardContent>
      </Card>

      {selectedUser && (
        <>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-primary/10 p-2 rounded-full">
                <UserIcon className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h3 className="font-semibold text-lg">{selectedUser.fullName || selectedUser.userName}</h3>
                <p className="text-xs text-muted-foreground">{selectedUser.email} • {selectedUser.id}</p>
              </div>
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
                  <DialogDescription>Create a new transaction for {selectedUser.fullName || selectedUser.userName}.</DialogDescription>
                </DialogHeader>
                <form onSubmit={handleAdd} className="space-y-4">
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
                    <Button type="submit" disabled={isLoading}>Save Transaction</Button>
                  </DialogFooter>
                </form>
              </DialogContent>
            </Dialog>
          </div>

          <div className="flex items-center gap-2 max-w-sm relative">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input 
              placeholder="Search in user transactions..." 
              className="pl-9" 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          <div className="rounded-md border border-border overflow-hidden bg-card shadow-sm">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-muted/50 border-b border-border">
                  <tr>
                    <th className="h-12 px-4 text-left font-semibold text-foreground">User Name</th>
                    <th className="h-12 px-4 text-left font-semibold text-foreground">Date</th>
                    <th className="h-12 px-4 text-left font-semibold text-foreground">Description</th>
                    <th className="h-12 px-4 text-left font-semibold text-foreground">Category</th>
                    <th className="h-12 px-4 text-left font-semibold text-foreground">Account Name</th>
                    <th className="h-12 px-4 text-right font-semibold text-foreground">Amount</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {filteredTransactions.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="h-32 text-center text-muted-foreground italic">
                        {isLoading ? "Loading..." : "No transactions found for this user."}
                      </td>
                    </tr>
                  ) : (
                    filteredTransactions.map(t => (
                      <tr key={t.id} className="hover:bg-muted/30 transition-colors">
                        <td className="p-4 align-middle font-medium">
                          {selectedUser.fullName || selectedUser.userName}
                        </td>
                        <td className="p-4 align-middle text-muted-foreground">
                          <div className="flex items-center gap-1.5">
                            <Calendar className="h-3 w-3" />
                            {new Date(t.transactionDate).toLocaleDateString()}
                          </div>
                        </td>
                        <td className="p-4 align-middle">
                          <div className="flex flex-col">
                            <span className="font-semibold">{t.description}</span>
                            <span className="text-[10px] uppercase font-bold text-muted-foreground tracking-tighter">
                              {getTransactionTypeName(t.type)}
                            </span>
                          </div>
                        </td>
                        <td className="p-4 align-middle">
                          <span className="inline-flex items-center rounded-full bg-secondary px-2.5 py-0.5 text-xs font-semibold text-secondary-foreground">
                            {t.category}
                          </span>
                        </td>
                        <td className="p-4 align-middle text-muted-foreground italic">
                          {getAccountName(t.accountId)}
                        </td>
                        <td className={`p-4 align-middle text-right font-bold text-base ${t.type === TransactionType.Expense ? 'text-foreground' : 'text-emerald-600'}`}>
                          {t.type === TransactionType.Expense ? '-' : '+'}{t.money?.amount?.toLocaleString('en-US', { style: 'currency', currency: t.money?.currency || 'INR' })}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}

      {!selectedUser && !isLoading && (
        <div className="py-20 text-center flex flex-col items-center gap-4 text-muted-foreground border-2 border-dashed border-border rounded-xl bg-muted/20">
          <FileText className="h-12 w-12 opacity-20" />
          <p>Search for a user to manage their transactions.</p>
        </div>
      )}
    </div>
  )
}
