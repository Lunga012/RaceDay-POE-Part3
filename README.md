# RaceDay - Full-Stack Event Management Platform
## PROG6212 Portfolio of Evidence (PoE) — Part 3: Web Front-End, Azure Blob Storage & Docker Containerisation

---

### Student & Submission Information
- **Module Name:** Programming 2B (PROG6212 / PROG6212/w)
- **Student Name:** Lunga Njabulo Khumalo
- **Student Number:** ST10483560
- **GitHub Repository:** [https://github.com/Lunga012/RaceDay-POE-Part1.git](https://github.com/Lunga012/RaceDay-POE-Part1.git)
- **Video Presentation Link (YouTube):** [https://youtu.be/8a6NFXWOK9M](https://youtu.be/8a6NFXWOK9M) *(Unlisted YouTube Walkthrough)*

---

## 1. Project Overview & Background
South Africa has a rich road events culture, from the iconic **Comrades Marathon** between Pietermaritzburg and Durban, to the **Cape Town Cycle Tour**, the **Soweto Marathon**, the **Two Oceans Marathon**, and hundreds of community walks, park runs, and charity cycling events held in towns and cities across the country every weekend. Despite enormous participation, many events are still managed through paper registration, disconnected spreadsheets, and cumbersome manual processes.

**RaceDay** is a comprehensive, production-ready, full-stack event management platform built across three progressive parts:
- **Part 1:** System Planning, Entity Relationship Diagram (ERD), full RESTful API Specification Table, and SSMS Relational Database Script (`/docs`).
- **Part 2:** Secure RESTful Backend Web API built with **ASP.NET Core Web API (C#)**, **Entity Framework Core**, **SQL Server**, session token authentication, role authorization, Swagger UI, and unit test suite.
- **Part 3:** Responsive, role-aware **ASP.NET Core MVC** Web Application front-end that consumes the REST API without direct database access, integrates **Azure Blob Storage** for event banners and profile image uploads, and containerises the application with **Docker**.

---

## 2. User Roles & Distinct Navigation Architecture

The platform enforces strict role separation across both the REST API and the MVC front-end:

| Feature / Page | Organiser | Participant | Guest / Unauthenticated |
| :--- | :---: | :---: | :---: |
| **Browse Upcoming Events** | Yes | Yes (with search & sport filter) | Yes |
| **View Event Details & Categories** | Yes | Yes | Yes |
| **User Registration / Login** | Yes | Yes | Allowed |
| **Organiser Dashboard** | Yes (Summary metrics, enrolments count) | No (Redirects to Access Denied) | Redirects to Login |
| **Create / Edit / Delete Events** | Yes (with Banner Upload via API) | No | Redirects to Login |
| **Manage Event Categories** | Yes (Age & Distance categories) | No | Redirects to Login |
| **View All Event Enrolments** | Yes (Colour-coded badges) | No | Redirects to Login |
| **Capture & Publish Results** | Yes (Finish time & Position) | No | Redirects to Login |
| **Enrol for an Event** | No | Yes (Select category, instant record) | Redirects to Login |
| **My Enrolments Page** | No | Yes (Status: Confirmed / Pending) | Redirects to Login |
| **My Results Page** | No | Yes (Event, date, finish time, position) | Redirects to Login |
| **User Profile & Avatar** | Yes | Yes (View, update, picture upload) | Redirects to Login |

### Session Security & Role Enforcement
- **Client Session (`.RaceDay.Mvc.Session`):** Stores `SessionToken`, `UserID`, `UserName`, and `UserRole`.
- **API Delegation:** The MVC project performs **no direct database queries**. All data retrieval and mutations pass through the strongly typed `IApiClient` service, transmitting the user session token in request headers.
- **`[AuthorizeSession]` Filter:** Enforces authentication and specific roles (`[AuthorizeSession("Organiser")]` or `[AuthorizeSession("Participant")]`). Unauthenticated users are redirected to `/Account/Login`, while unauthorized roles are redirected to `/Account/AccessDenied`.
- **Logout:** Calling `/Account/Logout` clears both the local MVC session and calls `/api/auth/logout` on the REST API to invalidate the server session.

---

## 3. Azure Blob Storage Integration

Azure Blob Storage is integrated cleanly into the **Part 2 API layer** via `AzureBlobStorageService` and `IBlobStorageService`:
- **Architecture Principle:** In compliance with the PoE Part 3 requirements, the MVC application **never communicates directly with Azure**. All file uploads are submitted via multipart `IFormFile` to the REST API endpoints:
  - `POST /api/events/{id}/banner` (Organiser event banner upload)
  - `POST /api/profile/picture` (Participant profile picture upload)
- **Cloud & Local Fallback:** When Azure credentials (`ConnectionStrings` or Azurite) are supplied, files are uploaded directly to the `raceday-media` Azure Blob container and a public SAS/blob URL is stored in SQL Server. If running in offline test environments, the service falls back gracefully to a safe local file storage mechanism with identical behaviour.

---

## 4. Docker Containerisation

The RaceDay platform includes production-grade containerisation support:

### Dockerfile Highlights
- **Multi-stage build:** Uses `mcr.microsoft.com/dotnet/sdk:10.0` for compilation and publish, producing an ultra-lightweight image based on `mcr.microsoft.com/dotnet/aspnet:10.0`.
- **Layer caching:** Project files (`.csproj`) are copied and restored before source code to optimize build times.
- **Security:** Runs without root escalation on standard container ports (`8080`).

### Docker Single Command Run
To build and run the MVC container locally:

```bash
# 1. Build the Docker image
docker build -t raceday-mvc:latest -f Dockerfile .

# 2. Run the Docker container
docker run -d -p 5001:8080 --name raceday-mvc -e ApiSettings__BaseUrl="http://host.docker.internal:7276/" raceday-mvc:latest
```

### Full-Stack Docker Compose
You can also launch the entire platform (Web API + MVC Front-End) with a single command:
```bash
docker-compose up --build
```
- **MVC Front-End:** `http://localhost:5001`
- **RESTful Web API & Swagger:** `http://localhost:5000/swagger`

---

## 5. UI Design & Theming (RaceDay Brand)

The user interface was crafted specifically for the South African sports culture:
- **Brand Aesthetic:** Modern, dynamic athletics theme utilizing vibrant emerald, energetic electric blue, and crisp typography.
- **Responsiveness:** Built on **Bootstrap 5.3** ensuring seamless viewing across mobile, tablet, and desktop displays.
- **Visual Status Badges:** Colour-coded indicators for enrolment states:
  - `Confirmed`: Vibrant Green badge (`bg-success`)
  - `Pending`: Amber / Warning badge (`bg-warning text-dark`)
  - `Cancelled`: Red / Neutral badge (`bg-danger`)
- **Graceful Error Handling:** Form validations handle invalid inputs with friendly client and server-side alerts without displaying raw stack traces.

---

## 6. Comprehensive Automated Unit Tests

The test suite in `tests/RaceDay.Tests` provides comprehensive test coverage across both the REST Web API and the MVC Web Application:
- **Part 2 API Tests:**
  - Authentication (Registration, login, password hashing verification, duplicate rejection).
  - Role Rejection (Organiser actions rejected for Participants with `403 Forbidden`).
  - Event Management (CRUD operations, distance validation, filtering).
  - Categories (Event category assignment, retrieval).
  - Enrolments (Immediate registration, category association, duplicate prevention).
  - Results (Finish time & position capture, personal results history).
  - Azure Blob Storage (Upload, stream validation, file extension checks).
- **Part 3 MVC Tests:**
  - Session lifecycle and cookie state management.
  - Role-specific menu and route authorization.
  - Organiser controller orchestration (Dashboard metrics, result publishing).
  - Participant controller flows (Event browsing, category selection, profile updates).

### Test Results
```text
Test run for RaceDay.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 48, Skipped: 0, Total: 48, Duration: 24 s - RaceDay.Tests.dll (net10.0)
```

---

## 7. CI/CD Pipeline & GitHub Actions

The automated pipeline in `.github/workflows/ci.yml`:
1. Validates repository structure, `/docs` directory, and `Dockerfile`.
2. Restores all solution dependencies.
3. Compiles the entire solution (`RaceDay.Api`, `RaceDay.Mvc`, and `RaceDay.Tests`) in `Release` mode.
4. Executes all 48 unit tests with verbose logging.
5. Verifies container definition syntax.

### CI/CD Green Build Screenshot
![GitHub Actions CI/CD Green Build](docs/Screenshots/action%20work%20flow%20success.png)

---

## 8. How to Setup and Run Locally

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- [SQL Server](https://www.microsoft.com/sql-server/) / LocalDB
- [Docker Desktop](https://www.docker.com/) *(optional, for containerised run)*

### Running with .NET CLI
```bash
# Clone the repository
git clone https://github.com/Lunga012/RaceDay-POE-Part1.git
cd RaceDay-POE-Part1

# Build the solution
dotnet build RaceDay.sln

# Run all unit tests
dotnet test RaceDay.sln

# Start the RESTful Web API (Terminal 1)
cd src/RaceDay.Api
dotnet run

# Start the MVC Web Application (Terminal 2)
cd ../RaceDay.Mvc
dotnet run
```
Navigate to `https://localhost:7190` or `http://localhost:5190` in your web browser.

---

## 9. Video Presentation Demonstration Guide

When recording the unlisted YouTube video presentation for Part 3:
1. **Introduction:** State student name (*Lunga Njabulo Khumalo*), student number (*ST10483560*), and module (*PROG6212 / PROG6212/w*).
2. **Architecture & Project Overview:**
   - Explain how the MVC web application consumes the REST API via `IApiClient` without any direct database access.
   - Show how session authentication flows from the MVC client to the API.
3. **Azure Blob Storage Demonstration:**
   - Walk through `AzureBlobStorageService.cs` in `RaceDay.Api`.
   - Explain how the MVC front-end posts the image file to the API, and the API uploads it to Blob Storage and persists the URL in SQL Server.
4. **Organiser Workflow Demonstration:**
   - Log in with Organiser credentials (`khumalolungsta121@gmail.com`).
   - View Dashboard metrics (events summary and total participant enrolments).
   - Create a new event and upload an event banner.
   - Manage event categories (add 10km, 21km, 42.2km).
   - View Enrolments list and capture finish times/positions in Results Entry.
5. **Participant Workflow Demonstration:**
   - Log in with Participant credentials (`sipho.nkosi@gmail.com`).
   - Browse upcoming road running and cycling events with category filters.
   - Enrol for an event by selecting a category.
   - View **My Enrolments** page with colour-coded statuses.
   - View **My Results** page showing completed races, finish times, and finishing ranks.
   - View and update Profile details and upload an avatar picture.
6. **Docker & CI/CD Walkthrough:**
   - Show the multi-stage `Dockerfile` and `docker-compose.yml`.
   - Run the Docker container command live in the terminal.
   - Show GitHub Actions `.github/workflows/ci.yml` and the passing green build check.
7. **Live Test Execution:**
   - Run `dotnet test` in the terminal to demonstrate all 48 tests passing cleanly.
