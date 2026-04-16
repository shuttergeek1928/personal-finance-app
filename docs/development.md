# Development Guidelines

## Getting Started
Ensure you have the following installed:
- Node.js (v20+)
- .NET 8 SDK
- Docker & Docker Compose

## Typical Workflow
1. Check existing `.NET` Query schemas using `MediatR` before building duplicate API calls.
2. Ensure you modify the database context using `EF Core Migrations`. Run `dotnet ef migrations add <Name>` inside the specific microservice folder.
3. Adhere to **CQRS**: Separate your Command controllers from your Query endpoints.

## Code formatting
- **Backend**: Standard C# Roslyn conventions.
- **Frontend**: Managed via ESLint/Prettier inside the `apps/web` directory. Do not bypass Husky hooks.
