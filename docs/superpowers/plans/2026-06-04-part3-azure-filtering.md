# Part 3 — Filtering + Azure Migration + Report Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add EventType classification + advanced filters to the Events search, migrate the app from local (SQL + Azurite) to live Azure (SQL DB, Storage, App Service), and produce the reflective report.

**Architecture:** Part A is pure local code (new lookup entity, FK on Event, extended controller query + view, one EF migration). Part B swaps local connection strings for Azure-hosted ones supplied at runtime (App Service settings), with provisioning driven by `az` CLI scripts the user runs. Part C is documentation.

**Tech Stack:** ASP.NET Core MVC (.NET 10), EF Core 10 / SQL Server, Azure.Storage.Blobs, Azure CLI, Bootstrap views.

**Verification approach:** No test project exists in this repo. Each code task is verified by `dotnet build` (compile) plus a runtime check in the running app. Commit after each task.

---

## PART A — Advanced Filtering

### Task 1: Create the EventType lookup entity

**Files:**
- Create: `Models/EventType.cs`

- [ ] **Step 1: Create the model**

```csharp
using System.ComponentModel.DataAnnotations;

namespace EventEaseLocal.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(60)]
        [Display(Name = "Event Type")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
```

- [ ] **Step 2: Build to verify it compiles**

Run: `dotnet build`
Expected: Build succeeded (the new class is unused so far but must compile).

- [ ] **Step 3: Commit**

```bash
git add Models/EventType.cs
git commit -m "feat: add EventType lookup entity"
```

---

### Task 2: Add EventType FK to Event

**Files:**
- Modify: `Models/Event.cs`

- [ ] **Step 1: Add the FK + nav property**

In `Models/Event.cs`, after the `Venue` nav property (after line 24, before the `Bookings` collection), add:

```csharp
        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [ForeignKey("EventTypeId")]
        public EventType? EventType { get; set; }
```

(`int?` nullable keeps the column optional so the migration applies cleanly to existing rows.)

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Models/Event.cs
git commit -m "feat: add EventTypeId FK to Event"
```

---

### Task 3: Register EventType in DbContext + seed categories and assignments

**Files:**
- Modify: `Models/ApplicationDbContext.cs`

- [ ] **Step 1: Add the DbSet**

After line 14 (`public DbSet<Booking> Bookings { get; set; }`), add:

```csharp
        public DbSet<EventType> EventTypes { get; set; }
```

- [ ] **Step 2: Add the FK relationship**

Inside `OnModelCreating`, after the `Event` entity config block (the one configuring `e.Venue`, ending at line 38), add a relationship for EventType:

```csharp
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasOne(e => e.EventType)
                      .WithMany(t => t.Events)
                      .HasForeignKey(e => e.EventTypeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
```

- [ ] **Step 3: Seed the predefined categories**

Immediately before the existing `modelBuilder.Entity<Venue>().HasData(` call (line 53), add:

```csharp
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, Name = "Conference" },
                new EventType { EventTypeId = 2, Name = "Wedding" },
                new EventType { EventTypeId = 3, Name = "Corporate" },
                new EventType { EventTypeId = 4, Name = "Concert / Festival" },
                new EventType { EventTypeId = 5, Name = "Birthday / Private" },
                new EventType { EventTypeId = 6, Name = "Charity" },
                new EventType { EventTypeId = 7, Name = "Workshop" },
                new EventType { EventTypeId = 8, Name = "Other" }
            );
```

- [ ] **Step 4: Assign EventTypeId to existing seed events**

Replace the entire `modelBuilder.Entity<Event>().HasData(...)` block with this version (adds `EventTypeId` to each row):

```csharp
            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Annual Tech Conference", Description = "A premier technology conference featuring industry leaders.", VenueId = 1, EventTypeId = 1 },
                new Event { EventId = 2, EventName = "Wedding Reception - Mokoena", Description = "Private wedding reception for the Mokoena family.", VenueId = 2, EventTypeId = 2 },
                new Event { EventId = 3, EventName = "Corporate Year-End Gala", Description = "Formal dinner and awards ceremony for Zephyr Corp employees.", VenueId = 6, EventTypeId = 3 },
                new Event { EventId = 4, EventName = "Charity Fun Run Launch", Description = "Kickoff event for the annual Sunshine Charity 10K.", VenueId = 3, EventTypeId = 6 },
                new Event { EventId = 5, EventName = "Product Launch - Nova Phone", Description = "Exclusive launch event for the new Nova smartphone line.", VenueId = 7, EventTypeId = 3 },
                new Event { EventId = 6, EventName = "Birthday Celebration - Naidoo", Description = "50th birthday celebration for the Naidoo family.", VenueId = 4, EventTypeId = 5 },
                new Event { EventId = 7, EventName = "Team Building Retreat", Description = "Two-day team building workshop for Apex Solutions.", VenueId = 5, EventTypeId = 7 },
                new Event { EventId = 8, EventName = "Music Festival Day Pass", Description = "Live music performances across multiple stages.", VenueId = 1, EventTypeId = 4 }
            );
```

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add Models/ApplicationDbContext.cs
git commit -m "feat: register EventType DbSet, seed categories, assign to events"
```

---

### Task 4: Generate and apply the EF migration

**Files:**
- Create: `Migrations/*_AddEventType.cs` (generated)

- [ ] **Step 1: Ensure dotnet-ef is available**

Run: `dotnet tool install --global dotnet-ef` (skip if already installed; harmless if it reports already installed)
Expected: success or "already installed".

- [ ] **Step 2: Add the migration**

Run: `dotnet ef migrations add AddEventType`
Expected: Creates `Migrations/<timestamp>_AddEventType.cs` adding the `EventTypes` table, the `EventTypeId` column on `Events`, the FK, the EventType seed rows, and `UpdateData` for the 8 events.

- [ ] **Step 3: Inspect the generated migration**

Run: `git status` and open the new migration file.
Expected: `CreateTable(name: "EventTypes", ...)`, `AddColumn<int>(name: "EventTypeId", table: "Events", nullable: true)`, an index + FK on `EventTypeId`, and `InsertData` for EventTypes.

- [ ] **Step 4: Apply against local DB (if local SQL is running)**

Run: `dotnet ef database update`
Expected: "Done." (If local SQL isn't up, this is fine — `Program.cs` runs `Migrate()` on startup; skip and verify in Task 7's runtime check.)

- [ ] **Step 5: Commit**

```bash
git add Migrations/
git commit -m "feat: add EF migration AddEventType"
```

---

### Task 5: Extend EventsController.Index with the new filters

**Files:**
- Modify: `Controllers/EventsController.cs:15-23` (the `Index` action)

- [ ] **Step 1: Replace the Index action**

Replace the existing `Index` method (lines 15-23) with:

```csharp
        public async Task<IActionResult> Index(string? searchString, int? venueId, int? eventTypeId,
            DateTime? dateFrom, DateTime? dateTo, bool availableOnly = false)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                events = events.Where(e => e.EventName.Contains(searchString)
                    || (e.Description != null && e.Description.Contains(searchString)));

            if (venueId.HasValue)
                events = events.Where(e => e.VenueId == venueId.Value);

            if (eventTypeId.HasValue)
                events = events.Where(e => e.EventTypeId == eventTypeId.Value);

            if (availableOnly && (dateFrom.HasValue || dateTo.HasValue))
            {
                // Venues with any booking in the chosen range are "busy" -> exclude their events.
                var busyVenueIds = _context.Bookings
                    .Where(b => (!dateFrom.HasValue || b.EventDate >= dateFrom.Value)
                             && (!dateTo.HasValue || b.EventDate <= dateTo.Value))
                    .Select(b => b.VenueId);
                events = events.Where(e => !busyVenueIds.Contains(e.VenueId));
            }
            else if (dateFrom.HasValue || dateTo.HasValue)
            {
                // Events that have a booking within the range.
                events = events.Where(e => e.Bookings.Any(b =>
                    (!dateFrom.HasValue || b.EventDate >= dateFrom.Value) &&
                    (!dateTo.HasValue || b.EventDate <= dateTo.Value)));
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["VenueFilter"] = venueId;
            ViewData["EventTypeFilter"] = eventTypeId;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");
            ViewData["AvailableOnly"] = availableOnly;
            ViewData["Venues"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", venueId);
            ViewData["EventTypes"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", eventTypeId);
            return View(await events.ToListAsync());
        }
```

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add Controllers/EventsController.cs
git commit -m "feat: add event type, date range and availability filters to Events.Index"
```

---

### Task 6: Update the Events Index view with the filter UI + type badge

**Files:**
- Modify: `Views/Events/Index.cshtml`

- [ ] **Step 1: Replace the filter form**

Replace the existing `<form asp-action="Index" ...>...</form>` block with:

```html
<form asp-action="Index" method="get" class="mb-4"><div class="row g-2 align-items-end">
<div class="col-12 col-md-3"><label class="form-label small text-muted mb-1">Search</label><input type="text" name="searchString" value="@ViewData["CurrentFilter"]" class="form-control" placeholder="Search events..." /></div>
<div class="col-6 col-md-2"><label class="form-label small text-muted mb-1">Venue</label><select name="venueId" class="form-select" asp-items="@(ViewData["Venues"] as SelectList)"><option value="">All Venues</option></select></div>
<div class="col-6 col-md-2"><label class="form-label small text-muted mb-1">Event Type</label><select name="eventTypeId" class="form-select" asp-items="@(ViewData["EventTypes"] as SelectList)"><option value="">All Types</option></select></div>
<div class="col-6 col-md-2"><label class="form-label small text-muted mb-1">Date From</label><input type="date" name="dateFrom" value="@ViewData["DateFrom"]" class="form-control" /></div>
<div class="col-6 col-md-2"><label class="form-label small text-muted mb-1">Date To</label><input type="date" name="dateTo" value="@ViewData["DateTo"]" class="form-control" /></div>
<div class="col-12 col-md-1"><button type="submit" class="btn btn-outline-secondary w-100"><i class="bi bi-search"></i></button></div>
<div class="col-12"><div class="form-check"><input class="form-check-input" type="checkbox" name="availableOnly" value="true" id="availableOnly" @(((bool?)ViewData["AvailableOnly"] ?? false) ? "checked" : "")><label class="form-check-label small" for="availableOnly">Only show events at venues that are <strong>free</strong> in the selected date range</label></div></div>
<div class="col-12"><a asp-action="Index" class="small text-muted">Clear filters</a></div>
</div></form>
```

- [ ] **Step 2: Add an Event Type column/badge to the table**

In the `<thead>` row, add a header after `<th>Venue</th>`:

```html
<th>Type</th>
```

In the `@foreach` row, add a cell after the venue badge cell (`<td><span class="badge bg-primary">@evt.Venue?.VenueName</span></td>`):

```html
<td>@if (evt.EventType != null) { <span class="badge bg-info text-dark">@evt.EventType.Name</span> } else { <span class="text-muted small">—</span> }</td>
```

- [ ] **Step 3: Build and run**

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add Views/Events/Index.cshtml
git commit -m "feat: add filter controls and event type badge to Events index"
```

---

### Task 7: Add EventType dropdown to Create/Edit + runtime verification

**Files:**
- Modify: `Controllers/EventsController.cs` (Create GET/POST, Edit GET/POST — `[Bind]` + ViewData)
- Modify: `Views/Events/Create.cshtml`, `Views/Events/Edit.cshtml`

- [ ] **Step 1: Add EventTypes SelectList to the 4 controller actions**

In `EventsController.cs`, in `Create()` (GET), `Create(...)` (POST, the re-display path), `Edit(...)` (GET), and `Edit(...)` (POST, the re-display path), add this line wherever `ViewData["VenueId"] = new SelectList(...)` appears:

```csharp
            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", evt.EventTypeId);
```

For the GET `Create()` (which has no `evt` variable), use:

```csharp
            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name");
```

- [ ] **Step 2: Add EventTypeId to the [Bind] lists**

Change `[Bind("EventName,Description,VenueId")]` (Create POST) to `[Bind("EventName,Description,VenueId,EventTypeId")]`.
Change `[Bind("EventId,EventName,Description,VenueId")]` (Edit POST) to `[Bind("EventId,EventName,Description,VenueId,EventTypeId")]`.

- [ ] **Step 3: Add the dropdown to Create.cshtml and Edit.cshtml**

In both `Views/Events/Create.cshtml` and `Views/Events/Edit.cshtml`, after the Venue `<select>` form group, add:

```html
<div class="mb-3">
    <label asp-for="EventTypeId" class="form-label"></label>
    <select asp-for="EventTypeId" class="form-select" asp-items="@(ViewData["EventTypeId"] as SelectList)">
        <option value="">— Select a type —</option>
    </select>
    <span asp-validation-for="EventTypeId" class="text-danger"></span>
</div>
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: Build succeeded.

- [ ] **Step 5: Runtime verification (all of Part A)**

Run: `dotnet run` (ensure local SQL is reachable; `Migrate()` applies the new migration + seed on startup). Log in, then on `/Events`:
- Each event row shows a Type badge.
- "Event Type" dropdown filters to that category.
- Date From/To narrows to events with a booking in range.
- "Available venues only" + a date range hides events whose venue is booked in that range.
- Filters combine and the form repopulates after submit.
- Create/Edit show the Event Type dropdown and persist the choice.

Expected: all behaviors correct.

- [ ] **Step 6: Commit**

```bash
git add Controllers/EventsController.cs Views/Events/Create.cshtml Views/Events/Edit.cshtml
git commit -m "feat: select event type when creating/editing events"
```

---

## PART B — Azure "Go Live" Migration

> These tasks are run by the **user** in their terminal/Azure account. The agent's job is to create the scripts and config, then guide. Replace `<UNIQUE>` with a globally-unique suffix (e.g. initials+number) and choose a strong SQL password.

### Task 8: Rename the blob service for clarity (Azurite -> Azure)

**Files:**
- Rename: `Services/AzuriteBlobStorageService.cs` -> `Services/AzureBlobStorageService.cs`
- Modify: `Program.cs:10`

- [ ] **Step 1: Rename file + class**

```bash
git mv Services/AzuriteBlobStorageService.cs Services/AzureBlobStorageService.cs
```
In the file, rename the class `AzuriteBlobStorageService` -> `AzureBlobStorageService` (and its constructor).

- [ ] **Step 2: Update DI registration**

In `Program.cs` line 10, change:
```csharp
builder.Services.AddSingleton<IBlobStorageService, AzuriteBlobStorageService>();
```
to:
```csharp
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
```

- [ ] **Step 3: Build + commit**

Run: `dotnet build` (Expected: succeeded)
```bash
git add -A
git commit -m "refactor: rename blob service to AzureBlobStorageService"
```

---

### Task 9: Verify Azure access

- [ ] **Step 1: Install Azure CLI**

Run: `brew install azure-cli`
Expected: `az` becomes available — verify with `az version`.

- [ ] **Step 2: Log in**

Run: `az login`
Expected: browser opens; on success the subscription(s) print.

- [ ] **Step 3: Confirm an active subscription**

Run: `az account show --output table`
Expected: shows a subscription (e.g. "Azure for Students"). If none, the user must activate a free/student subscription before continuing. Note the subscription is active and has credit.

---

### Task 10: Provision Azure resources

**Files:**
- Create: `poe/part3/azure-provision.sh`

- [ ] **Step 1: Write the provisioning script**

```bash
#!/usr/bin/env bash
set -euo pipefail

# ---- EDIT THESE ----
SUFFIX="<UNIQUE>"                       # e.g. lm2026
LOCATION="southafricanorth"
SQL_ADMIN="eventeaseadmin"
SQL_PASSWORD="<STRONG_PASSWORD>"        # >=12 chars, mixed case+digit+symbol
# --------------------

RG="rg-eventease"
SQL_SERVER="sql-eventease-$SUFFIX"
SQL_DB="EventEaseDb"
STORAGE="steventease$SUFFIX"            # lowercase, <=24 chars, no dashes
PLAN="plan-eventease"
WEBAPP="app-eventease-$SUFFIX"

echo "==> Resource group"
az group create --name "$RG" --location "$LOCATION"

echo "==> SQL server + database"
az sql server create --name "$SQL_SERVER" --resource-group "$RG" --location "$LOCATION" \
  --admin-user "$SQL_ADMIN" --admin-password "$SQL_PASSWORD"
az sql db create --resource-group "$RG" --server "$SQL_SERVER" --name "$SQL_DB" \
  --service-objective Basic
# Allow Azure services + this machine's IP
az sql server firewall-rule create --resource-group "$RG" --server "$SQL_SERVER" \
  --name AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
MYIP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create --resource-group "$RG" --server "$SQL_SERVER" \
  --name MyMachine --start-ip-address "$MYIP" --end-ip-address "$MYIP"

echo "==> Storage account + container"
az storage account create --name "$STORAGE" --resource-group "$RG" \
  --location "$LOCATION" --sku Standard_LRS --allow-blob-public-access true
STORAGE_CONN=$(az storage account show-connection-string --name "$STORAGE" \
  --resource-group "$RG" --query connectionString -o tsv)
az storage container create --name venue-images --connection-string "$STORAGE_CONN" \
  --public-access blob

echo "==> App Service plan + web app"
az appservice plan create --name "$PLAN" --resource-group "$RG" --sku B1 --is-linux
az webapp create --resource-group "$RG" --plan "$PLAN" --name "$WEBAPP" \
  --runtime "DOTNETCORE:10.0"

echo "==> App settings (live connection strings)"
SQL_CONN="Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=$SQL_DB;User ID=$SQL_ADMIN;Password=$SQL_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
az webapp config connection-string set --resource-group "$RG" --name "$WEBAPP" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$SQL_CONN"
az webapp config appsettings set --resource-group "$RG" --name "$WEBAPP" \
  --settings ConnectionStrings__AzureStorage="$STORAGE_CONN"

echo "==> DONE. Web app: https://$WEBAPP.azurewebsites.net"
```

- [ ] **Step 2: Run it**

Run: `bash poe/part3/azure-provision.sh` (after editing the EDIT THESE block)
Expected: each resource creates successfully; final line prints the web app URL.

- [ ] **Step 3: Screenshot proof**

In the Azure Portal, screenshot the `rg-eventease` resource group showing the SQL DB, storage account, App Service plan, and web app. Save to `poe/part3/`.

- [ ] **Step 4: Commit the script**

```bash
git add poe/part3/azure-provision.sh
git commit -m "chore: add Azure provisioning script"
```

> Note: `DefaultConnection` is set as an Azure SQL connection string and `AzureStorage` via the `ConnectionStrings__AzureStorage` app-setting key, so `GetConnectionString("AzureStorage")` resolves at runtime. No secrets are committed — they live only in App Service.

---

### Task 11: Deploy the application

- [ ] **Step 1: Publish**

Run: `dotnet publish -c Release -o ./publish`
Expected: builds to `./publish`.

- [ ] **Step 2: Zip + deploy**

```bash
cd publish && zip -r ../app.zip . && cd ..
az webapp deploy --resource-group rg-eventease --name app-eventease-<UNIQUE> \
  --src-path app.zip --type zip
```
Expected: deployment succeeds.

- [ ] **Step 3: Verify live**

Open `https://app-eventease-<UNIQUE>.azurewebsites.net`. On first boot, `db.Database.Migrate()` creates the schema, the `vw_BookingDetails` view, and seeds (incl. EventTypes). Log in; confirm venues/events load from Azure SQL and venue images upload to Azure Storage. Screenshot the running site (URL visible) to `poe/part3/`.

Expected: app works against live DB + storage.

---

### Task 12: Tear down + proof

- [ ] **Step 1: Delete all resources**

```bash
az group delete --name rg-eventease --yes --no-wait
```
Expected: deletion starts.

- [ ] **Step 2: Confirm + screenshot**

Run: `az group exists --name rg-eventease` (expect `false` once complete) and screenshot the Portal showing the resource group is gone. Save to `poe/part3/`.

---

## PART C — Documentation

### Task 13: Un-ignore poe/part3 deliverables

**Files:**
- Modify: `.gitignore`

- [ ] **Step 1: Add an exception**

Change the `poe/` line in `.gitignore` to:
```
poe/
!poe/part3/
!poe/part3/**
```

- [ ] **Step 2: Commit**

```bash
git add .gitignore
git commit -m "chore: include poe/part3 deliverables in repo"
```

---

### Task 14: Write the reflective report

**Files:**
- Create: `poe/part3/REPORT.md`

- [ ] **Step 1: Write the report**

Full draft with these sections (agent drafts complete prose; user fills URL/YouTube placeholders and personalizes the reflection):
1. **Application feature list** — auth, venues (CRUD + image upload to blob), events (CRUD + classification + advanced filters), bookings (CRUD + `vw_BookingDetails`), seed data.
2. **Component discussion — Azure services & why:** Azure SQL Database (managed relational store, replaces local SQL), Azure Storage / Blob (venue images, replaces Azurite), Azure App Service (managed PaaS hosting for the .NET app). Why each.
3. **Migration experience:** LocalDB/Azurite → Azure SQL/Storage; configuration changes required (connection strings moved to App Service settings, `UseDevelopmentStorage=true` → real account, firewall rules); why environment separation matters (security, parity, no secrets in source).
4. **Technologies used & why:** ASP.NET Core MVC, EF Core + migrations, Bootstrap, cookie auth, Azure SDK.
5. **Theory questions:** (a) how Cosmos DB differs from traditional relational DBs (global distribution, schema-free, horizontal partitioning, multi-model, tunable consistency); (b) key considerations for Logic Apps handling sensitive data (managed identities/Key Vault, encryption in transit/at rest, secure parameters, IP restrictions, least privilege, audit logging); (c) how Event Grid + other services build robust event-driven workflows (decoupling, fan-out, retries/dead-lettering, reacting to Storage/Resource events).
6. **References & code attribution** (traditional referencing) + **submission links** (web app URL, GitHub repo, YouTube) as labeled placeholders.

- [ ] **Step 2: Commit**

```bash
git add poe/part3/REPORT.md
git commit -m "docs: add Part 3 reflective technical report"
```

---

## Self-Review (completed)

- **Spec coverage:** A — EventType entity (T1-T3), migration (T4), filters incl. availability (T5-T6), create/edit (T7). B — service rename (T8), access check (T9), provision (T10), deploy (T11), teardown (T12). C — gitignore (T13), report incl. theory (T14). All spec sections mapped.
- **Placeholder scan:** Only intentional user-supplied values (`<UNIQUE>`, `<STRONG_PASSWORD>`, submission URLs) remain — these are runtime secrets/inputs, not plan gaps.
- **Type consistency:** `EventTypeId`/`EventType.Name`/`EventTypes` DbSet used consistently across model, context, controller, views; `AzureBlobStorageService` name consistent in file + DI; `ConnectionStrings__AzureStorage` key matches `GetConnectionString("AzureStorage")`.
