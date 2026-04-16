# Testing Guidelines

## Backend (.NET)
- Place unit tests inside the `tests/` folder adjacent to their corresponding `src/` service directory.
- We utilize xUnit and Moq for isolating components.
- Do not mock the database if you are testing Entity Framework Queries; use a testing Fixture or InMemory context.

## Frontend (Next.js)
- Ensure critical UI components possess basic interaction testing using Jest/React Testing Library.
- Playwright is used for End-to-End browser tests (Configured in root).
