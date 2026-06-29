# CQRS Microservices — Proof of Concept

> A beginner-friendly guide to understanding **CQRS**, **Event-Driven Architecture**, and **Microservices** using .NET 10, Kafka, and SQL Server.

---

## Table of Contents

1. [What Problem Are We Solving?](#1-what-problem-are-we-solving)
2. [What is CQRS?](#2-what-is-cqrs)
3. [What is Event-Driven Architecture?](#3-what-is-event-driven-architecture)
4. [How This Project Is Structured](#4-how-this-project-is-structured)
5. [Walking Through a Real Request](#5-walking-through-a-real-request)
6. [Project Folder Structure](#6-project-folder-structure)
7. [Key Technologies Explained](#7-key-technologies-explained)
8. [How to Run the Application](#8-how-to-run-the-application)
9. [Testing the Application](#9-testing-the-application)
10. [Common Questions from Freshers](#10-common-questions-from-freshers)

---

## 1. What Problem Are We Solving?

Imagine you are building a blog platform. You have a single database and a single API that does everything — it saves articles, fetches articles, sends notifications, etc.

```
User ──► Single API ──► Single Database
```

This works fine for a small app. But as it grows:

- **Reading** articles happens 100× more than **writing** them — yet both share the same database load
- Changing how you store articles might break how you read them
- Adding a notification feature means touching core business logic
- One bug in the notification code could crash the whole app

**CQRS and microservices solve this by separating concerns into independent services that each do one job well.**

---

## 2. What is CQRS?

**CQRS** stands for **Command Query Responsibility Segregation**.

The big idea is simple:

> **Commands** change data. **Queries** read data. Keep them completely separate.

### The everyday analogy

Think of a **restaurant**:

- When you **order food** (command), the waiter writes it on a ticket and sends it to the kitchen. You do not watch the chef cook.
- When you **ask for the menu** (query), the waiter just brings you the menu. No cooking involved.

The ordering process (command) and the menu display (query) are completely separate workflows. Neither one blocks or interferes with the other.

### In code terms (before CQRS)

```csharp
// Everything in one service — reads and writes mixed together
public class ArticleService
{
    public Article CreateArticle(string title, string content) { ... } // write
    public Article UpdateArticle(Guid id, string title)        { ... } // write
    public List<Article> GetAllArticles()                       { ... } // read
    public Article GetArticleById(Guid id)                     { ... } // read
}
```

### In code terms (after CQRS)

```csharp
// Commands — only for changing data
public record CreateArticleCommand(string Title, string Content, string Author);
public record UpdateArticleCommand(Guid Id, string Title, string Content);

// Queries — only for reading data
public record GetArticlesQuery(int Page, int PageSize);
public record GetArticleByIdQuery(Guid Id);
```

Commands go to one service. Queries go to a different service. They can even use **different databases** optimised for their specific job.

### The two database model

```
                    ┌─────────────────────┐
                    │   WRITE DATABASE     │
 Commands ─────────►│  (ArticleDb)         │
 (Create, Update)   │  Normalised SQL      │
                    │  Optimised for       │
                    │  writes & integrity  │
                    └─────────────────────┘

                    ┌─────────────────────┐
                    │   READ DATABASE      │
 Queries  ◄─────────│  (ReadDb)            │
 (GetAll, GetById)  │  Denormalised SQL    │
                    │  Optimised for       │
                    │  fast reads          │
                    └─────────────────────┘
```

The two databases stay in sync via **Kafka events** (explained next).

---

## 3. What is Event-Driven Architecture?

When something important happens in your system (e.g. an article is created), you **publish an event** — a message that says "this thing happened". Other services **listen** for that event and react to it.

### The newspaper analogy

- A **journalist** writes an article and publishes it in the newspaper (the event)
- The **readers** (subscribers) each get their own copy and do their own thing with it — some read it for news, some clip it for recipes, some ignore it entirely
- The journalist does not know or care who reads it

In our system:
- **ArticleService** publishes an `ArticleCreatedEvent` to Kafka
- **QueryService** listens and updates its read database
- **NotificationService** listens and sends a notification email
- Neither QueryService nor NotificationService needed to be modified when the other was added

### Why Kafka?

**Apache Kafka** is a message broker — a middleman that holds messages until consumers are ready to read them. Think of it as a very fast, reliable post office.

```
ArticleService                   Kafka Topic                  Consumers
──────────────    publishes     ─────────────────   reads    ──────────────────
  Creates    ──► article-created ──────────────────────────► QueryService
  article                                           reads    ──────────────────
                                                  ─────────► NotificationService
```

Key benefit: if the NotificationService is temporarily down, Kafka holds the message. When it comes back up, it processes the message — **no data is lost**.

---

## 4. How This Project Is Structured

```
┌─────────────────────────────────────────────────────────────────────┐
│                          React Frontend                              │
│                    (Testing UI at :3000)                             │
└────────────────────────────┬────────────────────────────────────────┘
                             │ HTTP
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        API Gateway (:5000)                           │
│                   Ocelot — routes requests to the                    │
│                   correct downstream service                         │
└──────────────┬──────────────────────────────┬───────────────────────┘
               │ POST/PUT/DELETE               │ GET
               ▼                               ▼
┌──────────────────────────┐   ┌──────────────────────────────────────┐
│   ArticleService (:5001) │   │        QueryService (:5003)           │
│                          │   │                                       │
│  Handles all WRITES      │   │  Handles all READS                    │
│  - CreateArticle         │   │  - GetAllArticles                     │
│  - UpdateArticle         │   │  - GetArticleById                     │
│                          │   │                                       │
│  Writes to: ArticleDb    │   │  Reads from: ReadDb                   │
└──────────┬───────────────┘   └──────────────────────────────────────┘
           │                                      ▲
           │ Publishes Kafka events                │ Listens for events
           ▼                                       │
┌─────────────────────────────────────────────────┘
│                    Apache Kafka
│        Topics: article-created, article-updated
│
└─────────────────────────────────────────────────────────────────────┐
                                                                       ▼
                                                     ┌─────────────────────────────┐
                                                     │  NotificationService (:5005) │
                                                     │  Listens for events          │
                                                     │  Sends email notifications   │
                                                     └─────────────────────────────┘
```

### Each service is independent

| Service | Job | Database | Port |
|---|---|---|---|
| **ApiGateway** | Route requests to the right service | None | 5000 |
| **ArticleService** | Handle commands (write operations) | ArticleDb | 5001 |
| **QueryService** | Handle queries (read operations) | ReadDb | 5003 |
| **NotificationService** | React to events, send notifications | None | 5005 |

---

## 5. Walking Through a Real Request

### Creating an article (Command flow)

Let us trace exactly what happens when a user creates a new article:

```
Step 1: User submits the form in the React UI
        POST http://localhost:3000
              │
              ▼
Step 2: Request hits the API Gateway
        POST http://localhost:5000/api/articles
              │  Ocelot forwards it to ArticleService
              ▼
Step 3: ArticleService receives the request
        ArticleController.Create(CreateArticleCommand)
              │  Sends command via MediatR
              ▼
Step 4: CreateArticleHandler processes the command
        - Validates the data
        - Saves article to ArticleDb (SQL Server)
        - Publishes ArticleCreatedEvent to Kafka topic "article-created"
        - Returns the created article to the caller
              │
        ┌─────┴──────────────────────────────────────┐
        │                                             │
        ▼                                             ▼
Step 5a: QueryService consumes the event    Step 5b: NotificationService consumes the event
         - ArticleCreatedConsumer                     - ArticleCreatedNotificationConsumer
         - Saves to ReadDb                            - Logs / sends email notification
         (read model is now in sync)
```

### Reading articles (Query flow)

```
Step 1: User clicks "Refresh" in the React UI
        GET http://localhost:5000/api/articles
              │  Ocelot forwards it to QueryService
              ▼
Step 2: QueryService handles the request
        QueryController.GetArticles()
              │  Sends query via MediatR
              ▼
Step 3: GetArticlesHandler fetches from ReadDb
        - Reads directly from the denormalised read database
        - Returns the list to the caller

Note: ArticleService is never involved in reads. It is free to handle more writes.
```

---

## 6. Project Folder Structure

```
microservices-poc-cqrs/
├── backend/
│   ├── docker-compose.yml          ← Runs all services together
│   └── src/
│       ├── ApiGateway/             ← Entry point, routes requests (Ocelot)
│       │   ├── Program.cs
│       │   └── ocelot.json         ← Route configuration
│       │
│       ├── Services/
│       │   ├── ArticleService/     ← WRITE side (Commands)
│       │   │   ├── Controllers/
│       │   │   │   └── ArticleController.cs
│       │   │   ├── Data/
│       │   │   │   └── ArticleDbContext.cs
│       │   │   └── Handlers/Commands/
│       │   │       ├── CreateArticleHandler.cs
│       │   │       └── UpdateArticleHandler.cs
│       │   │
│       │   ├── QueryService/       ← READ side (Queries)
│       │   │   ├── Controllers/
│       │   │   │   └── QueryController.cs
│       │   │   ├── Consumers/
│       │   │   │   └── ArticleCreatedConsumer.cs  ← Kafka listener
│       │   │   └── Handlers/Queries/
│       │   │       └── GetArticlesHandler.cs
│       │   │
│       │   └── NotificationService/ ← Reacts to events
│       │       ├── Consumers/
│       │       │   ├── ArticleCreatedNotificationConsumer.cs
│       │       │   └── ArticleUpdatedNotificationConsumer.cs
│       │       └── Services/
│       │           ├── EmailService.cs
│       │           └── NotificationService.cs
│       │
│       └── Shared/                 ← Code shared between all services
│           ├── Contracts/
│           │   └── ArticleContracts.cs  ← Commands, Queries, DTOs
│           ├── Events/
│           │   └── ArticleEvents.cs     ← Kafka event definitions
│           └── Infrastructure/
│               ├── KafkaProducer.cs
│               ├── KafkaConsumer.cs
│               └── KafkaTopicConstants.cs
│
└── frontend/
    └── ui/                         ← React testing UI
```

---

## 7. Key Technologies Explained

### MediatR — the in-process message bus

**MediatR** is a NuGet package that implements the **Mediator pattern**. Instead of your controller calling a service directly, it sends a command/query to MediatR, which finds the right handler and calls it.

```csharp
// Without MediatR — controller knows about ArticleService
public class ArticleController : ControllerBase
{
    private readonly ArticleService _articleService;
    public async Task<IActionResult> Create(...)
        => Ok(await _articleService.CreateArticle(...));
}

// With MediatR — controller only knows about IMediator
public class ArticleController : ControllerBase
{
    private readonly IMediator _mediator;
    public async Task<IActionResult> Create(CreateArticleCommand command)
        => CreatedAtAction(..., await _mediator.Send(command));
}
```

Why is this better? The controller has **zero knowledge** of how an article is created. You can change the entire `CreateArticleHandler` without touching the controller.

### Entity Framework Core — database access

EF Core is Microsoft's Object-Relational Mapper (ORM). It lets you work with the database using C# classes instead of writing raw SQL.

```csharp
// Instead of: SELECT * FROM Articles WHERE Id = @id
var article = await _dbContext.Articles.FindAsync(id);

// Instead of: INSERT INTO Articles (Id, Title, ...) VALUES (...)
await _dbContext.Articles.AddAsync(article);
await _dbContext.SaveChangesAsync();
```

### Ocelot — API Gateway

**Ocelot** is a NuGet package that turns an ASP.NET Core app into an API Gateway. You configure routes in `ocelot.json`:

```json
{
  "UpstreamPathTemplate": "/api/articles",   ← what the client calls
  "UpstreamHttpMethod": ["POST"],
  "DownstreamPathTemplate": "/api/article",   ← what ArticleService expects
  "DownstreamHostAndPorts": [{ "Host": "articleservice", "Port": 5001 }]
}
```

The client only ever talks to port 5000. It never needs to know that ArticleService exists on 5001.

### Docker Compose — running everything together

Docker packages each service into a **container** — an isolated environment that has everything the service needs to run. `docker-compose.yml` describes all the containers and how they connect.

```yaml
services:
  kafka:         # Message broker
  sqlserver:     # Database
  articleservice: # Our write service
  queryservice:  # Our read service
  apigateway:    # Routes requests
```

Running `docker-compose up` starts all of them together with one command.

---

## 8. How to Run the Application

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [Node.js](https://nodejs.org/) (for the React frontend)

### Step 1 — Start the backend

```bash
cd backend
docker-compose build --no-cache
docker-compose up -d
```

This will download and start:
- Apache Kafka (message broker)
- SQL Server 2022 (database)
- ArticleService, QueryService, NotificationService
- API Gateway

Check all containers are running:

```bash
docker-compose ps
```

All services should show status `Up`.

### Step 2 — Start the frontend

```bash
cd frontend/ui
npm install
npm start
```

Opens the testing UI at **http://localhost:3000**

### Step 3 — Verify services are healthy

You can access the Swagger UI for each service directly:

| Service | Swagger URL |
|---|---|
| ArticleService | http://localhost:5001/swagger |
| QueryService | http://localhost:5003/swagger |

### Stopping everything

```bash
cd backend
docker-compose down
```

To also delete the database volumes (fresh start):

```bash
docker-compose down -v
```

---

## 9. Testing the Application

The React UI at http://localhost:3000 lets you test the full CQRS flow:

### Create an article

1. Fill in Title, Content, and Author
2. Click **POST /api/articles**
3. The request goes: UI → Gateway → ArticleService → ArticleDb + Kafka

### See the CQRS propagation happen

After creating an article:
- Wait 1–2 seconds (Kafka event propagation)
- Click **GET /api/articles** (Refresh)
- The article now appears — fetched from **QueryService's ReadDb**, not ArticleService

This demonstrates **eventual consistency** — the read model is not updated instantly, but it will be updated shortly after the write.

### Update an article

1. Click **Edit** on any article in the list — the Update form prefills automatically
2. Change the title or content
3. Click **PUT /api/articles/{id}**

### Test with curl (optional)

```bash
# Create an article
curl -X POST http://localhost:5000/api/articles \
  -H "Content-Type: application/json" \
  -d '{"title":"My First Article","content":"Hello CQRS world!","author":"Developer"}'

# Get all articles (from QueryService via Gateway)
curl http://localhost:5000/api/articles
```

---

## 10. Common Questions from Freshers

**Q: Why not just use one database for both reads and writes?**

You can, and many apps do. CQRS with separate databases shines when:
- Your read queries are complex (many JOINs) — the read database can store pre-joined data
- You have very high read traffic — the read database can be scaled independently
- You want to support multiple read formats (e.g. a search index + SQL + a cache)

For small apps, one database is perfectly fine.

---

**Q: What is eventual consistency? Should I be worried?**

Eventual consistency means the read database will *eventually* match the write database — but there may be a small delay (milliseconds to seconds) while the Kafka event propagates.

For most real-world scenarios (blogs, product listings, dashboards) this is completely acceptable. Users do not notice a 500ms delay. However, for bank account balances or stock levels where you need the absolute latest value, you would read directly from the write database instead.

---

**Q: What is MediatR doing that I could not do myself?**

Nothing magical — you could call your handler directly. MediatR gives you:
- **Decoupling**: Controllers do not import or depend on handler classes
- **Pipeline behaviours**: You can add logging, validation, or caching for all commands/queries in one place without touching each handler
- **Consistency**: Every command/query follows the same pattern

---

**Q: Why does ArticleService use Kafka but the controller returns immediately?**

Publishing to Kafka is **fire and forget** — the ArticleService publishes the event and immediately returns a response to the client. It does not wait for the QueryService to process the event. This is what makes the system fast and loosely coupled.

---

**Q: What happens if Kafka is down when an article is created?**

In this demo, the article would still be saved to ArticleDb (the write succeeds), but the Kafka event would fail to publish and the read database would not be updated. In a production system you would use the **Outbox Pattern** — save the event to the database in the same transaction, then have a background process publish it to Kafka, guaranteeing delivery.

---

**Q: Why is there an API Gateway? Can the frontend not call ArticleService directly?**

It could, but then the frontend would need to know the address of every service, and you would have to configure CORS on every service. The gateway gives you:
- A **single entry point** — clients only need one URL
- **Routing logic** in one place — move a service to a different port without changing any client code
- A place to add **authentication** for all services at once

---

## Architecture Summary

```
┌─────────────────────────────────────────────────────┐
│                 CQRS in one sentence                  │
│                                                       │
│  "Use a different model to update information         │
│   than the model you use to read information."        │
│                           — Greg Young (coined CQRS)  │
└─────────────────────────────────────────────────────┘

Commands (write)                     Queries (read)
────────────────                     ──────────────
CreateArticleCommand                 GetArticlesQuery
UpdateArticleCommand                 GetArticleByIdQuery
      │                                     │
      ▼                                     ▼
ArticleService                       QueryService
ArticleDbContext                     ReadDbContext
      │                                     ▲
      └──── Kafka event ───────────────────┘
             (bridges the two sides)
```

The event is the glue. It keeps two independent models in sync without either one knowing about the other.
