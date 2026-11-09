# 🏃 RaceDay — Portfolio of Evidence (PROG6212)

> **Full-stack South African road-event management platform** for Comrades Marathon, Cape Town Cycle Tour, Soweto Marathon, park runs, community walks & charity cycles.  
> **Student:** Lesedi Mogatwe  
> **Student Number:** ST10476247  
> **Repository:** [https://github.com/ST10476247/PROG6212_POE](https://github.com/ST10476247/PROG6212_POE)

[![RaceDay Part 3 - MVC + API + Docker CI](https://github.com/ST10476247/PROG6212_POE/actions/workflows/part3-ci.yml/badge.svg)](https://github.com/ST10476247/PROG6212_POE/actions/workflows/part3-ci.yml)
[![RaceDay Part 2 - RESTful API & Unit Tests CI](https://github.com/ST10476247/PROG6212_POE/actions/workflows/part2-ci.yml/badge.svg)](https://github.com/ST10476247/PROG6212_POE/actions/workflows/part2-ci.yml)
![Tests](https://img.shields.io/badge/Unit%20Tests-30%20Passed%20(100%25)-brightgreen?style=flat-square)
![Framework](https://img.shields.io/badge/.NET-8.0%20LTS-purple?style=flat-square)
![Docker](https://img.shields.io/badge/Docker-Containerised-2496ED?style=flat-square&logo=docker&logoColor=white)
![MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC%20Web%20App-512BD4?style=flat-square)
![Azure](https://img.shields.io/badge/Azure-Blob%20Storage-0078D4?style=flat-square&logo=microsoft-azure)
![ORM](https://img.shields.io/badge/Entity%20Framework%20Core-Code--First-blue?style=flat-square)
![Security](https://img.shields.io/badge/Auth-Session%20%2B%20JWT%20Bearer-green?style=flat-square)
![Documentation](https://img.shields.io/badge/API%20Docs-Swagger%20%2F%20OpenAPI-orange?style=flat-square)

---

## 🧭 POE Progressive Roadmap

- [x] **Part 1 — System Planning, ERD & Database Design** (Completed & Submitted)
- [x] **Part 2 — RESTful API Development & Unit Testing** (Completed & Submitted)
- [x] **Part 3 — MVC Web Application, Azure Blob Storage & Docker Containerisation** 

---

## 📦 Part 2 — Deliverables & Solution Architecture

Part 2 implements the robust, production-ready RESTful Web API and comprehensive unit testing suite powering the RaceDay platform. Built strictly according to the approved Part 1 API Endpoint Plan, ERD, and SQL schema.

```
RaceDay/
├── RaceDay.sln                              ← Visual Studio Classic Solution
├── RaceDay.slnx                             ← Visual Studio 2026 / Modern Solution
├── .github/
│   └── workflows/
│       ├── part1-validation.yml             ← Part 1 docs validation
│       └── part2-ci.yml                     ← Part 2 CI: build & 30 unit tests
├── docs/                                    ← Part 1 Planning Deliverables
│   ├── RaceDay_ERD.png                      ← ERD diagram render
│   ├── RaceDay_ERD.pdf                      ← ERD PDF submission document
│   ├── API_Endpoint_Plan.md                 ← Master 30+ endpoint plan
│   └── RaceDay_DB_Script.sql                ← SSMS T-SQL schema script
├── RaceDay.API/                             ← ASP.NET Core Web API Project
│   ├── Common/                              ← Session extensions & user context
│   │   └── SessionExtensions.cs
│   ├── Controllers/                         ← RESTful API Controllers
│   │   ├── AuthController.cs                ← Register, login, session, logout
│   │   ├── ProfileController.cs             ← View/edit profile, change password
│   │   ├── EventsController.cs              ← Events CRUD, categories, weather, route
│   │   ├── CategoriesController.cs          ← Distance & age categories CRUD
│   │   ├── EnrolmentsController.cs          ← Race enrolment & bib allocation
│   │   ├── ResultsController.cs             ← Timing mat capture & leaderboards
│   │   ├── OrganisersController.cs          ← Organiser dashboard utilities
│   │   └── ParticipantsController.cs        ← Athlete history milestones
│   ├── Data/                                ← EF Core Code-First Layer
│   │   ├── RaceDayDbContext.cs              ← 8 Entities, relations, constraints
│   │   └── DbInitializer.cs                 ← Realistic SA seed data & BCrypt hashing
│   ├── Filters/                             ← Role-Based Access Filters
│   │   ├── RequireAuthAttribute.cs          ← Gated session/JWT check (401)
│   │   └── RequireRoleAttribute.cs          ← Strict role separation (403)
│   ├── Models/
│   │   ├── Entities/                        ← Domain models (8 core entities)
│   │   │   ├── User.cs
│   │   │   ├── Organiser.cs
│   │   │   ├── Participant.cs
│   │   │   ├── Event.cs
│   │   │   ├── EventCategory.cs
│   │   │   ├── EventEnrolment.cs
│   │   │   ├── Result.cs
│   │   │   └── WeatherSnapshot.cs
│   │   └── DTOs/                            ← Request & Response Data Transfer Objects
│   │       ├── AuthDtos.cs
│   │       ├── ProfileDtos.cs
│   │       ├── EventDtos.cs
│   │       ├── CategoryDtos.cs
│   │       ├── EnrolmentDtos.cs
│   │       ├── ResultDtos.cs
│   │       └── WeatherDtos.cs
│   ├── Services/                            ← Business Logic Services
│   │   ├── IPasswordHasher.cs               ← BCrypt password hashing
│   │   ├── PasswordHasher.cs
│   │   ├── ITokenService.cs                 ← Signed JWT token generator
│   │   └── TokenService.cs
│   ├── appsettings.json                     ← SQL Server / LocalDB & JWT settings
│   └── Program.cs                           ← Middleware, Session, Swagger, EF Core
└── RaceDay.Tests/                           ← xUnit Test Suite (30 Unit Tests)
    ├── Helpers/                             ← In-Memory DbContext & Controller helpers
    │   ├── TestDbContextFactory.cs
    │   └── TestControllerHelper.cs
    ├── AuthTests.cs                         ← Registration, login & hash tests
    ├── EventManagementTests.cs              ← Event CRUD & conflict rules
    ├── RoleSecurityTests.cs                 ← 401 Unauthorized & 403 Forbidden tests
    ├── EnrolmentTests.cs                    ← Bib allocation, capacity, duplicate tests
    └── ResultTests.cs                       ← Timing capture, batch import & ranks
```

---

## 👥 Two User Roles & Strict Access Control

RaceDay strictly separates capabilities between two core roles across all endpoints:

| Feature / Domain | 🟦 Organiser Role | 🟩 Participant Role | Public (Unauthenticated) |
|---|:---:|:---:|:---:|
| **Browse / Search Events** | ✅ | ✅ | ✅ |
| **View Event Categories & Route Profile** | ✅ | ✅ | ✅ |
| **View Live Weather & Hydration Advice** | ✅ | ✅ | ✅ |
| **View Official Leaderboards & Certificates**| ✅ | ✅ | ✅ |
| **Register / Log In Account** | ✅ | ✅ | ✅ |
| **Edit Own User Profile & Rotate Password** | ✅ (Own) | ✅ (Own) | ❌ (401) |
| **Create, Update & Delete Events** | ✅ | ❌ (403 Forbidden) | ❌ (401) |
| **Create & Configure Distance Categories** | ✅ | ❌ (403 Forbidden) | ❌ (401) |
| **View Full Event Enrolment Roster** | ✅ (Own Events) | ❌ (403 Forbidden) | ❌ (401) |
| **Enrol into Category & Receive Bib** | ❌ (403 Forbidden) | ✅ | ❌ (401) |
| **View Personal Race Entry Tickets** | ❌ (403 Forbidden) | ✅ (Own Entries) | ❌ (401) |
| **Capture Gun / Chip Timing Results** | ✅ | ❌ (403 Forbidden) | ❌ (401) |
| **Batch Import Timing Mat CSV Results** | ✅ | ❌ (403 Forbidden) | ❌ (401) |

### 🔐 Authentication & Session Management
- **Password Security:** Passwords are never stored in plain text. Hashed using **BCrypt** (`workFactor: 11`) with cryptographic salt.
- **Server-Side Session:** Upon login (`POST /api/auth/login`), `HttpContext.Session` stores the user's `UserId`, `Role`, `Email`, and `OrganiserId` / `ParticipantId` to maintain state across subsequent requests.
- **Dual Authentication:** Supports both ASP.NET Core Session cookies (`.RaceDay.Session`) and signed JWT Bearer Tokens (`Authorization: Bearer <token>`).
- **Role Enforcement:** Handled via custom action filters `[RequireRole("Organiser")]`, `[RequireRole("Participant")]`, and `[RequireAuth]`.

---

## 🌐 Implemented RESTful Endpoints (30+ Endpoints)

All endpoints match the Part 1 approved plan and are testable directly through Swagger:

### 1. Authentication (`/api/auth`)
- `POST /api/auth/register/participant` — Registers athlete, hashes password, starts session, returns token.
- `POST /api/auth/register/organiser` — Registers event organiser/club director, starts session.
- `POST /api/auth/login` — Authenticates credentials, establishes server session, returns JWT.
- `POST /api/auth/logout` — Terminates active session.
- `GET /api/auth/session` — Inspects active session state and role claims.
- `POST /api/auth/refresh` — Refreshes authentication token.

### 2. User Profile (`/api/profile`)
- `GET /api/profile/me` — Retrieves authenticated caller's profile with role-specific details.
- `PUT /api/profile/me` — Updates editable profile demographics and emergency contacts.
- `PUT /api/profile/change-password` — Validates current password and securely hashes new password.

### 3. Events (`/api/events`)
- `GET /api/events` — Paged event listings with filters: `?city=&province=&eventType=&status=&search=&page=1&pageSize=20`.
- `GET /api/events/{id}` — Full event details, categories, entrant count, and weather forecast.
- `GET /api/events/featured` — Featured national events (Comrades, Cape Town Cycle Tour, Soweto Marathon).
- `GET /api/events/search` — Keyword and province event discovery.
- `POST /api/events` — Creates new road event (`[RequireRole("Organiser")]`).
- `PUT /api/events/{id}` — Updates existing event (`[RequireRole("Organiser")]` - owner only).
- `DELETE /api/events/{id}` — Deletes event (`[RequireRole("Organiser")]` - returns 409 Conflict if active paid enrolments exist).
- `GET /api/events/{id}/categories` — Lists distance/age categories under the event.
- `POST /api/events/{id}/categories` — Adds race category to event (`[RequireRole("Organiser")]`).
- `GET /api/events/{id}/enrolments` — Event participant roster with bib allocations (`[RequireRole("Organiser")]`).
- `GET /api/events/{id}/results` — Public leaderboard with gun & chip times.
- `POST /api/events/{id}/results/batch` — Bulk import of timing mat results (`[RequireRole("Organiser")]`).
- `GET /api/events/{id}/weather` — Race morning temperature, wind, and hydration advice.
- `POST /api/events/{id}/weather` — Records weather snapshot (`[RequireRole("Organiser")]`).
- `GET /api/events/{id}/route` — Course route elevation gain, terrain, and landmarks.

### 4. Categories (`/api/categories`)
- `GET /api/categories/{id}` — Single category capacity and details.
- `PUT /api/categories/{id}` — Updates entry fee, distance, cutoff (`[RequireRole("Organiser")]`).
- `DELETE /api/categories/{id}` — Removes category (`[RequireRole("Organiser")]` - 409 if athletes enrolled).

### 5. Event Enrolments (`/api/enrolments`)
- `POST /api/enrolments` — Participant enters category, allocates bib and payment reference (`[RequireRole("Participant")]`).
- `POST /api/events/{eventId}/categories/{categoryId}/enrol` — Hierarchical enrolment route alias.
- `GET /api/enrolments/my` (and `/api/enrolments/mine`) — Calling athlete's personal race tickets.
- `GET /api/enrolments/{id}` — Single ticket receipt (own entry or event organiser).
- `PUT /api/enrolments/{id}` — Adjusts bib/payment status (Organiser) or requests withdrawal (Participant).
- `DELETE /api/enrolments/{id}` — Removes pending enrolment (`[RequireRole("Organiser")]`).

### 6. Timing Results (`/api/results`)
- `GET /api/results/{id}` — Individual finisher certificate and ranking breakdown.
- `GET /api/results/my` — Athlete personal race history across all entered events.
- `POST /api/results` — Records finish time, chip time, rank (`[RequireRole("Organiser")]`).
- `PUT /api/results/{id}` — Edits timing record or adjusts DNF/DNS status (`[RequireRole("Organiser")]`).
- `DELETE /api/results/{id}` — Deletes timing record (`[RequireRole("Organiser")]`).
- `GET /api/enrolments/{enrolmentId}/result` — Finisher certificate by enrolment ID.

### 7. Organiser & Participant Utilities
- `GET /api/organisers/{organiserId}/events` — Organiser dashboard event overview.
- `GET /api/participants/{participantId}/history` — Participant total km and finish milestones.

---

## 📑 Swagger / OpenAPI Integration

The API integrates **Swagger UI** with full XML documentation annotations for interactive in-browser testing:

- **Swagger UI URL:** `http://localhost:5000/swagger` (or `https://localhost:5001/swagger`)
- **Direct Root Redirect:** Navigating to `http://localhost:5000/` automatically redirects to `/swagger`.
- **Authorisation in Swagger:** Click the **Authorize** button in Swagger UI and enter `Bearer <your_jwt_token>` obtained from `POST /api/auth/login`.
- **Rich Documentation:** Every endpoint includes summaries, parameter descriptions, expected input DTOs, and HTTP response codes (`200`, `201`, `400`, `401`, `403`, `404`, `409`).

---

## 🧪 Unit Testing Suite (30 Tests — 100% Passing)

Comprehensive test coverage built with **xUnit**, **FluentAssertions**, and **EF Core InMemory Database**:

```
Test run for RaceDay.Tests.dll (.NETCoreApp,Version=v8.0)
Total tests: 30
     Passed: 30
     Failed: 0
 Total time: 9.0110 Seconds
```

### Tested Test Areas:
1. **User Registration & Login (`AuthTests.cs`)**:
   - `RegisterParticipant_WithValidData_Returns201AndToken` ✅
   - `RegisterParticipant_WithDuplicateEmail_Returns409Conflict` ✅
   - `RegisterOrganiser_WithValidData_Returns201AndOrganiserId` ✅
   - `Login_WithCorrectCredentials_Returns200AndValidToken` ✅
   - `Login_WithIncorrectPassword_Returns401Unauthorized` ✅
   - `Login_WithNonExistentEmail_Returns401Unauthorized` ✅
   - `PasswordHasher_HashesSecurelyAndVerifiesAccurately` ✅
2. **Event Management (`EventManagementTests.cs`)**:
   - `GetEvents_PublicAccess_ReturnsOkAndPagedEvents` ✅
   - `GetEventById_ExistingId_ReturnsFullEventWithCategories` ✅
   - `GetEventById_NonExistingId_Returns404NotFound` ✅
   - `CreateEvent_WhenOrganiser_Returns201Created` ✅
   - `UpdateEvent_WhenOwnerOrganiser_Returns200Ok` ✅
   - `DeleteEvent_WhenNoPaidEntries_Returns204NoContent` ✅
   - `DeleteEvent_WhenActivePaidEntriesExist_Returns409Conflict` ✅
3. **Role-Based Access & Security (`RoleSecurityTests.cs`)**:
   - `RequireAuthAttribute_WhenUnauthenticated_Returns401Unauthorized` ✅
   - `RequireRoleAttribute_WhenWrongRole_Returns403Forbidden` ✅
   - `RequireRoleAttribute_WhenUnauthenticated_Returns401Unauthorized` ✅
   - `RequireRoleAttribute_WhenParticipantRoleRequired_RejectsOrganiserWith403` ✅
   - `RequireRoleAttribute_WhenCorrectRoleProvided_AllowsAccess` ✅
   - `UpdateEvent_WhenNonOwnerOrganiser_Returns403Forbidden` ✅
   - `GetEventEnrolments_WhenNonOwnerOrganiser_Returns403Forbidden` ✅
4. **Event Enrolments & Bibs (`EnrolmentTests.cs`)**:
   - `EnrolParticipant_WithValidCategory_AllocatesBibAndPaymentReference` ✅
   - `EnrolParticipant_WhenAlreadyEnrolled_Returns409Conflict` ✅
   - `EnrolParticipant_WhenCategoryDoesNotExist_Returns404NotFound` ✅
   - `GetMyEnrolments_ReturnsParticipantEnrolmentHistory` ✅
   - `GetEnrolmentById_WhenAccessingOtherParticipantTicket_Returns403Forbidden` ✅
5. **Results & Timing Mat (`ResultTests.cs`)**:
   - `CaptureResult_WhenOrganiser_RecordsFinishTimesAndRankings` ✅
   - `CaptureResult_WhenDuplicateForSameEnrolment_Returns409Conflict` ✅
   - `BatchImportResults_WithValidBibs_UpsertsSuccessfully` ✅
   - `GetMyResults_ReturnsOnlyParticipantFinishes` ✅

---

## 🟢 CI / CD Green Build — GitHub Actions

The repository is configured with an automated CI/CD pipeline at [`.github/workflows/part2-ci.yml`](.github/workflows/part2-ci.yml). On every push to `main` or `part2`:
1. Checks out repository source code.
2. Configures .NET 8.0 SDK environment.
3. Restores all NuGet packages.
4. Compiles `RaceDay.sln` in Release mode with 0 warnings/errors.
5. Executes all 30 xUnit tests with detailed TRX reporting.

### CI/CD Green Build Screenshot
![RaceDay Part 2 CI Green Build Screenshot](./docs/screenshots/ci-green-build-part2.png)

```text
✓ Checkout Source Code
✓ Setup .NET SDK (8.0.x)
✓ Verify Solution & Projects
✓ Restore NuGet Dependencies
✓ Build Solution (Release) - 0 Errors, 0 Warnings
✓ Execute Unit Tests (xUnit) - 30 Passed, 0 Failed
✓ Upload Test Results Artifact
Status: ALL CHECKS PASSED (GREEN BUILD)
```

---

## 🎬 YouTube Video Presentation — Part 2

> 🎞️ **Unlisted YouTube Video Link:**  
> `https://youtu.be/REPLACE_WITH_YOUR_UNLISTED_VIDEO_ID`

---

## 🚀 How to Run Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or LocalDB / SSMS)

### Step 1: Clone Repository
```powershell
git clone https://github.com/ST10476247/PROG6212_POE.git
cd PROG6212_POE
```

### Step 2: Run Unit Tests
```powershell
dotnet test RaceDay.sln --verbosity normal
```

### Step 3: Run the API
```powershell
dotnet run --project RaceDay.API/RaceDay.API.csproj
```

### Step 4: Open Swagger UI
Open your browser and navigate to:
```
http://localhost:5000/swagger
```
or
```
https://localhost:5001/swagger
```

### Default Seed Credentials for Testing

| Role | Email | Password | Details |
|---|---|---|---|
| **Organiser** | `tebogo@soulcityevents.co.za` | `Password123!` | Soul City Events (Comrades & Soweto Marathon) |
| **Organiser** | `anika@ctcycletours.co.za` | `Password123!` | Cape Town Cycling Tours |
| **Participant** | `sibusiso.ndlovu@gmail.com` | `Password123!` | Hollywoodbets AC Athlete |
| **Participant** | `leighanne.botes@outlook.com` | `Password123!` | Nedbank Running Club Athlete |

---

## 🧑‍💼 Academic Integrity / AI Disclosure

- **Author:** Lesedi Mogatwe (ST10476247)
- **Module:** PROG6212 — Portfolio of Evidence (Part 2)
- Generative AI coding assistance was utilized as a pair-programming tool to format boilerplate syntax, structure XML comments, and draft unit test scaffolding. All architectural decisions, entity schemas, business rules, role authorization boundaries, and endpoint logic were verified, tested, and validated by the student.

---

## 🌐 Part 3 — MVC Web Application, Azure Blob Storage & Docker

Part 3 wraps the Part 2 REST API in a polished, athlete-branded ASP.NET Core MVC web application and containerises the entire platform with Docker.

### Architecture Overview

```
Browser ──► RaceDay.Web (MVC, port 8080)
                │
                │  HTTP (IRaceDayApiClient — typed HttpClient)
                ▼
           RaceDay.API (REST, port 5000)
                │
                ├── SQL Server / EF Core In-Memory Database
                └── Azure Blob Storage (event banners & profile pictures)
```

> **Zero DB access in MVC** — `RaceDay.Web` contains no DbContext, no Entity Framework, and no business logic. Every data operation is delegated to the API via `IRaceDayApiClient`.

### Part 3 Solution Structure

```
RaceDay/
├── RaceDay.Web/                             ← ASP.NET Core MVC Project (Part 3)
│   ├── Common/
│   │   ├── SessionKeys.cs                   ← Session key constants
│   │   └── UserSessionExtensions.cs         ← SetUser / GetCurrentUser / IsLoggedIn
│   ├── Controllers/
│   │   ├── HomeController.cs                ← Public home / events listing
│   │   ├── AccountController.cs             ← Login, Register, Logout
│   │   ├── EventsController.cs              ← Public event details & enrolment flow
│   │   ├── OrganiserController.cs           ← Full organiser dashboard (Organiser role)
│   │   └── ParticipantController.cs         ← Athlete dashboard (Participant role)
│   ├── Filters/
│   │   └── AuthorizeRoleAttribute.cs        ← Session-based role guard (IActionFilter)
│   ├── Models/ViewModels/
│   │   ├── AuthViewModels.cs
│   │   ├── EventViewModels.cs
│   │   ├── CategoryViewModels.cs
│   │   ├── EnrolmentViewModels.cs
│   │   ├── ResultViewModels.cs
│   │   ├── OrganiserDashboardViewModel.cs
│   │   └── ProfileViewModels.cs
│   ├── Services/
│   │   ├── IRaceDayApiClient.cs             ← Full typed HTTP client interface
│   │   └── RaceDayApiClient.cs              ← Implementation (all API endpoints)
│   ├── Views/
│   │   ├── Account/     Login, Register, AccessDenied
│   │   ├── Events/      Index (directory), Details (enrol CTA)
│   │   ├── Home/        Hero landing page
│   │   ├── Organiser/   Dashboard, Events CRUD, Categories, Enrolments, Results
│   │   ├── Participant/ Home, Enrol, MyEvents, MyResults, Profile
│   │   └── Shared/      _Layout, Error
│   ├── wwwroot/
│   │   ├── css/site.css                     ← Full athletic RaceDay brand theme
│   │   └── js/site.js                       ← Alert dismiss, auto-rank, demo creds
│   ├── appsettings.json                     ← ApiSettings:BaseUrl
│   ├── Program.cs                           ← Session, cookie auth, typed HttpClient
│   └── Dockerfile                           ← Multi-stage Docker build
├── RaceDay.API/
│   ├── Services/
│   │   ├── IBlobStorageService.cs           ← Azure Blob interface
│   │   └── AzureBlobStorageService.cs       ← Azure + local fallback implementation
│   ├── Controllers/
│   │   └── UploadController.cs              ← /api/upload/banner, /api/upload/avatar
│   └── Dockerfile                           ← Multi-stage Docker build
├── docker-compose.yml                       ← Orchestrates API + Web containers
├── .dockerignore                            ← Excludes bin/obj/secrets from images
└── .github/workflows/
    └── part3-ci.yml                         ← CI: Build API + MVC + Docker lint
```

### Role-Aware UI Features

| Feature | 🟦 Organiser | 🟩 Participant | 🌐 Public |
|---|:---:|:---:|:---:|
| Browse & filter upcoming events | ✅ | ✅ | ✅ |
| View event detail & categories | ✅ | ✅ | ✅ |
| Register / Log in | ✅ | ✅ | ✅ |
| **Organiser Dashboard** (event summary, enrolment counts, upcoming dates) | ✅ | ❌ | ❌ |
| **Create / Edit / Delete events** (with banner image upload) | ✅ | ❌ | ❌ |
| **Manage event categories** (age & distance groups) | ✅ | ❌ | ❌ |
| **View enrolment roster** per event (name, category, status) | ✅ | ❌ | ❌ |
| **Capture finish times & positions** after event concludes | ✅ | ❌ | ❌ |
| **Enrol into an event** and select category | ❌ | ✅ | ❌ |
| **My Events** — enrolled races with coloured status badges | ❌ | ✅ | ❌ |
| **My Results** — race history with finishing position (e.g. 47th / 312) | ❌ | ✅ | ❌ |
| **Profile** — update personal info & upload profile picture | ❌ | ✅ | ❌ |

### Azure Blob Storage Integration

- **Event Banners:** Organisers upload banner images via `POST /api/upload/banner` on the API. The MVC app calls the API endpoint (never Azure directly).
- **Profile Pictures:** Participants upload avatar photos via `POST /api/upload/avatar` on the API.
- **Local Fallback:** When `AzureBlobStorage:ConnectionString` is `UseDevelopmentStorage=true`, files are stored in `RaceDay.API/wwwroot/uploads/` for offline grading.
- **Displayed On:** Event detail pages show the uploaded banner; Profile page shows the participant's avatar.

---

## 🐳 Docker — Run with a Single Command

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

### Option A — Run Both API + Web Together (Recommended)

```bash
docker compose up --build
```

Then open:
- **MVC Web App:** http://localhost:8080
- **API Swagger:** http://localhost:5000/swagger

To stop:
```bash
docker compose down
```

### Option B — Run Only the MVC Web App

> The API must be running separately (see "Run the API" below) on port 5000.

```bash
docker build -f RaceDay.Web/Dockerfile -t raceday-web .
docker run -d -p 8080:80 -e ApiSettings__BaseUrl=http://host.docker.internal:5000 --name raceday-web raceday-web
```

Then open http://localhost:8080

### Option C — Run Only the API

```bash
docker build -f RaceDay.API/Dockerfile -t raceday-api .
docker run -d -p 5000:80 -e USE_INMEMORY_DB=true --name raceday-api raceday-api
```

Then open http://localhost:5000/swagger

### Environment Variables

| Variable | Service | Default | Description |
|---|---|---|---|
| `ApiSettings__BaseUrl` | Web | `http://localhost:5000` | URL of the API (use container name inside compose) |
| `USE_INMEMORY_DB` | API | `false` | Use EF Core InMemory DB (no SQL Server needed) |
| `AzureBlobStorage__ConnectionString` | API | `UseDevelopmentStorage=true` | Azure Blob or local fallback |
| `AzureBlobStorage__ContainerName` | API | `raceday-images` | Blob container name |

---

## 🎬 YouTube Video Presentation — Part 3

> 🎞️ **Unlisted YouTube Video Link:**
> `https://youtu.be/REPLACE_WITH_YOUR_PART3_VIDEO_ID`

---

## 🧑‍💼 Academic Integrity / AI Disclosure

- **Author:** Lesedi Mogatwe (ST10476247)
- **Module:** PROG6212 — Portfolio of Evidence (Parts 1, 2 & 3)
- Generative AI coding assistance was utilized as a pair-programming tool to format boilerplate syntax, structure XML comments, draft unit test scaffolding, and generate Razor view markup. All architectural decisions, entity schemas, business rules, role authorization boundaries, endpoint logic, Docker configuration, and Azure Blob integration were verified, tested, and validated by the student.
