# Personal Finance App Architecture

## Overview
The **Personal Finance App** is built using a modern **Microservices** pattern. The frontend connects to the backend through an API Gateway, directing requests to independent domain services.

## High-Level Components
1. **Frontend**: Next.js 15 (React 19, TailwindCSS, Zustand)
2. **API Gateway**: YARP (.NET 8)
3. **Backend Microservices**: .NET 8 Web APIs
    - `PersonalFinance.Services.UserManagement`
    - `PersonalFinance.Services.Transactions`
    - `PersonalFinance.Services.Obligations`
    - `PersonalFinance.Services.Accounts`
4. **Database**: SQL Server (managed via Entity Framework Core)
5. **Message Broker**: RabbitMQ (for inter-service async events)

## Golden Rules
- **Backend-Heavy Logic**: Do not aggregate datasets in the Next.js frontend. Build exact, performant endpoints using `.NET` query parameters instead.
- **Microservice Isolation**: Services do not share databases directly. They communicate synchronously via the API Gateway or asynchronously via RabbitMQ.
