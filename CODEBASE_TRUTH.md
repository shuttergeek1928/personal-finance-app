# 🏛️ Core System Architecture & Service Truths

This document serves as the technical ground truth for the Finance Flow application, detailing the service definitions, communication patterns, and infrastructure rules.

---

## 🏗️ 1. Project Structure (Monorepo)
The application is organized as a monorepo split between frontend and backend services:
*   **Web (`/apps/web`)**: Next.js 14+ application using TypeScript and Tailwind CSS.
*   **Backend Services (`/apps/dotnet-services`)**: .NET 8 source code containing microservices and the API Gateway.
*   **Infrastructure**: Centralized `docker-compose.yml` in the root.

---

## 📡 2. Service Inventory & Endpoints

All service calls are prefixed to allow Nginx path-based routing. The **API Gateway** (running on port 5000) acts as the entry point for all frontend requests.

| Service | Responsibility | Frontend Route Prefix | Internal Port |
| :--- | :--- | :--- | :--- |
| **User Management** | Identity, Auth, RBAC, Profiles | `/gateway-users/` | 5001 |
| **Accounts** | Bank accounts, Balances, Transfers | `/gateway-accounts/` | 5002 |
| **Transactions** | Income, Expenses, History | `/gateway-transactions/` | 5003 |
| **Obligations** | Loans, EMIs, Subscriptions, CCs | `/gateway-obligations/` | 5004 |
| **API Gateway** | Request aggregation and routing | (Proxy Entry) | 5000 |

---

## ⚙️ 3. The "Truth" of API Routing

### 🌍 Relative Path Logic
The frontend is configured to use **relative paths** in production. 
*   **Variable**: `NEXT_PUBLIC_API_URL` is set to an empty string (`""`).
*   **Result**: A call to `authService.login` results in a browser request to `/gateway-users/api/Auth/login`.
*   **Portability**: This allows the app to work on any IP or domain (Azure VM, Localhost, or Production URL) without changing the build.

### 🛡️ Reverse Proxy (Nginx) Rules
Nginx identifies the service based on the URL prefix and forwards the request to the .NET Gateway:
1.  Frontend finds `/gateway-users/`
2.  Nginx **retains** the `/gateway-users/` prefix (no trailing slash in `proxy_pass`) and proxies to `http://localhost:5000`.
3.  The .NET Gateway receives the full route and routes it to the specific microservice.

---

## 🛠️ 4. Technology Stack & Persistence

*   **Database**: SQL Server 2022 (Docker container `finance-sqlserver`).
*   **Messaging**: RabbitMQ (Docker container `finance-rabbitmq`) for async communication (e.g., Transaction -> Account balance updates).
*   **Auth**: JWT Bearer tokens stored in `localStorage` as `auth_token`.
*   **State Management**: Zustand (Client-side).
*   **API Client**: Axios instance in `services/api.ts`.

---

## 🧱 5. Build & Deployment Rules

1.  **Immutability**: Next.js bakes environment variables starting with `NEXT_PUBLIC_` into the static JavaScript chunks at **Build Time**. Changing an env file requires a `docker-compose build --no-cache web`.
2.  **Network Isolation**: In Docker, services communicate via the `finance_network` bridge. The API Gateway is the only backend service with ports mapped to the host (5000).
3.  **Persistence**: SQL Data and RabbitMQ states are persisted via Docker volumes (`sqlserver_data` and `rabbitmq_data`).

---

## 🚨 6. Common Developer "Gotchas"
*   **Server Block**: All `location` directives must be wrapped in a `server { ... }` block in Nginx.
*   **No Trailing Slash**: For this Gateway, the Nginx `proxy_pass http://localhost:5000` must **not** have a trailing slash to preserve the service prefix needed by the Gateway.
*   **Localhost vs IP**: Frontend code must NEVER reference `localhost:5000` directly, as this refers to the user's personal computer, not the server.
