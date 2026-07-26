# Room Booking Assistant — Solution Documentation

## Project overview

I approached the challenge by separating natural-language interpretation from
the booking rules. The chatbot uses an LLM to understand each request and choose
an operation, but deterministic backend services validate availability,
capacity, time slots, ownership, and overlaps before reading or changing data.
Authenticated users can inspect room availability, view schedules, create
bookings, list their bookings, and cancel bookings they own.

The main implementation challenges were converting phrases such as “tomorrow at
10” into consistent Montevideo times, preventing stale availability from
causing double bookings, keeping tool calls compatible across multiple LLM
providers, and ensuring that the model could not invent a successful booking.
These were addressed with a centralized booking clock and date normalizer, a
transactional overlap recheck, provider-independent tool definitions, explicit
confirmation turns, and server-side validation of every tool result.

The solution is split into:

- a React and TypeScript single-page application;
- an ASP.NET Core API organized as vertical feature slices;
- an LLM orchestration layer with Gemini, Groq, and OpenRouter fallback;
- server-side booking tools and validation;
- an EF Core SQLite persistence layer.

The office catalog defaults to rooms A–E. Startup seeds `RoomCatalog` defaults
into an empty rooms table; existing room rows are left as-is.

## Challenge requirement coverage

| Requirement | Implementation |
| --- | --- |
| User1 and User2 login | JWT access and rotating refresh tokens in httpOnly cookies |
| Rooms A–E with capacities | `RoomCatalog` defaults seeded when the rooms table is empty |
| 30-minute slots | Enforced by `SlotRules` (with `IBookingClock` timezone) and `BookingService` |
| Maximum three-hour booking | Enforced by `BookingService` |
| Capacity limit | Checked against the selected room before persistence |
| No overlapping bookings | Rechecked inside the booking transaction |
| Required meeting title | Validated by the service and reinforced in the system prompt |
| Create a booking through chat tools | `create_booking` |
| List available rooms | `list_available_rooms` |
| Retrieve free/occupied room schedule | `get_room_schedule` tool, `GET /rooms/{code}/schedule`, and schedule panel free/occupied events |
| Cancel only the current user's booking | `cancel_booking` with owner validation |
| Explain stack with code examples | [`technology-walkthrough.ipynb`](technology-walkthrough.ipynb) |
| Component diagram | [`component-diagram.md`](component-diagram.md) |

## Development process

1. **Model the domain rules.** The time-slot, duration, capacity, ownership, and
   overlap constraints were implemented in server-side domain and application
   services so they cannot be bypassed by the UI or LLM.
2. **Build deterministic booking operations.** Repository and service methods
   became the source of truth for room schedules and booking mutations.
3. **Expose operations as LLM tools.** Tool schemas describe the supported
   operations, while `ChatBookingTools` validates arguments and dispatches to
   deterministic services.
4. **Add conversational safeguards.** The system prompt requires availability
   checks and explicit confirmation before creating or cancelling a booking.
5. **Add authentication.** Login issues short-lived access tokens and rotating
   refresh tokens in httpOnly cookies. Booking ownership comes from the
   authenticated identity, never from model-provided arguments.
6. **Build the frontend.** The mobile-first workspace combines chat with a room
   schedule panel and handles loading, empty, and error states.
7. **Add tests and provider fallback.** Unit tests cover slot and booking rules,
   seeding, tool date handling, and service behavior. Chat retries
   configured providers in the order Gemini, Groq, then OpenRouter.

## Key decisions

### Deterministic tools around a probabilistic model

The model interprets language and selects tools, but it cannot write directly to
the database. Validation and ownership checks remain in application services.
This keeps business rules consistent even if the model produces invalid
arguments.

### Server-authoritative identity

The API derives the username from the authenticated JWT. Tool calls do not
accept an owner argument, preventing a user from creating or cancelling a
booking as somebody else.

### Fixed room catalog

Rooms are not administrable through the chatbot. `RoomCatalog` defines the
default A–E seed. Startup inserts those rooms only when the rooms table is
empty; it does not rewrite capacities or delete extra rooms afterward.

### Provider fallback

The chat orchestrator tries configured providers in sequence. A failed provider
does not pass its partial internal tool loop to the next provider; the next
provider restarts from the same client-visible message history.

### SQLite for the challenge implementation

SQLite keeps local setup small and reproducible. Repository code accounts for
SQLite's limited `DateTimeOffset` translation by filtering selected ranges in
memory. This is appropriate for the challenge's small room and booking volume,
but not a claim of multi-instance production scalability.

## Challenges and resolutions

### Date and time interpretation

User phrases such as “tomorrow at 10” must become exact instants. Time is
centralized in `IBookingClock` / `BookingClock` (Montevideo timezone +
`TimeProvider` for “now”). The system prompt supplies the current local date
and offset; `IToolDateTimeNormalizer` parses tool arguments (offset-less values
as Montevideo local). `SlotRules` validates alignment, duration, and business
hours against that timezone; `BusinessCalendar` expands calendar days into
08:00–20:00 bounds. The schedule HTTP API uses half-open Montevideo calendar
dates (`fromDate`, `toDateExclusive`) so the UI and chat tools share one
contract.

### Preventing hallucinated bookings

The assistant is instructed to report success only from a successful tool
result. Creation and cancellation require a separate explicit confirmation
turn, and the booking ID returned by the service is included in confirmation.

### Preventing double booking

Availability displayed before confirmation can become stale. The booking
service therefore repeats the overlap check inside the persistence transaction
immediately before insertion.

### Cross-provider compatibility

Gemini and OpenAI-compatible providers use different tool declaration and
response formats. A shared internal `ToolDefinition` is converted into each
provider's format, while `ChatBookingTools` remains provider-independent.

### Keeping the catalog compliant

Earlier iterations allowed rooms to be created dynamically. That exceeded the
challenge scope. The mutation tool and service path were removed so rooms stay
limited to the seeded A–E defaults for a fresh database.

## Repository guide

- `backend/RoomBooking.Api/Features/Auth` — login, refresh, logout, and users.
- `backend/RoomBooking.Api/Features/Rooms` — room listing and schedules.
- `backend/RoomBooking.Api/Features/Bookings` — booking rules and persistence.
- `backend/RoomBooking.Api/Features/Chat` — orchestration, providers, and tools.
- `backend/RoomBooking.Api/Shared` — database, domain rules, booking clock, HTTP, and security.
- `frontend/src/app` — providers, routing, layouts, and guards.
- `frontend/src/features` — auth, chat, and rooms feature slices.
- `frontend/src/features/*/types` — feature-owned domain types (auth, chat, rooms).
- `frontend/src/features/rooms/lib/calendar-date-range.ts` — day/month half-open date ranges for schedule fetches.
- `frontend/src/components` — shared UI primitives.
- `frontend/src/types` — cross-feature shared types such as API envelopes.

## Local setup

### Backend

```powershell
cd backend
copy .env.example .env
# Set at least Jwt__Secret and one configured LLM provider API key.
dotnet run --project RoomBooking.Api
```

The API listens on `http://localhost:8000`; `GET /health` is unauthenticated.

### Frontend

```powershell
cd frontend
copy .env.example .env
npm install
npm run dev
```

The development app listens on `http://localhost:5173`.

Demo credentials:

- `User1` / `TechnicalChallengePromtior`
- `User2` / `TechnicalChallengePromtior`

## Verification

```powershell
cd backend
dotnet test --configuration Release

cd ..\frontend
npm run build
npm run lint
```

The notebook is self-contained and can be executed without application secrets:

```powershell
jupyter nbconvert --to notebook --execute doc/technology-walkthrough.ipynb `
  --output technology-walkthrough.executed.ipynb
```

