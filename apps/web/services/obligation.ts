import api from "./api";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "";
const GATEWAY_BASE_URL = `${BASE_URL}/gateway-obligations`;

// ── Enums ──────────────────────────────────────────────────────────────

export enum LiabilityType {
  HomeLoan = 0,
  PersonalLoan = 1,
  CarLoan = 2,
  EducationLoan = 3,
  CreditCardEmi = 4,
  Other = 5,
}

export const LiabilityTypeLabels: Record<LiabilityType, string> = {
  [LiabilityType.HomeLoan]: "Home Loan",
  [LiabilityType.PersonalLoan]: "Personal Loan",
  [LiabilityType.CarLoan]: "Car Loan",
  [LiabilityType.EducationLoan]: "Education Loan",
  [LiabilityType.CreditCardEmi]: "Credit Card EMI",
  [LiabilityType.Other]: "Other",
};

export enum CreditCardNetwork {
  Visa = 0,
  MasterCard = 1,
  Amex = 2,
  RuPay = 3,
  Other = 4,
}

export const CreditCardNetworkLabels: Record<CreditCardNetwork, string> = {
  [CreditCardNetwork.Visa]: "Visa",
  [CreditCardNetwork.MasterCard]: "MasterCard",
  [CreditCardNetwork.Amex]: "American Express",
  [CreditCardNetwork.RuPay]: "RuPay",
  [CreditCardNetwork.Other]: "Other",
};

export enum SubscriptionType {
  Entertainment = 0,
  Utility = 1,
  Insurance = 2,
  Software = 3,
  Fitness = 4,
  Other = 5,
}

export const SubscriptionTypeLabels: Record<SubscriptionType, string> = {
  [SubscriptionType.Entertainment]: "Entertainment",
  [SubscriptionType.Utility]: "Utility",
  [SubscriptionType.Insurance]: "Insurance",
  [SubscriptionType.Software]: "Software",
  [SubscriptionType.Fitness]: "Fitness",
  [SubscriptionType.Other]: "Other",
};

export enum BillingCycle {
  Monthly = 0,
  Quarterly = 1,
  HalfYearly = 2,
  Yearly = 3,
}

export const BillingCycleLabels: Record<BillingCycle, string> = {
  [BillingCycle.Monthly]: "Monthly",
  [BillingCycle.Quarterly]: "Quarterly",
  [BillingCycle.HalfYearly]: "Half-Yearly",
  [BillingCycle.Yearly]: "Yearly",
};
// ── Utility Functions ──────────────────────────────────────────────────

/**
 * Calculates EMI using the reducing balance formula.
 * EMI = P × r × (1+r)^n / ((1+r)^n - 1)
 */
export function calculateEmi(principal: number, annualRate: number, tenureMonths: number): number {
  if (annualRate === 0) return Math.round((principal / tenureMonths) * 100) / 100;
  const monthlyRate = annualRate / 12 / 100;
  const power = Math.pow(1 + monthlyRate, tenureMonths);
  return Math.round((principal * monthlyRate * power) / (power - 1) * 100) / 100;
}

/**
 * Calculates the theoretical outstanding balance after elapsed months
 * using the reducing balance amortization method.
 */
function calculateTheoreticalOutstanding(
  principal: number,
  annualRate: number,
  tenureMonths: number,
  startDate: Date
): number {
  const now = new Date();
  const elapsed = (now.getFullYear() - startDate.getFullYear()) * 12 + (now.getMonth() - startDate.getMonth());

  if (elapsed <= 0) return principal;
  const cappedElapsed = Math.min(elapsed, tenureMonths);

  const emi = calculateEmi(principal, annualRate, tenureMonths);
  const monthlyRate = annualRate / 12 / 100;
  let outstanding = principal;

  for (let i = 0; i < cappedElapsed; i++) {
    const interest = annualRate === 0 ? 0 : Math.round(outstanding * monthlyRate * 100) / 100;
    const principalComponent = emi - interest;
    outstanding -= principalComponent;
    if (outstanding <= 0) return 0;
  }

  return Math.round(Math.max(outstanding, 0) * 100) / 100;
}

/**
 * Computes the effective outstanding balance for a liability.
 * Takes the MINIMUM of:
 *  - Theoretical outstanding (based on elapsed time from startDate to now)
 *  - Stored outstanding (which reflects manual extra payments)
 *
 * This ensures both elapsed EMIs AND manual payments are accounted for.
 */
export function getEffectiveOutstanding(l: LiabilityDto): number {
  const theoretical = calculateTheoreticalOutstanding(
    l.principalAmount.amount,
    l.interestRate,
    l.tenureMonths,
    new Date(l.startDate)
  );
  // The stored outstanding could be lower if manual payments were made
  return Math.min(theoretical, l.outstandingBalance.amount);
}

/**
 * Computes the paid percentage for a liability's progress bar.
 * Combines both elapsed time-based EMI payments and manual payments.
 */
export function getLiabilityProgress(l: LiabilityDto): { paidPercent: number; paidAmount: number; effectiveOutstanding: number } {
  const principal = l.principalAmount.amount;
  if (principal <= 0) return { paidPercent: 0, paidAmount: 0, effectiveOutstanding: 0 };

  const effectiveOutstanding = getEffectiveOutstanding(l);
  const paidAmount = principal - effectiveOutstanding;
  const paidPercent = Math.min(Math.round((paidAmount / principal) * 100), 100);

  return { paidPercent, paidAmount, effectiveOutstanding };
}

// ── Interfaces ─────────────────────────────────────────────────────────

export interface Money {
  amount: number;
  currency: string;
}

export interface LiabilityDto {
  id: string;
  name: string;
  type: LiabilityType;
  lenderName: string;
  principalAmount: Money;
  outstandingBalance: Money;
  interestRate: number;
  tenureMonths: number;
  emiAmount: Money;
  startDate: string;
  endDate: string;
  userId: string;
  accountId: string | null;
  creditCardId: string | null;
  creditCard: CreditCardDto | null;
  isNoCostEmi: boolean;
  processingFee: Money | null;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

export interface CreditCardDto {
  id: string;
  userId: string;
  bankName: string;
  cardName: string;
  last4Digits: string;
  expiryMonth: number;
  expiryYear: number;
  networkProvider: CreditCardNetwork;
  totalLimit: Money;
  outstandingAmount: Money;
  createdAt: string;
  updatedAt: string;
}

export interface SubscriptionDto {
  id: string;
  name: string;
  type: SubscriptionType;
  provider: string;
  amount: Money;
  billingCycle: BillingCycle;
  nextBillingDate: string;
  startDate: string;
  endDate: string | null;
  autoRenew: boolean;
  userId: string;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

export interface AmortizationScheduleItemDto {
  month: number;
  paymentDate: string;
  emiAmount: number;
  principalComponent: number;
  interestComponent: number;
  outstandingBalance: number;
}

export interface AmortizationScheduleDto {
  liabilityId: string;
  liabilityName: string;
  totalAmountPayable: number;
  totalInterestPayable: number;
  monthlyEmi: number;
  schedule: AmortizationScheduleItemDto[];
}

export interface ObligationDashboardDto {
  totalActiveLiabilities: number;
  totalOutstandingBalance: number;
  totalMonthlyEmi: number;
  totalActiveSubscriptions: number;
  totalMonthlySubscriptionCost: number;
  totalMonthlyObligations: number;
  liabilities: LiabilityDto[];
  subscriptions: SubscriptionDto[];
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
  timestamp: string;
}

// ── Requests ───────────────────────────────────────────────────────────

export interface CreateLiabilityRequest {
  name: string;
  type: LiabilityType;
  lenderName: string;
  principalAmount: number;
  interestRate: number;
  tenureMonths: number;
  startDate: string;
  userId: string;
  accountId?: string | null;
  creditCardId?: string | null;
  isNoCostEmi?: boolean;
  processingFee?: number | null;
}

export interface UpdateLiabilityRequest {
  name: string;
  type: LiabilityType;
  lenderName: string;
  principalAmount: number;
  interestRate: number;
  tenureMonths: number;
  startDate: string;
  accountId?: string | null;
  creditCardId?: string | null;
  isNoCostEmi?: boolean;
  processingFee?: number | null;
}

export interface CreateCreditCardRequest {
  userId: string;
  bankName: string;
  cardName: string;
  last4Digits: string;
  expiryMonth: number;
  expiryYear: number;
  networkProvider: CreditCardNetwork;
  totalLimit: number;
  outstandingAmount: number;
}

export interface UpdateCreditCardRequest {
  bankName: string;
  cardName: string;
  last4Digits: string;
  expiryMonth: number;
  expiryYear: number;
  networkProvider: CreditCardNetwork;
  totalLimit: number;
  outstandingAmount: number;
}

export interface CreateSubscriptionRequest {
  name: string;
  type: SubscriptionType;
  provider: string;
  amount: number;
  billingCycle: BillingCycle;
  startDate: string;
  userId: string;
  autoRenew: boolean;
  endDate?: string | null;
}

export interface UpdateSubscriptionRequest {
  name: string;
  type: SubscriptionType;
  provider: string;
  amount: number;
  billingCycle: BillingCycle;
  startDate: string;
  autoRenew: boolean;
  endDate?: string | null;
}

export interface MakePaymentRequest {
  amount: number;
  note?: string;
}

// ── Service ────────────────────────────────────────────────────────────

export const obligationService = {
  // ── Liabilities ────────────────────────────────────────────────────

  createLiability: async (data: CreateLiabilityRequest): Promise<ApiResponse<LiabilityDto>> => {
    const response = await api.post<ApiResponse<LiabilityDto>>("/api/Obligations/liabilities", data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  updateLiability: async (id: string, data: UpdateLiabilityRequest): Promise<ApiResponse<LiabilityDto>> => {
    const response = await api.put<ApiResponse<LiabilityDto>>(`/api/Obligations/liabilities/${id}`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deleteLiability: async (id: string): Promise<ApiResponse<LiabilityDto>> => {
    const response = await api.delete<ApiResponse<LiabilityDto>>(`/api/Obligations/liabilities/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getLiabilitiesByUserId: async (userId: string): Promise<ApiResponse<LiabilityDto[]>> => {
    const response = await api.get<ApiResponse<LiabilityDto[]>>(`/api/Obligations/liabilities/user/${userId}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getLiabilityById: async (id: string): Promise<ApiResponse<LiabilityDto>> => {
    const response = await api.get<ApiResponse<LiabilityDto>>(`/api/Obligations/liabilities/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getAmortizationSchedule: async (id: string): Promise<ApiResponse<AmortizationScheduleDto>> => {
    const response = await api.get<ApiResponse<AmortizationScheduleDto>>(`/api/Obligations/liabilities/${id}/amortization`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  makePayment: async (id: string, data: MakePaymentRequest): Promise<ApiResponse<LiabilityDto>> => {
    const response = await api.post<ApiResponse<LiabilityDto>>(`/api/Obligations/liabilities/${id}/payment`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  // ── Subscriptions ──────────────────────────────────────────────────

  createSubscription: async (data: CreateSubscriptionRequest): Promise<ApiResponse<SubscriptionDto>> => {
    const response = await api.post<ApiResponse<SubscriptionDto>>("/api/Obligations/subscriptions", data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  updateSubscription: async (id: string, data: UpdateSubscriptionRequest): Promise<ApiResponse<SubscriptionDto>> => {
    const response = await api.put<ApiResponse<SubscriptionDto>>(`/api/Obligations/subscriptions/${id}`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deleteSubscription: async (id: string): Promise<ApiResponse<SubscriptionDto>> => {
    const response = await api.delete<ApiResponse<SubscriptionDto>>(`/api/Obligations/subscriptions/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getSubscriptionsByUserId: async (userId: string): Promise<ApiResponse<SubscriptionDto[]>> => {
    const response = await api.get<ApiResponse<SubscriptionDto[]>>(`/api/Obligations/subscriptions/user/${userId}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getSubscriptionById: async (id: string): Promise<ApiResponse<SubscriptionDto>> => {
    const response = await api.get<ApiResponse<SubscriptionDto>>(`/api/Obligations/subscriptions/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  // ── Dashboard ──────────────────────────────────────────────────────

  getDashboard: async (userId: string): Promise<ApiResponse<ObligationDashboardDto>> => {
    const response = await api.get<ApiResponse<ObligationDashboardDto>>(`/api/Obligations/dashboard/${userId}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  // ── Credit Cards ───────────────────────────────────────────────────

  createCreditCard: async (data: CreateCreditCardRequest): Promise<ApiResponse<CreditCardDto>> => {
    const response = await api.post<ApiResponse<CreditCardDto>>("/api/CreditCards", data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  updateCreditCard: async (id: string, data: UpdateCreditCardRequest): Promise<ApiResponse<CreditCardDto>> => {
    const response = await api.put<ApiResponse<CreditCardDto>>(`/api/CreditCards/${id}`, data, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  deleteCreditCard: async (id: string): Promise<ApiResponse<boolean>> => {
    const response = await api.delete<ApiResponse<boolean>>(`/api/CreditCards/${id}`, { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },

  getCreditCardsByUserId: async (): Promise<ApiResponse<CreditCardDto[]>> => {
    const response = await api.get<ApiResponse<CreditCardDto[]>>("/api/CreditCards", { baseURL: GATEWAY_BASE_URL });
    return response.data;
  },
};
