import React, { useState } from "react";
import { View, ScrollView, TouchableOpacity, TextInput } from "react-native";
import { ChevronLeft, Plus } from "lucide-react-native";
import { Container } from "../components/Container";
import { H1, H2, Muted } from "../components/ui/Typography";
import { Input } from "../components/ui/Input";
import { Button } from "../components/ui/Button";
import { Card } from "../components/ui/Card";
import { useFinanceStore } from "../store/useFinanceStore";

const IncomeScreen = ({ navigation }: { navigation: any }) => {
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState("");
  const { addTransaction, isLoading } = useFinanceStore();

  const handleSave = async () => {
    if (!amount || !description) return;
    
    await addTransaction({
      amount: parseFloat(amount),
      description,
      category: category || "General",
      date: new Date().toISOString(),
      type: "income",
      accountId: "a1",
    });
    
    navigation.goBack();
  };

  return (
    <Container className="bg-zinc-50 dark:bg-zinc-950">
      <View className="flex-row items-center mb-8">
        <TouchableOpacity 
          onPress={() => navigation.goBack()}
          className="w-10 h-10 rounded-full bg-zinc-200 dark:bg-zinc-800 items-center justify-center mr-4"
        >
          <ChevronLeft size={24} color="#3f3f46" />
        </TouchableOpacity>
        <H2>Add Income</H2>
      </View>

      <ScrollView showsVerticalScrollIndicator={false}>
        <View className="space-y-6">
          <View className="items-center py-8">
            <Muted className="mb-2">Enter Amount</Muted>
            <View className="flex-row items-center justify-center">
              <H1 className="text-emerald-500 mr-2">$</H1>
              <TextInput
                placeholder="0.00"
                keyboardType="numeric"
                className="text-5xl font-bold text-emerald-600 dark:text-emerald-400 p-0"
                value={amount}
                onChangeText={setAmount}
                autoFocus
              />
            </View>
          </View>

          <Card className="border-0 bg-white dark:bg-zinc-900 shadow-sm p-4">
            <View className="space-y-4">
              <Input
                label="Description"
                placeholder="Where did this income come from?"
                value={description}
                onChangeText={setDescription}
                className="bg-transparent border-0 border-b border-zinc-100 dark:border-zinc-800 rounded-none px-0"
              />
              
              <Input
                label="Category"
                placeholder="Salary, Bonus, Gift, etc."
                value={category}
                onChangeText={setCategory}
                className="bg-transparent border-0 border-b border-zinc-100 dark:border-zinc-800 rounded-none px-0"
              />
            </View>
          </Card>

          <View className="pt-4">
            <Button
              label="Save Income"
              onPress={handleSave}
              isLoading={isLoading}
              size="lg"
              className="bg-emerald-600 dark:bg-emerald-500"
            />
          </View>
        </View>
      </ScrollView>
    </Container>
  );
};

export default IncomeScreen;
