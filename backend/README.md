# RoomBooking API (.NET 10)

Room booking REST API built with ASP.NET Core Controllers and feature-sliced folders (`Features/Auth`, `Features/Rooms`, `Features/Bookings`, `Features/Chat`, `Shared/`).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A `.env` file in the `backend/` directory (see `.env.example`)

## Configuration

Copy `.env.example` to `.env` and fill in:

```
Gemini__ApiKey=your-key
Gemini__Model=gemini-3-flash-preview
Groq__ApiKey=your-groq-key
Groq__Model=llama-3.3-70b-versatile
OpenRouter__ApiKey=your-openrouter-key
OpenRouter__Model=openai/gpt-4o-mini
Jwt__Secret=your-secret-min-32-chars
ConnectionStrings__Default=Data Source=Data/roombooking.db;Cache=Shared;Default Timeout=30
Cors__Origins=http://localhost:5173,http://127.0.0.1:5173,http://localhost:5174,http://127.0.0.1:5174
```

Names use ASP.NET’s `Section__Key` convention (maps to `Section:Key` in config).

Chat tries providers in order: **Gemini → Groq → OpenRouter**. On failure (rate limit / API error), the next configured provider retries with the same client message history (not mid-tool-loop state from the failed provider).

## Run

```bash
cd backend
dotnet run --project RoomBooking.Api
```

The API starts on **http://localhost:8000**.

## Test

```bash
cd backend
dotnet test
```

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | /health | No | Health check |
| POST | /auth/login | No | Login (sets httpOnly JWT cookies) |
| POST | /auth/refresh | No | Refresh session cookies |
| POST | /auth/logout | No | Clear session cookies |
| GET | /auth/me | Yes | Current user |
| GET | /rooms | Yes | List all rooms |
| GET | /rooms/{code}/schedule | Yes | Free/occupied slots for a half-open Montevideo date range (`fromDate`, `toDateExclusive`) |
| POST | /chat | Yes | AI assistant for availability, schedules, booking creation, and own-booking cancellation |

## Seed Data

- **Rooms**: A (4), B (6), C (8), D (10), E (12)
- **Users**: User1, User2 (password: `TechnicalChallengePromtior`)

`RoomCatalog` seeds rooms A–E when the rooms table is empty. Rooms cannot be
created through the API or chatbot; startup does not rewrite an already-populated
rooms table.

## Additional documentation

- [Solution overview](../doc/README.md)
- [Component diagram](../doc/component-diagram.md)
- [Executable technology walkthrough](../doc/technology-walkthrough.ipynb)
- [Backend architecture](ARCHITECTURE.md)
