"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import {
  obligationService,
  LiabilityDto,
  LiabilityType,
  LiabilityTypeLabels,
  CreateLiabilityRequest,
  UpdateLiabilityRequest,
  AmortizationScheduleDto,
  MakePaymentRequest,
  getLiabilityProgress,
  CreditCardDto,
} from "@/services/obligation";
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
} from "@/components/ui/dialog";
import {
  Plus,
  Pencil,
  Trash2,
  BarChart3,
  Landmark,
  ArrowLeft,
  IndianRupee,
  Calendar,
  BadgePercent,
  Clock,
  Banknote,
  CreditCard as CreditCardIcon,
} from "lucide-react";
import Link from "next/link";

export default function LiabilitiesPage() {
  const { user } = useAuthStore();
  const [liabilities, setLiabilities] = useState<LiabilityDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<LiabilityDto | null>(null);
  const [scheduleData, setScheduleData] = useState<AmortizationScheduleDto | null>(null);
  const [scheduleOpen, setScheduleOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [creditCards, setCreditCards] = useState<CreditCardDto[]>([]);

  // Detail & Payment dialog state
  const [detailItem, setDetailItem] = useState<LiabilityDto | null>(null);
  const [detailOpen, setDetailOpen] = useState(false);
  const [paymentOpen, setPaymentOpen] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState("");
  const [paymentNote, setPaymentNote] = useState("");
  const [paymentError, setPaymentError] = useState("");
  const [paymentSuccess, setPaymentSuccess] = useState("");

  // Form state
  const [form, setForm] = useState({
    name: "",
    type: LiabilityType.HomeLoan as LiabilityType,
    lenderName: "",
    principalAmount: "",
    interestRate: "",
    tenureMonths: "",
    startDate: "",
    accountId: "",
    creditCardId: "none",
    isNoCostEmi: false,
    processingFee: "",
  });

  const fetchData = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const [liabilitiesRes, cardsRes] = await Promise.all([
        obligationService.getLiabilitiesByUserId(user.id),
        obligationService.getCreditCardsByUserId()
      ]);

      if (liabilitiesRes.success && liabilitiesRes.data) {
        setLiabilities(Array.isArray(liabilitiesRes.data) ? liabilitiesRes.data : []);
      }

      if (cardsRes.success && cardsRes.data) {
        setCreditCards(Array.isArray(cardsRes.data) ? cardsRes.data : []);
      }
    } catch {
      // silent
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [user]);

  const resetForm = () => {
    setForm({ name: "", type: LiabilityType.HomeLoan, lenderName: "", principalAmount: "", interestRate: "", tenureMonths: "", startDate: "", accountId: "", creditCardId: "none", isNoCostEmi: false, processingFee: "" });
    setEditItem(null);
  };

  const openCreate = () => { resetForm(); setDialogOpen(true); };

  const openEdit = (l: LiabilityDto) => {
    setEditItem(l);
    setForm({
      name: l.name, type: l.type, lenderName: l.lenderName,
      principalAmount: String(l.principalAmount.amount), interestRate: String(l.interestRate),
      tenureMonths: String(l.tenureMonths), startDate: l.startDate.split("T")[0], accountId: l.accountId ?? "",
      creditCardId: l.creditCardId ?? "none", isNoCostEmi: l.isNoCostEmi, processingFee: l.processingFee ? String(l.processingFee.amount) : "",
    });
    setDetailOpen(false);
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (!user) return;
    setSubmitting(true);
    try {
      const isCC = form.type === LiabilityType.CreditCardEmi;
      if (editItem) {
        const req: UpdateLiabilityRequest = {
          name: form.name, type: form.type, lenderName: form.lenderName,
          principalAmount: parseFloat(form.principalAmount), interestRate: parseFloat(form.interestRate),
          tenureMonths: parseInt(form.tenureMonths), startDate: form.startDate, accountId: form.accountId || null,
          creditCardId: isCC && form.creditCardId !== "none" ? form.creditCardId : null,
          isNoCostEmi: isCC ? form.isNoCostEmi : false,
          processingFee: isCC && form.processingFee ? parseFloat(form.processingFee) : null,
        };
        await obligationService.updateLiability(editItem.id, req);
      } else {
        const req: CreateLiabilityRequest = {
          name: form.name, type: form.type, lenderName: form.lenderName,
          principalAmount: parseFloat(form.principalAmount), interestRate: parseFloat(form.interestRate),
          tenureMonths: parseInt(form.tenureMonths), startDate: form.startDate, userId: user.id, accountId: form.accountId || null,
          creditCardId: isCC && form.creditCardId !== "none" ? form.creditCardId : null,
          isNoCostEmi: isCC ? form.isNoCostEmi : false,
          processingFee: isCC && form.processingFee ? parseFloat(form.processingFee) : null,
        };
        await obligationService.createLiability(req);
      }
      setDialogOpen(false);
      resetForm();
      fetchData();
    } catch { /* silent */ } finally { setSubmitting(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this liability?")) return;
    try {
      await obligationService.deleteLiability(id);
      setDetailOpen(false);
      fetchData();
    } catch { /* silent */ }
  };

  const viewAmortization = async (id: string) => {
    try {
      const res = await obligationService.getAmortizationSchedule(id);
      if (res.success && res.data) { setScheduleData(res.data); setScheduleOpen(true); }
    } catch { /* silent */ }
  };

  const openDetail = (l: LiabilityDto) => { setDetailItem(l); setDetailOpen(true); };

  const openPayment = (l: LiabilityDto) => {
    setDetailItem(l);
    setPaymentAmount("");
    setPaymentNote("");
    setPaymentError("");
    setPaymentSuccess("");
    setPaymentOpen(true);
  };

  const handlePayment = async () => {
    if (!detailItem) return;
    setPaymentError(""); setPaymentSuccess("");
    const amount = parseFloat(paymentAmount);
    if (isNaN(amount) || amount <= 0) { setPaymentError("Enter a valid positive amount"); return; }
    if (amount > detailItem.outstandingBalance.amount) { setPaymentError(`Amount exceeds outstanding balance (₹${detailItem.outstandingBalance.amount.toLocaleString("en-IN")})`); return; }

    setSubmitting(true);
    try {
      const res = await obligationService.makePayment(detailItem.id, { amount, note: paymentNote || undefined });
      if (res.success) {
        setPaymentSuccess(res.message || "Payment recorded successfully!");
        fetchData();
        setTimeout(() => { setPaymentOpen(false); }, 1200);
      } else {
        setPaymentError(res.errors?.join(", ") || "Failed to record payment");
      }
    } catch {
      setPaymentError("An error occurred while recording the payment");
    } finally { setSubmitting(false); }
  };

  const fmt = (amount: number) => amount.toLocaleString("en-IN", { style: "currency", currency: "INR" });

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading liabilities...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-6 pb-20">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="flex items-center gap-3">
          <Link href="/my/obligations" className="h-9 w-9 rounded-lg border border-zinc-300 dark:border-zinc-700 flex items-center justify-center hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors">
            <ArrowLeft className="h-4 w-4" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Loans & EMIs</h1>
            <p className="text-sm text-muted-foreground">Manage your loan liabilities and EMIs</p>
          </div>
        </div>
        <Button onClick={openCreate} className="bg-indigo-600 hover:bg-indigo-700 text-white">
          <Plus className="h-4 w-4 mr-2" /> Add Loan
        </Button>
      </div>

      {/* Empty State */}
      {liabilities.length === 0 ? (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <Landmark className="h-12 w-12 text-muted-foreground/40 mb-4" />
            <p className="text-lg font-medium text-muted-foreground">No liabilities yet</p>
            <p className="text-sm text-muted-foreground/70 mt-1 max-w-md">Start tracking your loans and EMIs by adding your first liability.</p>
            <Button onClick={openCreate} className="mt-6 bg-indigo-600 hover:bg-indigo-700 text-white">
              <Plus className="h-4 w-4 mr-2" /> Add Your First Loan
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {liabilities.map((l) => {
            const { paidPercent, effectiveOutstanding } = getLiabilityProgress(l);
            return (
              <Card
                key={l.id}
                className="group hover:shadow-lg transition-all cursor-pointer hover:border-indigo-300 dark:hover:border-indigo-700"
                onClick={() => openDetail(l)}
              >
                <CardContent className="p-5 space-y-4">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="font-semibold text-base flex items-center gap-2">
                        {l.name}
                        {l.type === LiabilityType.CreditCardEmi && l.isNoCostEmi && (
                          <span className="text-[10px] px-1.5 py-0.5 rounded bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">No Cost</span>
                        )}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {l.type === LiabilityType.CreditCardEmi && l.creditCard ? (
                          <span className="flex items-center gap-1 mt-0.5">
                            <CreditCardIcon className="h-3 w-3" />
                            {l.creditCard.cardName} •••• {l.creditCard.last4Digits}
                          </span>
                        ) : (
                          <>{l.lenderName} · {LiabilityTypeLabels[l.type]}</>
                        )}
                      </p>
                    </div>
                    <span className="text-xs px-2 py-1 rounded-full bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 font-medium whitespace-nowrap">
                      {l.interestRate}% p.a.
                    </span>
                  </div>

                  {/* Progress bar */}
                  <div>
                    <div className="flex justify-between text-xs text-muted-foreground mb-1.5">
                      <span>Paid: {paidPercent}%</span>
                      <span>Remaining: {fmt(effectiveOutstanding)}</span>
                    </div>
                    <div className="h-2 rounded-full bg-zinc-200 dark:bg-zinc-700 overflow-hidden">
                      <div
                        className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400 transition-all"
                        style={{ width: `${paidPercent}%` }}
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div>
                      <p className="text-muted-foreground text-xs">EMI / Month</p>
                      <p className="font-semibold">{fmt(l.emiAmount.amount)}</p>
                    </div>
                    <div>
                      <p className="text-muted-foreground text-xs">Tenure</p>
                      <p className="font-semibold">{l.tenureMonths} months</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}

      {/* ── Detail Dialog ── */}
      <Dialog open={detailOpen} onOpenChange={setDetailOpen}>
        <DialogContent className="sm:max-w-lg">
          {detailItem && (() => {
            const l = detailItem;
            const { paidPercent, paidAmount, effectiveOutstanding } = getLiabilityProgress(l);
            const monthsElapsed = Math.max(0, Math.floor(
              ((new Date().getFullYear() - new Date(l.startDate).getFullYear()) * 12) +
              (new Date().getMonth() - new Date(l.startDate).getMonth())
            ));

            return (
              <>
                <DialogHeader>
                  <DialogTitle className="text-xl">{l.name}</DialogTitle>
                  <DialogDescription>{l.lenderName} · {LiabilityTypeLabels[l.type]}</DialogDescription>
                </DialogHeader>

                <div className="space-y-5 py-2">
                  {/* Progress */}
                  <div>
                    <div className="flex justify-between text-sm mb-2">
                      <span className="font-medium text-emerald-600 dark:text-emerald-400">Paid: {fmt(paidAmount)} ({paidPercent}%)</span>
                      <span className="font-medium text-amber-600 dark:text-amber-400">Outstanding: {fmt(effectiveOutstanding)}</span>
                    </div>
                    <div className="h-3 rounded-full bg-zinc-200 dark:bg-zinc-700 overflow-hidden">
                      <div className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400 transition-all" style={{ width: `${paidPercent}%` }} />
                    </div>
                  </div>

                  {/* Details grid */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <IndianRupee className="h-5 w-5 text-indigo-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Principal</p>
                        <p className="font-semibold text-sm">{fmt(l.principalAmount.amount)}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Banknote className="h-5 w-5 text-amber-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">EMI / Month</p>
                        <p className="font-semibold text-sm">{fmt(l.emiAmount.amount)}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <BadgePercent className="h-5 w-5 text-rose-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Interest Rate</p>
                        <p className="font-semibold text-sm">{l.interestRate}% p.a.</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Clock className="h-5 w-5 text-violet-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Tenure</p>
                        <p className="font-semibold text-sm">{l.tenureMonths} months ({monthsElapsed} elapsed)</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Calendar className="h-5 w-5 text-emerald-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Start Date</p>
                        <p className="font-semibold text-sm">{new Date(l.startDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Calendar className="h-5 w-5 text-red-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">End Date</p>
                        <p className="font-semibold text-sm">{new Date(l.endDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}</p>
                      </div>
                    </div>
                    {l.type === LiabilityType.CreditCardEmi && l.creditCard && (
                      <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                        <CreditCardIcon className="h-5 w-5 text-sky-500" />
                        <div>
                          <p className="text-xs text-muted-foreground">Credit Card</p>
                          <p className="font-semibold text-sm">{l.creditCard.cardName} · {l.creditCard.bankName} (Ending {l.creditCard.last4Digits})</p>
                        </div>
                      </div>
                    )}
                    {l.type === LiabilityType.CreditCardEmi && l.processingFee && (
                      <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                        <IndianRupee className="h-5 w-5 text-gray-500" />
                        <div>
                          <p className="text-xs text-muted-foreground">Processing Fee</p>
                          <p className="font-semibold text-sm">{fmt(l.processingFee.amount)}</p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

                {/* Actions */}
                <div className="flex flex-wrap gap-2 pt-2">
                  <Button onClick={() => openPayment(l)} className="bg-emerald-600 hover:bg-emerald-700 text-white flex-1">
                    <IndianRupee className="h-4 w-4 mr-1" /> Record Payment
                  </Button>
                  <Button variant="outline" onClick={() => { setDetailOpen(false); viewAmortization(l.id); }} className="flex-1">
                    <BarChart3 className="h-4 w-4 mr-1" /> Schedule
                  </Button>
                  <Button variant="outline" onClick={() => openEdit(l)}>
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button variant="outline" onClick={() => handleDelete(l.id)} className="text-red-600 border-red-200 hover:bg-red-50 dark:text-red-400 dark:border-red-800 dark:hover:bg-red-900/20">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </>
            );
          })()}
        </DialogContent>
      </Dialog>

      {/* ── Payment Dialog ── */}
      <Dialog open={paymentOpen} onOpenChange={setPaymentOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Record Payment</DialogTitle>
            <DialogDescription>
              {detailItem?.name} · Outstanding: {fmt(detailItem?.outstandingBalance.amount ?? 0)}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="payment-amount">Payment Amount (₹)</Label>
              <Input
                id="payment-amount"
                type="number"
                placeholder={`Max: ${detailItem?.outstandingBalance.amount.toLocaleString("en-IN") ?? ""}`}
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(e.target.value)}
                autoFocus
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="payment-note">Note (optional)</Label>
              <Input
                id="payment-note"
                placeholder="e.g. January EMI, Prepayment"
                value={paymentNote}
                onChange={(e) => setPaymentNote(e.target.value)}
              />
            </div>
            {/* Quick fill buttons */}
            {detailItem && (
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => setPaymentAmount(String(detailItem.emiAmount.amount))}
                  className="text-xs px-3 py-1.5 rounded-lg border border-indigo-200 text-indigo-600 hover:bg-indigo-50 dark:border-indigo-800 dark:text-indigo-400 dark:hover:bg-indigo-900/20 transition-colors"
                >
                  1 EMI ({fmt(detailItem.emiAmount.amount)})
                </button>
                <button
                  type="button"
                  onClick={() => setPaymentAmount(String(Math.min(detailItem.emiAmount.amount * 3, detailItem.outstandingBalance.amount)))}
                  className="text-xs px-3 py-1.5 rounded-lg border border-indigo-200 text-indigo-600 hover:bg-indigo-50 dark:border-indigo-800 dark:text-indigo-400 dark:hover:bg-indigo-900/20 transition-colors"
                >
                  3 EMIs
                </button>
                <button
                  type="button"
                  onClick={() => setPaymentAmount(String(detailItem.outstandingBalance.amount))}
                  className="text-xs px-3 py-1.5 rounded-lg border border-emerald-200 text-emerald-600 hover:bg-emerald-50 dark:border-emerald-800 dark:text-emerald-400 dark:hover:bg-emerald-900/20 transition-colors"
                >
                  Full Balance
                </button>
              </div>
            )}
            {paymentError && <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 rounded-lg px-3 py-2">{paymentError}</p>}
            {paymentSuccess && <p className="text-sm text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-900/20 rounded-lg px-3 py-2">{paymentSuccess}</p>}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPaymentOpen(false)}>Cancel</Button>
            <Button onClick={handlePayment} disabled={submitting} className="bg-emerald-600 hover:bg-emerald-700 text-white">
              {submitting ? "Recording..." : "Record Payment"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* ── Create / Edit Dialog ── */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>{editItem ? "Edit Loan" : "Add New Loan"}</DialogTitle>
            <DialogDescription>{editItem ? "Update the loan details below." : "Enter the loan details to start tracking."}</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="liability-name">Loan Name</Label>
                <Input id="liability-name" placeholder="e.g. Home Loan" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="liability-type">Type</Label>
                <Select value={String(form.type)} onValueChange={(v) => {
                  const newType = parseInt(v) as LiabilityType;
                  setForm({
                    ...form,
                    type: newType,
                    interestRate: newType === LiabilityType.CreditCardEmi && form.isNoCostEmi ? "0" : form.interestRate
                  });
                }}>
                  <SelectTrigger id="liability-type"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(LiabilityTypeLabels).map(([k, v]) => (
                      <SelectItem key={k} value={k}>{v}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {form.type === LiabilityType.CreditCardEmi && (
              <div className="bg-indigo-50/50 dark:bg-indigo-950/20 p-4 rounded-lg space-y-4 border border-indigo-100 dark:border-indigo-900/40">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="liability-credit-card">Select Credit Card</Label>
                    <Select value={form.creditCardId} onValueChange={(v) => setForm({ ...form, creditCardId: v })}>
                      <SelectTrigger id="liability-credit-card"><SelectValue placeholder="Choose a card" /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="none">None / Not Listed</SelectItem>
                        {creditCards.map(c => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.cardName} · {c.bankName} (•••• {c.last4Digits})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="flex flex-col justify-end pb-2">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-600"
                        checked={form.isNoCostEmi}
                        onChange={(e) => setForm({ ...form, isNoCostEmi: e.target.checked, interestRate: e.target.checked ? "0" : "" })}
                      />
                      <span className="text-sm font-medium">No-Cost EMI?</span>
                    </label>
                    <p className="text-xs text-muted-foreground ml-6 mt-1">Sets interest to 0%.</p>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="liability-processing-fee">Processing Fee (₹) <span className="text-xs font-normal text-muted-foreground">(Billed separately)</span></Label>
                  <Input id="liability-processing-fee" type="number" placeholder="299" value={form.processingFee} onChange={(e) => setForm({ ...form, processingFee: e.target.value })} />
                </div>
              </div>
            )}

            <div className="space-y-2">
              <Label htmlFor="liability-lender">Lender Name / Merchant</Label>
              <Input id="liability-lender" placeholder="e.g. SBI, HDFC" value={form.lenderName} onChange={(e) => setForm({ ...form, lenderName: e.target.value })} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="liability-principal">Principal Amount (₹)</Label>
                <Input id="liability-principal" type="number" placeholder="500000" value={form.principalAmount} onChange={(e) => setForm({ ...form, principalAmount: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="liability-interest">Interest Rate (% p.a.)</Label>
                <Input id="liability-interest" type="number" step="0.01" placeholder="8.5" value={form.interestRate} onChange={(e) => setForm({ ...form, interestRate: e.target.value })} />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="liability-tenure">Tenure (months)</Label>
                <Input id="liability-tenure" type="number" placeholder="240" value={form.tenureMonths} onChange={(e) => setForm({ ...form, tenureMonths: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="liability-start">Start Date</Label>
                <Input id="liability-start" type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSubmit} disabled={submitting} className="bg-indigo-600 hover:bg-indigo-700 text-white">
              {submitting ? "Saving..." : editItem ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* ── Amortization Schedule Dialog ── */}
      <Dialog open={scheduleOpen} onOpenChange={setScheduleOpen}>
        <DialogContent className="sm:max-w-4xl max-h-[80vh] overflow-hidden flex flex-col">
          <DialogHeader>
            <DialogTitle>Amortization Schedule — {scheduleData?.liabilityName}</DialogTitle>
            <DialogDescription>
              Reducing balance EMI breakdown · Total payable: {fmt(scheduleData?.totalAmountPayable ?? 0)} · Total interest: {fmt(scheduleData?.totalInterestPayable ?? 0)}
            </DialogDescription>
          </DialogHeader>
          <div className="overflow-y-auto flex-1 -mx-6 px-6">
            <table className="w-full text-sm">
              <thead className="sticky top-0 bg-white dark:bg-zinc-950 border-b">
                <tr className="text-left text-xs text-muted-foreground uppercase tracking-wider">
                  <th className="py-3 pr-4">#</th>
                  <th className="py-3 pr-4">Date</th>
                  <th className="py-3 pr-4 text-right">EMI</th>
                  <th className="py-3 pr-4 text-right">Principal</th>
                  <th className="py-3 pr-4 text-right">Interest</th>
                  <th className="py-3 text-right">Outstanding</th>
                </tr>
              </thead>
              <tbody>
                {scheduleData?.schedule.map((s) => (
                  <tr key={s.month} className="border-b border-zinc-100 dark:border-zinc-800 hover:bg-zinc-50 dark:hover:bg-zinc-900/50">
                    <td className="py-2.5 pr-4 text-muted-foreground">{s.month}</td>
                    <td className="py-2.5 pr-4">{new Date(s.paymentDate).toLocaleDateString("en-IN", { month: "short", year: "numeric" })}</td>
                    <td className="py-2.5 pr-4 text-right font-medium">{fmt(s.emiAmount)}</td>
                    <td className="py-2.5 pr-4 text-right text-emerald-600 dark:text-emerald-400">{fmt(s.principalComponent)}</td>
                    <td className="py-2.5 pr-4 text-right text-amber-600 dark:text-amber-400">{fmt(s.interestComponent)}</td>
                    <td className="py-2.5 text-right font-medium">{fmt(s.outstandingBalance)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
