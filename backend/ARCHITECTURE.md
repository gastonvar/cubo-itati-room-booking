# Backend architecture

Onboarding guide for `RoomBooking.Api` after the feature-sliced reorganization. Describes the **current** layout and conventions—not aspirational rules.

## Tree

```
backend/
├── RoomBooking.Api/
│   ├── Program.cs                 # Composition root only
│   ├── Bootstrap/                 # App-wide startup wiring
│   │   ├── DependencyInjection.cs
│   │   ├── EnvironmentConfiguration.cs
│   │   ├── DatabaseInitialization.cs
│   │   └── WebApplicationExtensions.cs
│   ├── Features/
│   │   ├── Auth/                  # Login / refresh / logout
│   │   ├── Bookings/              # Booking service/repo (used by Chat tools)
│   │   ├── Rooms/                 # Rooms list + schedules (+ availability for Chat)
│   │   └── Chat/                  # LLM assistant + room/booking tools
│   └── Shared/                    # Cross-cutting only
│       ├── Config/                # JwtSettings
│       ├── Data/                  # AppDbContext, migrations, seeder
│       ├── Domain/                # SlotRules, BusinessCalendar, RoomCatalog
│       ├── Time/                  # IBookingClock / BookingClock (Montevideo)
│       ├── Http/                  # ApiResponse / ApiError
│       └── Security/              # JwtTokenService, CurrentUser
└── RoomBooking.Api.Tests/         # Mirrors feature/shared folders
    ├── Features/Bookings/
    ├── Features/Chat/Tools/
    ├── Shared/Domain/
    └── Shared/Time/
```

Typical feature folders (not every feature has every subfolder):

| Folder | Responsibility |
|--------|----------------|
| `Controllers/` | HTTP endpoints |
| `Services/` | Application logic + interfaces |
| `Repositories/` | Data access |
| `Entities/` | EF entities owned by the feature |
| `Config/` | Feature-owned options (Chat LLM settings) |
| `*DependencyInjection.cs` | Feature DI registration at the feature root |

Chat-only extras:

| Folder | Responsibility |
|--------|----------------|
| `Llm/` | Provider clients (Gemini, Groq, OpenRouter, OpenAI-compatible shared client) |
| `Tools/` | Tool schemas (`BookingToolDefinitions`), arg/result helpers, `ToolDateTime` |

## Responsibilities

### `Program.cs`

Composition only: load env, register `TimeProvider` + `IBookingClock`, infrastructure + features, initialize DB, configure the pipeline. Keeps `public partial class Program` for integration/test hosting.

### `Bootstrap/`

- **DependencyInjection** — controllers + camelCase JSON, SQLite `AppDbContext`, JWT auth/authorization (including `JwtSettings` binding), CORS
- **EnvironmentConfiguration** — discover/load `.env`, re-add environment variables
- **DatabaseInitialization** — ensure `Data/`, migrate with SQLITE_BUSY retry, legacy-schema recovery, seed
- **WebApplicationExtensions** — CORS → auth → `/health` → controllers

### Features

Each feature registers itself via `Add*Feature` (Chat also binds `Gemini` / `Groq` / `OpenRouter` sections).

- **Auth** — users, refresh tokens, `IAuthService`, JWT issuance via Shared security
- **Bookings** — booking rules via `IBookingService` / repository (no HTTP; consumed by Chat tools)
- **Rooms** — room listing, half-open calendar-date schedules (`fromDate` / `toDateExclusive`), and availability via HTTP and Chat tools (A–E seeded on empty DB)
- **Chat** — `ChatOrchestrator` (provider fallback), `ChatSystemPromptBuilder`, `ChatBookingTools` (dispatch/handlers), LLM clients, tool definitions, `IToolDateTimeNormalizer`

### `Shared/`

Truly cross-cutting pieces used by more than one feature or by Bootstrap:

- DB context + migrations + empty-table seeding (`RoomCatalog` / `UserCatalog`)
- `SlotRules` (pure slot/duration/overlap validation), `BusinessCalendar` (day bounds), and A–E `RoomCatalog` defaults
- `IBookingClock` / `BookingClock` — Montevideo timezone, “now”, and schedule range expansion (backed by `TimeProvider`)
- JWT settings/token service and current-user helpers
- shared HTTP response shapes

**Do not put feature-only code in Shared.** Chat schemas, LLM clients, booking/room/auth services, and feature entities belong under `Features/<Name>/`.

### Tests

Tests live under `RoomBooking.Api.Tests` with folders/namespaces aligned to the code under test (`Features.*` / `Shared.*`). Prefer adding new tests next to that structure.

## Intentional cross-feature dependencies

Features are allowed to call each other where the domain overlaps. Today:

- **Chat → Rooms + Bookings** — tools call `IRoomService` / `IBookingService`; the system prompt builder reads `IRoomRepository`
- **Rooms ↔ Bookings** — `RoomService` uses `IBookingRepository` for occupancy; `BookingService` uses `IRoomRepository` for room lookup; EF navigations link `Room` ↔ `Booking`
- **Auth → Shared.Security** — token creation via `JwtTokenService`
- **Bookings / Rooms / Chat → Shared.Time** — services and tool datetime normalization use `IBookingClock`
- **Features → Shared.Data** — repositories/services use `AppDbContext`

There is no hard “no feature-to-feature imports” rule in this codebase.

## Adding a feature (checklist)

1. Create `Features/<Name>/` with the folders you need (`Controllers`, `Services`, `Repositories`, `Entities`, …).
2. Add `<Name>DependencyInjection.cs` with `Add<Name>Feature` (and bind feature config there if it is not app-wide).
3. Call the extension from `Program.cs` in a sensible order relative to existing features.
4. Keep HTTP thin; put behavior in services; keep data access in repositories.
5. Put only genuinely shared types in `Shared/`—default to the feature folder.
6. Add tests under `RoomBooking.Api.Tests/Features/<Name>/` (or `Shared/...` when testing Shared) with matching namespaces.
7. Run `dotnet test --configuration Release` from `backend/`.
