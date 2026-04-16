# API Reference

*Note: Fully documented interactive API references can be found by navigating to the `/swagger` endpoint of any running service.*

## Core Wrappers
All endpoints return standard wrapper implementations:
```json
{
    "success": true,
    "data": { ... },
    "message": "Action completed successfully",
    "errors": []
}
```

## User Management Service (`/api/Auth` and `/api/UserManagement`)
**Authentication**
- `POST /api/Auth/login`: Authenticate and receive JWT
- `POST /api/Auth/register`: Register new user
- `POST /api/Auth/google-login`: OAuth login
- `POST /api/Auth/refresh`: Refresh JWT token

**User Management**
- `GET /api/UserManagement`: Get all users
- `GET /api/UserManagement/{id}`: Get user by ID
- `GET /api/UserManagement/by-email/{email}`: Find user by email
- `POST /api/UserManagement/{id}/confirm-email`: Confirm email
- `PUT /api/UserManagement/{id}/profile`: Update user profile
- `PUT /api/UserManagement/{id}`: Edit User 
- `PUT /api/UserManagement/{id}/roles`: Manage User roles
- `DELETE /api/UserManagement/{id}`: Delete user

## Accounts Service (`/api/Accounts`)
- `POST /api/Accounts/create`: Create a new bank account wallet
- `GET /api/Accounts/{id}`: Get account by ID
- `GET /api/Accounts/{number}`: Get account by strict Account Number
- `GET /api/Accounts/userid/{userId}`: List all checking & savings accounts
- `PUT /api/Accounts/{id}/deposit`: Direct push money into account
- `PUT /api/Accounts/{id}/withdraw`: Direct pull money
- `PUT /api/Accounts/transfer`: Perform internal wallet-to-wallet transition
- `PUT /api/Accounts/{userId}/set-default`: Define the default payment account
- `DELETE /api/Accounts/{userId}/{accountId}`: Close a bank account

## Transactions Service (`/api/Transaction`)
- `POST /api/Transaction/income/deposit`: Create income log
- `POST /api/Transaction/expense/withdraw`: Create expense log
- `POST /api/Transaction/transfer`: Create cross-transfer ledger
- `GET /api/Transaction/{id}`: Get specific transaction data
- `GET /api/Transaction/user/{userId}`: Fetch bulk transaction list
- `GET /api/Transaction/user/{userId}/paged`: Fast paginated search and cursor API
- `GET /api/Transaction/user/{userId}/dashboard-summary`: Analytics aggregation for Heatmaps and Flow Trends

## Obligations Service (`/api/Obligations` & `/api/CreditCards`)
**Liabilities & Loans**
- `GET /api/Obligations/dashboard/{userId}`: Global summary of active liabilities and due EMIs
- `POST /api/Obligations/liabilities`: Create a new tracking loan/EMI
- `GET /api/Obligations/liabilities/{id}`: Get single liability detail
- `GET /api/Obligations/liabilities/user/{userId}`: List all active trackers
- `PUT /api/Obligations/liabilities/{id}`: Update specific loan
- `DELETE /api/Obligations/liabilities/{id}`: Close loan
- `GET /api/Obligations/liabilities/{id}/amortization`: Mathematical chart projection 
- `POST /api/Obligations/liabilities/{id}/payment`: Log specific EMI payoff

**Subscriptions**
- `POST /api/Obligations/subscriptions`: Create recurring Sub
- `GET /api/Obligations/subscriptions/{id}`: Get specific Sub
- `GET /api/Obligations/subscriptions/user/{userId}`: Get user's Subs
- `PUT /api/Obligations/subscriptions/{id}`: Edit Subscription details
- `DELETE /api/Obligations/subscriptions/{id}`: Revoke Sub

**Credit Cards**
- `GET /api/CreditCards`: List all active credit cards for the user context
- `POST /api/CreditCards`: Register a new credit card instance
- `PUT /api/CreditCards/{id}`: Update CC limits and billing
- `DELETE /api/CreditCards/{id}`: Disable a credit card
