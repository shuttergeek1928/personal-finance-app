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
import { Plus, CreditCard as CreditCardIcon, Trash2, Edit2, Loader2, IndianRupee } from "lucide-react";
import { cn } from "@/lib/utils";

export default function CreditCardsPage() {
  const { user } = useAuthStore();
  const [cards, setCards] = useState<CreditCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  
  // Dialog state
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingCard, setEditingCard] = useState<CreditCardDto | null>(null);
  
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
          outstandingAmount: parseFloat(form.outstandingAmount)
        };
        const res = await obligationService.updateCreditCard(editingCard.id, req);
        if (res.success) {
          setIsDialogOpen(false);
          fetchCards();
        }
      } else {
        const req: CreateCreditCardRequest = {
          userId: user.id,
          ...form,
          totalLimit: parseFloat(form.totalLimit),
          outstandingAmount: parseFloat(form.outstandingAmount)
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
        setCards(cards.filter(c => c.id !== id));
      } else {
        alert(res.message || "Failed to delete");
      }
    } catch {
      alert("Error deleting card");
    }
  };

  const getCardGradient = (index: number) => {
    const gradients = [
      "from-indigo-600 to-violet-800", // Indigo
      "from-zinc-700 to-zinc-900",     // Dark / Space Gray
      "from-blue-600 to-cyan-500",     // Ocean
      "from-rose-600 to-orange-500",   // Sunset
      "from-emerald-600 to-teal-500",  // Forest
      "from-slate-700 to-slate-900",   // Slate
      "from-amber-500 to-orange-600",  // Gold/Amber
      "from-purple-600 to-pink-600",   // Berry
    ];
    return gradients[index % gradients.length];
  };

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading credit cards...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-7xl mx-auto space-y-8">
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
            <p className="text-xl font-medium text-foreground">No credit cards added</p>
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
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {cards.map((c, index) => (
            <Card key={c.id} className="relative overflow-hidden border-0 shadow-lg group hover:shadow-2xl transition-all h-60 min-w-[340px]">
              <div className={cn(
                "absolute inset-0 bg-gradient-to-br opacity-95 transition-opacity group-hover:opacity-100", 
                getCardGradient(index)
              )}></div>
              
              {/* Subtle patterns/glows */}
              <div className="absolute top-0 right-0 -m-12 h-40 w-40 rounded-full bg-white/10 blur-3xl group-hover:bg-white/20 transition-colors"></div>
              
              <CardContent className="relative p-7 text-white h-full flex flex-col justify-between">
                <div className="flex justify-between items-start">
                  <div className="space-y-1">
                    <p className="font-bold text-xl tracking-tight leading-none">{c.cardName || "Credit Card"}</p>
                    <p className="text-white/70 text-xs font-medium uppercase tracking-widest">{c.bankName} • {CreditCardNetworkLabels[c.networkProvider]}</p>
                  </div>
                  <div className="p-2 rounded-xl bg-white/10 backdrop-blur-md">
                    <CreditCardIcon className="h-6 w-6" />
                  </div>
                </div>
                
                <div className="space-y-3">
                  {/* Progress Bar & Limits */}
                  <div className="space-y-1.5">
                    <div className="flex justify-between items-end mb-1">
                      <span className="text-[10px] text-white/60 font-semibold uppercase tracking-wider">Utilization</span>
                      <span className="text-xs font-bold">{Math.round((c.outstandingAmount.amount / (c.totalLimit.amount || 1)) * 100)}%</span>
                    </div>
                    <div className="h-2 w-full bg-black/20 rounded-full overflow-hidden backdrop-blur-sm border border-white/5">
                      <div 
                        className="h-full bg-white rounded-full shadow-[0_0_8px_rgba(255,255,255,0.5)] transition-all duration-700 ease-out"
                        style={{ width: `${Math.min((c.outstandingAmount.amount / (c.totalLimit.amount || 1)) * 100, 100)}%` }}
                      />
                    </div>
                    <div className="flex justify-between text-[11px] text-white/80 font-mono tracking-tight pt-1">
                      <span>Used: ₹{c.outstandingAmount.amount.toLocaleString("en-IN")}</span>
                      <span>Limit: ₹{c.totalLimit.amount.toLocaleString("en-IN")}</span>
                    </div>
                  </div>

                  <div className="flex items-center gap-4 font-mono text-2xl tracking-[0.2em] opacity-100 drop-shadow-md py-1">
                    <span className="text-white/30 truncate">•••• •••• ••••</span>
                    <span className="text-white">{c.last4Digits}</span>
                  </div>
                </div>

                <div className="flex justify-between items-end mt-auto">
                  <div className="space-y-0.5">
                    <p className="text-white/60 text-[10px] font-bold uppercase tracking-widest">Available Balance</p>
                    <p className="text-2xl font-bold tracking-tight">
                      ₹{(c.totalLimit.amount - c.outstandingAmount.amount).toLocaleString("en-IN")}
                    </p>
                  </div>
                  
                  <div className="flex gap-2.5">
                     <button 
                       className="p-2.5 rounded-xl bg-white/10 hover:bg-white/20 transition-all border border-white/10 backdrop-blur-md"
                       title="Edit Card"
                       onClick={(e) => handleOpenEdit(c, e)}
                     >
                       <Edit2 className="h-4 w-4" />
                     </button>
                     <button 
                       className="p-2.5 rounded-xl bg-rose-500/30 hover:bg-rose-500/50 transition-all border border-rose-500/20 backdrop-blur-md"
                       title="Delete Card"
                       onClick={(e) => handleDelete(c.id, e)}
                     >
                       <Trash2 className="h-4 w-4" />
                     </button>
                  </div>
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
            <DialogTitle>{editingCard ? "Edit Credit Card" : "Add New Credit Card"}</DialogTitle>
            <DialogDescription>
               Enter your credit card details below. We only store the bank name and the last 4 digits for security.
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
                  onChange={(e) => setForm({...form, bankName: e.target.value})}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="cardName">Card Name</Label>
                <Input 
                  id="cardName" 
                  placeholder="e.g. Amazon Pay" 
                  value={form.cardName}
                  onChange={(e) => setForm({...form, cardName: e.target.value})}
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
                  onChange={(e) => setForm({...form, last4Digits: e.target.value.replace(/\D/g, '')})}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="network">Network</Label>
                <Select 
                  value={String(form.networkProvider)} 
                  onValueChange={(val) => setForm({...form, networkProvider: parseInt(val)})}
                >
                  <SelectTrigger id="network">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(CreditCardNetworkLabels).map(([val, label]) => (
                      <SelectItem key={val} value={val}>{label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
               <div className="space-y-2">
                <Label htmlFor="expiryMonth">Expiry Month</Label>
                <Select 
                  value={String(form.expiryMonth)} 
                  onValueChange={(val) => setForm({...form, expiryMonth: parseInt(val)})}
                >
                  <SelectTrigger id="expiryMonth">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from({length: 12}, (_, i) => i + 1).map(m => (
                      <SelectItem key={m} value={String(m)}>{String(m).padStart(2, '0')}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="expiryYear">Expiry Year</Label>
                <Select 
                  value={String(form.expiryYear)} 
                  onValueChange={(val) => setForm({...form, expiryYear: parseInt(val)})}
                >
                  <SelectTrigger id="expiryYear">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from({length: 15}, (_, i) => new Date().getFullYear() + i).map(y => (
                      <SelectItem key={y} value={String(y)}>{y}</SelectItem>
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
                  onChange={(e) => setForm({...form, totalLimit: e.target.value})}
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
                  onChange={(e) => setForm({...form, outstandingAmount: e.target.value})}
                  required
                />
              </div>
            </div>
            
            <DialogFooter className="pt-4">
              <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" className="bg-indigo-600 hover:bg-indigo-700" disabled={submitting}>
                {submitting ? (
                  <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Saving...</>
                ) : (
                  editingCard ? "Update Card" : "Add Card"
                )}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

