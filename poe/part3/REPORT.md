# EventEase — Part 3 Reflective Technical Report

*Draft note: This document is a draft intended to be personalised and verified before submission. Please confirm every technical detail against your own final implementation, replace the placeholder submission links, and rewrite the reflection sections (Section 4 in particular) in your own voice and from your own experience. The prose below is a professional starting point, not a finished personal account.*

---

## 1. Introduction and Overview

EventEase is a venue-booking web application built with ASP.NET Core MVC. It allows an administrator to manage venues, the events that take place at those venues, and the bookings that schedule those events on specific dates and times. The application is structured around the standard Model-View-Controller pattern, uses Entity Framework Core for data access with a code-first migration strategy, and persists venue images to blob storage.

This report documents the journey of EventEase from **Part 2** to **Part 3** of the Portfolio of Evidence. In Part 2 the application was developed and tested entirely on a local machine: it ran with `dotnet run`, talked to a local SQL Server instance, and stored images in the Azurite storage emulator. Part 3 is about taking that same application *live* — deploying it to real, managed Microsoft Azure cloud services so that it runs as a production-style hosted web application accessible over the internet.

The purpose of this report is therefore twofold. First, it gives a complete account of what the application does and how it is built. Second, and more importantly for Part 3, it reflects on the **"go live" migration**: the Azure services that were adopted, the configuration changes that were required, the reasoning behind each technology choice, and the broader software-engineering lessons that come from separating a local development environment from a live production environment. The report closes with a theoretical discussion of three further Azure technologies (Cosmos DB, Logic Apps, and Event Grid).

---

## 2. Full Feature List

The features of EventEase can be grouped into authentication, the three core data-management areas (venues, events, bookings), and the supporting data infrastructure.

### Authentication and Access Control
- **Cookie-based authentication** with a dedicated login page. The administrator signs in with configured admin credentials, and a sign-in cookie maintains the session.
- **Authorisation on protected controllers.** Most controllers are decorated with `[Authorize]`, so the management features are only reachable once the user is signed in. Unauthenticated requests are redirected to `/Account/Login`.

### Venue Management
- **Full CRUD** for venues (create, read, update, delete). A venue records its name, location, capacity, and an optional image URL.
- **Venue image upload to blob storage.** Images are validated and uploaded through a dedicated blob storage service. The service accepts only `.jpg`, `.jpeg`, and `.png` files up to a maximum of 5 MB, generates a unique GUID-based blob name, sets the correct content type, stores the file in a `venue-images` container, and supports deletion of the blob when a venue's image is removed or replaced.

### Event Management
- **Full CRUD** for events. An event has a name, an optional description, and a foreign key to the venue at which it is held.
- **Event type classification (new in Part 3).** Each event is now categorised against an `EventType` lookup table. Eight predefined categories are seeded: Conference, Wedding, Corporate, Concert / Festival, Birthday / Private, Charity, Workshop, and Other.
- **Advanced search and filtering on the Events page (new in Part 3).** Several filters can be combined together (they apply with AND logic), and the filter form repopulates with the chosen values after each submission:
  - **Free-text search** across event name and description.
  - **Filter by venue.**
  - **Filter by event type.**
  - **Filter by date range** (a "From" and "To" date evaluated against the `EventDate` of the events' bookings).
  - **Venue-availability filter** — an option to show only events held at venues that are *free* within the selected date range. This excludes any event whose venue already has a booking that falls inside the range, which is useful for finding events at venues that still have open capacity for a given period.

### Booking Management
- **Full CRUD** for bookings. A booking links an event and a venue to a specific date, a start time, an end time, and the name of the person who made the booking (`BookedBy`).
- **Consolidated read via a database view.** A SQL view, `vw_BookingDetails`, joins booking, event, and venue data into a single result set. It is mapped in Entity Framework Core as a keyless entity (`BookingDetailsView`) so the joined data can be queried directly without re-assembling it in application code.

### Data Infrastructure
- **Seed data** is supplied through EF Core's `HasData` mechanism: 7 venues, 8 events, 8 event types, and 8 bookings.
- **Automatic migrations on startup.** When the application starts it calls `db.Database.Migrate()`, which applies any outstanding migrations. This creates and updates the tables, creates the `vw_BookingDetails` view, and inserts the seed rows. This is the mechanism that makes the schema appear automatically in a fresh database.

---

## 3. Component Discussion — Azure Services Used and Why

Going live meant replacing three local components with three managed Azure services. The choices were guided by a single principle: **adopt managed Platform-as-a-Service (PaaS) offerings that match the local components closely enough that the application code stays almost identical, while gaining the operational benefits of the cloud.**

### Azure SQL Database
Azure SQL Database is a fully managed relational database service. It replaced the local SQL Server instance. It was the natural choice because EventEase already uses a relational schema with foreign keys and a SQL view, and Entity Framework Core targets it with no code changes — only the connection string differs. As a managed PaaS service it also provides automated backups, point-in-time restore, and the ability to scale compute and storage without managing an operating system or patching a database engine.

### Azure Storage (Blob)
Azure Blob Storage is object storage for unstructured binary data. It replaced the Azurite emulator that was used locally for venue images. Blob storage is the correct fit for image files: it is inexpensive, designed for large numbers of binary objects, and can serve files directly over HTTP via a public URL. Because the application already used the `Azure.Storage.Blobs` SDK against Azurite, moving to a real storage account required nothing more than swapping the connection string from `UseDevelopmentStorage=true` to a real account connection string — the SDK calls are identical.

### Azure App Service
Azure App Service is managed PaaS hosting for web applications. It replaced running the app locally with `dotnet run`. EventEase is hosted on a Linux App Service configured for the `DOTNETCORE:10.0` runtime. App Service was chosen because it offers a simple publish workflow, managed TLS certificates, built-in scaling, and an integrated configuration system for storing settings and connection strings outside the source code.

### Comparison: Local (Part 2) vs Live (Part 3)

| Concern | Part 2 — Local | Part 3 — Live (Azure) | Why the Azure service |
|---|---|---|---|
| Relational database | Local SQL Server (`Server=localhost,1433`) | Azure SQL Database | Managed PaaS, EF Core works unchanged, automated backups and scaling |
| Image / file storage | Azurite emulator (`UseDevelopmentStorage=true`) | Azure Storage (Blob) | Right fit for unstructured binaries, cheap, served over HTTP, same SDK |
| Application hosting | `dotnet run` on the developer's machine | Azure App Service (Linux, DOTNETCORE:10.0) | Easy publish, managed TLS and scaling, integrated configuration |

All three services were provisioned inside a single resource group, `rg-eventease`, which is significant for teardown (discussed below).

---

## 4. The Migration Experience (Reflection)

Moving EventEase from local development to live Azure hosting was, on the surface, surprisingly small in terms of code — and that is exactly the point I want to reflect on. The application logic, the EF Core data access, and the blob SDK calls did not change at all. What changed was **configuration** and **environment**, and working through that distinction taught me more about professional software practice than any single feature did.

### Configuration changes that were required

The concrete changes needed to take the application live were:

1. **The SQL connection string** (`DefaultConnection`) was changed from the local SQL Server value to the Azure SQL Database connection string. Critically, this is **not** stored in source control. It is set as an App Service connection string of type *SQLAzure*.
2. **The blob storage connection string** (`AzureStorage`) was changed from `UseDevelopmentStorage=true` to the real storage account connection string. It is supplied through the App Service application setting key `ConnectionStrings__AzureStorage`, so that `GetConnectionString("AzureStorage")` resolves it correctly at runtime. Again, it is not committed to source.
3. **SQL firewall rules** were added on the Azure SQL server: one to allow Azure services to connect (so App Service can reach the database) and one to allow the developer's own IP address (for administration and verification).
4. **The blob service class was renamed** from `AzuriteBlobStorageService` to `AzureBlobStorageService` to honestly reflect that it now targets a real Azure storage account. The implementation itself was not altered — it is the same `Azure.Storage.Blobs` code — but the name now tells the truth about what it does.
5. **Schema and seed data reached Azure SQL automatically**, because `db.Database.Migrate()` runs on first startup. On the empty Azure database it applied every migration in order, creating the tables, the `vw_BookingDetails` view, and the seed rows (including the new `EventTypes`). For migrating *real, accumulated runtime data* rather than seeded data, the appropriate alternative would be a BACPAC export/import using SqlPackage, which captures both schema and existing data.

### Challenges

The main challenges were not in the C# but in the surrounding plumbing. The connection-string key format for the storage account (`ConnectionStrings__AzureStorage` using the double-underscore convention that App Service translates into a configuration section) is easy to get subtly wrong, and a wrong key produces a runtime failure rather than a compile error. The SQL firewall is another quiet trap: until the "allow Azure services" rule exists, App Service simply cannot connect, and the failure only shows up at startup. These are typical cloud-deployment issues — everything compiles, but the environment is not yet wired up.

### Why separation of environments matters

The deeper lesson of Part 3 is *why* professional development insists on a clean separation between local/development and production environments. Working through this migration made several of these reasons tangible:

- **Configuration externalised from code.** Hardcoding a database server or an account key into source code makes it impossible to run the same build in two places. By reading connection strings from configuration, the identical compiled application runs locally against SQL Server/Azurite and in the cloud against Azure SQL/Storage with no rebuild.
- **Secrets management.** Production secrets — the SQL password, the storage account key — must never live in a Git repository, where they would be exposed forever in history. Keeping them as App Service settings (and, more robustly, in a secret store such as Azure Key Vault) keeps them out of source control and out of developers' machines.
- **Environment parity.** Azurite and a local SQL Server were chosen *because* they behave like their cloud counterparts. The closer dev and production behave, the fewer surprises appear only after deployment — bugs that "only happen in production" are usually parity gaps.
- **Security and least privilege.** Production has a tighter security posture: firewall rules restrict who can reach the database, TLS is enforced, and access should be scoped to only what each component needs. A local environment that ran wide open would be unacceptable in production.
- **Blast radius.** Mistakes in development should not be able to damage real data or real users. Separate environments contain the consequences of an error to the place where it happened.
- **Reproducibility.** Because the schema and seed data are produced by migrations that run automatically, the environment can be rebuilt from scratch deterministically. This is also what made the final teardown safe.

Finally, after demonstrating the live application, **all resources were destroyed with a single `az group delete` command on the `rg-eventease` resource group**, captured with a screenshot as proof. Grouping every resource into one resource group meant the entire deployment could be created and removed atomically — a clean illustration of why disciplined resource organisation matters in the cloud, both for cost control and for avoiding orphaned, charged-for resources.

---

## 5. Technologies Used and Why

| Technology | Role | Rationale |
|---|---|---|
| **ASP.NET Core MVC (.NET 10, C#)** | Web application framework | Mature, cross-platform framework with a clear separation of models, views, and controllers; first-class support on Azure App Service. |
| **Entity Framework Core 10 (code-first migrations)** | Data access and schema management | Lets the data model be expressed in C# classes; migrations version the schema and apply it automatically, which is what made the schema appear in Azure SQL on first run. |
| **SQL Server / Azure SQL Database** | Relational store | Suits the relational, foreign-key-based model; the same provider works locally and in Azure with only a connection-string change. |
| **Azure.Storage.Blobs SDK** | Image storage access | Single SDK that targets both the Azurite emulator and real Azure Storage, so binary uploads are handled identically in dev and production. |
| **Bootstrap 5 + bootstrap-icons** | Front-end styling | Provides a responsive, consistent UI quickly without hand-writing CSS, plus a clean icon set. |
| **Cookie-based authentication** | Login and session | A simple, built-in authentication scheme appropriate for a single-administrator management application. |

---

## 6. Theoretical Discussion

### (a) How Cosmos DB Differs from Traditional Relational Databases

Azure Cosmos DB is a globally distributed, multi-model **NoSQL** database, and it differs from a traditional relational database such as SQL Server in several fundamental ways.

The first difference is the **data model and schema**. Relational databases enforce a rigid, predefined schema of tables, columns, and typed relationships, and data is normalised across many tables joined at query time. Cosmos DB is schema-flexible: it stores items (typically JSON documents, but also key-value, graph, or column-family data depending on the API) without requiring every item to share the same shape. This makes it well suited to evolving or heterogeneous data where forcing a fixed schema would be awkward.

The second difference is **distribution and replication**. Cosmos DB offers turnkey global distribution: with a few clicks the same data can be replicated to multiple Azure regions around the world, placing data close to users and providing regional failover. Achieving comparable multi-region replication with a traditional relational database is considerably more manual and operationally heavy.

The third difference is **scaling**. Relational databases traditionally scale *vertically* — you move the workload to a bigger machine with more CPU and memory, which eventually hits a ceiling. Cosmos DB is designed to scale *horizontally* through partitioning: data is divided across logical partitions by a partition key and spread across physical partitions, so throughput and storage grow by adding partitions rather than by buying a larger server.

The fourth difference is **consistency**. Relational databases generally provide strong, transactional consistency. Cosmos DB exposes five **tunable consistency levels** — strong, bounded staleness, session, consistent prefix, and eventual — letting the developer trade consistency for lower latency and higher availability according to the needs of each workload.

Finally, the **pricing model** differs. Cosmos DB charges in **Request Units (RUs)**, an abstract currency representing the cost of database operations (a function of CPU, memory, and I/O), provisioned either as a fixed throughput or on a serverless/autoscale basis. Relational services are typically priced by compute tier and storage.

**When to choose each:** a relational database is the right choice for highly structured data with complex relationships, multi-table joins, and strong transactional integrity — which is exactly why EventEase uses Azure SQL. Cosmos DB is the better choice when you need massive horizontal scale, very low latency at global scale, flexible or rapidly changing document schemas, and multi-region availability.

### (b) Key Considerations When Designing Logic Apps That Handle Sensitive Data

Azure Logic Apps lets you build automated workflows by connecting services together, and when those workflows touch sensitive data the design must put security first.

The starting point is **never hardcoding secrets**. Connection strings, API keys, and credentials should not be embedded in the workflow definition. Logic Apps supports **secure parameters and secure inputs/outputs** so that sensitive values are not stored in plain text or shown in run history, and secrets should be sourced from **Azure Key Vault** rather than from the workflow itself. Better still, components should authenticate using **managed identities**, which remove credentials from the picture entirely by letting Azure issue and rotate the identity automatically.

**Encryption** must apply both in transit and at rest: traffic should use TLS/HTTPS, and any stored sensitive data should be encrypted at rest. Access should follow the **principle of least privilege** using role-based access control (RBAC), so that the Logic App and the people managing it have only the permissions they actually need.

The **network boundary** matters too. IP restrictions and **private endpoints** can ensure that triggers and connectors are only reachable from trusted networks rather than the open internet. Within the workflow itself, **input validation** guards against malformed or malicious data being propagated to downstream systems.

Finally, sensitive workflows require strong **audit logging and monitoring** — integrating with Azure Monitor and diagnostic logs so that access and actions are traceable — and they must respect **compliance and data-residency** requirements, ensuring that regulated data stays in approved regions and that the design meets the relevant standards (for example GDPR or POPIA) for the data it handles.

### (c) How Event Grid Combines with Other Services for Robust Event-Driven Workflows

Azure Event Grid is a managed event-routing service built on a **publish/subscribe** model, and its real power emerges when it is combined with other Azure services to build event-driven systems.

The central benefit is **decoupling**. Event *publishers* (such as Azure Storage, resource groups emitting management events, or custom application topics) simply announce that something happened; they do not need to know who, if anyone, is listening. *Subscribers* register their interest and react. This loose coupling means publishers and subscribers can evolve independently, and new reactions can be added without touching the source of the event.

Event Grid supports **fan-out**: a single event can be delivered to many subscribers at once, so one occurrence can trigger several independent reactions in parallel. It integrates naturally with **Azure Functions, Logic Apps, and Service Bus**, allowing each event to be handled by serverless code, an automated workflow, or a durable message queue as appropriate.

For reliability, Event Grid provides **built-in retry with back-off** and **dead-lettering**: if a subscriber is temporarily unavailable, delivery is retried, and events that still cannot be delivered are sent to a dead-letter destination for later inspection rather than being silently lost. Combined with its **near-real-time** delivery and its ability to **scale** to very high event volumes, this makes Event Grid a dependable backbone for event-driven architectures.

**A concrete example** relevant to EventEase: imagine that when an administrator uploads a venue image, the blob is written to Azure Storage. Storage publishes a *blob-created* event to Event Grid. Event Grid routes that event to an Azure Function, which processes the image — for example generating a resized thumbnail and writing it back to a thumbnails container — while simultaneously (via fan-out) notifying a Logic App that records the upload for auditing. The web application never blocks on this processing; it simply saves the file, and the rest happens asynchronously, reliably, and at scale. This is the essence of a robust event-driven workflow: small, independent components reacting to events instead of being tightly wired together.

---

## 7. References and Code Attribution

References follow a consistent author–date style. All Microsoft documentation is drawn from Microsoft Learn.

- Microsoft. (2024) *ASP.NET Core MVC overview*. Microsoft Learn. Available at: https://learn.microsoft.com/aspnet/core/mvc/overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Entity Framework Core — Migrations overview*. Microsoft Learn. Available at: https://learn.microsoft.com/ef/core/managing-schemas/migrations (Accessed: 4 June 2026).
- Microsoft. (2024) *What is Azure SQL Database?*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/azure-sql/database/sql-database-paas-overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Introduction to Azure Blob Storage*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/storage/blobs/storage-blobs-introduction (Accessed: 4 June 2026).
- Microsoft. (2024) *App Service overview*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/app-service/overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Configure connection strings and app settings in App Service*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/app-service/configure-common (Accessed: 4 June 2026).
- Microsoft. (2024) *Welcome to Azure Cosmos DB*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/cosmos-db/introduction (Accessed: 4 June 2026).
- Microsoft. (2024) *Consistency levels in Azure Cosmos DB*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/cosmos-db/consistency-levels (Accessed: 4 June 2026).
- Microsoft. (2024) *What is Azure Logic Apps?*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/logic-apps/logic-apps-overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Secure access and data in Azure Logic Apps*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/logic-apps/logic-apps-securing-a-logic-app (Accessed: 4 June 2026).
- Microsoft. (2024) *What is Azure Event Grid?*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/event-grid/overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Azure Key Vault overview*. Microsoft Learn. Available at: https://learn.microsoft.com/azure/key-vault/general/overview (Accessed: 4 June 2026).
- Microsoft. (2024) *Azure Storage SDK for .NET — Azure.Storage.Blobs*. Microsoft Learn. Available at: https://learn.microsoft.com/dotnet/api/overview/azure/storage.blobs-readme (Accessed: 4 June 2026).
- Bootstrap. (2024) *Bootstrap 5 documentation*. Available at: https://getbootstrap.com/docs/5.0 (Accessed: 4 June 2026).

### Code Attribution
The EventEase application was written by the student for this Portfolio of Evidence. The structure follows the conventional ASP.NET Core MVC scaffolding patterns and the official Microsoft Learn documentation and quickstarts cited above, particularly for Entity Framework Core migrations, cookie authentication, and the Azure Storage Blobs SDK usage. Where standard framework idioms or documented patterns were used, they are attributed to the relevant Microsoft Learn references rather than reproduced as original work. No third-party source code was copied wholesale into the project.

---

## 8. Submission Links

<!-- TODO: student to fill in -->

| Item | Link |
|---|---|
| **Live web app URL** | https://app-eventease-lmck0604.azurewebsites.net *(deployed for the demo; resources dropped after — see teardown proof)* |
| **GitHub repository URL** | `<!-- TODO: student to fill in -->` |
| **YouTube walkthrough URL** | `<!-- TODO: student to fill in -->` |
