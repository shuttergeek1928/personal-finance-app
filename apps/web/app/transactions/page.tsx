"use client"

import { useState, useEffect } from "react"
import { useFinanceStore } from "@/store/useFinanceStore"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter, DialogClose } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Plus, Trash2, Search } from "lucide-react"

export default function TransactionsPage() {
  const { transactions, accounts, deleteTransaction, addTransaction, fetchDashboardData, isLoading } = useFinanceStore()
  
  const [searchTerm, setSearchTerm] = useState("")
  const [isAddOpen, setIsAddOpen] = useState(false)

  // Form states
  const [amount, setAmount] = useState("")
  const [description, setDescription] = useState("")
  const [category, setCategory] = useState("Food & Dining")
  const [type, setType] = useState<"expense" | "income">("expense")
  const [accountId, setAccountId] = useState("")

  useEffect(() => {
    if (transactions.length === 0 && !isLoading) {
      fetchDashboardData()
    }
  }, [fetchDashboardData, transactions.length, isLoading])

  const filteredTransactions = transactions.filter(t => 
    t.description.toLowerCase().includes(searchTerm.toLowerCase()) || 
    t.category.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!amount || !description || !accountId) return;

    await addTransaction({
      amount: parseFloat(amount),
      description,
      category,
      type,
      accountId,
      date: new Date().toISOString()
    });

    setIsAddOpen(false);
    setAmount("");
    setDescription("");
  };

  return (
    <div className="flex-1 space-y-6 p-8 max-w-7xl mx-auto">
      <div className="flex items-center justify-between">
        <h2 className="text-3xl font-bold tracking-tight">Transactions</h2>
        <Dialog open={isAddOpen} onOpenChange={setIsAddOpen}>
          <DialogTrigger asChild>
            <Button className="bg-indigo-600 hover:bg-indigo-700 text-white shadow-md">
              <Plus className="h-4 w-4 mr-2" /> Add Transaction
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Add New Transaction</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleAdd} className="space-y-4">
              <div className="grid gap-4 py-4">
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
                  <Label>Account</Label>
                  <Select value={accountId} onValueChange={setAccountId} required>
                    <SelectTrigger>
                      <SelectValue placeholder="Select account" />
                    </SelectTrigger>
                    <SelectContent>
                      {accounts.map(acc => (
                        <SelectItem key={acc.id} value={acc.id}>{acc.name} ({acc.type})</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
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
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-zinc-500" />
        <Input 
          placeholder="Search transactions..." 
          className="pl-9" 
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      <div className="rounded-md border border-zinc-200 dark:border-zinc-800 overflow-hidden bg-white dark:bg-zinc-950">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-zinc-50 dark:bg-zinc-900 border-b border-zinc-200 dark:border-zinc-800">
              <tr>
                <th className="h-10 px-4 text-left font-medium text-zinc-500 dark:text-zinc-400">Date</th>
                <th className="h-10 px-4 text-left font-medium text-zinc-500 dark:text-zinc-400">Description</th>
                <th className="h-10 px-4 text-left font-medium text-zinc-500 dark:text-zinc-400">Category</th>
                <th className="h-10 px-4 text-left font-medium text-zinc-500 dark:text-zinc-400">Account</th>
                <th className="h-10 px-4 text-right font-medium text-zinc-500 dark:text-zinc-400">Amount</th>
                <th className="h-10 px-4 text-right font-medium text-zinc-500 dark:text-zinc-400">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredTransactions.length === 0 ? (
                <tr>
                  <td colSpan={6} className="h-24 text-center text-zinc-500">
                    No transactions found.
                  </td>
                </tr>
              ) : (
                filteredTransactions.map(t => {
                  const account = accounts.find(a => a.id === t.accountId);
                  return (
                  <tr key={t.id} className="border-b border-zinc-200 dark:border-zinc-800 last:border-0 hover:bg-zinc-50 dark:hover:bg-zinc-900/50 transition-colors">
                    <td className="p-4 align-middle text-zinc-600 dark:text-zinc-400 whitespace-nowrap">
                      {new Date(t.date).toLocaleDateString()}
                    </td>
                    <td className="p-4 align-middle font-medium">
                      {t.description}
                    </td>
                    <td className="p-4 align-middle">
                      <span className="inline-flex items-center rounded-full border border-zinc-200 px-2.5 py-0.5 text-xs font-semibold dark:border-zinc-800 text-zinc-600 dark:text-zinc-300">
                        {t.category}
                      </span>
                    </td>
                    <td className="p-4 align-middle text-zinc-600 dark:text-zinc-400">
                      {account?.name || 'Unknown Account'}
                    </td>
                    <td className={`p-4 align-middle text-right font-medium ${t.type === 'expense' ? 'text-zinc-900 dark:text-zinc-100' : 'text-emerald-600 dark:text-emerald-400'}`}>
                      {t.type === 'expense' ? '-' : '+'}${t.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                    </td>
                    <td className="p-4 align-middle text-right">
                      <Button variant="ghost" size="icon" className="hover:text-red-500" onClick={() => deleteTransaction(t.id)}>
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </td>
                  </tr>
                )})
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
