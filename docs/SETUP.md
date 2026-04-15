# EventEase — Local Setup

## Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB, Express, or Docker)

## Database

The app auto-runs EF Core migrations on startup. Configure your connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=EventEaseDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

## Admin Login

Set credentials for the booking specialist login:

```bash
dotnet user-secrets set "AdminCredentials:Username" "admin"
dotnet user-secrets set "AdminCredentials:Password" "your_password"
```

## Run

```bash
dotnet run
```

Open https://localhost:5001 (or the port shown in console output).
