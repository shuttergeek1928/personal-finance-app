"use client";

import { useEffect, useState } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import {
  obligationService,
  SubscriptionDto,
  SubscriptionType,
  SubscriptionTypeLabels,
  BillingCycle,
  BillingCycleLabels,
  CreateSubscriptionRequest,
  UpdateSubscriptionRequest,
} from "@/services/obligation";
import { Card, CardContent } from "@/components/ui/card";
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
  Receipt,
  ArrowLeft,
  CalendarClock,
  RefreshCw,
  Calendar,
  IndianRupee,
  Building2,
  Tag,
} from "lucide-react";
import Link from "next/link";

export default function SubscriptionsPage() {
  const { user } = useAuthStore();
  const [subscriptions, setSubscriptions] = useState<SubscriptionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<SubscriptionDto | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Detail dialog state
  const [detailItem, setDetailItem] = useState<SubscriptionDto | null>(null);
  const [detailOpen, setDetailOpen] = useState(false);

  const [form, setForm] = useState({
    name: "",
    type: SubscriptionType.Entertainment as SubscriptionType,
    provider: "",
    amount: "",
    billingCycle: BillingCycle.Monthly as BillingCycle,
    startDate: "",
    autoRenew: true,
    endDate: "",
  });

  const fetchData = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const res = await obligationService.getSubscriptionsByUserId(user.id);
      if (res.success && res.data) {
        setSubscriptions(Array.isArray(res.data) ? res.data : []);
      }
    } catch { /* silent */ } finally { setLoading(false); }
  };

  useEffect(() => { fetchData(); }, [user]);

  const resetForm = () => {
    setForm({ name: "", type: SubscriptionType.Entertainment, provider: "", amount: "", billingCycle: BillingCycle.Monthly, startDate: "", autoRenew: true, endDate: "" });
    setEditItem(null);
  };

  const openCreate = () => { resetForm(); setDialogOpen(true); };

  const openEdit = (s: SubscriptionDto) => {
    setEditItem(s);
    setForm({
      name: s.name, type: s.type, provider: s.provider, amount: String(s.amount.amount),
      billingCycle: s.billingCycle, startDate: s.startDate.split("T")[0],
      autoRenew: s.autoRenew, endDate: s.endDate ? s.endDate.split("T")[0] : "",
    });
    setDetailOpen(false);
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (!user) return;
    setSubmitting(true);
    try {
      if (editItem) {
        const req: UpdateSubscriptionRequest = {
          name: form.name, type: form.type, provider: form.provider, amount: parseFloat(form.amount),
          billingCycle: form.billingCycle, startDate: form.startDate, autoRenew: form.autoRenew, endDate: form.endDate || null,
        };
        await obligationService.updateSubscription(editItem.id, req);
      } else {
        const req: CreateSubscriptionRequest = {
          name: form.name, type: form.type, provider: form.provider, amount: parseFloat(form.amount),
          billingCycle: form.billingCycle, startDate: form.startDate, userId: user.id, autoRenew: form.autoRenew, endDate: form.endDate || null,
        };
        await obligationService.createSubscription(req);
      }
      setDialogOpen(false);
      resetForm();
      fetchData();
    } catch { /* silent */ } finally { setSubmitting(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this subscription?")) return;
    try {
      await obligationService.deleteSubscription(id);
      setDetailOpen(false);
      fetchData();
    } catch { /* silent */ }
  };

  const openDetail = (s: SubscriptionDto) => { setDetailItem(s); setDetailOpen(true); };

  const fmt = (amount: number) => amount.toLocaleString("en-IN", { style: "currency", currency: "INR" });

  const cycleColors: Record<BillingCycle, string> = {
    [BillingCycle.Monthly]: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    [BillingCycle.Quarterly]: "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400",
    [BillingCycle.HalfYearly]: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
    [BillingCycle.Yearly]: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
  };

  const typeIcons: Record<SubscriptionType, string> = {
    [SubscriptionType.Entertainment]: "🎬",
    [SubscriptionType.Utility]: "⚡",
    [SubscriptionType.Insurance]: "🛡️",
    [SubscriptionType.Software]: "💻",
    [SubscriptionType.Fitness]: "💪",
    [SubscriptionType.Other]: "📦",
  };

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading subscriptions...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-7xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="flex items-center gap-3">
          <Link href="/my/obligations" className="h-9 w-9 rounded-lg border border-zinc-300 dark:border-zinc-700 flex items-center justify-center hover:bg-zinc-50 dark:hover:bg-zinc-800 transition-colors">
            <ArrowLeft className="h-4 w-4" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Subscriptions</h1>
            <p className="text-sm text-muted-foreground">Manage your recurring subscriptions</p>
          </div>
        </div>
        <Button onClick={openCreate} className="bg-indigo-600 hover:bg-indigo-700 text-white">
          <Plus className="h-4 w-4 mr-2" /> Add Subscription
        </Button>
      </div>

      {/* Empty State */}
      {subscriptions.length === 0 ? (
        <Card className="border-dashed">
          <CardContent className="flex flex-col items-center justify-center py-16 text-center">
            <Receipt className="h-12 w-12 text-muted-foreground/40 mb-4" />
            <p className="text-lg font-medium text-muted-foreground">No subscriptions yet</p>
            <p className="text-sm text-muted-foreground/70 mt-1 max-w-md">Start tracking your recurring payments by adding your first subscription.</p>
            <Button onClick={openCreate} className="mt-6 bg-indigo-600 hover:bg-indigo-700 text-white">
              <Plus className="h-4 w-4 mr-2" /> Add Your First Subscription
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {subscriptions.map((s) => (
            <Card
              key={s.id}
              className="group hover:shadow-lg transition-all cursor-pointer hover:border-indigo-300 dark:hover:border-indigo-700 relative overflow-hidden"
              onClick={() => openDetail(s)}
            >
              <CardContent className="p-5 space-y-4">
                {/* Top row */}
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-xl bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-xl">
                      {typeIcons[s.type]}
                    </div>
                    <div>
                      <p className="font-semibold">{s.name}</p>
                      <p className="text-xs text-muted-foreground">{s.provider}</p>
                    </div>
                  </div>
                  <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${cycleColors[s.billingCycle]}`}>
                    {BillingCycleLabels[s.billingCycle]}
                  </span>
                </div>

                {/* Amount & details */}
                <div className="flex items-end justify-between">
                  <div>
                    <p className="text-2xl font-bold text-foreground">{fmt(s.amount.amount)}</p>
                    <p className="text-xs text-muted-foreground">per {BillingCycleLabels[s.billingCycle].toLowerCase()}</p>
                  </div>
                  <div className={`flex items-center gap-1 text-xs px-2 py-1 rounded-full ${
                    s.autoRenew
                      ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400"
                      : "bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400"
                  }`}>
                    {s.autoRenew && <RefreshCw className="h-3 w-3" />}
                    {s.autoRenew ? "Auto-renew" : "Manual"}
                  </div>
                </div>

                {/* Next billing */}
                <div className="flex items-center gap-2 text-xs text-muted-foreground bg-zinc-50 dark:bg-zinc-900 rounded-lg px-3 py-2">
                  <CalendarClock className="h-3.5 w-3.5" />
                  Next: {new Date(s.nextBillingDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* ── Detail Dialog ── */}
      <Dialog open={detailOpen} onOpenChange={setDetailOpen}>
        <DialogContent className="sm:max-w-lg">
          {detailItem && (() => {
            const s = detailItem;
            const monthlyEquiv = s.billingCycle === BillingCycle.Monthly ? s.amount.amount
              : s.billingCycle === BillingCycle.Quarterly ? s.amount.amount / 3
              : s.billingCycle === BillingCycle.HalfYearly ? s.amount.amount / 6
              : s.amount.amount / 12;

            return (
              <>
                <DialogHeader>
                  <div className="flex items-center gap-3">
                    <div className="h-12 w-12 rounded-xl bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center text-2xl">
                      {typeIcons[s.type]}
                    </div>
                    <div>
                      <DialogTitle className="text-xl">{s.name}</DialogTitle>
                      <DialogDescription>{s.provider} · {SubscriptionTypeLabels[s.type]}</DialogDescription>
                    </div>
                  </div>
                </DialogHeader>

                <div className="space-y-5 py-2">
                  {/* Amount highlight */}
                  <div className="text-center bg-gradient-to-br from-indigo-50 to-violet-50 dark:from-indigo-950/30 dark:to-violet-950/30 rounded-xl p-5">
                    <p className="text-3xl font-bold text-foreground">{fmt(s.amount.amount)}</p>
                    <p className="text-sm text-muted-foreground mt-1">per {BillingCycleLabels[s.billingCycle].toLowerCase()}</p>
                    {s.billingCycle !== BillingCycle.Monthly && (
                      <p className="text-xs text-indigo-600 dark:text-indigo-400 mt-1">≈ {fmt(Math.round(monthlyEquiv * 100) / 100)} / month</p>
                    )}
                  </div>

                  {/* Details grid */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Tag className="h-5 w-5 text-violet-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Billing Cycle</p>
                        <p className="font-semibold text-sm">{BillingCycleLabels[s.billingCycle]}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <RefreshCw className="h-5 w-5 text-emerald-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Auto-Renew</p>
                        <p className="font-semibold text-sm">{s.autoRenew ? "Yes" : "No"}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <CalendarClock className="h-5 w-5 text-amber-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Next Billing</p>
                        <p className="font-semibold text-sm">{new Date(s.nextBillingDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3">
                      <Calendar className="h-5 w-5 text-indigo-500" />
                      <div>
                        <p className="text-xs text-muted-foreground">Start Date</p>
                        <p className="font-semibold text-sm">{new Date(s.startDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}</p>
                      </div>
                    </div>
                    {s.endDate && (
                      <div className="flex items-center gap-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg p-3 col-span-2">
                        <Calendar className="h-5 w-5 text-red-500" />
                        <div>
                          <p className="text-xs text-muted-foreground">End Date</p>
                          <p className="font-semibold text-sm">{new Date(s.endDate).toLocaleDateString("en-IN", { day: "numeric", month: "short", year: "numeric" })}</p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

                {/* Actions */}
                <div className="flex gap-2 pt-2">
                  <Button variant="outline" className="flex-1" onClick={() => openEdit(s)}>
                    <Pencil className="h-4 w-4 mr-1" /> Edit
                  </Button>
                  <Button
                    variant="outline"
                    className="flex-1 text-red-600 border-red-200 hover:bg-red-50 dark:text-red-400 dark:border-red-800 dark:hover:bg-red-900/20"
                    onClick={() => handleDelete(s.id)}
                  >
                    <Trash2 className="h-4 w-4 mr-1" /> Delete
                  </Button>
                </div>
              </>
            );
          })()}
        </DialogContent>
      </Dialog>

      {/* ── Create / Edit Dialog ── */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>{editItem ? "Edit Subscription" : "Add New Subscription"}</DialogTitle>
            <DialogDescription>{editItem ? "Update the subscription details below." : "Enter the subscription details to start tracking."}</DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sub-name">Name</Label>
                <Input id="sub-name" placeholder="e.g. Netflix" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="sub-type">Type</Label>
                <Select value={String(form.type)} onValueChange={(v) => setForm({ ...form, type: parseInt(v) as SubscriptionType })}>
                  <SelectTrigger id="sub-type"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(SubscriptionTypeLabels).map(([k, v]) => (
                      <SelectItem key={k} value={k}>{v}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="sub-provider">Provider</Label>
              <Input id="sub-provider" placeholder="e.g. Netflix, Spotify" value={form.provider} onChange={(e) => setForm({ ...form, provider: e.target.value })} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sub-amount">Amount (₹)</Label>
                <Input id="sub-amount" type="number" placeholder="499" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="sub-cycle">Billing Cycle</Label>
                <Select value={String(form.billingCycle)} onValueChange={(v) => setForm({ ...form, billingCycle: parseInt(v) as BillingCycle })}>
                  <SelectTrigger id="sub-cycle"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(BillingCycleLabels).map(([k, v]) => (
                      <SelectItem key={k} value={k}>{v}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sub-start">Start Date</Label>
                <Input id="sub-start" type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="sub-end">End Date (optional)</Label>
                <Input id="sub-end" type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
              </div>
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" id="sub-autorenew" checked={form.autoRenew} onChange={(e) => setForm({ ...form, autoRenew: e.target.checked })} className="rounded border-zinc-300" />
              <Label htmlFor="sub-autorenew" className="text-sm font-normal">Auto-renew this subscription</Label>
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
    </div>
  );
}
