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

