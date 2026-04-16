# AI Agent Instructions for Personal Finance App

Welcome to the **Personal Finance App** codebase! This document outlines the architectural rules, coding standards, and best practices you must follow when contributing to this monorepo.
By adhering to these rules, you'll ensure that the codebase remains maintainable, scalable, and blazingly fast.

## 1. Project Architecture
The application uses a **Microservices Architecture** with a backend-heavy design. 

- **Frontend (`apps/web`)**: Next.js 15 (React 19, TailwindCSS, Lucide Icons, Zustand for state management).
- **API Gateway**: YARP (Yet Another Reverse Proxy), routing requests to microservices.
- **Backend Microservices (`apps/dotnet-services`)**: .NET 8 Web APIs using CQRS (MediatR) and Entity Framework Core with SQL Server.

### Microservices
The backend is split into independent domains:
- `PersonalFinance.Services.UserManagement` (Auth, JWT, Users)
- `PersonalFinance.Services.Transactions` (Income, Expenses, Cash Flows)
- `PersonalFinance.Services.Obligations` (Loans, EMIs, Subscriptions, Credit Cards)
- `PersonalFinance.Services.Accounts` (Bank Accounts, Balances)

## 2. Core Architectural Principles
**Rule #1: Backend-Heavy Processing**
The frontend must remain lightweight. ANY heavy data parsing, mapping, aggregation, filtering, or math computations MUST be handled in the `.NET` backend natively via Entity Framework `IQueryable` operations before returning to the frontend.

- ❌ **Do not:** Fetch raw arrays and `map`/`reduce`/`filter` them in React (e.g., `transactions.reduce()`).
- ✅ **Do:** Create optimized Query Handlers returning structured DTOs (e.g., `GetDashboardSummaryQuery`, paginated lists).

**Rule #2: CQRS Implementation**
The .NET backend utilizes MediatR, segregating read logic (Queries) and write logic (Commands).
- **Commands**: Alter state (Create, Update, Delete). Must return an `ApiResponse`.
- **Queries**: Read state with no side effects. Keep logic lean by utilizing `.Select()` DTO mapping directly from EF Core. Let the database do the grouping operations (`GroupBy`).

**Rule #3: Server-side Pagination & Searching**
List views (like transactions or liabilities) must implement server-side pagination (`take`/`skip`), filtering, and search logic to prevent unbounded data fetching.

**Rule #4: Unified API Responses**
Always wrap endpoints in standardized `ApiResponse<T>` objects:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}
```

## 3. Tech Stack Best Practices

### .NET Backend
- **Data Migrations**: Use Docker configurations to update databases automatically or handle global error retry logic (`EF Core Execution Strategies`).
- **Mapping**: Extend `IMapper` natively through extension methods (`MappingExtensions.cs`) or `Profile` implementations (`ObligationMappingProfile.cs`). Do not put business logic mapping inside controllers.
- **RESTful Endpoints**: Adhere to REST standards. (e.g., `GET /api/Transaction/user/{userId}/paged`) 

### Next.js Frontend
- **Design Aesthetic**: Focus heavily on dynamic micro-animations, glassmorphism, responsive palettes, and premium user experience out of the box using TailwindCSS.
- **Client Components**: Always write `'use client';` explicitly if state (`useState`, `useEffect`) or interactions are necessary.
- **TypeScript**: Strictly type responses using the DTO interfaces outlined in `/services/`. Ensure mapping matches the backend models (e.g., `Money` type).

## 4. Typical Development Flow
When told to add a new feature (e.g. "Add a Heatmap graph"), use the following sequence:
1. **Backend Endpoint**: Identify or create a new `.NET` DTO to securely map the math.
2. **Backend Query/Handler**: Create the MediatR Query logic that accesses the DB and maps to the DTO.
3. **API Controller**: Expose the new query via the proper `.NET` service controller.
4. **Service Hooks**: Update the `apps/web/services` folder (e.g. `transaction.ts`) with the new TypeScript Interface strictly typing the DTO and adding the `axios` API call.
5. **Frontend UI**: Update standard React components, fetching and safely mapping your new structure directly utilizing pure rendering.

## 5. Docker Guidelines
- Running the application locally is strictly managed via Docker Compose.
- The command `docker-compose -p finance-flow up --build -d` controls 12 containers including rabbitmq, sqlserver, web interface, and microservices.
- Base host paths (like API URLs) are automatically managed through `.env` configurations injected during container startup.

## 6. Documentation guidelines
- Whenever a new feature is added or something is modified, update the documentation accordingly.
- The docuemtnation files are located in the `docs` folder.
- The main README.md file is located at the root level.
- The main README.md file contains the overall architecture and guidelines for the project.
- The `docs` folder contains the following files, Create new if not found and if needed:
    - `architecture.md`: Contains the overall architecture and guidelines for the project.
    - `development.md`: Contains the development guidelines for the project.
    - `testing.md`: Contains the testing guidelines for the project.
    - `deployment.md`: Contains the deployment guidelines for the project.
    - `api-reference.md`: Contains the API reference for the project.
    - `database.md`: Contains the database schema and guidelines for the project.
    - `frontend.md`: Contains the frontend guidelines for the project.
    - `backend.md`: Contains the backend guidelines for the project.
    - `docker.md`: Contains the docker guidelines for the project.