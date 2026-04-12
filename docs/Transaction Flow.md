# Transaction Processing Flow

This document explains the flow of transactions in the Personal Finance System, covering Income, Expense, and Transfer operations.

## Architecture Overview

The system uses a Microservices architecture with an Event-Driven design:

-   **Transaction Service**: Records the transaction intent and acts as the source of truth for transaction history.
-   **Accounts Service**: Manages account balances and state.
-   **Service Bus (MassTransit)**: Handles asynchronous communication between services via integration events.

---

## 1. Income / Deposit Transaction

**Endpoint**: `POST /api/transaction/income/deposit`

### Sequence:

1. **API Call**: User submits a `CreateIncomeTransactionRequest`.
2. **Transaction Service**:
    - Creates a `Transaction` entity with `Type = Income`.
    - Saves the transaction to `TransactionDbContext`.
    - Publishes `IncomeTransactionCreatedEvent`.
3. **Accounts Service** (Consumer):
    - Consumes `IncomeTransactionCreatedEvent`.
    - Finds the target account.
    - Calls `account.Deposit(amount)`.
    - Saves balance change to `AccountDbContext`.
    - Publishes `AccountBalanceUpdatedEvent`.

---

## 2. Expense / Withdraw Transaction

**Endpoint**: `POST /api/transaction/expense/withdraw`

### Sequence:

1. **API Call**: User submits a `CreateExpenseTransactionRequest`.
2. **Transaction Service**:
    - Creates a `Transaction` entity with `Type = Expense`.
    - Saves the transaction to `TransactionDbContext`.
    - Publishes `ExpenseTransactionCreatedEvent`.
3. **Accounts Service** (Consumer):
    - Consumes `ExpenseTransactionCreatedEvent`.
    - Finds the target account.
    - Calls `account.Withdraw(amount)`.
    - Saves balance change to `AccountDbContext`.
    - Publishes `AccountBalanceUpdatedEvent`.

---

## 3. Fund Transfer Transaction

**Endpoint**: `POST /api/transaction/transfer`

### Sequence:

1. **API Call**: User submits a `CreateTransferTransactionRequest` including `AccountId` (source) and `ToAccountId` (destination).
2. **Transaction Service**:
    - Creates a `Transaction` entity with `Type = Transfer`, recording both accounts.
    - Saves the transaction to `TransactionDbContext`.
    - Publishes `TransferTransactionCreatedEvent`.
3. **Accounts Service** (Consumer):
    - Consumes `TransferTransactionCreatedEvent`.
    - Starts a database transaction.
    - **Withdraws** from the source account.
    - **Deposits** to the destination account.
    - Commits the database transaction.
    - Publishes two `AccountBalanceUpdatedEvent` (one for each account).

---

## Data Models

### Transaction Entity

-   `Id`: Unique Identifier
-   `UserId`: Owner of the transaction
-   `AccountId`: Primary account (Source for transfers, Target for income)
-   `ToAccountId`: Destination account (Only for transfers)
-   `Amount`: Numerical value
-   `Currency`: e.g., "INR"
-   `Type`: Income, Expense, or Transfer
-   `Status`: Pending, Approved, or Rejected
