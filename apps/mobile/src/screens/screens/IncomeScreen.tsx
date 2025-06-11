import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  SafeAreaView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import Layout from '../components/layout.tsx';

interface IncomeEntry {
  id: string;
  amount: number;
  description: string;
  date: string;
}

const IncomeScreen = ({navigation}:{navigation:any}) => {
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [incomes, setIncomes] = useState<IncomeEntry[]>([]);

  const handleAddIncome = () => {
    if (amount && description) {
      const newIncome: IncomeEntry = {
        id: Date.now().toString(),
        amount: parseFloat(amount),
        description,
        date: new Date().toLocaleDateString(),
      };
      setIncomes([...incomes, newIncome]);
      setAmount('');
      setDescription('');
    }
  };

  const totalIncome = incomes.reduce((sum, income) => sum + income.amount, 0);

  return (
    <Layout navigation={navigation}>
      <ScrollView style={styles.content}>
        {/* Income Form */}
        <View style={styles.formContainer}>
          <Text style={styles.sectionTitle}>Add New Income</Text>
          <TextInput
            style={styles.input}
            placeholder="Amount"
            keyboardType="numeric"
            value={amount}
            onChangeText={setAmount}
          />
          <TextInput
            style={styles.input}
            placeholder="Description"
            value={description}
            onChangeText={setDescription}
          />
          <TouchableOpacity style={styles.addButton} onPress={handleAddIncome}>
            <Text style={styles.addButtonText}>Add Income</Text>
          </TouchableOpacity>
        </View>

        {/* Summary Section */}
        <View style={styles.summaryContainer}>
          <Text style={styles.sectionTitle}>Summary</Text>
          <View style={styles.summaryCard}>
            <Text style={styles.summaryLabel}>Total Income</Text>
            <Text style={styles.summaryAmount}>${totalIncome.toFixed(2)}</Text>
          </View>
        </View>

        {/* Recent Income List */}
        <View style={styles.listContainer}>
          <Text style={styles.sectionTitle}>Recent Income</Text>
          {incomes.map((income) => (
            <View key={income.id} style={styles.incomeItem}>
              <View>
                <Text style={styles.incomeDescription}>{income.description}</Text>
                <Text style={styles.incomeDate}>{income.date}</Text>
              </View>
              <Text style={styles.incomeAmount}>${income.amount.toFixed(2)}</Text>
            </View>
          ))}
        </View>
      </ScrollView>
      </Layout>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 16,
    backgroundColor: '#fff',
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  menuButton: {
    marginRight: 16,
  },
  headerTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#333',
  },
  content: {
    flex: 1,
  },
  formContainer: {
    padding: 16,
    backgroundColor: '#fff',
    margin: 16,
    borderRadius: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 16,
    color: '#333',
  },
  input: {
    backgroundColor: '#f5f5f5',
    padding: 12,
    borderRadius: 8,
    marginBottom: 12,
    fontSize: 16,
  },
  addButton: {
    backgroundColor: '#4CAF50',
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
  },
  addButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  summaryContainer: {
    padding: 16,
  },
  summaryCard: {
    backgroundColor: '#fff',
    padding: 16,
    borderRadius: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  summaryLabel: {
    fontSize: 16,
    color: '#666',
  },
  summaryAmount: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#4CAF50',
    marginTop: 8,
  },
  listContainer: {
    padding: 16,
  },
  incomeItem: {
    backgroundColor: '#fff',
    padding: 16,
    borderRadius: 12,
    marginBottom: 8,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  incomeDescription: {
    fontSize: 16,
    fontWeight: '500',
    color: '#333',
  },
  incomeDate: {
    fontSize: 14,
    color: '#666',
    marginTop: 4,
  },
  incomeAmount: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#4CAF50',
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    padding: 16,
    backgroundColor: '#fff',
    borderTopWidth: 1,
    borderTopColor: '#e0e0e0',
  },
  footerButton: {
    alignItems: 'center',
  },
  footerText: {
    marginTop: 4,
    fontSize: 12,
    color: '#666',
  },
  activeText: {
    color: '#4CAF50',
    fontWeight: 'bold',
  },
});

export default IncomeScreen; 