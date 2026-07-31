# Project Structure - Restructured CogStay Solution

This document details the file and folder organization after reorganizing the CogStay solution.

---

## 1. Directory Tree Hierarchy

The solution is divided into two projects located in their respective folders under the solution root:

```text
CogStaySolution
│
├── CogStay (Primary Project - ASP.NET Core MVC)
│   ├── Controllers (MVC convention-based controllers)
│   ├── Views (Razor view files)
│   ├── wwwroot (Static web assets: CSS, JS, images)
│   ├── Data (HotelDbContext and EF configurations)
│   ├── Models (Core domain entity classes)
│   ├── DTOs (Data Transfer Objects)
│   ├── Enums (Lodge operations enums)
│   ├── Repositories (Data access interfaces & SQL implementations)
│   ├── Services (Business logic interfaces & implementations)
│   ├── Migrations (EF database migrations)
│   ├── Properties (launchSettings.json)
│   ├── Program.cs (MVC and DI configuration)
│   ├── appsettings.json (Database connection and API URLs)
│   └── CogStay.csproj (Targets net10.0, holds EF packages)
│
├── CogStayApi (Lightweight API Layer - ASP.NET Core Web API)
│   ├── Controllers (REST API controllers, stateless endpoints)
│   ├── Properties (launchSettings.json)
│   ├── Program.cs (API startup and DI resolver)
│   ├── appsettings.json (Database connection string)
│   └── CogStayApi.csproj (Targets net10.0, references CogStay project)
│
└── CogStaySolution.sln (Main solution file)
```

---

## 2. Project Responsibilities

### A. CogStay (MVC Project)
* **Core Role**: Serves as the primary application project hosting all logic, schemas, and UI elements.
* **Dependencies**: Completely self-contained regarding business operations. It does not reference the API project.
* **Contents**:
  - **Models/DTOs/Enums**: Domain entities, request/response models, and states.
  - **Data/Migrations**: The Entity Framework Core `HotelDbContext` and SQL Server schemas.
  - **Repositories/Services**: All CRUD operations, validation constraints, and business logic.
  - **Controllers/Views**: Convention-based MVC routes and Razor pages.

### B. CogStayApi (Web API Project)
* **Core Role**: Act solely as a lightweight API gateway and distribution channel.
* **Dependencies**: References `CogStay` as a project reference to resolve DTOs, domain models, services, and repositories.
* **Contents**:
  - **Controllers**: Only stateless REST API Controllers (e.g. `RoomApiController`, `GuestApiController`) exposing JSON endpoints.
  - **Views**: None.

---

## 3. Rationale for Layer Relocations

* **Single Source of Truth**: Placing all repositories, services, DTOs, and models inside `CogStay` consolidates the logic into a single project. This prevents duplication and prevents code drift.
* **Decoupled Gateway**: The `CogStayApi` project is stripped down to just the controllers. It acts solely as a communication layer exposing endpoints without owning database contexts or logic definitions.
* **Simplified Dependencies**: The Web API project references the MVC project, meaning any additions or modifications to services/repositories inside the MVC project are automatically made available to the API project without copy-pasting.
