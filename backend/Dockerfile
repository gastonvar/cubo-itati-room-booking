# Build context: backend/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RoomBooking.Api/RoomBooking.Api.csproj RoomBooking.Api/
RUN dotnet restore RoomBooking.Api/RoomBooking.Api.csproj

COPY RoomBooking.Api/ RoomBooking.Api/
RUN dotnet publish RoomBooking.Api/RoomBooking.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/Data

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__Default="Data Source=Data/roombooking.db;Cache=Shared;Default Timeout=30"

EXPOSE 8080

# Railway injects PORT at runtime; bind all interfaces.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet RoomBooking.Api.dll"]
