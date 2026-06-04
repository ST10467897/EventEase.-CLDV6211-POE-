# Part 3 — Advanced Filtering, Reporting & Azure "Go Live" Migration

**Date:** 2026-06-04
**Project:** EventEaseLocal (ASP.NET Core MVC, .NET 10, EF Core / SQL Server)
**POE Part 3 — 100 marks**

## Context

EventEaseLocal is a venue-booking app with three entities (`Venue`, `Event`, `Booking`),
a `vw_BookingDetails` SQL view, cookie auth, and venue image upload. Everything currently
targets **local** infrastructure:

- **Database:** `Server=localhost,1433` local SQL; migrations auto-run on startup via
  `db.Database.Migrate()` in `Program.cs`.
- **Storage:** `UseDevelopmentStorage=true` → **Azurite** emulator. The implementing
  service is `Services/AzuriteBlobStorageService.cs`.
- **Search (Part 2):** `EventsController.Index` already supports `searchString` + `venueId`.

Part 3 has three deliverables: (A) advanced filtering + event-type classification,
(B) migrate all local resources to live Azure, (C) a reflective technical report.

The user had previously created Azure resources and deleted them "to save space," so
Part B starts from a clean provisioning.

## Decisions (locked with user)

- **Venue-availability filter:** "free venues in date range" — exclude events whose venue
  has any conflicting booking in the chosen range.
- **Filter location:** extend the existing Events search page (matches the brief's
  "Extend the search from Part 2").
- **Provisioning style:** `az` CLI scripts the user runs after logging in.
- **Build order:** A (filtering) first, then B (Azure), then C (report).
- **Report:** full draft for the user to personalize/verify.

---

## A. Advanced Filtering

### Data model

New lookup entity:

```csharp
public class EventType
{
    public int EventTypeId { get; set; }
    public string Name { get; set; } = string.Empty;   // [Required], [StringLength(60)]
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
```

`Event` gains:

```csharp
public int? EventTypeId { get; set; }      // nullable so existing rows stay valid
public EventType? EventType { get; set; }  // nav, FK on EventTypeId
```

`ApplicationDbContext`:
- `public DbSet<EventType> EventTypes { get; set; }`
- FK config: `Event` → `EventType`, `OnDelete(DeleteBehavior.SetNull)` (deleting a type
  doesn't delete events).
- Seed predefined categories (IDs 1–8):
  `Conference`, `Wedding`, `Corporate`, `Concert / Festival`, `Birthday / Private`,
  `Charity`, `Workshop`, `Other`.
- Assign `EventTypeId` to the 8 existing seed events:
  1 Tech Conference→Conference(1), 2 Wedding→Wedding(2), 3 Corporate Gala→Corporate(3),
  4 Charity Fun Run→Charity(6), 5 Product Launch→Corporate(3), 6 Birthday→Birthday/Private(5),
  7 Team Building→Workshop(7), 8 Music Festival→Concert/Festival(4).

New EF migration: `AddEventType`.

### Controller — `EventsController.Index`

New signature:

```csharp
public async Task<IActionResult> Index(
    string? searchString, int? venueId, int? eventTypeId,
    DateTime? dateFrom, DateTime? dateTo, bool availableOnly = false)
```

Query composition (all filters AND together, applied to `IQueryable`):

1. `searchString` — existing name/description contains (Part 2).
2. `venueId` — existing `e.VenueId == venueId` (Part 2).
3. `eventTypeId` — `e.EventTypeId == eventTypeId`.
4. **Date range / availability** (operate via the `Booking` relationship since dates live on `Booking`):
   - If `availableOnly` **and** a date range is given → exclude events whose venue has any
     booking in range:
     ```csharp
     var busyVenueIds = _context.Bookings
         .Where(b => (!dateFrom.HasValue || b.EventDate >= dateFrom)
                  && (!dateTo.HasValue   || b.EventDate <= dateTo))
         .Select(b => b.VenueId);
     events = events.Where(e => !busyVenueIds.Contains(e.VenueId));
     ```
   - Else if a date range is given (without availability) → events that *have* a booking in range:
     ```csharp
     events = events.Where(e => e.Bookings.Any(b =>
         (!dateFrom.HasValue || b.EventDate >= dateFrom) &&
         (!dateTo.HasValue   || b.EventDate <= dateTo)));
     ```

`Include(e => e.EventType)` added. All filter values returned to the view via `ViewData`
so the form repopulates after submit. `EventTypes` SelectList provided for the dropdown.

### View — `Views/Events/Index.cshtml`

Extend the existing filter `<form method="get">` with: Event Type `<select>`, Date From /
Date To `<input type="date">`, and an "Available venues only" checkbox. Add an EventType
badge to each event row. Add a "Clear" link that resets to the unfiltered Index.

### Create / Edit forms

`Events/Create.cshtml` and `Events/Edit.cshtml` + their controller actions gain an
EventType dropdown (`SelectList` from `EventTypes`). `[Bind(...)]` lists updated to include
`EventTypeId`.

### Tests / verification

- App builds (`dotnet build`) and runs.
- Migration applies cleanly against a fresh DB (seed populates types + assigns events).
- Manual: each filter alone and in combination returns expected rows; form repopulates;
  "available only" excludes booked venues for the chosen range.

---

## B. "Go Live" Azure Migration

Executed by the user via `az` CLI scripts that this project will provide
(`docs/azure/` or `poe/part3/`). All resources live in one resource group
(`rg-eventease`) for one-command teardown.

1. **Verify access:** `brew install azure-cli` → `az login` → `az account show`
   (confirm Student/Free subscription).
2. **Provision:**
   - Azure SQL Server + Database (Basic / serverless); firewall: client IP +
     "Allow Azure services."
   - Storage Account + `venue-images` blob container (public blob access).
   - App Service plan (F1/B1) + Web App (.NET 10).
3. **Config swap (live, not local):** `DefaultConnection` and `AzureStorage` set as
   **App Service connection strings / settings** — never committed to `appsettings.json`.
   Rename `AzuriteBlobStorageService` → `AzureBlobStorageService` (same Azure SDK; works
   against both emulator and live) and update the DI registration in `Program.cs`.
4. **Data migration:** schema, the `vw_BookingDetails` view, and seed data auto-apply on
   first boot through the existing `db.Database.Migrate()`. BACPAC/SqlPackage noted as the
   alternative for migrating real runtime data.
5. **Deploy:** `dotnet publish -c Release` → `az webapp deploy` (zip).
6. **Proof + teardown:** capture screenshots of provisioned resources and the live URL,
   then `az group delete --name rg-eventease` and screenshot the emptied/ deleted group.

---

## C. Reflective Technical Report — `poe/part3/REPORT.md`

Full draft covering:
- Full feature list of the application.
- **Component discussion:** which Azure services were used and why (SQL Database, Storage
  Account/Blob, App Service).
- **Migration experience:** LocalDB/Azurite → Azure SQL/Storage; what configuration changes
  were required; why separation of environments matters professionally.
- Technologies used and rationale.
- **Theory questions:** (1) how Cosmos DB differs from traditional databases; (2) key
  considerations when designing Logic Apps that handle sensitive data; (3) how combining
  Event Grid with other services creates robust workflows.
- Code attribution / referencing, web-app URL, GitHub repo URL, screenshots, YouTube link
  placeholders.

### Repo housekeeping

`.gitignore` currently ignores all of `poe/`. Add an exception so `poe/part3/`
deliverables (report + screenshots) are committed and reach the GitHub repo, as the brief
requires.

---

## Out of scope (YAGNI)

- No Cosmos DB, Logic Apps, or Event Grid implementation (theory discussion only).
- No CI/CD pipeline; manual `az webapp deploy` is sufficient for the POE.
- No automated cloud teardown scheduling; manual `az group delete` with screenshot proof.
