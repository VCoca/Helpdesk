# Internship Project Brief — 4 Weeks
## Helpdesk: A Ticket Tracking Web Application

**Duration:** 4 weeks
**Stack:** .NET (backend) · React + TypeScript (frontend) · PostgreSQL (database) · Docker · GitHub

---

## How to read this document

- **Part 1** explains what you are building and why.
- **Part 2** is a glossary of every term and technology used in this document. If anything below is unfamiliar, look it up there first. Nothing here assumes you have used these tools before.
- **Part 3** is the required scope — this is what "done" means.
- **Part 4** is the week-by-week plan.
- **Part 5** is optional extra work, only if the required scope is finished on time.
- **Part 6** covers deliverables, working practices, and how your work will be reviewed.

Read the whole document before you write any code. If something is unclear or seems unreasonable, say so on day one — that conversation is part of the exercise.

---
---

# PART 1 — What You Are Building

A web application where people report problems and support staff resolve them. The domain is deliberately familiar so that all your effort goes into learning the technology rather than understanding business rules.

### Two kinds of users

- **User** — reports a problem, sees only their own tickets, can comment on them.
- **Agent** — sees all tickets, assigns tickets to themselves, changes status, comments, closes tickets.

### Ticket lifecycle

```
   New ──► InProgress ──► Resolved ──► Closed
    │           │             │
    └───────────┴─────────────┴──► Rejected
                              │
             InProgress ◄─────┘   (reopened if the user is not satisfied)
```

Transitions are **not** arbitrary. A ticket cannot go from `New` straight to `Closed`. This set of rules is a small **state machine**, and it is the only non-trivial piece of business logic in the project. It is there on purpose: it forces you to think, it is easy to test, and it stays small.

### The three moving parts

Unlike a single-program application, this system runs as three separate processes that communicate over a network:

```
   BROWSER                     SERVER                    DATABASE
┌───────────────┐   HTTP    ┌──────────────┐   SQL   ┌────────────┐
│   React app   │ ────────► │  .NET Web    │ ──────► │ PostgreSQL │
│ (TypeScript)  │ ◄──────── │     API      │ ◄────── │            │
└───────────────┘   JSON    └──────────────┘         └────────────┘
    "frontend"               "backend"
```

Three processes, three languages (TypeScript, C#, SQL). Most of the difficulty for a newcomer is at the **boundaries** between them, not inside any one of them. That is exactly why Week 1 requires all three boundaries to work before anything else gets built.

---
---

# PART 2 — Glossary

## 2.1 Web and API concepts

**HTTP** — The protocol the browser uses to talk to the server. The client sends a *request*, the server returns a *response*. Each request has a **method** stating intent:

| Method | Meaning |
|---|---|
| `GET` | Retrieve data (changes nothing) |
| `POST` | Create something new |
| `PUT` | Replace / update an existing thing |
| `PATCH` | Partially update an existing thing |
| `DELETE` | Remove something |

Each response carries a **status code**:

| Code | Meaning |
|---|---|
| `200 OK` | Success |
| `201 Created` | Successfully created a resource |
| `204 No Content` | Success, nothing to return |
| `400 Bad Request` | The request was malformed or invalid |
| `401 Unauthorized` | You are not logged in |
| `403 Forbidden` | You are logged in but not allowed to do this |
| `404 Not Found` | The thing does not exist |
| `500 Internal Server Error` | Something broke on the server |

Returning the correct status code is part of the required scope. Returning `200 OK` with an error message inside the body is wrong.

**REST API** — A convention for organising a server's interface. Resources are plural nouns in the URL; the HTTP method says what happens to them:

```
GET    /api/tickets        list tickets
GET    /api/tickets/42     get one ticket
POST   /api/tickets        create a ticket
PUT    /api/tickets/42     update a ticket
DELETE /api/tickets/42     delete a ticket
```

REST is not a technology or a library. It is an agreement. What matters is that you apply it consistently.

**JSON** — A text format for exchanging structured data between the server and the browser. Human-readable:

```json
{ "id": 42, "title": "Printer is not working", "status": "New" }
```

**Endpoint** — One specific method-plus-path combination, e.g. `POST /api/tickets`. In .NET this corresponds to one method inside a controller class.

**Request body / response body** — The JSON payload sent with a request or returned in a response.

**Query string** — Parameters appended to a URL, used for filtering and paging: `/api/tickets?status=New&page=2&pageSize=20`.

**CORS** (*Cross-Origin Resource Sharing*) — A browser security rule. A page served from `localhost:5173` is **not** allowed to call a server at `localhost:5000` unless that server explicitly permits it. During development this is one of the first things that will block you, and the error message in the browser console is not obvious. When a request works in Swagger but fails from React, suspect CORS first.

## 2.2 Backend concepts (.NET / C#)

**.NET** — The runtime and platform for running C# applications.

**ASP.NET Core Web API** — The framework (and project template) for building HTTP APIs in C#. It includes the web server and request routing.

**Controller** — A C# class that groups the endpoints of one resource. `TicketsController` holds the methods for listing, reading, creating, and updating tickets. A controller should be *thin*: read the request, call a service, return a response.

**Service layer** — Classes that contain the actual business logic. Keeping logic out of controllers matters because a service can be tested directly, without starting a web server or sending HTTP requests.

**Entity** — A C# class that maps to one database table. The `Ticket` class corresponds to the `Tickets` table.

**DTO** (*Data Transfer Object*) — A separate class describing exactly what goes out of, or comes into, an API endpoint. Do **not** return entities directly. Two reasons: entities contain fields that must never leave the server (password hashes, internal flags), and entities reference each other in both directions, which makes JSON serialisation loop infinitely. Your internal data model and your public wire format are two different things that happen to look similar at the start.

**ORM** (*Object-Relational Mapper*) — A library that maps classes to tables and writes SQL for you.

**Entity Framework Core (EF Core)** — The standard ORM for .NET. Instead of writing SQL by hand, you write:

```csharp
var tickets = await db.Tickets
    .Where(t => t.Status == TicketStatus.New)
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync();
```

EF Core translates this into a `SELECT ... WHERE ... ORDER BY ...` query. Convenient, but it hides things — see *N+1 problem* below.

**LINQ** — The C# query syntax used above (`Where`, `Select`, `OrderBy`, `GroupBy`). It works on both in-memory collections and database queries, which is powerful and occasionally confusing: the same code can run in SQL or in your server's memory depending on how you write it. Knowing which is happening matters (see *deferred execution*).

**Deferred execution** — An EF Core query does not run when you write it. It runs when you call something like `ToListAsync()`. Everything you chain before that point becomes part of the SQL. Everything after it happens in memory on the server. This distinction is the single most common source of accidental performance problems.

**DbContext** — The EF Core class representing a session with the database. It exposes your tables as properties and tracks changes to loaded objects.

**Code First** — An approach where you write the C# classes first and generate the database schema from them, rather than designing tables first and generating classes.

**Migration** — A versioned, generated description of a schema change. You add a property to a class, run `dotnet ef migrations add AddPriorityToTicket`, and EF Core produces a C# file that knows how to apply that change and how to undo it. Migrations are committed to Git alongside your code. This solves the recurring problem of "my code expects a column that does not exist in your database" — everyone just applies the migrations.

**Seed data** — Initial test data inserted when the database is created: a handful of users, a few dozen tickets. Required for this project. Without it you will develop against an empty database, everything will feel instant and look fine, and the first realistic amount of data will expose problems you never saw.

**N+1 problem** — The most common performance mistake with an ORM. You load 50 tickets with one query, then loop over them and read `ticket.Author.FullName` for each — which silently issues 50 more queries. Fixed by explicitly loading the related data (`.Include(t => t.Author)`). Enable SQL logging during development so you can see when this happens; it is very hard to notice otherwise.

**Async / await** — C# syntax for non-blocking operations. While the database is answering, the thread is released to serve other requests instead of waiting. In ASP.NET Core this is the default way to write everything that touches the database or the network, not an optimisation you add later.

**Dependency Injection (DI)** — A pattern where a class declares what it needs in its constructor and a container supplies it, instead of the class constructing its own dependencies. You register `ITicketService → TicketService` once at startup, and every controller that asks for `ITicketService` receives one. This exists so that dependencies can be swapped — most usefully, replaced with fakes in tests.

**Middleware** — A pipeline of components every HTTP request passes through in order: logging → CORS → authentication → authorisation → controller. Each component can handle the request, modify it, pass it along, or stop it. You will write one yourself: a global exception handler.

**Global exception handling** — A single middleware that catches unhandled exceptions and converts them into a proper error response with the right status code. Without it, an unexpected null reference reaches the client as an HTML stack-trace page, which is both useless and a security problem.

**Model validation** — Checking that incoming data is acceptable (title not empty, priority is a valid value, description under 2000 characters) before any logic runs. `FluentValidation` is the library used here.

**Swagger / OpenAPI** — An auto-generated page, served by your own API, that lists every endpoint and lets you call each one from the browser with no frontend code at all. For the first two weeks this is your primary testing tool.

**Authentication vs authorisation** — Authentication answers *who are you* (logging in). Authorisation answers *what are you allowed to do* (roles and permissions). Two distinct steps, frequently confused. You need both.

**Password hashing** — Passwords are never stored. A one-way hash is stored instead, so that a database leak does not expose passwords. Never implement this yourself; ASP.NET Core Identity handles it.

**ASP.NET Core Identity** — The built-in library for user accounts, password hashing, and roles.

**JWT** (*JSON Web Token*) — A signed token the server issues when you log in successfully. The client stores it and attaches it to every subsequent request in an HTTP header (`Authorization: Bearer <token>`). The server verifies the cryptographic signature and reads the user's identity and role out of the token itself, so it does not need to store session state. Important: a JWT is signed, **not** encrypted — anyone holding it can read its contents. Never put secrets inside it.

**Claim** — A single piece of information carried inside a token, such as the user id or the role.

**Server-side paging** — Returning one page of results at a time (`page`, `pageSize`) together with a total count, rather than sending the whole table and slicing it in the browser. Required in this project. The filtering, sorting, and paging must all happen in SQL.

## 2.3 Frontend concepts (React)

**SPA** (*Single Page Application*) — The browser loads the JavaScript bundle once; from then on the app updates the page itself and fetches data as needed. No full page reload on every click.

**React** — A library for building user interfaces out of **components**. A component is a function that receives data and returns a description of what should appear on screen. You never manipulate the page directly — you change data, and React works out what to update in the DOM.

**Component** — A reusable, self-contained piece of UI: a button, a table, a whole page.

**JSX / TSX** — The syntax that lets you write markup inside a TypeScript file. `.tsx` is the file extension for a TypeScript file containing JSX.

**Props** — The input parameters a component receives from its parent.

**State** — Data owned by a component. When state changes, React re-renders the component. This is the central idea to internalise: you do not update the screen, you update the state and the screen follows.

**Hook** — A function provided by React that gives a component extra capabilities. `useState` holds state; `useEffect` runs side effects after rendering. Hooks may only be called at the top level of a component, never inside a condition or loop.

**Controlled component** — A form input whose displayed value comes from state rather than from the DOM. The standard way to handle forms in React.

**TypeScript** — JavaScript with static types. Types are checked at build time, before your code runs, which catches a large class of mistakes and makes editor autocomplete genuinely useful. **Required for this project** — do not use plain JavaScript.

**Vite** — The build tool and development server for the frontend. It provides **hot module replacement**: save a file and the change appears in the browser almost instantly, without a page reload.

**npm** — The JavaScript package manager. `package.json` declares your dependencies; `npm install` fetches them into `node_modules`, which is never committed to Git.

**React Router** — Maps URLs to components, so `/tickets/42` renders the ticket detail page. Also used to protect routes that require login.

**TanStack Query** — A library for fetching and caching server data. It handles loading states, error states, retries, caching, and refetching after a change. Writing this by hand with `useEffect` is possible and is how most tutorials do it, but it goes wrong in subtle ways. Use the library.

**React Hook Form** — A library for managing form state and submission with minimal re-rendering.

**Zod** — A validation library. You define a schema once and use it both to validate input and to derive the TypeScript type, so the two cannot drift apart.

**Tailwind CSS / MUI** — Two approaches to styling. Tailwind provides small utility classes you compose; MUI provides ready-made components. Pick one at the start of the project and stay with it. Mixing both is a common and avoidable mess.

**Loading / empty / error states** — Every screen that fetches data has four possible states, not one: loading, loaded with data, loaded with nothing, and failed. Handling all four is part of the required scope. A screen that shows a blank white area while loading is not finished.

**Optimistic update** — Updating the UI immediately, before the server confirms, and rolling back if the request fails. Not required, but TanStack Query makes it straightforward.

## 2.4 Database concepts

**PostgreSQL** — The relational database used here. Runs in a Docker container, so you never install it on your machine.

**Primary key** — The column uniquely identifying each row, conventionally `Id`.

**Foreign key** — A column referencing the primary key of another table, e.g. `Tickets.AuthorId → Users.Id`. This is what enforces that a ticket cannot reference a user who does not exist.

**Navigation property** — In EF Core, the C# property that represents a foreign-key relationship as an object (`ticket.Author`) rather than as a raw id (`ticket.AuthorId`). You will typically have both.

**Index** — A structure that makes lookups on a column fast. If you filter or sort by a column frequently, it likely needs one.

**Transaction** — A group of operations that either all succeed or all fail together. Relevant when a single action writes to more than one table — for example, changing a ticket's status *and* appending a history row.

**Connection string** — The text specifying how to reach the database (host, port, database name, credentials). It must come from configuration or environment variables, never be hardcoded in source and never be committed to Git.

## 2.5 Tooling, Docker, and GitHub

**Git** — The version control system. Tracks your changes as a history of commits.

**Repository** — The project together with its full history.

**Commit** — One recorded change with a message describing it. Commit often, in small, coherent units, with messages that say what changed and why.

**Branch** — An independent line of work. You will not commit directly to `main`.

**Pull Request (PR)** — A proposal to merge one branch into another, opened on GitHub, reviewed before it is merged. This is where code review happens and is the single most valuable working habit you will take away from this internship.

**`.gitignore`** — A file listing paths Git must ignore: `bin/`, `obj/`, `node_modules/`, local settings, anything containing secrets.

**Docker** — A tool that runs software in **containers**: isolated processes bundled with their own filesystem and dependencies. Practical benefit here — you start PostgreSQL with one command, and if you corrupt it, you delete it and get a clean one in seconds.

**Image vs container** — An image is the packaged, immutable template. A container is a running instance of an image.

**Dockerfile** — The recipe for building an image from your source code.

**Multi-stage build** — A Dockerfile with a build stage and a runtime stage. You compile in the first, then copy only the compiled output into a small final image. This keeps the SDK and source code out of the shipped image, making it much smaller.

**Docker Compose** — A single `docker-compose.yml` file describing several containers and how they connect. `docker compose up` starts all of them together.

**Volume** — Persistent storage attached to a container. Without one, your database contents disappear when the container stops.

**Environment variable** — Configuration passed into a container from outside. This is how connection strings, passwords, and signing keys reach the application without ever being written into an image or committed to Git.

**Health check** — A way for Compose to know a container is actually ready, not merely started. Needed because your API will otherwise try to connect to PostgreSQL before PostgreSQL is accepting connections.

**xUnit** — The .NET testing framework used here.

**Unit test** — A test of one piece of logic in isolation, with no database and no HTTP. The status transition rules are the natural target.

**CI** (*Continuous Integration*) — Automatically building and testing the project on every push or pull request. GitHub Actions provides this and is listed as optional extra work.

---
---

# PART 3 — Required Scope

Everything in this part must be complete. Nothing from Part 5 counts until all of this is done.

## 3.1 Technology (fixed — do not substitute)

| Layer | Technology |
|---|---|
| Database | PostgreSQL, in a Docker container |
| Backend | .NET 10, ASP.NET Core Web API |
| Data access | EF Core, Code First, with migrations |
| Auth | ASP.NET Core Identity + JWT |
| Validation | FluentValidation |
| Frontend | React + TypeScript, built with Vite |
| Data fetching | TanStack Query |
| Forms | React Hook Form + Zod |
| Styling | Tailwind CSS **or** MUI — choose one |
| Tests | xUnit |
| Tooling | Git + GitHub, Docker, Docker Compose, Swagger |

**A note on development workflow:** for the first three weeks, run **only the database** in Docker, and run the backend and the frontend directly on your machine. Debugging and hot reload are far better that way. Containerising the backend and frontend is a Week 4 task and a required deliverable, but doing it on day one will slow you down for no benefit.

## 3.2 Solution structure

```
helpdesk/
├── Helpdesk.sln
├── src/
│   ├── Helpdesk.Api/               controllers, DTOs, middleware, configuration
│   ├── Helpdesk.Domain/            entities and enums (plain C#, no dependencies)
│   └── Helpdesk.Infrastructure/    DbContext, migrations, services
├── tests/
│   └── Helpdesk.Tests/
├── client/                         React + TypeScript
├── docker-compose.yml
├── docker-compose.dev.yml          database only, for local development
├── .gitignore
└── README.md
```

**Rule:** dependencies point in one direction only — `Api → Infrastructure → Domain`. `Domain` references nothing. This is not ceremony; it is what makes the business logic testable without a database or a web server.

## 3.3 Data model

| Table | Key fields | Relationships |
|---|---|---|
| **Users** | Id, Email, PasswordHash, FullName, Role | — |
| **Categories** | Id, Name | — |
| **Tickets** | Id, Title, Description, Status, Priority, CategoryId, AuthorId, AssignedAgentId, CreatedAt, UpdatedAt, ClosedAt | Author → Users, AssignedAgent → Users, Category → Categories |
| **Comments** | Id, TicketId, AuthorId, Text, CreatedAt | → Tickets, → Users |
| **TicketHistory** | Id, TicketId, ChangedById, FromStatus, ToStatus, ChangedAt | → Tickets, → Users |

`Status`, `Priority`, and `Role` are enums, not free-text strings.

Draw an **ERD** (entity relationship diagram) before writing any code — a diagram of tables, columns, and the foreign keys between them. Use draw.io or dbdiagram.io. Getting the data model wrong is the most expensive mistake available in this project: discovering in Week 3 that history needs its own table means rewriting both the backend and the frontend. On paper it costs fifteen minutes. Be ready to explain, out loud, why you separated what you separated.

## 3.4 Endpoints

```
POST   /api/auth/register
POST   /api/auth/login                    returns a JWT

GET    /api/tickets                       filtering, search, sorting, paging
GET    /api/tickets/{id}
POST   /api/tickets
PUT    /api/tickets/{id}
PATCH  /api/tickets/{id}/status           validates the transition
PATCH  /api/tickets/{id}/assign           agents only
GET    /api/tickets/{id}/history

GET    /api/tickets/{id}/comments
POST   /api/tickets/{id}/comments

GET    /api/categories
GET    /api/stats/summary                 ticket counts per status
```

`GET /api/tickets` must support: filter by status, priority and category; text search on the title; sort by creation date; and paging (`page`, `pageSize`), returning the total count alongside the page of results. All of it executed in SQL.

## 3.5 Screens

1. **Login / Register**
2. **Ticket list** — table, status/priority/category filters, title search, sorting, paging
3. **Ticket detail** — fields, comment thread, new-comment form, status change (agents), assignment (agents), change history
4. **New ticket** — form with validation and clear error messages
5. **Home** — ticket counts per status

The design must be clean and consistent. It does not need to be beautiful. It **does** need to handle loading, empty, and error states on every screen.

## 3.6 Rules that must hold on the server

Anything checked only in the browser is not checked at all — the frontend is fully under the user's control. Every one of these is enforced server-side:

- A `User` can read, update, and comment on **only their own** tickets.
- Only an `Agent` may change status or assign a ticket.
- Only transitions permitted by the state machine are accepted; anything else returns `400`.
- Every status change writes a row to `TicketHistory`, in the same transaction as the status update.
- All input is validated (required fields, lengths, valid enum values).
- Endpoints return correct status codes and a consistent error response shape.

## 3.7 Docker requirement

The finished project must start with **one command** on a clean machine:

```bash
docker compose up
```

This must bring up all three containers — PostgreSQL, the API, and the frontend — wired together and working. Specifically:

- A `Dockerfile` for the API, using a **multi-stage build** (SDK image to compile, runtime image to run).
- A `Dockerfile` for the frontend, using a multi-stage build (Node to produce the static bundle, then a small web server such as nginx to serve it).
- `docker-compose.yml` defining all three services, the network between them, and a **named volume** for PostgreSQL data.
- A **health check** on the database, with the API waiting for it, so the API does not start before PostgreSQL accepts connections.
- Database **migrations applied automatically** at API startup, and seed data inserted if the database is empty.
- **No secrets committed.** Connection strings, the JWT signing key, and passwords come from environment variables. Commit a `.env.example` with placeholder values and add `.env` to `.gitignore`.
- Nothing hardcoded to `localhost` in a way that breaks inside the container network — containers reach each other by service name, not by `localhost`.

## 3.8 GitHub requirement

The project lives in a GitHub repository from **day one**, not pushed in a single commit at the end.

- Work happens on **feature branches**, e.g. `feature/ticket-status-transitions`.
- Every branch is merged through a **pull request**, never pushed directly to `main`. `main` must be working at all times.
- Commits are small, frequent, and have meaningful messages. A single enormous commit on Friday afternoon is a problem, not a milestone.
- `.gitignore` covers `bin/`, `obj/`, `node_modules/`, `.env`, and local settings files. **No secrets, ever** — and note that removing a secret in a later commit does not remove it from history.
- The **README** is a required deliverable, not an afterthought. It must contain: what the project is, the technology used, prerequisites, how to run everything with Docker, how to run backend and frontend locally for development, how to run the tests, the seeded login credentials, and a short overview of the architecture. Anyone should be able to clone the repository and run the project using the README alone, without asking you anything.

---
---

# PART 4 — Week-by-Week Plan

## Week 1 — Foundations and one complete vertical slice

| Day | Task |
|---|---|
| 1 | Agree on scope. Draw the ERD. Set up the environment, create the GitHub repository, write `docker-compose.dev.yml` with PostgreSQL |
| 2 | Create the solution skeleton, entities, `DbContext`, first migration, seed data |
| 3 | `TicketsController` with `GET` (list) and `POST` (create). DTOs. Verify through Swagger |
| 4 | Create the React project, set up routing, build the ticket list page using TanStack Query |
| 5 | New-ticket form wired to the API. Clean up. Open a pull request, get it reviewed |

**Definition of success for the week:** you create a ticket in the browser, it appears in the list, and it survives a page refresh — because it is genuinely in the database.

**A "vertical slice" means one feature that cuts through every layer** — database, API, browser. No authentication, no styling, no validation, no filtering. Just one path that goes all the way through.

This ordering is deliberate. The instinctive approach is to spend the first week building foundations — repositories, services, abstractions — and end the week with nothing that runs. Building a slice instead makes you hit every boundary between the three parts of the system immediately: CORS, serialisation, why C# `PascalCase` arrives in JavaScript as `camelCase`, how to run the backend and frontend at the same time. Far better that those problems land on Monday of Week 1 than on Wednesday of Week 3.

## Week 2 — Authentication and business logic

| Day | Task |
|---|---|
| 1 | ASP.NET Core Identity, register and login endpoints, JWT issuing |
| 2 | Protect endpoints, add roles, scope query results by role (users see only their own tickets) |
| 3 | Frontend: login page, token storage, protected routes, attaching the token to requests |
| 4 | Status state machine, transition validation, history records, assignment |
| 5 | Comments — backend and frontend. Pull request and review |

**This is the hardest week of the four.** If you fall behind anywhere, it will be here. That is expected and normal — say so at standup rather than quietly slipping.

## Week 3 — Completing and hardening

| Day | Task |
|---|---|
| 1 | Server-side filtering, search, sorting, and paging |
| 2 | Frontend controls for filters, sorting, and paging |
| 3 | Global exception handling, consistent error responses, loading / empty / error states on every screen |
| 4 | Home page with counts. Consistent styling pass across all screens |
| 5 | Unit tests for the status transition rules (5–10 tests). Pull request and review |

## Week 4 — Containerisation and delivery

| Day | Task |
|---|---|
| 1 | Dockerfiles for API and frontend, multi-stage builds |
| 2 | Full `docker-compose.yml`, health check, automatic migrations, environment variables, `.env.example` |
| 3 | Write the README. Then **clone the repository into a fresh folder and follow your own README** — fix everything that does not work |
| 4–5 | **Reserve.** Address outstanding review comments. If genuinely finished, pick something from Part 5. Short demo and walkthrough |

The last two days are deliberately unallocated. They will be used.

---
---

# PART 5 — Optional Extra Work

Only after everything in Part 3 is complete and merged. Ordered by learning value relative to effort. Take them one at a time and finish each before starting the next.

**1. File attachments on tickets** *(~1 day)*
Upload, store, download, with size and content-type limits. Your first encounter with `multipart/form-data`, which behaves differently from JSON.

**2. Email notification on status change** *(~half a day)*
Run MailHog in a container instead of a real SMTP server, so you can inspect sent mail in a browser. Teaches you to treat a side effect as something that must not be able to fail the main operation.

**3. Real-time updates with SignalR** *(~1.5 days)*
When an agent changes a status, the user's ticket list updates by itself with no refresh. A genuinely new concept: a persistent two-way connection instead of request-response. The most interesting item on this list.

**4. Background job** *(~half a day)*
A recurring task that automatically closes tickets resolved more than seven days ago, using `BackgroundService` or Hangfire.

**5. Export the ticket list to CSV or Excel** *(~half a day)*
Generate the file server-side and download it from the browser, respecting the currently applied filters.

**6. Integration tests** *(~1 day)*
`WebApplicationFactory` plus Testcontainers: tests that spin up a real PostgreSQL container and call the real API over HTTP. Shows concretely how an integration test differs from a unit test, and why you want both.

**7. GitHub Actions CI** *(~half a day)*
On every pull request, automatically build the solution, run the tests, and build the Docker images. A red check on a PR is much cheaper than a broken `main`.

**8. Switch the database provider to SQL Server** *(~half a day)*
Change the EF Core provider and connection string, regenerate migrations, and find out what breaks. An excellent lesson in what an abstraction actually covers versus what it only appears to cover.

**9. Audit log via an EF Core interceptor** *(~1 day)*
Record all changes automatically instead of calling logging code by hand at each site. The most advanced item here — only if you are well ahead.

---
---

# PART 6 — Deliverables, Working Practices, and Review

## 6.1 Deliverables at the end of the internship

1. A GitHub repository with a clean commit history and pull requests, `main` in working order.
2. `docker compose up` starts the whole system on a clean machine.
3. A README complete enough that no verbal explanation is needed.
4. Unit tests covering the status transition rules.
5. A short demo — walk through the application and explain a few of your design decisions.

## 6.2 Definition of done for any task

- Merged through a **pull request**, not pushed straight to `main`.
- No hardcoded connection strings, secrets, keys, or URLs.
- No commented-out dead code, no leftover `Console.WriteLine` or `console.log`.
- Correct HTTP status codes and comprehensible error messages.
- Validation and permission checks enforced **on the server**, not only in the browser.
- Loading, empty, and error states handled on every screen that fetches data.
- The relevant part of the README updated.

## 6.3 Working practices

- **Daily standup, 10 minutes:** what you did yesterday, what you are doing today, what is blocking you.
- **If you are stuck for more than 60 minutes, ask.** This is a rule, not an offer. Struggling silently for half a day is the most common and most costly mistake interns make, and nobody will think less of you for asking.
- **Code review once a week**, on Fridays, in detail. Come prepared to explain your choices; review is a conversation, not a verdict.
- **From Week 2 you write your own task cards** (Trello, Jira, GitHub Issues — your choice) and estimate how long each will take. Then compare your estimates with reality. Estimation is a skill nobody practises at university, and the gap is the point of the exercise.
- **Commit daily.** Your history should show how the project grew.

## 6.4 Mistakes to watch for in your own code

These are the specific things that will come up in review:

- **Loading a whole table and filtering in memory.** Check the generated SQL: filtering, sorting, and paging must all be in the query.
- **N+1 queries.** Turn on SQL logging in development and read it occasionally.
- **Business logic in the controller** instead of the service layer.
- **Checks only on the frontend.** Repeat every validation and every permission check on the server.
- **Returning entities directly as JSON** instead of DTOs.
- **Over-engineering.** A repository wrapping a repository, an interface for every class, abstractions with exactly one implementation and no plausible second one. Simple, direct code is the goal; you are not building for imagined future requirements.
- **Secrets in the repository.** Check before every commit, because Git history remembers.

## 6.5 On scope, honestly

The required scope is intentionally sized so that it fits in three weeks, leaving the fourth for containerisation, documentation, and slack. Most people finish somewhat less than they plan, so if you find yourself running behind, that is information, not failure — raise it early so scope can be adjusted deliberately rather than by running out of time.

**A narrower scope finished properly is worth far more than a broad one left half-done.** Deliver something that runs end to end, is documented, and can be started with one command.

## 6.6 Learning resources

- **Microsoft Learn** — *Create a web API with ASP.NET Core* tutorial
- **Microsoft Learn** — EF Core documentation, particularly the sections on migrations and on loading related data
- **react.dev** — the official React tutorial and the "Thinking in React" guide
- **TanStack Query docs** — the Quick Start and the section on mutations and cache invalidation
- **typescriptlang.org** — the TypeScript Handbook
- **Docker docs** — *Multi-stage builds* and the Compose file reference
