import React from 'react';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import HomeScreen from './src/screens/screens/HomeScreen.tsx';
import ExpenseScreen from './src/screens/screens/ExpenseScreen.tsx';
import IncomeScreen from './src/screens/screens/IncomeScreen.tsx';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import Layout from './src/screens/components/layout.tsx';

const Stack = createNativeStackNavigator();

const App = () => {
  return (
    <NavigationContainer>
      <SafeAreaProvider>
        <StatusBar style="auto" />
        <Stack.Navigator initialRouteName="Home">
          <Stack.Screen name="Home" component={HomeScreen} options={{ headerShown: false }} />
          <Stack.Screen name="Expense" component={ExpenseScreen} options={{ headerShown: false }} />
          <Stack.Screen name="Income" component={IncomeScreen} options={{ headerShown: false }} />
        </Stack.Navigator>
      </SafeAreaProvider>
    </NavigationContainer>

  );
};

export default App;
