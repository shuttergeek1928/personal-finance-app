# Database Guidelines

Each microservice governs its own independent SQL Server database schema to adhere to domain-driven design definitions.

## Entity Framework Rules
- Always prioritize **LINQ `.Select()`** mapping over memory evaluations. Query filtering (`Where()`) and analytics calculations MUST translate properly into native SQL scripts.
- Never place `.ToList()` or `.ToArray()` until ALL analytical aggregations (`.Sum()`, `.GroupBy()`) map to a single DTO object.

## Avoiding Concurrency Errors
All database modification paths (`DbUpdateConcurrencyException`) must correctly load entities securely and wrap multi-entity modifications inside synchronous context saves.
