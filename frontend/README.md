# Room Booking Web

Mobile-first React client for the Cubo Itatí conversational room-booking
assistant.

## Stack

- React 19 and TypeScript
- Vite and React Router
- TanStack Query and Axios
- React Hook Form
- Tailwind CSS and React Big Calendar
- Oxlint

## Setup

```powershell
copy .env.example .env
npm install
npm run dev
```

Set `VITE_API_URL` in `.env` to the backend origin. The default development
value is `http://localhost:8000`. The Vite server runs at
`http://localhost:5173`.

## Scripts

```powershell
npm run dev      # Vite development server
npm run build    # TypeScript check and production bundle
npm run lint     # Oxlint
npm run preview  # Preview the built application
```

## Authentication

The backend stores access and refresh tokens in httpOnly cookies. Axios sends
credentials with each API request. After one 401 response, the centralized API
client deduplicates a refresh request and retries the original request once.
Tokens are never rendered or stored in browser-accessible application state.

## Structure

- `src/app` — providers, router, guards, layouts, and cross-feature composition.
- `src/features/auth` — login, logout, refresh, and session state (`types/auth.ts`).
- `src/features/chat` — conversation UI and chat mutation (`types/chat.ts`).
- `src/features/rooms` — room list, schedule queries, and calendar (`types/room.ts`).
- `src/components` — reusable domain-free primitives.
- `src/lib` — shared API and query clients.
- `src/config` — runtime environment values such as `VITE_API_URL`.
- `src/types` — cross-feature shared types only (for example API envelopes).

Feature domain types live under `features/<feature>/types`. Component `*Props`
stay next to their components.

See [ARCHITECTURE.md](ARCHITECTURE.md) for dependency rules and the
[solution documentation](../doc/README.md) for the complete system overview.
