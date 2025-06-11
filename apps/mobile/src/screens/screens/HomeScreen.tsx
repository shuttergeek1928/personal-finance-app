import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  SafeAreaView,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import Layout from '../components/layout.tsx';

const HomeScreen = ({ navigation }: { navigation: any }) => {
  // Mock data - replace with actual data from your state management
  const balance = 2500.00;
  const recentTransactions = [
    { id: 1, title: 'Grocery Shopping', amount: -85.50, date: '2024-03-20' },
    { id: 2, title: 'Salary', amount: 3000.00, date: '2024-03-19' },
    { id: 3, title: 'Netflix', amount: -15.99, date: '2024-03-18' },
  ];

  return (
    <Layout navigation={navigation}>
      <SafeAreaView style={styles.container}>
        <ScrollView>
          {/* Header Section */}
          <View style={styles.header}>
            <Text style={styles.greeting}>Hello, User</Text>
            <TouchableOpacity style={styles.profileButton}>
              <Ionicons name="person-circle-outline" size={32} color="#333" />
            </TouchableOpacity>
          </View>

          {/* Balance Card */}
          <View style={styles.balanceCard}>
            <Text style={styles.balanceLabel}>Total Balance</Text>
            <Text style={styles.balanceAmount}>${balance.toFixed(2)}</Text>
            <View style={styles.balanceTrend}>
              <Ionicons name="arrow-up" size={16} color="#4CAF50" />
              <Text style={styles.trendText}>2.5% from last month</Text>
            </View>
          </View>

          {/* Quick Actions */}
          <View style={styles.quickActions}>
            <TouchableOpacity style={styles.actionButton} onPress={() => navigation.navigate("Income")}>
              <Ionicons name="add-circle-outline" size={24} color="#333" />
              <Text style={styles.actionText}>Add Income</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionButton} onPress={() => navigation.navigate("Expense")}>
              <Ionicons name="remove-circle-outline" size={24} color="#333" />
              <Text style={styles.actionText}>Add Expense</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionButton}>
              <Ionicons name="swap-horizontal-outline" size={24} color="#333" />
              <Text style={styles.actionText}>Transfer</Text>
            </TouchableOpacity>
          </View>

          {/* Recent Transactions */}
          <View style={styles.transactionsSection}>
            <Text style={styles.sectionTitle}>Recent Transactions</Text>
            {recentTransactions.map((transaction) => (
              <View key={transaction.id} style={styles.transactionItem}>
                <View style={styles.transactionInfo}>
                  <Text style={styles.transactionTitle}>{transaction.title}</Text>
                  <Text style={styles.transactionDate}>{transaction.date}</Text>
                </View>
                <Text
                  style={[
                    styles.transactionAmount,
                    { color: transaction.amount > 0 ? '#4CAF50' : '#F44336' },
                  ]}
                >
                  {transaction.amount > 0 ? '+' : ''}
                  ${Math.abs(transaction.amount).toFixed(2)}
                </Text>
              </View>
            ))}
          </View>
        </ScrollView>
      </SafeAreaView>
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
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 16,
  },
  greeting: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
  },
  profileButton: {
    padding: 4,
  },
  balanceCard: {
    backgroundColor: '#fff',
    margin: 16,
    padding: 20,
    borderRadius: 12,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  balanceLabel: {
    fontSize: 16,
    color: '#666',
  },
  balanceAmount: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#333',
    marginVertical: 8,
  },
  balanceTrend: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  trendText: {
    color: '#4CAF50',
    marginLeft: 4,
  },
  quickActions: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    padding: 16,
    backgroundColor: '#fff',
    marginHorizontal: 16,
    borderRadius: 12,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  actionButton: {
    alignItems: 'center',
  },
  actionText: {
    marginTop: 4,
    color: '#333',
  },
  transactionsSection: {
    margin: 16,
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    elevation: 2,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#333',
    marginBottom: 16,
  },
  transactionItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  transactionInfo: {
    flex: 1,
  },
  transactionTitle: {
    fontSize: 16,
    color: '#333',
  },
  transactionDate: {
    fontSize: 12,
    color: '#666',
    marginTop: 4,
  },
  transactionAmount: {
    fontSize: 16,
    fontWeight: 'bold',
  },
});

export default HomeScreen;
