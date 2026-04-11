"use client";

import { useEffect, useState, useMemo } from "react";
import { useAuthStore } from "@/store/useAuthStore";
import { accountService, AccountTransferObject, AccountType } from "@/services/account";
import { transactionService, Transaction, TransactionStatus } from "@/services/transaction";
import {
  obligationService,
  ObligationDashboardDto,
  LiabilityTypeLabels,
  getLiabilityProgress,
} from "@/services/obligation";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import {
  Wallet,
  ArrowUpCircle,
  ArrowDownCircle,
  TrendingUp,
  ArrowRightLeft,
  Receipt,
  Landmark,
  PieChart as PieChartIcon,
  BarChart3,
  Activity,
  CalendarDays,
  HeartPulse,
  TrendingDown,
  X,
} from "lucide-react";
import Link from "next/link";
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  BarChart,
  Bar,
  RadialBarChart,
  RadialBar,
  PolarAngleAxis,
} from "recharts";

// ── Color Palette ──────────────────────────────────────────────────────
const COLORS = {
  indigo: "#6366f1",
  emerald: "#10b981",
  rose: "#f43f5e",
  amber: "#f59e0b",
  violet: "#8b5cf6",
  cyan: "#06b6d4",
  pink: "#ec4899",
  orange: "#f97316",
  teal: "#14b8a6",
  blue: "#3b82f6",
};

const PIE_COLORS = [COLORS.indigo, COLORS.emerald, COLORS.amber, COLORS.violet, COLORS.cyan, COLORS.pink, COLORS.orange, COLORS.teal];
const ACCOUNT_TYPE_LABELS: Record<number, string> = { 0: "Checking", 1: "Savings", 2: "Credit", 3: "Loan" };

const fmt = (amount: number) => amount.toLocaleString("en-IN", { style: "currency", currency: "INR" });
const fmtShort = (amount: number) => {
  if (amount >= 10000000) return `₹${(amount / 10000000).toFixed(1)}Cr`;
  if (amount >= 100000) return `₹${(amount / 100000).toFixed(1)}L`;
  if (amount >= 1000) return `₹${(amount / 1000).toFixed(1)}K`;
  return `₹${amount.toFixed(0)}`;
};

// Custom tooltip for charts
const ChartTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-700 rounded-lg shadow-lg px-3 py-2 text-sm z-50">
      {label && <p className="font-medium text-muted-foreground mb-1">{label}</p>}
      {payload.map((p: any, i: number) => (
        <p key={i} style={{ color: p.color || p.fill || p.payload?.fill }} className="font-semibold">
          {p.name}: {fmt(p.value)}
        </p>
      ))}
    </div>
  );
};

export default function MyDashboardPage() {
  const { user } = useAuthStore();
  const [accounts, setAccounts] = useState<AccountTransferObject[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [obligations, setObligations] = useState<ObligationDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);

  // New interactive states
  const [timePeriod, setTimePeriod] = useState<"ALL_TIME" | "THIS_MONTH" | "LAST_3_MONTHS" | "THIS_YEAR">("THIS_MONTH");
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;

    const fetchData = async () => {
      setLoading(true);
      try {
        const [accRes, txRes, oblRes] = await Promise.allSettled([
          accountService.getAccountsByUserId(user.id),
          transactionService.getTransactionsByUserId(user.id),
          obligationService.getDashboard(user.id),
        ]);

        if (accRes.status === "fulfilled" && accRes.value.success && accRes.value.data) {
          const data = Array.isArray(accRes.value.data) ? accRes.value.data : [accRes.value.data];
          setAccounts(data);
        }
        if (txRes.status === "fulfilled" && txRes.value.success && txRes.value.data) {
          setTransactions(txRes.value.data);
        }
        if (oblRes.status === "fulfilled" && oblRes.value.success && oblRes.value.data) {
          setObligations(oblRes.value.data);
        }
      } catch {
        // silent
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [user]);

  // ── Date Filtering Logic ──
  const validTransactions = useMemo(() => transactions.filter(t => t.status !== TransactionStatus.Rejected), [transactions]);

  const filteredTransactions = useMemo(() => {
    const now = new Date();
    // Exclude rejected transactions from analytics
    return validTransactions.filter((t) => {
      const txDate = new Date(t.transactionDate);
      if (timePeriod === "ALL_TIME") return true;
      if (timePeriod === "THIS_MONTH") return txDate.getMonth() === now.getMonth() && txDate.getFullYear() === now.getFullYear();
      if (timePeriod === "LAST_3_MONTHS") {
        const threeMonthsAgo = new Date();
        threeMonthsAgo.setMonth(now.getMonth() - 2);
        threeMonthsAgo.setDate(1);
        return txDate >= threeMonthsAgo;
      }
      if (timePeriod === "THIS_YEAR") return txDate.getFullYear() === now.getFullYear();
      return true;
    });
  }, [validTransactions, timePeriod]);


  // ── Computed Data based on Filter ──
  const totalBalance = accounts.reduce((sum, acc) => sum + (acc.balance?.amount || 0), 0);
  const totalIncome = filteredTransactions.filter((t) => t.type === 0).reduce((sum, t) => sum + (t.money?.amount || 0), 0);
  const totalExpenses = filteredTransactions.filter((t) => t.type === 1).reduce((sum, t) => sum + (t.money?.amount || 0), 0);
  const netFlow = totalIncome - totalExpenses;
  const totalOutstanding = obligations?.totalOutstandingBalance ?? 0;
  const totalMonthlyObl = obligations?.totalMonthlyObligations ?? 0;
  const netWorth = totalBalance - totalOutstanding;

  // ── Month-over-Month Comparison ──
  const { momIncome, momExpense } = useMemo(() => {
    const now = new Date();
    const thisMonth = { income: 0, expense: 0 };
    const lastMonth = { income: 0, expense: 0 };

    validTransactions.forEach(t => {
      const d = new Date(t.transactionDate);
      if (d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth()) {
        t.type === 0 ? thisMonth.income += (t.money?.amount || 0) : thisMonth.expense += (t.money?.amount || 0);
      } else if (
        (d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth() - 1) ||
        (now.getMonth() === 0 && d.getFullYear() === now.getFullYear() - 1 && d.getMonth() === 11)
      ) {
        t.type === 0 ? lastMonth.income += (t.money?.amount || 0) : lastMonth.expense += (t.money?.amount || 0);
      }
    });

    const calcChange = (curr: number, prev: number) => {
      if (prev === 0) return curr > 0 ? 100 : 0;
      return ((curr - prev) / prev) * 100;
    };

    return {
      momIncome: { value: thisMonth.income, change: calcChange(thisMonth.income, lastMonth.income) },
      momExpense: { value: thisMonth.expense, change: calcChange(thisMonth.expense, lastMonth.expense) },
    };
  }, [validTransactions]);

  // ── Financial Health Score ──
  const healthScore = useMemo(() => {
    // Score out of 100
    // Savings Rate: up to 40 points (target > 20%)
    const globalTotalIncome = validTransactions.filter((t) => t.type === 0).reduce((sum, t) => sum + (t.money?.amount || 0), 0);
    const globalTotalExpense = validTransactions.filter((t) => t.type === 1).reduce((sum, t) => sum + (t.money?.amount || 0), 0);
    const savingsRate = globalTotalIncome > 0 ? ((globalTotalIncome - globalTotalExpense) / globalTotalIncome) : 0;
    const savingsScore = savingsRate > 0.2 ? 40 : (savingsRate > 0 ? (savingsRate / 0.2) * 40 : 0);

    // Debt Ratio: up to 30 points (Liabilities vs Assets, closer to 0 is better)
    const debtRatio = totalBalance > 0 ? (totalOutstanding / totalBalance) : (totalOutstanding > 0 ? 1 : 0);
    const debtScore = debtRatio === 0 ? 30 : (debtRatio < 0.5 ? ((0.5 - debtRatio) / 0.5) * 30 : 0);

    // Baseline: 30 points for just having setup accounts
    const score = 30 + savingsScore + debtScore;
    return Math.min(Math.round(score), 100);
  }, [validTransactions, totalBalance, totalOutstanding]);

  // ── Upcoming Payments (Next 7-30 days) ──
  const upcomingPayments = useMemo(() => {
    if (!obligations) return [];
    const upcoming: { id: string, name: string, date: Date, amount: number, type: string }[] = [];
    const now = new Date();
    // Start of today to ignore time differences
    now.setHours(0, 0, 0, 0);

    obligations.subscriptions?.forEach(s => {
      if (s.nextBillingDate) {
        const d = new Date(s.nextBillingDate);
        if (d >= now) upcoming.push({ id: `sub-${s.id}`, name: s.name, date: d, amount: s.amount.amount, type: 'Subscription' });
      }
    });

    obligations.liabilities?.forEach(l => {
      const sDate = new Date(l.startDate);
      let nextDate = new Date(now.getFullYear(), now.getMonth(), sDate.getDate());
      if (nextDate < now) {
        nextDate.setMonth(now.getMonth() + 1);
      }
      upcoming.push({ id: `loan-${l.id}`, name: l.name, date: nextDate, amount: l.emiAmount.amount, type: 'EMI' });
    });

    return upcoming.sort((a, b) => a.date.getTime() - b.date.getTime()).slice(0, 4);
  }, [obligations]);

  // ── Expense Heatmap (Last 30 Days) ──
  const expenseHeatmap = useMemo(() => {
    const map: Record<string, number> = {};
    const maxDate = new Date();
    maxDate.setHours(0,0,0,0);
    
    validTransactions.filter(t => t.type === 1).forEach(t => {
      // Use local date string (YYYY-MM-DD) to match the heatmap generation
      const d = new Date(t.transactionDate);
      const dStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
      map[dStr] = (map[dStr] || 0) + (t.money?.amount || 0);
    });

    let maxAmount = 1;
    for(const val of Object.values(map)) {
      if(val > maxAmount) maxAmount = val;
    }

    const days = [];
    for(let i = 29; i >= 0; i--) {
      const d = new Date(maxDate);
      d.setDate(d.getDate() - i);
      const dStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
      const amount = map[dStr] || 0;
      days.push({
        dateStr: d.toLocaleDateString("en-IN", { month: "short", day: "numeric" }),
        amount,
        intensity: amount > 0 ? Math.max(0.15, Math.min(1, amount / (maxAmount * 0.8 || 1))) : 0
      });
    }
    return days;
  }, [validTransactions]);


  // ── Chart Data ─────────────────────────────────────────────────────

  // Income vs Expense by month (area/bar)
  const monthlyFlow = useMemo(() => {
    const map: Record<string, { income: number; expense: number }> = {};
    filteredTransactions.forEach((t) => {
      const d = new Date(t.transactionDate);
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
      if (!map[key]) map[key] = { income: 0, expense: 0 };
      if (t.type === 0) map[key].income += t.money?.amount || 0;
      if (t.type === 1) map[key].expense += t.money?.amount || 0;
    });
    return Object.entries(map)
      .sort(([a], [b]) => a.localeCompare(b))
      .slice(-12)
      .map(([key, val]) => {
        const [y, m] = key.split("-");
        const month = new Date(parseInt(y), parseInt(m) - 1).toLocaleDateString("en-IN", { month: "short", year: "2-digit" });
        return { month, income: val.income, expense: val.expense, net: val.income - val.expense };
      });
  }, [filteredTransactions]);

  // Expense by category (pie)
  const expenseByCategory = useMemo(() => {
    const map: Record<string, number> = {};
    filteredTransactions
      .filter((t) => t.type === 1)
      .forEach((t) => {
        const cat = t.category || "Uncategorized";
        map[cat] = (map[cat] || 0) + (t.money?.amount || 0);
      });
    return Object.entries(map)
      .sort(([, a], [, b]) => b - a)
      .map(([name, value]) => ({ name, value })); // Return all for drill down logic
  }, [filteredTransactions]);

  // Filtered transactions for drill-down
  const drillDownTransactions = useMemo(() => {
    if(!selectedCategory) return [];
    return filteredTransactions
      .filter(t => t.type === 1 && (t.category === selectedCategory || (!t.category && selectedCategory === "Uncategorized")))
      .sort((a, b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime())
      .slice(0, 5); // Limit to top 5
  }, [filteredTransactions, selectedCategory]);


  // Account balances (horizontal bars)
  const accountBalances = useMemo(() => {
    return accounts
      .filter((a) => a.isActive)
      .sort((a, b) => (b.balance?.amount || 0) - (a.balance?.amount || 0))
      .slice(0, 5)
      .map((a) => ({
        name: a.name || `Account ${a.accountNumber?.slice(-4)}`,
        balance: a.balance?.amount || 0,
      }));
  }, [accounts]);

  const healthScoreColor = healthScore >= 75 ? COLORS.emerald : healthScore >= 40 ? COLORS.amber : COLORS.rose;

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[60vh]">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading your dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 md:p-8 max-w-[1600px] mx-auto space-y-8 pb-20">
      {/* ── Header Area ── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Welcome back, {user?.firstName || user?.userName || "User"} 👋
          </h1>
          <p className="text-muted-foreground mt-1">Here&apos;s a comprehensive view of your financial health.</p>
        </div>
        
        {/* Time Period Filter */}
        <div className="flex items-center gap-2 bg-white dark:bg-zinc-950 p-1 rounded-lg border shadow-sm">
          <Select value={timePeriod} onValueChange={(v: any) => setTimePeriod(v)}>
            <SelectTrigger className="w-[180px] border-none shadow-none focus:ring-0">
              <CalendarDays className="w-4 h-4 mr-2 text-muted-foreground" />
              <SelectValue placeholder="Select Period" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="THIS_MONTH">This Month</SelectItem>
              <SelectItem value="LAST_3_MONTHS">Last 3 Months</SelectItem>
              <SelectItem value="THIS_YEAR">This Year</SelectItem>
              <SelectItem value="ALL_TIME">All Time</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* ── Top Row: Key Metrics & Health Score ── */}
      <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-4 xl:grid-cols-5">
        
        {/* Financial Health Score (Takes 1 Col) */}
        <Card className="flex flex-col justify-center items-center py-6 shadow-md border-indigo-100 dark:border-indigo-900/40 relative overflow-hidden">
          <div className="absolute top-0 w-full h-1 bg-gradient-to-r from-indigo-500 to-violet-500"></div>
          <HeartPulse className="h-6 w-6 text-indigo-500 mb-2 absolute top-4 left-4 opacity-20" />
          <CardTitle className="text-sm font-medium text-muted-foreground mb-4 z-10 w-full text-center">Financial Health</CardTitle>
          <div className="h-[120px] w-[120px]">
            <ResponsiveContainer width="100%" height="100%">
              <RadialBarChart 
                cx="50%" cy="50%" 
                innerRadius="80%" outerRadius="100%" 
                barSize={12} data={[{ name: "Score", value: healthScore, fill: healthScoreColor }]} 
                startAngle={225} endAngle={-45}
              >
                <PolarAngleAxis type="number" domain={[0, 100]} angleAxisId={0} tick={false} />
                <RadialBar background={{ fill: 'hsl(var(--muted))' }} dataKey="value" cornerRadius={10} />
              </RadialBarChart>
            </ResponsiveContainer>
          </div>
          <div className="absolute top-[50%] flex flex-col items-center justify-center translate-y-[-10px]">
            <span className="text-3xl font-bold tracking-tighter" style={{ color: healthScoreColor }}>{healthScore}</span>
          </div>
          <p className="text-xs text-muted-foreground mt-2 px-6 text-center">
            {healthScore >= 75 ? "Excellent standing!" : healthScore >= 40 ? "Needs some attention" : "Critical state"}
          </p>
        </Card>

        {/* Dynamic Metrics (Takes remaining Cols) */}
        <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-4 col-span-1 md:col-span-1 lg:col-span-3 xl:col-span-4 transition-all">
          <Card className="shadow-md relative overflow-hidden">
             <div className="absolute -right-6 -bottom-6 opacity-[0.03]">
                 <Wallet className="h-32 w-32" />
             </div>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">Total Balance</CardTitle>
              <Wallet className="h-5 w-5 text-indigo-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{fmt(totalBalance)}</div>
              <p className="text-xs text-muted-foreground mt-1">{accounts.length} active accounts</p>
            </CardContent>
          </Card>

          <Card className="shadow-md relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">Period Income</CardTitle>
              <ArrowDownCircle className="h-5 w-5 text-emerald-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">+{fmt(totalIncome)}</div>
              {timePeriod === "THIS_MONTH" && (
                <div className="flex items-center gap-1 mt-1 text-xs">
                  <span className={momIncome.change >= 0 ? "text-emerald-600" : "text-rose-500"}>
                    {momIncome.change >= 0 ? "↑" : "↓"} {Math.abs(momIncome.change).toFixed(1)}%
                  </span>
                  <span className="text-muted-foreground">vs last month</span>
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="shadow-md relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">Period Expenses</CardTitle>
              <ArrowUpCircle className="h-5 w-5 text-red-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-red-600 dark:text-red-400">-{fmt(totalExpenses)}</div>
               {timePeriod === "THIS_MONTH" && (
                <div className="flex items-center gap-1 mt-1 text-xs">
                  <span className={momExpense.change <= 0 ? "text-emerald-600" : "text-rose-500"}>
                    {momExpense.change <= 0 ? "↓" : "↑"} {Math.abs(momExpense.change).toFixed(1)}%
                  </span>
                  <span className="text-muted-foreground">vs last month</span>
                </div>
              )}
            </CardContent>
          </Card>

          <Card className={`shadow-md border-0 bg-gradient-to-br ${netWorth >= 0 ? "from-emerald-500 to-emerald-700" : "from-rose-500 to-rose-700"} text-white`}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-white/80">Net Worth</CardTitle>
              <TrendingUp className="h-5 w-5 text-white/60" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{fmt(netWorth)}</div>
              <p className="text-xs text-white/70 mt-1">Total Assets − Total Liabilities</p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* ── Middle Row: Heatmap & Upcoming Payments ── */}
      <div className="grid gap-6 grid-cols-1 lg:grid-cols-3">
        {/* Expense Heatmap (2 cols) */}
        <Card className="lg:col-span-2 shadow-sm">
          <CardHeader className="pb-4">
            <CardTitle className="text-lg flex items-center gap-2">
              <Activity className="h-5 w-5 text-indigo-500" /> 30-Day Spending Intensity
            </CardTitle>
            <CardDescription>Daily expense heatmap reflecting transaction volume</CardDescription>
          </CardHeader>
          <CardContent>
            {expenseHeatmap.length > 0 ? (
               <div className="bg-zinc-50 dark:bg-zinc-950 p-6 rounded-xl border flex flex-col items-center min-h-[140px]">
                 <div className="flex gap-1 mx-auto w-full items-center h-10 mb-6 px-1">
                   {expenseHeatmap.map((day, i) => (
                      <div key={i} className="flex-1 flex flex-col items-center group relative h-full justify-center">
                        <div 
                           className="w-full h-8 rounded-sm transition-all duration-300 group-hover:scale-110 shadow-sm" 
                           style={{ 
                             backgroundColor: day.amount === 0 ? 'rgba(0,0,0,0.05)' : `rgba(244, 63, 94, ${0.1 + (day.intensity * 0.9)})`,
                             boxShadow: day.amount > 0 ? `0 0 10px rgba(244, 63, 94, ${day.intensity * 0.2})` : 'none'
                           }}
                        ></div>
                        {/* Improved Tooltip */}
                        <div className="absolute bottom-full mb-3 left-1/2 -translate-x-1/2 bg-zinc-900 text-white text-[10px] px-2.5 py-2 rounded-lg opacity-0 group-hover:opacity-100 pointer-events-none transition-all whitespace-nowrap z-50 shadow-2xl border border-white/10 scale-90 group-hover:scale-100 origin-bottom">
                          <p className="font-bold opacity-70 mb-0.5">{day.dateStr}</p>
                          <p className="text-sm font-black text-rose-400">{fmt(day.amount)}</p>
                        </div>
                        {i % 7 === 0 && (
                          <span className="text-[10px] text-muted-foreground absolute -bottom-7 font-bold whitespace-nowrap pt-1">
                            {day.dateStr}
                          </span>
                        )}
                      </div>
                   ))}
                 </div>
                 <div className="flex items-center gap-4 mt-2 text-[10px] text-muted-foreground font-bold">
                    <span>Low Spending</span>
                    <div className="flex gap-1">
                       {[0.1, 0.3, 0.5, 0.7, 1].map(v => (
                          <div key={v} className="w-3 h-3 rounded-sm" style={{ backgroundColor: `rgba(244, 63, 94, ${v})` }}></div>
                       ))}
                    </div>
                    <span>High Spending</span>
                 </div>
               </div>
            ) : (
              <div className="h-[100px] flex items-center justify-center text-muted-foreground/50 border rounded-xl bg-zinc-50/50 dark:bg-zinc-950/50 text-sm">
                No spending in the last 30 days
              </div>
            )}
          </CardContent>
        </Card>

        {/* Upcoming Payments (1 col) */}
        <Card className="shadow-sm">
           <CardHeader className="pb-4">
            <div className="flex items-center justify-between">
              <CardTitle className="text-lg flex items-center gap-2">
                <Receipt className="h-5 w-5 text-amber-500" /> Next Payments
              </CardTitle>
              <Link href="/my/obligations" className="text-xs text-indigo-600 hover:underline">Manage All</Link>
            </div>
          </CardHeader>
          <CardContent>
            {upcomingPayments.length > 0 ? (
              <div className="space-y-3">
                {upcomingPayments.map((p) => {
                   const daysLeft = Math.ceil((p.date.getTime() - new Date().getTime()) / (1000 * 3600 * 24));
                   return (
                    <div key={p.id} className="flex items-center justify-between p-3 rounded-lg border bg-white dark:bg-zinc-950 shadow-sm hover:shadow-md transition-all">
                      <div className="flex flex-col">
                        <span className="font-medium text-sm truncate max-w-[140px]">{p.name}</span>
                        <div className="flex items-center gap-2 mt-0.5">
                          <span className={`text-[10px] px-1.5 py-0.5 rounded-full font-medium ${p.type === 'EMI' ? 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400' : 'bg-pink-100 text-pink-700 dark:bg-pink-900/30 dark:text-pink-400'}`}>{p.type}</span>
                          <span className="text-xs text-muted-foreground">
                            {p.date.toLocaleDateString("en-IN", { month: "short", day: "numeric" })}
                          </span>
                        </div>
                      </div>
                      <div className="flex flex-col items-end">
                         <span className="font-bold text-sm text-foreground">{fmt(p.amount)}</span>
                         <span className={`text-xs mt-0.5 font-medium ${daysLeft <= 3 ? 'text-rose-500' : 'text-emerald-600'}`}>
                           {daysLeft === 0 ? 'Today' : daysLeft === 1 ? 'Tomorrow' : `In ${daysLeft} days`}
                         </span>
                      </div>
                    </div>
                  )
                })}
              </div>
            ) : (
               <div className="h-[150px] flex items-center justify-center text-muted-foreground/50 border border-dashed rounded-xl text-sm">
                No upcoming payments found
              </div>
            )}
          </CardContent>
        </Card>
      </div>


      {/* ── Charts Row: Cash Flow & Interactive Drill Down ── */}
      <div className="grid gap-6 grid-cols-1 lg:grid-cols-2">
        {/* Income vs Expense Area Chart */}
        <Card className="shadow-sm">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-lg flex items-center gap-2">
                <Activity className="h-5 w-5 text-indigo-500" /> Cash Flow Trend
              </CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            {monthlyFlow.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <AreaChart data={monthlyFlow} margin={{ top: 5, right: 10, left: 0, bottom: 0 }}>
                  <defs>
                    <linearGradient id="incomeGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor={COLORS.emerald} stopOpacity={0.3} />
                      <stop offset="95%" stopColor={COLORS.emerald} stopOpacity={0} />
                    </linearGradient>
                    <linearGradient id="expenseGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor={COLORS.rose} stopOpacity={0.3} />
                      <stop offset="95%" stopColor={COLORS.rose} stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                  <XAxis dataKey="month" tick={{ fontSize: 11 }} stroke="hsl(var(--muted-foreground))" />
                  <YAxis tickFormatter={fmtShort} tick={{ fontSize: 11 }} stroke="hsl(var(--muted-foreground))" />
                  <Tooltip content={<ChartTooltip />} />
                  <Area type="monotone" dataKey="income" name="Income" stroke={COLORS.emerald} fill="url(#incomeGrad)" strokeWidth={2} />
                  <Area type="monotone" dataKey="expense" name="Expense" stroke={COLORS.rose} fill="url(#expenseGrad)" strokeWidth={2} />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[300px] flex items-center justify-center text-muted-foreground/50 border rounded-xl bg-zinc-50/50 dark:bg-zinc-950/50">
                <p>No transaction data yet based on filters</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Interactive Expense Breakdown */}
        <Card className="shadow-sm">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-lg flex items-center gap-2">
                <PieChartIcon className="h-5 w-5 text-amber-500" /> Category Drill-down
              </CardTitle>
              {selectedCategory && (
                <Button variant="ghost" size="sm" onClick={() => setSelectedCategory(null)} className="h-8 text-xs px-2">
                  <X className="h-3 w-3 mr-1"/> Clear Selection
                </Button>
              )}
            </div>
            <CardDescription>Click on a slice to view recent transactions</CardDescription>
          </CardHeader>
          <CardContent>
            {expenseByCategory.length > 0 ? (
              <div className="flex flex-col h-[300px]">
                {!selectedCategory ? (
                  // Show Pie Chart & Legend
                  <div className="flex items-center h-full">
                    <ResponsiveContainer width="50%" height="100%">
                      <PieChart>
                        <Pie
                          data={expenseByCategory.slice(0, 10)}
                          cx="50%" cy="50%"
                          outerRadius={95} innerRadius={60}
                          dataKey="value"
                          strokeWidth={2} stroke="hsl(var(--background))"
                          className="cursor-pointer transition-all hover:scale-105 outline-none"
                          onClick={(data) => setSelectedCategory(data.name || null)}
                        >
                          {expenseByCategory.slice(0, 10).map((_, i) => (
                            <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                          ))}
                        </Pie>
                        <Tooltip content={<ChartTooltip />} />
                      </PieChart>
                    </ResponsiveContainer>
                    <div className="flex-1 space-y-2 overflow-y-auto max-h-[250px] pr-2 custom-scrollbar">
                      {expenseByCategory.slice(0, 10).map((item, i) => (
                        <div 
                           key={item.name} 
                           className="flex items-center justify-between text-sm p-1.5 rounded-md hover:bg-zinc-100 dark:hover:bg-zinc-800 cursor-pointer transition-colors"
                           onClick={() => setSelectedCategory(item.name)}
                        >
                          <div className="flex items-center gap-2">
                            <div className="h-3 w-3 rounded-full" style={{ background: PIE_COLORS[i % PIE_COLORS.length] }} />
                            <span className="text-muted-foreground truncate max-w-[100px]">{item.name}</span>
                          </div>
                          <span className="font-semibold">{fmt(item.value)}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : (
                  // Show Filtered Transactions
                  <div className="h-full flex flex-col">
                     <div className="flex items-center gap-2 mb-4 p-3 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 rounded-lg border border-rose-100 dark:border-rose-900/30">
                        <TrendingDown className="h-5 w-5" />
                        <div>
                          <p className="font-medium text-sm">Recent {selectedCategory} Expenses</p>
                        </div>
                     </div>
                     <div className="flex-1 overflow-y-auto space-y-2 pr-2 custom-scrollbar">
                        {drillDownTransactions.length > 0 ? drillDownTransactions.map(t => (
                           <div key={t.id} className="flex justify-between items-center p-3 rounded-lg border bg-white dark:bg-zinc-950 shadow-sm">
                              <div>
                                 <p className="text-sm font-medium">{t.description || t.category}</p>
                                 <p className="text-xs text-muted-foreground">{new Date(t.transactionDate).toLocaleDateString()}</p>
                              </div>
                              <span className="font-semibold text-rose-600 dark:text-rose-400">-{fmt(t.money.amount)}</span>
                           </div>
                        )) : (
                          <div className="text-sm text-muted-foreground text-center py-8">No recent transactions found for this category.</div>
                        )}
                     </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="h-[300px] flex items-center justify-center text-muted-foreground/50 border rounded-xl bg-zinc-50/50 dark:bg-zinc-950/50">
                <p>No expense data in this period</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

       {/* ── Balances & Progress Row ── */}
       <div className="grid gap-6 grid-cols-1 lg:grid-cols-2">
         {/* Account Balances Bar */}
        <Card className="shadow-sm">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-lg flex items-center gap-2">
                <BarChart3 className="h-5 w-5 text-indigo-500" /> Account Balances
              </CardTitle>
              <Link href="/my/accounts" className="text-sm text-indigo-600 hover:underline">Manage All</Link>
            </div>
          </CardHeader>
          <CardContent>
            {accountBalances.length > 0 ? (
              <ResponsiveContainer width="100%" height={260}>
                <BarChart data={accountBalances} layout="vertical" margin={{ top: 0, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" horizontal={false} />
                  <XAxis type="number" tickFormatter={fmtShort} tick={{ fontSize: 11 }} stroke="hsl(var(--muted-foreground))" />
                  <YAxis type="category" dataKey="name" tick={{ fontSize: 11 }} stroke="hsl(var(--muted-foreground))" width={110} />
                  <Tooltip content={<ChartTooltip />} />
                  <Bar dataKey="balance" name="Balance" radius={[0, 4, 4, 0]} maxBarSize={32}>
                    {accountBalances.map((_, i) => (
                      <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[260px] flex items-center justify-center text-muted-foreground/50 border rounded-xl bg-zinc-50/50 dark:bg-zinc-950/50">
                <p>No accounts yet</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Loan Progress Cards */}
        {obligations && obligations.liabilities?.length > 0 ? (
          <Card className="shadow-sm border-amber-100 dark:border-amber-900/30 relative overflow-hidden">
             <div className="absolute top-0 w-full h-1 bg-gradient-to-r from-amber-400 to-orange-500"></div>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Landmark className="h-5 w-5 text-amber-500" /> Loan Progress
                </CardTitle>
                <Link href="/my/obligations/liabilities" className="text-sm text-indigo-600 dark:text-indigo-400 hover:underline">
                  View all
                </Link>
              </div>
            </CardHeader>
            <CardContent>
              <div className="grid gap-3">
                {obligations.liabilities.slice(0, 3).map((l) => {
                  const { paidPercent, effectiveOutstanding } = getLiabilityProgress(l);
                  return (
                    <div key={l.id} className="bg-zinc-50 border dark:bg-zinc-950/50 rounded-xl p-4 transition-all hover:bg-white dark:hover:bg-zinc-950 hover:shadow-md">
                      <div className="flex items-start justify-between mb-2">
                        <div>
                          <p className="font-semibold text-sm">{l.name}</p>
                          <p className="text-xs text-muted-foreground mt-0.5">{LiabilityTypeLabels[l.type]}</p>
                        </div>
                        <span className="text-xs font-bold px-2 py-1 bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400 rounded-lg">{paidPercent}% Paid</span>
                      </div>
                      <div className="h-2 rounded-full bg-zinc-200 dark:bg-zinc-800 overflow-hidden mb-2">
                        <div
                          className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400 transition-all duration-1000"
                          style={{ width: `${paidPercent}%` }}
                        />
                      </div>
                      <div className="flex justify-between text-xs text-muted-foreground font-medium">
                        <span>Left: {fmt(effectiveOutstanding)}</span>
                        <span>Total: {fmt(l.principalAmount.amount)}</span>
                      </div>
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        ) : (
          <Card className="shadow-sm">
             <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                  <Landmark className="h-5 w-5 text-amber-500" /> Loan Progress
                </CardTitle>
             </CardHeader>
             <CardContent>
                <div className="h-[200px] flex flex-col items-center justify-center text-muted-foreground/50 border border-dashed rounded-xl gap-2">
                  <p>You have no active loans.</p>
                  <Link href="/my/obligations/liabilities">
                     <Button variant="outline" size="sm">Add Liability</Button>
                  </Link>
                </div>
             </CardContent>
          </Card>
        )}
       </div>

    </div>
  );
}
