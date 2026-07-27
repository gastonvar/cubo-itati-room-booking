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
| Cloud deployment | Railway: API + frontend services (see [Deployment](#deployment-railway)) |

## Development process

This was my first experience building a chatbot assistant, so I treated the
challenge as an opportunity to experiment and learn. I started with the parts
that were already familiar to me. The frontend is a React SPA organized with
the Bulletproof React approach: feature-first modules and a clear separation
between application setup, shared components, and feature code. This is close
to my day-to-day frontend work and gives the small application a structure that
can remain maintainable as it grows.

For the backend, I chose .NET 10. At work I normally use .NET 8, so this also
gave me an opportunity to work with a newer runtime while using modern C#
syntax, including language features introduced since C# 12. The API is a
single project organized entirely as vertical feature slices. I first modeled
the domain rules and built only the services and endpoints required by the
challenge. Time-slot, duration, capacity, ownership, and overlap constraints
live in deterministic backend services and cannot be bypassed by either the UI
or the LLM.

Once the backend could satisfy the PDF requirements without unnecessary
endpoints, I implemented the frontend as a deliberately thin client. Its
backend interactions are limited to authentication, chatbot messages, and the
room schedule used by the calendar panel. The browser does not attempt to
reimplement booking decisions that belong to the server.

With those familiar pieces complete, I moved on to the part I needed to learn:
LLM integration. I initially expected this to be a single API call, but quickly
learned that the application has to orchestrate the conversation and the tool
execution loop itself. I implemented Gemini first. After exhausting its free
usage while testing, I added Groq and OpenRouter as fallback providers. Their
OpenAI-compatible APIs allowed them to share most of one implementation,
although Gemini requires a different request and tool format.

The SPA sends the complete visible user/assistant history with every chat
request. Tool calls and tool results are temporary server-side context within
that request; they are not exposed to the browser or persisted as chat history.

The first working orchestrator was rough around the edges. I iteratively tuned
the system prompt, tool names and descriptions, argument schemas, and success
and error results so that the model had enough context to choose tools and
explain the outcome accurately. I then added deterministic guardrails wherever
possible: authenticated identity comes from the JWT, mutations require an
explicit confirmation flag, dates are normalized on the server, and every
booking rule is checked by application services. Unit tests cover slot and
booking rules, seeding, tool date handling, and service behavior.

Finally, I deployed the monorepo to Railway as separate API and SPA Docker
services, with CORS and `VITE_API_URL` wiring, forwarded headers for secure
cookies, and a persistent SQLite volume.

### How chat orchestration and tool calling work

1. The authenticated SPA sends `POST /chat` with the full visible conversation.
   The controller takes the username from the JWT; the model is never trusted
   to choose the booking owner.
2. `ChatSystemPromptBuilder` creates a fresh system prompt containing the
   booking rules, room catalog, and authoritative current Montevideo date and
   time. `ChatOrchestrator` then tries the configured providers in order:
   Gemini, Groq, and OpenRouter. If a provider is unavailable, the next one
   starts from the same client-visible history rather than inheriting a partial
   internal tool trace.
3. `BookingToolDefinitions` describes each available function to the model:
   its name, purpose, parameters, required fields, and expected formats. The
   shared definitions are converted to Gemini function declarations or
   OpenAI-compatible tool schemas. The tools cover availability, room
   schedules, the current user's bookings, creation, and cancellation.
4. If the model returns ordinary text without a tool call, that text is the
   final answer. The backend exits the provider loop and returns one assistant
   reply to the SPA.
5. If the model returns one or more tool calls, `ChatBookingTools` parses their
   arguments and dispatches them to the deterministic room or booking service.
   Invalid arguments and domain failures become structured tool results just
   like successful operations; the model cannot report a database write as
   successful merely because it requested one.
6. The backend appends the tool result to the provider's temporary conversation
   and calls the model again. The model can request another tool or use the
   result to produce a user-facing explanation. This continues until the model
   returns text with no tool calls, or until the eight-round safety limit is
   reached.
7. Only the final text leaves the orchestration loop. Intermediate tool calls,
   results, and provider details remain internal; the frontend receives a
   single `reply` and adds it to the visible conversation.

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
memory. This is appropriate for the challenge's small room and booking volume
and for a single Railway API instance with a mounted volume.

### Railway dual-service deploy

The challenge tip allows Railway. The API and SPA are separate services from the
same GitHub monorepo, each with a Dockerfile and `railway.toml`. Watch paths
limit rebuilds to `/backend/**` or `/frontend/**`. The API uses forwarded
headers so httpOnly Secure cookies work behind Railway’s HTTPS proxy, and a
volume at `/app/Data` keeps SQLite across redeploys.

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

This was the main challenge encountered during development. During early tests,
the models repeatedly hallucinated dates in 2023 even when the user's wording
clearly referred to the present. Prompt wording alone was not reliable enough,
so the solution now hardens date handling at several levels: the server's
current time is injected into every system prompt, tool descriptions restate
the expected date semantics, tool arguments are normalized centrally, and the
booking services reject invalid or past times.

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

## Deployment (Railway)

The solution is deployed on Railway as two services from
[gastonvar/cubo-itati-room-booking](https://github.com/gastonvar/cubo-itati-room-booking):

| Service | Public URL |
| --- | --- |
| Frontend (SPA) | https://web-production-701d0.up.railway.app |
| API | https://api-production-f9f92.up.railway.app |

Configuration summary:

- **API** — root `backend/`, Dockerfile builder, health check `GET /health`,
  volume mounted at `/app/Data`, watch paths `/backend/**`
- **Frontend** — root `frontend/`, Dockerfile builder, build-time
  `VITE_API_URL` pointing at the API origin, watch paths `/frontend/**`
- **CORS** — API `Cors__Origins` set to the frontend origin (HTTPS, no trailing
  slash)
- **Secrets** — `Jwt__Secret` and at least one LLM provider key
  (`Gemini__ApiKey`, and optionally Groq / OpenRouter)

Full env checklist and local Docker smoke commands live in the root
[`README.md`](../README.md#deployment-railway).

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

