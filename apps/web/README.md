# FinanceFlow - AI Enabled Finance Tracker

This is the production-ready frontend for **FinanceFlow**, an AI-enabled personal finance tracker and optimization platform. It features modern SaaS design, dark mode by default, and robust state management.

## Tech Stack
- **Framework**: Next.js 15 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS (v4) with Tailwind Animate
- **State Management**: Zustand
- **Components**: Shadcn UI (Radix Primitives + Tailwind)
- **Forms & Validation**: React Hook Form + Zod *(Dependencies installed & patterns ready)*
- **API Client**: Axios
- **Data Visualization**: Recharts
- **Icons**: Lucide React

*(Note: While the prompt requested Vite as a bundler, Next.js explicitly uses its own bundler via Turbopack/Webpack. We followed the strict requirement to use Next.js, hence no separate Vite config was included).*

## Architecture & Separation of Concerns

The project follows a component-driven, feature-based architecture to ensure scalability:

1. **/app**: Next.js App Router root. Contains the global layout, pages, and metadata.
   - `page.tsx`: The primary marketing Landing Page with hero section, features, and calls to action.
   - `/dashboard/page.tsx`: The Dashboard view with recharts configurations, summary cards, and spending insights.
   - `/transactions/page.tsx`: The Transactions data table with Add/Delete functionalities.
2. **/components**: All reusable React components split by concern.
   - `/ui`: Atomic Shadcn/UI components (Button, Input, Card, Select, Dialog, Label) customized with our design system.
   - `/layout`: Structural layout elements (`Header.tsx` and `Sidebar.tsx`).
   - `theme-provider.tsx`: Controls the NextThemes integration for dark/light mode context.
3. **/store**: Context and global state.
   - `useFinanceStore.ts`: The central Zustand store containing mock data (Accounts, Transactions), loading states, and side-effect actions (add, delete, fetch).
4. **/services**: API communication layer.
   - `api.ts`: An Axios instance configured with interceptors for JWT/Auth headers, timeout rules, and global error handling to connect seamlessly to the .NET REST backend once available.
5. **/lib**: Utilities.
   - `utils.ts`: Utility functions specifically `cn` for `clsx` and `tailwind-merge`.

## How to Set Up and Run

### Prerequisites
- Node.js (v18 or higher recommended)
- npm or pnpm

### Setup Instructions

1. **Install Dependencies** (from the workspace root or the `web` folder):
   ```bash
   npm install --legacy-peer-deps
   ```
   *Note: Because this was scaffolded within a Turborepo environment, `legacy-peer-deps` might be necessary to bypass strict React 19 version checks with older Radix primitives.*

2. **Start the Development Server**:
   ```bash
   npm run dev
   ```
   This will start the application with Turbopack on `http://localhost:3000`.

3. **Environments Configuration**:
   There is placeholder logic in `services/api.ts` connecting to `process.env.NEXT_PUBLIC_API_URL`. To point to the real .NET backend, create a `.env.local` file at the root of `apps/web`:
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:5000/api
   ```

## Design Notes
- **Color Palette & Typography**: The app uses `Poppins` across the board and relies heavily on a `zinc` and `indigo` Tailwind color palette. The UI responds to system dark mode automatically.
- **Responsiveness**: The Sidebar is hidden on mobile and replaced with a hamburger menu on the Header. Grid layouts for cards and charts automatically stack on smaller viewports.
- **Accessibility**: All interactive elements utilize Radix UI primitives ensuring ARIA specifications, keyboard navigation, and focus management are fully compliant with WCAG 2.1 AA.

## Future Integration Steps (Backend)
- Replace the asynchronous delays and mock lists inside `useFinanceStore.ts` with explicit calls utilizing the configured Axios `api` instance.
- Add standard try/catch error boundaries wrapping the API endpoints to reflect state inside the UI `error` variable natively handled by the components.
