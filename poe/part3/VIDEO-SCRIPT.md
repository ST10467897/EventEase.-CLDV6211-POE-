# EventEase — Part 3 Video Walkthrough Script

A detailed, rubric-aligned walkthrough (~15–18 min). `[SHOW: …]` = what to have on
screen. *Italics* = narration you can paraphrase in your own words — don't read it
robotically. Record **while the Azure resources are still live**, then do the teardown
on camera at the end.

> Tip: Have these tabs/windows open before you hit record:
> 1. The live site — https://app-eventease-lmck0604.azurewebsites.net
> 2. A terminal in the project folder, already signed in with `az login` (this whole
>    walkthrough uses the Azure CLI — no portal)
> 3. VS Code / your editor on the project
> 4. Your GitHub repo page
>
> Before recording, export the values the CLI commands use so nothing secret is typed on
> camera: `export EE_SUFFIX=lmck0604` and `export EE_SQL_PASSWORD='<your-sql-password>'`.

---

## 1. Introduction  (~0:00–1:00)
*[SHOW: your face cam or the project README / solution in the editor]*

*"Hi, my name is **[your name]**, student number **[number]**. This is my Part 3 walkthrough
for EventEase, a venue-booking web application. It's built with ASP.NET Core MVC on .NET 10,
using C#, Entity Framework Core with code-first migrations, a SQL Server database, the Azure
Blob Storage SDK for images, cookie-based authentication, and Bootstrap for the UI. In Part 3
I added advanced filtering with event-type classification, and I migrated the whole
application from running locally to running live on Microsoft Azure. I'll demo the features,
show it running in the cloud, explain the migration, then drop all the resources as proof."*

---

## 2. Login & app overview  (~1:00–2:00)
*[SHOW: the live Azure URL in the browser address bar — emphasise it's the azurewebsites.net URL]*

*"First, notice the address bar — this is running on Azure App Service, not my machine.
The whole site is behind cookie authentication, so unauthenticated users are redirected to
the login page."*

*[SHOW: log in with admin / admin123 → dashboard]*

*"I'll sign in as the administrator. The credentials aren't in the source code — on Azure
they come from App Service configuration, which I'll show later."*

---

## 3. Advanced Filtering — Part 3 Feature A  (~2:00–5:30)
*[SHOW: Events page]*

*"This is the core of Part 3's filtering work. In Part 2 the events search only had a text
box and a venue filter. For Part 3 I added a new **EventType** lookup table to the database
with eight predefined categories — Conference, Wedding, Corporate, Concert/Festival,
Birthday/Private, Charity, Workshop, and Other — and each event is now classified."*

*[SHOW: point at the EventType badges on the event rows]*

*"You can see each event now shows its type as a coloured badge."*

*[SHOW: demonstrate each filter one at a time]*
- *"Filter by **event type** — I'll pick 'Conference' and only conferences show."*
- *"Filter by **date range** — From/To dates filter to events that have a booking in that window."*
- *"And the **venue-availability** filter — 'only show events at venues that are free in the
  selected date range'. This excludes any event whose venue is already booked in that window,
  which is useful for finding open venues."*
- *"All of these combine together, and the form keeps my selections after I filter."*

*[SHOW: open an event's Create or Edit form]*

*"When creating or editing an event, there's now a dropdown to assign its type from the same
lookup table."*

---

## 4. Venues & Azure Blob image upload — proves Storage  (~5:30–7:00)
*[SHOW: Venues page, then Create/Edit a venue and upload an image]*

*"Venues have full create-read-update-delete. The important Part 3 point here is the image
upload. I'll upload a venue image now."*

*[SHOW: after upload, right-click the image → 'Open image in new tab' so the blob URL shows]*

*"Look at the image URL — it's served from `steventeaselmck0604.blob.core.windows.net`,
my live Azure Storage account, not the local Azurite emulator I used in Part 2. The upload
validates file type and size, gives the blob a unique name, and stores it in the
`venue-images` container."*

---

## 5. Bookings & the database view  (~7:00–8:00)
*[SHOW: Bookings page]*

*"Bookings link an event and a venue to a date and time. Behind the scenes there's a SQL
view, `vw_BookingDetails`, that joins booking, event, and venue data — it's mapped in EF Core
as a keyless entity. That view is created automatically by a migration when the app first
starts against the database."*

---

## 6. Going Live on Azure — Part 3 Feature B  (~8:00–13:00)
*[SHOW: VS Code — open `poe/part3/azure-provision.sh`]*

*"Now the migration. I did the whole thing from the command line with the Azure CLI — no
portal clicking. This script, `azure-provision.sh`, creates every resource, and a companion
`azure-deploy.sh` publishes the app. Everything is created inside one resource group,
`rg-eventease`, so the entire stack can be torn down with a single command. Let me prove it's
all live from the CLI."*

*[SHOW: terminal — list everything in the one resource group]*
```bash
az resource list --resource-group rg-eventease --output table
```

*"One command, and you can see all four pieces sitting in the single resource group. Let me
walk through each and why I chose it."*

- **Azure SQL Database** — *"This managed relational database replaces the local SQL Server
  from Part 2. I chose it because my schema is relational with foreign keys and a view, and
  Entity Framework Core works against it with no code changes — only the connection string
  differs. It also gives me automated backups and scaling."*
- **Azure Storage account** — *"This replaces the Azurite emulator for venue images. Blob
  storage is the right fit for binary files, it's cheap, and it serves images over HTTP. Same
  SDK as before — only the connection string changed."*
- **App Service plan + Web App** — *"This is managed hosting that replaces `dotnet run` on my
  laptop. It's a Linux App Service on the .NET 10 runtime, with managed HTTPS."*

*[SHOW: terminal — prove the configuration lives in App Service, listing KEYS only so no
secret is shown on camera]*
```bash
# SQL connection string — note the type is SQLAzure, set on the web app (not in source)
az webapp config connection-string list \
  --resource-group rg-eventease --name app-eventease-lmck0604 \
  --query "[].{name:name, type:value.type}" --output table

# App settings — show only the KEYS, not the secret values
az webapp config appsettings list \
  --resource-group rg-eventease --name app-eventease-lmck0604 \
  --query "[].name" --output table
```

*"This is the most important configuration point for the rubric. The app does NOT read from my
local machine. The SQL connection string is set on the web app as a SQLAzure connection
string, and the storage connection string and admin credentials are app settings —
`ConnectionStrings__AzureStorage`, `AdminCredentials__Username`, `AdminCredentials__Password`.
I'm deliberately listing only the keys, not the values, so I don't leak secrets on camera.
None of these are in my source code or on GitHub — the deployed app reads everything from this
live configuration."*

*[SHOW: terminal — prove the uploaded image is in the live storage container]*
```bash
az storage blob list \
  --account-name steventeaselmck0604 --container-name venue-images \
  --auth-mode login --query "[].name" --output table
```

*"Here's the image I uploaded a moment ago, sitting in the live `venue-images` container —
proof the app is writing to Azure Storage. The container is public, so I can also open that
blob URL straight in the browser."*

*[SHOW: terminal — prove the live Azure SQL database has my tables and seed data. The password
comes from the `EE_SQL_PASSWORD` environment variable, so it never appears on screen.]*
```bash
sqlcmd -S tcp:sql-eventease-lmck0604.database.windows.net,1433 \
  -d EventEaseDb -U eventeaseadmin -P "$EE_SQL_PASSWORD" \
  -Q "SELECT Id, Name FROM EventTypes ORDER BY Id;"
```

*"And here's the live Azure SQL database returning my new EventTypes table with its eight seed
rows — the same data driving the filter I demoed earlier."*

---

## 7. The migration experience  (~13:00–14:00)
*[SHOW: back in the editor — appsettings.json and Program.cs]*

*"To reflect on the migration: the application code barely changed — what changed was
configuration and environment. The connection strings moved out of source and into App
Service settings. I added SQL firewall rules so App Service and my machine can connect. The
schema and seed data reached Azure automatically because the app runs `db.Database.Migrate()`
on startup. This separation of local and production environments matters because secrets stay
out of source control, the same build runs in both places, and mistakes in development can't
touch live data."*

---

## 8. Drop all resources — proof  (~14:00–15:00)
*[SHOW: terminal in the project folder]*

*"Finally, the rubric asks me to drop all the resources and prove it. Because everything is in
one resource group, one command removes it all."*

*[SHOW: run it on camera]*
```
bash poe/part3/azure-teardown.sh
```
*[SHOW: wait, then run the proof]*
```
az group exists -n rg-eventease     # prints 'false' once deletion completes
az group list --output table        # confirm rg-eventease is no longer listed
```
*"It now returns `false` and the resource group is gone from the list — every resource has been
deleted, all confirmed from the CLI."*

---

## 9. Report & theory  (~15:00–17:00)
*[SHOW: REPORT.md]*

*"My written report covers the full feature list, the Azure services and why I used them, the
migration reflection, and three theory questions: how Cosmos DB differs from relational
databases — NoSQL, schema-flexible, globally distributed, horizontally scaled with tunable
consistency; the key considerations for Logic Apps handling sensitive data — Key Vault,
managed identities, encryption, least privilege, and auditing; and how Event Grid's
publish/subscribe model combines with Functions and Logic Apps to build reliable
event-driven workflows."*

---

## 10. Wrap-up  (~17:00–17:30)
*[SHOW: GitHub repo page]*

*"All the code, the provisioning scripts, the report, and the screenshots are in this GitHub
repository, linked in the description. The references and code attribution are at the end of
the report. Thanks for watching."*

---

### Recording tips
- Record at 1080p; make the terminal font large so the `az` commands and their output are readable.
- Speak slowly when showing the address bar, the connection-string config, and the teardown
  proof — those are the rubric's highest-value moments.
- Have `EE_SUFFIX` and `EE_SQL_PASSWORD` exported before recording so no secret is typed on
  screen; the config and SQL commands rely on them.
- If a step is slow (Azure cold start), pause/cut rather than leaving dead air.
- Keep the video unlisted/public on YouTube and put the link in REPORT.md and your submission.
