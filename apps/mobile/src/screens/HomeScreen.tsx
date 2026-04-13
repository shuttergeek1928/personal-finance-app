import React, { useEffect, useMemo } from "react";
import { View, Image, TouchableOpacity, Text } from "react-native";
import { 
  DollarSign, 
  ArrowUpRight, 
  ArrowDownLeft, 
  TrendingUp,
  Plus,
  ChevronRight,
  Wallet
} from "lucide-react-native";
import { useFinanceStore } from "../store/useFinanceStore";
import { useAuthStore } from "../store/useAuthStore";
import { Container } from "../components/Container";
import { Card, CardContent } from "../components/ui/Card";
import { H1, H2, H3, P, Small, Muted } from "../components/ui/Typography";
import { Button } from "../components/ui/Button";
import { cn } from "../utils/cn";

const HomeScreen = ({ navigation }: { navigation: any }) => {
  const { user } = useAuthStore();
  const { accounts, transactions, isLoading, fetchDashboardData } = useFinanceStore();

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);

  const totalBalance = useMemo(
    () => accounts.reduce((acc, curr) => acc + curr.balance, 0),
    [accounts]
  );

  const monthlyIncome = useMemo(
    () => transactions
      .filter((t) => t.type === "income")
      .reduce((acc, curr) => acc + curr.amount, 0),
    [transactions]
  );

  const monthlyExpenses = useMemo(
    () => transactions
      .filter((t) => t.type === "expense")
      .reduce((acc, curr) => acc + curr.amount, 0),
    [transactions]
  );

  return (
    <Container scrollable className="bg-zinc-50 dark:bg-zinc-950">
      {/* Header */}
      <View className="flex-row justify-between items-center mb-6">
        <View>
          <Muted>Welcome back,</Muted>
          <H2>{user?.firstName || "Guest"}</H2>
        </View>
        <TouchableOpacity 
          className="w-12 h-12 rounded-full bg-zinc-200 dark:bg-zinc-800 items-center justify-center overflow-hidden"
          onPress={() => navigation.navigate("Profile")}
        >
          {user?.profile?.avatar ? (
            <Image source={{ uri: user.profile.avatar }} className="w-full h-full" />
          ) : (
            <Wallet size={24} color="#71717a" />
          )}
        </TouchableOpacity>
      </View>

      {/* Main Balance Card */}
      <Card className="bg-indigo-600 dark:bg-indigo-600 border-0 mb-6 overflow-hidden">
        <View className="absolute -right-8 -top-8 w-32 h-32 bg-white/10 rounded-full" />
        <CardContent className="p-6">
          <Text className="text-indigo-100 text-sm font-medium mb-1">Total Balance</Text>
          <Text className="text-white text-4xl font-bold mb-4">
            ${totalBalance.toLocaleString("en-US", { minimumFractionDigits: 2 })}
          </Text>
          <View className="flex-row justify-between">
            <View className="flex-row items-center">
              <View className="w-8 h-8 rounded-full bg-white/20 items-center justify-center mr-2">
                <ArrowUpRight size={16} color="white" />
              </View>
              <View>
                <Text className="text-indigo-100 text-xs text-opacity-80">Income</Text>
                <Text className="text-white font-semibold">+${monthlyIncome.toLocaleString()}</Text>
              </View>
            </View>
            <View className="flex-row items-center">
              <View className="w-8 h-8 rounded-full bg-white/20 items-center justify-center mr-2">
                <ArrowDownLeft size={16} color="white" />
              </View>
              <View>
                <Text className="text-indigo-100 text-xs text-opacity-80">Expenses</Text>
                <Text className="text-white font-semibold">-${monthlyExpenses.toLocaleString()}</Text>
              </View>
            </View>
          </View>
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <View className="flex-row justify-between mb-8">
        <TouchableOpacity 
          className="items-center"
          onPress={() => navigation.navigate("AddIncome")}
        >
          <View className="w-14 h-14 rounded-2xl bg-emerald-100 dark:bg-emerald-950/30 items-center justify-center mb-2">
            <Plus size={24} color="#10b981" />
          </View>
          <Small className="font-medium">Income</Small>
        </TouchableOpacity>
        <TouchableOpacity 
          className="items-center"
          onPress={() => navigation.navigate("AddExpense")}
        >
          <View className="w-14 h-14 rounded-2xl bg-red-100 dark:bg-red-950/30 items-center justify-center mb-2">
            <TrendingUp size={24} color="#ef4444" />
          </View>
          <Small className="font-medium">Expense</Small>
        </TouchableOpacity>
        <TouchableOpacity className="items-center">
          <View className="w-14 h-14 rounded-2xl bg-blue-100 dark:bg-blue-950/30 items-center justify-center mb-2">
            <Wallet size={24} color="#3b82f6" />
          </View>
          <Small className="font-medium">Wallets</Small>
        </TouchableOpacity>
        <TouchableOpacity className="items-center">
          <View className="w-14 h-14 rounded-2xl bg-zinc-200 dark:bg-zinc-800 items-center justify-center mb-2">
            <ChevronRight size={24} color="#71717a" />
          </View>
          <Small className="font-medium">More</Small>
        </TouchableOpacity>
      </View>

      {/* Recent Transactions */}
      <View className="mb-6">
        <View className="flex-row justify-between items-center mb-4">
          <H3>Recent Transactions</H3>
          <TouchableOpacity onPress={() => navigation.navigate("Transactions")}>
            <Text className="text-indigo-600 dark:text-indigo-400 font-medium">See All</Text>
          </TouchableOpacity>
        </View>

        <View className="space-y-4">
          {transactions.slice(0, 5).map((t) => (
            <Card key={t.id} className="border-0 bg-white dark:bg-zinc-900 shadow-sm">
              <CardContent className="p-4 flex-row items-center">
                <View className={cn(
                  "w-12 h-12 rounded-xl items-center justify-center mr-4",
                  t.type === "income" ? "bg-emerald-100 dark:bg-emerald-950/30" : "bg-zinc-100 dark:bg-zinc-800"
                )}>
                  {t.type === "income" ? (
                    <ArrowUpRight size={20} color="#10b981" />
                  ) : (
                    <ArrowDownLeft size={20} color="#71717a" />
                  )}
                </View>
                <View className="flex-1">
                  <Text className="text-zinc-900 dark:text-zinc-100 font-semibold">{t.description}</Text>
                  <Muted>{t.category}</Muted>
                </View>
                <Text className={cn(
                  "font-bold text-base",
                  t.type === "income" ? "text-emerald-600" : "text-zinc-900 dark:text-zinc-100"
                )}>
                  {t.type === "income" ? "+" : "-"}${t.amount.toLocaleString()}
                </Text>
              </CardContent>
            </Card>
          ))}
        </View>
      </View>
    </Container>
  );
};

export default HomeScreen;
