"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import {
  obligationService,
  CreditCardDto,
  CreditCardNetwork,
  CreditCardNetworkLabels,
  CreateCreditCardRequest,
  UpdateCreditCardRequest,
} from "@/services/obligation";
import {
  transactionService,
  Transaction,
  TransactionType,
  TransactionStatus,
} from "@/services/transaction";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogClose,
} from "@/components/ui/dialog";
import {
  Plus,
  CreditCard as CreditCardIcon,
  Trash2,
  Edit2,
  Loader2,
  IndianRupee,
  ArrowDownCircle,
  ArrowUpCircle,
  ArrowRightLeft,
  Calendar,
  Info,
  ShieldCheck,
} from "lucide-react";
import { cn } from "@/lib/utils";

export default function CreditCardsPage() {
  const { user } = useAuthStore();
  const [cards, setCards] = useState<CreditCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  // Dialog state
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingCard, setEditingCard] = useState<CreditCardDto | null>(null);

  // Drill-down state
  const [selectedCard, setSelectedCard] = useState<CreditCardDto | null>(null);
  const [cardTransactions, setCardTransactions] = useState<Transaction[]>([]);
  const [loadingTransactions, setLoadingTransactions] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);

  // Form state
  const [form, setForm] = useState({
    bankName: "",
    cardName: "",
    last4Digits: "",
    expiryMonth: new Date().getMonth() + 1,
    expiryYear: new Date().getFullYear() + 2,
    networkProvider: CreditCardNetwork.Visa,
    totalLimit: "",
    outstandingAmount: "",
  });

  useEffect(() => {
    if (!user) return;
    fetchCards();
  }, [user]);

  const fetchCards = async () => {
    setLoading(true);
    try {
      const res = await obligationService.getCreditCardsByUserId();
      if (res.success && res.data) {
        setCards(res.data);
      }
    } catch {
      // silent
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({
      bankName: "",
      cardName: "",
      last4Digits: "",
      expiryMonth: new Date().getMonth() + 1,
      expiryYear: new Date().getFullYear() + 2,
      networkProvider: CreditCardNetwork.Visa,
      totalLimit: "",
      outstandingAmount: "",
    });
    setEditingCard(null);
  };

  const handleOpenAdd = () => {
    resetForm();
    setIsDialogOpen(true);
  };

  const handleOpenEdit = (card: CreditCardDto, e: React.MouseEvent) => {
    e.stopPropagation();
    setEditingCard(card);
    setForm({
      bankName: card.bankName,
      cardName: card.cardName,
      last4Digits: card.last4Digits,
      expiryMonth: card.expiryMonth,
      expiryYear: card.expiryYear,
      networkProvider: card.networkProvider,
      totalLimit: String(card.totalLimit.amount),
      outstandingAmount: String(card.outstandingAmount.amount),
    });
    setIsDialogOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setSubmitting(true);
    try {
      if (editingCard) {
        const req: UpdateCreditCardRequest = {
          ...form,
          totalLimit: parseFloat(form.totalLimit),
          outstandingAmount: parseFloat(form.outstandingAmount),
        };
        const res = await obligationService.updateCreditCard(
          editingCard.id,
          req
        );
        if (res.success) {
          setIsDialogOpen(false);
          fetchCards();
        }
      } else {
        const req: CreateCreditCardRequest = {
          userId: user.id,
          ...form,
          totalLimit: parseFloat(form.totalLimit),
          outstandingAmount: parseFloat(form.outstandingAmount),
        };
        const res = await obligationService.createCreditCard(req);
        if (res.success) {
          setIsDialogOpen(false);
          fetchCards();
        }
      }
    } catch (error) {
      console.error("Error saving card:", error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm("Are you sure you want to delete this credit card?")) return;

    try {
      const res = await obligationService.deleteCreditCard(id);
      if (res.success) {
        setCards(cards.filter((c) => c.id !== id));
      } else {
        alert(res.message || "Failed to delete");
      }
    } catch {
      alert("Error deleting card");
    }
  };

  const handleOpenDetail = async (card: CreditCardDto) => {
    setSelectedCard(card);
    setIsDetailOpen(true);
    setLoadingTransactions(true);
    try {
      const res = await transactionService.getTransactionsByUserId(
        user?.id || ""
      );
      if (res.success && res.data) {
        // Filter transactions for this card specifically
        const filtered = res.data.filter(
          (tx) => tx.creditCardId === card.id || tx.toCreditCardId === card.id
        );
        setCardTransactions(filtered);
      }
    } catch (err) {
      console.error("Failed to fetch card transactions:", err);
    } finally {
      setLoadingTransactions(false);
    }
  };

  const getStatusBadge = (status: TransactionStatus) => {
    switch (status) {
      case TransactionStatus.Pending:
        return (
          <span className="px-2 py-0.5 text-[10px] font-bold rounded-full bg-yellow-100 text-yellow-700">
            Pending
          </span>
        );
      case TransactionStatus.Approved:
        return (
          <span className="px-2 py-0.5 text-[10px] font-bold rounded-full bg-emerald-100 text-emerald-700">
            Approved
          </span>
        );
      case TransactionStatus.Rejected:
        return (
          <span className="px-2 py-0.5 text-[10px] font-bold rounded-full bg-red-100 text-red-700">
            Rejected
          </span>
        );
    }
  };

  const getTypeIcon = (type: TransactionType) => {
    switch (type) {
      case TransactionType.Income:
        return <ArrowDownCircle className="h-4 w-4 text-emerald-500" />;
      case TransactionType.Expense:
        return <ArrowUpCircle className="h-4 w-4 text-red-500" />;
      case TransactionType.Transfer:
        return <ArrowRightLeft className="h-4 w-4 text-indigo-500" />;
    }
  };

  const getCardGradient = (index: number) => {
    const gradients = [
      "from-indigo-600 to-violet-800", // Indigo
      "from-zinc-700 to-zinc-900", // Dark / Space Gray
      "from-blue-600 to-cyan-500", // Ocean
      "from-rose-600 to-orange-500", // Sunset
      "from-emerald-600 to-teal-500", // Forest
      "from-slate-700 to-slate-900", // Slate
      "from-amber-500 to-orange-600", // Gold/Amber
      "from-purple-600 to-pink-600", // Berry
    ];
    return gradients[index % gradients.length];
  };

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">
            Loading credit cards...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-8 pb-20">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Credit Cards</h1>
          <p className="text-muted-foreground mt-1">
            Manage your credit cards to link merchant EMIs.
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 transition-colors shadow-md hover:shadow-lg"
            onClick={handleOpenAdd}
          >
            <Plus className="h-4 w-4" /> Add Card
          </Button>
        </div>
      </div>

      {cards.length === 0 ? (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <div className="h-16 w-16 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center mb-4">
              <CreditCardIcon className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />
            </div>
            <p className="text-xl font-medium text-foreground">
              No credit cards added
            </p>
            <p className="text-sm text-muted-foreground mt-2 max-w-sm">
              Add your credit cards to easily manage credit card EMIs.
            </p>
            <Button
              onClick={handleOpenAdd}
              className="mt-6 inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors"
            >
              <Plus className="h-4 w-4" /> Add Credit Card
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-10 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3">
          {cards.map((c, index) => (
            <Card
              key={c.id}
              onClick={() => handleOpenDetail(c)}
              className="relative overflow-hidden border-0 shadow-lg group hover:shadow-2xl transition-all h-[220px] w-full max-w-[348px] cursor-pointer"
            >
              <div
                className={cn(
                  "absolute inset-0 bg-gradient-to-br opacity-95 transition-opacity group-hover:opacity-100",
                  getCardGradient(index)
                )}
              ></div>

              {/* Subtle patterns/glows */}
              <div className="absolute top-0 right-0 -m-12 h-40 w-40 rounded-full bg-white/10 blur-3xl group-hover:bg-white/20 transition-colors"></div>

              <CardContent className="relative h-full flex flex-col justify-between text-white p-5 group/card">
                {/* Top: Bank & Chip */}
                <div className="flex justify-between items-start">
                  <div className="space-y-0.5">
                    <p className="font-extrabold text-base tracking-tight leading-none uppercase">
                      {c.bankName}
                    </p>
                    <p className="text-white/70 text-[10px] font-medium tracking-widest">
                      {c.cardName}
                    </p>
                  </div>
                  <div className="p-1 px-2 rounded bg-amber-400/20 border border-amber-400/30 backdrop-blur-sm">
                    <div className="h-5 w-7 rounded bg-gradient-to-br from-amber-400/80 to-amber-600/80"></div>
                  </div>
                </div>

                {/* Middle: Card Number */}
                <div className="font-mono text-xl tracking-[0.15em] opacity-90 drop-shadow-lg py-2 flex items-center gap-3">
                  <span className="text-white/30">••••</span>
                  <span className="text-white/30">••••</span>
                  <span className="text-white/30">••••</span>
                  <span>{c.last4Digits}</span>
                </div>

                {/* Bottom Area */}
                <div className="space-y-3">
                  {/* Utilization mini-bar */}
                  <div className="space-y-1">
                    <div className="h-1 w-full bg-black/20 rounded-full overflow-hidden backdrop-blur-sm">
                      <div
                        className="h-full bg-white rounded-full transition-all duration-1000"
                        style={{
                          width: `${Math.min(
                            (c.outstandingAmount.amount /
                              (c.totalLimit.amount || 1)) *
                              100,
                            100
                          )}%`,
                        }}
                      />
                    </div>
                  </div>

                  <div className="flex items-end justify-between">
                    <div className="space-y-0.5">
                      <p className="text-[8px] uppercase tracking-[0.2em] font-black text-white/50">
                        Outstanding
                      </p>
                      <p className="text-lg font-bold tracking-tight">
                        ₹{c.outstandingAmount.amount.toLocaleString("en-IN")}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-[8px] uppercase tracking-[0.2em] font-black text-white/50">
                        Valid Thru
                      </p>
                      <p className="text-xs font-mono">
                        {String(c.expiryMonth).padStart(2, "0")}/
                        {String(c.expiryYear).slice(-2)}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Floating Action Menu (Visible on Hover) */}
                <div className="absolute top-2 right-2 flex gap-1.5 opacity-0 group-hover/card:opacity-100 transition-all duration-300 translate-y-2 group-hover/card:translate-y-0 z-30">
                  <button
                    className="p-1.5 rounded-lg bg-white/10 hover:bg-white/20 transition-all border border-white/10 backdrop-blur-md"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleOpenEdit(c, e);
                    }}
                  >
                    <Edit2 className="h-3.5 w-3.5" />
                  </button>
                  <button
                    className="p-1.5 rounded-lg bg-rose-500/20 hover:bg-rose-500/40 transition-all border border-rose-500/20 backdrop-blur-md"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleDelete(c.id, e);
                    }}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Add / Edit Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>
              {editingCard ? "Edit Credit Card" : "Add New Credit Card"}
            </DialogTitle>
            <DialogDescription>
              Enter your credit card details below. We only store the bank name
              and the last 4 digits for security.
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="bankName">Bank Name</Label>
                <Input
                  id="bankName"
                  placeholder="e.g. ICICI Bank"
                  value={form.bankName}
                  onChange={(e) =>
                    setForm({ ...form, bankName: e.target.value })
                  }
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="cardName">Card Name</Label>
                <Input
                  id="cardName"
                  placeholder="e.g. Amazon Pay"
                  value={form.cardName}
                  onChange={(e) =>
                    setForm({ ...form, cardName: e.target.value })
                  }
                  required
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="last4Digits">Last 4 Digits</Label>
                <Input
                  id="last4Digits"
                  placeholder="1234"
                  maxLength={4}
                  pattern="\d{4}"
                  value={form.last4Digits}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      last4Digits: e.target.value.replace(/\D/g, ""),
                    })
                  }
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="network">Network</Label>
                <Select
                  value={String(form.networkProvider)}
                  onValueChange={(val) =>
                    setForm({ ...form, networkProvider: parseInt(val) })
                  }
                >
                  <SelectTrigger id="network">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(CreditCardNetworkLabels).map(
                      ([val, label]) => (
                        <SelectItem key={val} value={val}>
                          {label}
                        </SelectItem>
                      )
                    )}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="expiryMonth">Expiry Month</Label>
                <Select
                  value={String(form.expiryMonth)}
                  onValueChange={(val) =>
                    setForm({ ...form, expiryMonth: parseInt(val) })
                  }
                >
                  <SelectTrigger id="expiryMonth">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                      <SelectItem key={m} value={String(m)}>
                        {String(m).padStart(2, "0")}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="expiryYear">Expiry Year</Label>
                <Select
                  value={String(form.expiryYear)}
                  onValueChange={(val) =>
                    setForm({ ...form, expiryYear: parseInt(val) })
                  }
                >
                  <SelectTrigger id="expiryYear">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from(
                      { length: 15 },
                      (_, i) => new Date().getFullYear() + i
                    ).map((y) => (
                      <SelectItem key={y} value={String(y)}>
                        {y}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="totalLimit">Total Limit (₹)</Label>
                <Input
                  id="totalLimit"
                  type="number"
                  placeholder="500000"
                  value={form.totalLimit}
                  onChange={(e) =>
                    setForm({ ...form, totalLimit: e.target.value })
                  }
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="outstandingAmount">Used Limit (₹)</Label>
                <Input
                  id="outstandingAmount"
                  type="number"
                  placeholder="25000"
                  value={form.outstandingAmount}
                  onChange={(e) =>
                    setForm({ ...form, outstandingAmount: e.target.value })
                  }
                  required
                />
              </div>
            </div>

            <DialogFooter className="pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => setIsDialogOpen(false)}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                className="bg-indigo-600 hover:bg-indigo-700"
                disabled={submitting}
              >
                {submitting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Saving...
                  </>
                ) : editingCard ? (
                  "Update Card"
                ) : (
                  "Add Card"
                )}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Card Details & Transaction History Modal */}
      <Dialog open={isDetailOpen} onOpenChange={setIsDetailOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto w-[95vw] sm:w-full">
          {selectedCard && (
            <div className="space-y-6">
              <DialogHeader>
                <div className="flex items-center gap-3">
                  <div
                    className={cn(
                      "p-2 rounded-lg text-white",
                      getCardGradient(cards.indexOf(selectedCard))
                    )}
                  >
                    <CreditCardIcon className="h-5 w-5" />
                  </div>
                  <div>
                    <DialogTitle className="text-xl font-bold">
                      {selectedCard.cardName}
                    </DialogTitle>
                    <DialogDescription className="text-xs uppercase tracking-widest font-semibold flex items-center gap-2">
                      {selectedCard.bankName} • •••• {selectedCard.last4Digits}
                    </DialogDescription>
                  </div>
                </div>
              </DialogHeader>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card className="md:col-span-2 border-indigo-100 bg-indigo-50/20 shadow-none">
                  <CardContent className="p-6">
                    <div className="flex items-center justify-between mb-6">
                      <h3 className="font-bold text-indigo-900 flex items-center gap-2">
                        <Info className="h-4 w-4" /> Card Insights
                      </h3>
                      <span className="px-2 py-1 rounded bg-white font-mono text-[10px] border border-indigo-100 text-indigo-600">
                        Expires:{" "}
                        {String(selectedCard.expiryMonth).padStart(2, "0")}/
                        {selectedCard.expiryYear}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 lg:grid-cols-3 gap-8">
                      <div className="space-y-1">
                        <p className="text-[10px] font-bold uppercase tracking-widest text-indigo-400">
                          Total Limit
                        </p>
                        <p className="text-xl font-black text-indigo-900 leading-none">
                          ₹{selectedCard.totalLimit.amount.toLocaleString()}
                        </p>
                      </div>
                      <div className="space-y-1">
                        <p className="text-[10px] font-bold uppercase tracking-widest text-indigo-400">
                          Used Balance
                        </p>
                        <p className="text-xl font-black text-orange-600 leading-none">
                          ₹
                          {selectedCard.outstandingAmount.amount.toLocaleString()}
                        </p>
                      </div>
                      <div className="space-y-1">
                        <p className="text-[10px] font-bold uppercase tracking-widest text-indigo-400">
                          Utilization
                        </p>
                        <div className="flex items-center gap-2">
                          <p className="text-xl font-black text-indigo-900 leading-none">
                            {Math.round(
                              (selectedCard.outstandingAmount.amount /
                                (selectedCard.totalLimit.amount || 1)) *
                                100
                            )}
                            %
                          </p>
                          <div className="h-2 flex-1 bg-indigo-100 rounded-full overflow-hidden min-w-[60px]">
                            <div
                              className="h-full bg-indigo-600 rounded-full"
                              style={{
                                width: `${Math.min(
                                  (selectedCard.outstandingAmount.amount /
                                    (selectedCard.totalLimit.amount || 1)) *
                                    100,
                                  100
                                )}%`,
                              }}
                            />
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="mt-8 pt-6 border-t border-indigo-100 flex items-center justify-between text-xs font-semibold text-indigo-700/60 transition-all">
                      <div className="flex items-center gap-2">
                        <ShieldCheck className="h-4 w-4 text-emerald-500" />
                        <span>
                          Verified by{" "}
                          {
                            CreditCardNetworkLabels[
                              selectedCard.networkProvider
                            ]
                          }
                        </span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Calendar className="h-4 w-4" />
                        <span>
                          Updated:{" "}
                          {new Date(
                            selectedCard.updatedAt
                          ).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card className="border-none shadow-none bg-zinc-50 dark:bg-zinc-900/50">
                  <CardContent className="p-6 flex flex-col items-center justify-center text-center h-full space-y-3">
                    <div className="h-12 w-12 rounded-full bg-white dark:bg-zinc-800 flex items-center justify-center shadow-sm">
                      <IndianRupee className="h-6 w-6 text-indigo-600" />
                    </div>
                    <div>
                      <p className="text-[10px] font-bold uppercase tracking-widest text-muted-foreground">
                        Available Credit
                      </p>
                      <h3 className="text-2xl font-black text-emerald-600">
                        ₹
                        {(
                          selectedCard.totalLimit.amount -
                          selectedCard.outstandingAmount.amount
                        ).toLocaleString()}
                      </h3>
                    </div>
                  </CardContent>
                </Card>
              </div>

              <div className="space-y-4 pt-2">
                <div className="flex items-center justify-between border-b pb-2">
                  <h3 className="font-bold text-zinc-800 dark:text-zinc-200">
                    Transaction History
                  </h3>
                  <span className="text-xs text-muted-foreground">
                    {cardTransactions.length} items found
                  </span>
                </div>

                {loadingTransactions ? (
                  <div className="py-20 flex flex-col items-center justify-center gap-3 opacity-50">
                    <Loader2 className="h-6 w-6 animate-spin text-indigo-600" />
                    <p className="text-sm font-medium">Fetching history...</p>
                  </div>
                ) : cardTransactions.length === 0 ? (
                  <div className="py-20 text-center border border-dashed rounded-xl bg-muted/20">
                    <p className="text-sm text-muted-foreground">
                      No recent transactions for this card.
                    </p>
                  </div>
                ) : (
                  <div className="space-y-3 px-1 max-h-[400px] overflow-y-auto thin-scrollbar">
                    {cardTransactions.map((tx) => (
                      <div
                        key={tx.id}
                        className="flex items-center justify-between p-4 rounded-xl border bg-white dark:bg-zinc-950 hover:border-indigo-200 transition-colors group"
                      >
                        <div className="flex items-center gap-4">
                          <div className="p-2.5 rounded-xl bg-zinc-50 dark:bg-zinc-900 group-hover:bg-indigo-50 transition-colors">
                            {getTypeIcon(tx.type)}
                          </div>
                          <div className="space-y-0.5">
                            <h4 className="font-bold text-sm text-zinc-900 dark:text-zinc-100">
                              {tx.description || "Credit Charge"}
                            </h4>
                            <div className="flex items-center gap-2 text-[10px] font-medium text-muted-foreground">
                              <span className="bg-zinc-100 dark:bg-zinc-800 px-1.5 py-0.5 rounded uppercase tracking-tighter">
                                {tx.category}
                              </span>
                              <span>•</span>
                              <span>
                                {new Date(
                                  tx.transactionDate
                                ).toLocaleDateString()}
                              </span>
                            </div>
                          </div>
                        </div>
                        <div className="text-right space-y-1">
                          <p
                            className={cn(
                              "font-black text-sm",
                              tx.type === TransactionType.Income
                                ? "text-emerald-600"
                                : "text-zinc-900 dark:text-zinc-100"
                            )}
                          >
                            {tx.type === TransactionType.Income ? "+" : "-"}₹
                            {(tx.money?.amount || 0).toLocaleString()}
                          </p>
                          {getStatusBadge(tx.status)}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <DialogFooter className="pt-4 mt-auto">
                <Button
                  variant="outline"
                  onClick={() => setIsDetailOpen(false)}
                >
                  Close Activity
                </Button>
                <Button
                  onClick={() => (window.location.href = "/my/transactions")}
                  className="bg-indigo-600"
                >
                  Manage All Transactions
                </Button>
              </DialogFooter>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
