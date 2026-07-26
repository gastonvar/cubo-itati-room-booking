# Frontend Architecture

The frontend uses a small, feature-first structure inspired by Bulletproof React.

## Folder ownership

- `src/app`: application composition, providers, router, route-level layouts, and guards.
- `src/features/<feature>`: domain API calls, hooks, components, types, and feature routes.
- `src/features/<feature>/types`: domain and feature-owned types (API DTOs, form values, hook params, calendar models).
- `src/components`: reusable, domain-free UI primitives.
- `src/lib`: application-wide infrastructure such as Axios and TanStack Query clients.
- `src/config`: runtime environment values such as `VITE_API_URL`.
- `src/types`: types shared across multiple features only (for example `ApiResponse`).

Keep one-off React component `*Props` colocated with their components. Put reusable feature domain types in `features/<feature>/types`, not beside hooks, libs, or UI files.

## Dependency rules

1. Use absolute `@/` imports.
2. Features may depend on shared `components`, `lib`, `config`, and `types`.
3. A feature must not import another feature's internal `api`, `hooks`, `lib`, or `types`.
4. Cross-feature behavior belongs in `src/app` route composition. Prefer focused helpers over reaching into another feature’s query-key internals. Do not add barrel `index.ts` files.
5. Keep API request functions in `features/<feature>/api` and TanStack Query hooks in `features/<feature>/hooks`.
6. Keep route components inside a feature when one feature owns the screen. Cross-feature screens stay in `src/app/routes`.
7. Add a compound component only when related reusable parts genuinely share state or accessibility behavior. Prefer explicit props for one-off components.

## Current feature map

- `auth` (`types/auth.ts`): login form values, session status, auth API DTOs, and the login route.
- `chat` (`types/chat.ts`): messages, chat request/response DTOs, composer state, and chat mutation.
- `rooms` (`types/room.ts`): room/schedule DTOs, calendar events and date ranges, schedule hook params, room colors; calendar range helpers live in `lib/calendar-date-range.ts`.

The authenticated workspace route composes chat and rooms. It owns the reaction
that refreshes room data after a successful chat response, so the chat feature
does not depend on room query internals.

## HTTP clients

- `src/lib/api-client.ts` is the authenticated Axios client. It sends httpOnly
  auth cookies with credentials, refreshes once after a 401, and retries the
  original request after the backend rotates the refresh token.
- `src/features/auth/lib/auth-api.ts` is intentionally bare. Login, refresh, and
  logout use it to avoid recursively invoking authenticated interceptors.

The authenticated client depends on the auth session bridge as a deliberate
infrastructure exception. Keep this integration centralized.
