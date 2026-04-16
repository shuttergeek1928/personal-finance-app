# Frontend Guidelines (Next.js 15)

## Aesthetics Standards
The UI targets a premium dashboard aesthetic.
- **Tailwind**: Depend on standard, customized tailwind utility tokens (`dark:bg-zinc-950`).
- **Icons**: Utilize `lucide-react`.
- **Components**: Adhere to `shadcn/ui` style functional subcomponents strictly localized in `apps/web/src/components/ui`.

## Data Fetching
- Client-side computation should be reserved for instantaneous user filter selections.
- DO NOT perform large data formatting natively in React arrays. All "Total Counts," "Recent Summaries," and "Tracking Progress" strings must be mapped statically through efficient API responses (`axios` configured inside `services/`).
