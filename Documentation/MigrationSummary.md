# Migration Summary

This document summarizes the refactoring process, relocated files, routing updates, and project configurations applied to split the original monolithic project into a decoupled ASP.NET Core MVC + Web API architecture.

---

## 1. Project Restructuring

The project has been split into a two-project solution. A new solution file was created at the root, and the files were relocated:

* **Solution File Created**: `CogStaySolution.sln`
* **Sub-projects Created**:
  1. `CogStay/CogStay.csproj` (ASP.NET Core MVC UI application)
  2. `CogStayApi/CogStayApi.csproj` (ASP.NET Core Web API backend)

---

## 2. File and Directory Relocations

| Folder / File | Original Path (Root) | Migrated Path | Target Project |
| :--- | :--- | :--- | :--- |
| **Migrations** | `/Migrations/*` | `/CogStayApi/Migrations/*` | `CogStayApi` |
| **Database Context** | `/Data/*` | `/CogStayApi/Data/*` | `CogStayApi` |
| **Domain Models** | `/Models/*` | `/CogStayApi/Models/*` | `CogStayApi` |
| **DTOs** | `/DTOs/*` | `/CogStayApi/DTOs/*` | `CogStayApi` |
| **Enums** | `/Enums/*` | `/CogStayApi/Enums/*` | `CogStayApi` |
| **Repositories** | `/Repositories/*` | `/CogStayApi/Repositories/*` | `CogStayApi` |
| **Services** | `/Services/*` | `/CogStayApi/Services/*` | `CogStayApi` |
| **API Controllers** | `/Controllers/Api/*` | `/CogStayApi/Controllers/*` | `CogStayApi` |
| **MVC Views** | `/Views/*` | `/CogStay/Views/*` | `CogStay` |
| **Static Web Assets** | `/wwwroot/*` | `/CogStay/wwwroot/*` | `CogStay` |
| **MVC Controllers** | `/Controllers/*.cs` | `/CogStay/Controllers/*.cs` | `CogStay` |
| **Web API Config** | `/Program.cs` | `/CogStayApi/Program.cs` | `CogStayApi` |
| **Web API Settings** | `/appsettings.json` | `/CogStayApi/appsettings.json` | `CogStayApi` |
| **MVC Config** | `/Program.cs` | `/CogStay/Program.cs` | `CogStay` |
| **MVC Settings** | `/appsettings.json` | `/CogStay/appsettings.json` | `CogStay` |

---

## 3. Configuration Changes

### A. Web API Project (`CogStayApi`)
* **`Program.cs`**:
  - Configured for Web API with `builder.Services.AddControllers()`.
  - Configured DB Context and registers repositories and services (scoped lifetimes).
  - Maintained default administrator seeding logic.
* **`appsettings.json`**:
  - Stores SQL Server connection string under `ConnectionStrings:DefaultConnection`.
* **`Properties/launchSettings.json`**:
  - Configures the Web API to run on port `5001` (HTTPS) and `5000` (HTTP).

### B. MVC Client Project (`CogStay`)
* **`Program.cs`**:
  - Configured for MVC UI with `builder.Services.AddControllersWithViews()`.
  - Session state caching and memory support.
  - Named HttpClient client registration targeting the API project.
  - Removed all database configuration, services, and repositories (now run exclusively on the API project).
* **`appsettings.json`**:
  - Stores Web API URL base path under `ApiSettings:BaseUrl`:
    ```json
    "ApiSettings": {
      "BaseUrl": "https://localhost:5001/"
    }
    ```
* **`Properties/launchSettings.json`**:
  - Configures the front-end to run on its original port `61319` (HTTPS) and `61320` (HTTP).
* **`Controllers/ControllerExtensions.cs`**:
  - Replaced the direct controller `Unpack` calls with extension methods on `HttpClient` (e.g. `GetFromJsonOrThrowAsync<T>`, `PostAsJsonOrThrowAsync<T, TValue>`). These methods intercept errors and automatically translate API model validations/exceptions into client-side errors.

---

## 4. Routing Changes
* Web API controllers use attribute routing: `[Route("api/[controller]")]` or explicit path attributes (e.g. `[Route("api/stays")]`).
* MVC controllers use standard convention-based routing: `"{controller=Home}/{action=Index}/{id?}"`.
* Direct controller method invocations in MVC controllers have been entirely replaced with RESTful HTTP request payloads (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`).
