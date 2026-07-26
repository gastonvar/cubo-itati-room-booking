# Cubo Itatí Room Booking

Conversational meeting-room assistant for the Promtior AI Engineer challenge.

**Stack:** React (Vite, Bulletproof) · .NET 10 API (vertical slices) · Gemini tool calling · SQLite

## Quick start

### Backend

```powershell
cd backend
dotnet run --project RoomBooking.Api
```

API: http://localhost:8000 (health at `/health`)

Ensure `backend/.env` exists (copy from `.env.example`) with `Gemini__ApiKey` and `Jwt__Secret`.

### Frontend

```powershell
cd frontend
npm install
copy .env.example .env   # VITE_API_URL=http://localhost:8000
npm run dev
```

App: http://localhost:5173

### Demo users

| Username | Password |
|----------|----------|
| User1 | TechnicalChallengePromtior |
| User2 | TechnicalChallengePromtior |

## Features

- JWT login
- Chat assistant with LLM tools: create bookings, list availability/schedules, list own bookings, and cancel own bookings
- Fixed Cubo Itatí room catalog: A–E
- Schedule side panel (rooms A–E; occupied and available slots for a selected room)
- Booking rules enforced on the server (30-min slots, max 3h, capacities, no overlaps, 08:00–20:00 America/Montevideo via BookingClock)

## Documentation

- [Project overview, implementation approach, and challenges](doc/README.md)
- [Component and conversation diagrams](doc/component-diagram.md)
- [Executable technology walkthrough](doc/technology-walkthrough.ipynb)
- [Backend architecture](backend/ARCHITECTURE.md)
- [Frontend architecture](frontend/ARCHITECTURE.md)

## Tests

```powershell
cd backend
dotnet test
```

## Deployment (Railway)

Live demo:

| Service | URL |
| --- | --- |
| Frontend | https://web-production-701d0.up.railway.app |
| API | https://api-production-f9f92.up.railway.app (`/health`) |

Deploy as **two services** from this monorepo (challenge tip: Railway).
Each service’s `railway.toml` sets watch paths so only matching folder changes trigger a redeploy (`/backend/**` vs `/frontend/**`).

### 1. API service

- Root directory: `backend`
- Builder: Dockerfile (`backend/Dockerfile`)
- Watch paths: `/backend/**`
- Health check: `GET /health`
- Attach a volume mounted at `/app/Data` so SQLite survives redeploys

Required variables:

| Variable | Example |
| --- | --- |
| `Jwt__Secret` | long random string (≥32 chars) |
| `Gemini__ApiKey` (and/or Groq / OpenRouter) | your provider key |
| `Cors__Origins` | `https://<frontend-domain>` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | `Data Source=Data/roombooking.db;Cache=Shared;Default Timeout=30` |

Optional: `Gemini__Model`, `Groq__ApiKey`, `Groq__Model`, `OpenRouter__ApiKey`, `OpenRouter__Model`.

### 2. Frontend service

- Root directory: `frontend`
- Builder: Dockerfile (`frontend/Dockerfile`)
- Watch paths: `/frontend/**`
- Set `VITE_API_URL=https://<api-domain>` as a **build-time** variable (Vite inlines it)

### 3. Wire the public URLs

1. Create both services and generate public domains.
2. Set API `Cors__Origins` to the exact frontend origin (HTTPS, no trailing slash).
3. Set frontend `VITE_API_URL` to the exact API origin, then **redeploy the frontend** so the build picks it up.
4. Smoke test: open the SPA → login as `User1` → confirm `/health` on the API → create a booking → refresh and confirm it persists.

Local Docker smoke (optional, Docker Desktop running):

```powershell
cd backend
docker build -t cubo-itati-api .
docker run --rm -p 8080:8080 -e PORT=8080 -e Jwt__Secret=dev-secret-at-least-32-characters!! -e Gemini__ApiKey=unused cubo-itati-api

cd ..\frontend
docker build --build-arg VITE_API_URL=http://localhost:8080 -t cubo-itati-web .
docker run --rm -p 3000:3000 -e PORT=3000 cubo-itati-web
```

