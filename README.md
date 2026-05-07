# EventEase — Venue Booking Platform

EventEase is an ASP.NET Core MVC web application for managing venue bookings. Built for booking specialists, it provides a central platform to browse venues, schedule events, and manage bookings while preventing double-bookings and enforcing referential integrity.

## Features

- **Venue Management** — Create, edit, and delete venues with image uploads (stored in Azurite local blob storage), location, and capacity details
- **Event Management** — Create and manage events linked to specific venues with descriptions
- **Booking Management** — Schedule bookings with date, start time, and end time; double-booking prevention ensures no venue has overlapping bookings
- **Consolidated Bookings View** — `vw_BookingDetails` SQL view joins booking, event, and venue data for the bookings list/search page
- **Search and Filtering** — Search bookings by Booking ID or Event Name; search events by name or description; filter by venue
- **Dashboard** — Overview of total venues, events, and bookings at a glance
- **Authentication** — Cookie-based login for authorised booking specialists
- **Data Integrity** — Venues and events with existing bookings cannot be deleted (Restrict delete behaviour)

## Tech Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **ORM**: Entity Framework Core 10
- **Database**: SQL Server (LocalDB / Express / Docker)
- **Frontend**: Bootstrap 5, jQuery, jQuery Validation
- **Authentication**: Cookie-based (credentials stored in User Secrets)

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or Docker)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) — Azure Storage Emulator (used for local blob storage of venue images)
- [Azure Storage Explorer](https://azure.microsoft.com/features/storage-explorer/) (optional, for verifying uploaded blobs)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/EventEaseLocal.git
cd EventEaseLocal
```

### 2. Configure the Database Connection

Set the connection string using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=EventEaseDb;Trusted_Connection=True;TrustServerCertificate=True"
```

For SQL Server Express or Docker, adjust the connection string accordingly:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=EventEaseDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

### 3. Configure Admin Credentials

Set the login credentials for the booking specialist portal:

```bash
dotnet user-secrets set "AdminCredentials:Username" "admin"
dotnet user-secrets set "AdminCredentials:Password" "your_password"
```

### 4. Start Azurite (Local Blob Storage Emulator)

Venue image uploads are stored in a local `venue-images` container served by Azurite. Start it before running the app:

```bash
# install once (requires Node.js)
npm install -g azurite

# run the emulator (default ports: blob 10000, queue 10001, table 10002)
azurite --silent --location ./azurite-data --debug ./azurite-data/debug.log
```

The application reads the connection string `UseDevelopmentStorage=true` from `appsettings.json` and creates the `venue-images` container automatically on first upload. Verify uploads in **Azure Storage Explorer** under *Local & Attached → Storage Accounts → (Emulator – Default Ports) → Blob Containers → venue-images*.

### 5. Run the Application

```bash
dotnet run
```

Open the URL shown in the console output (e.g. `https://localhost:5001`).

The database is created and seeded automatically on first run — no manual migration commands needed. The `vw_BookingDetails` SQL view (used by the bookings list/search) is created by the `AddBookingDetailsView` migration.

## Project Structure

```
EventEaseLocal/
├── Controllers/
│   ├── AccountController.cs      # Login / logout
│   ├── HomeController.cs         # Dashboard
│   ├── VenuesController.cs       # Venue CRUD
│   ├── EventsController.cs       # Event CRUD
│   └── BookingsController.cs     # Booking CRUD
├── Models/
│   ├── Venue.cs
│   ├── Event.cs
│   ├── Booking.cs
│   └── ApplicationDbContext.cs   # EF Core context, relationships, seed data
├── Views/
│   ├── Home/                     # Dashboard
│   ├── Venues/                   # Venue views (Index, Create, Edit, Details, Delete)
│   ├── Events/                   # Event views
│   ├── Bookings/                 # Booking views
│   ├── Account/                  # Login page
│   └── Shared/                   # Layout, validation partials
├── wwwroot/                      # Static assets (CSS, JS, images)
├── Migrations/                   # EF Core migrations
└── docs/
    └── ERD/                      # Entity Relationship Diagram
```

## Database Design

The application uses three core entities:

| Entity  | Description |
|---------|-------------|
| Venue   | A bookable location with name, address, capacity, and optional image |
| Event   | A named event linked to a specific venue |
| Booking | A scheduled booking linking an event to a venue with date and time |

All foreign key relationships use **Restrict** delete behaviour — venues and events cannot be removed while they have associated bookings.

Double-booking prevention: a booking cannot overlap with another booking at the same venue on the same date.

See [docs/ERD/EventEase-ERD.md](docs/ERD/EventEase-ERD.md) for the full ERD with diagram, field definitions, and business rules.

## Seed Data

The database ships with sample data for immediate testing:

- **7 Venues** across South Africa (Johannesburg, Sandton, Pretoria, Durban, Drakensberg, Stellenbosch)
- **8 Events** including conferences, weddings, corporate galas, product launches, and celebrations
- **8 Bookings** spanning April to October 2026
