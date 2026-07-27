# Backend Component Documentation

This document explains each backend layer of the **CogStayMVC** application in detail. It outlines their purpose, design responsibilities, implementation choices, and how they interact to form a robust, stable hotel/lodge management system.

---

## 1. MVC Controllers

### Purpose & Responsibility
MVC Controllers inherit from `Microsoft.AspNetCore.Mvc.Controller` and are responsible for serving HTML views (`.cshtml`) and coordinating interactions for browser users. They act as traffic directors that:
- Verify session state (authentication/authorization role checks).
- Call appropriate services to fetch or persist data.
- Manage temporary communication parameters (using `ViewBag`, `ViewData`, `TempData`).
- Return rendered HTML views (`View(model)`) or instruct browsers to redirect (`RedirectToAction`).

### Request & Response Flow
```text
[Browser User] ──► [HTTP GET/POST] ──► [MVC Controller Action]
                                             │
                                             ├─► Session check fails ──► [Redirect to Login]
                                             │
                                             ├─► Model binding / validation 
                                             │     └─► [IsValid == false] ──► Return View(dto)
                                             │
                                             ├─► Call Service Layer (await)
                                             │     └─► [Success] ──► Set TempData["Success"] ──► Redirect
                                             │     └─► [Exception] ─► ModelState.AddModelError ─► Return View(dto)
                                             ▼
                                      [Rendered CSHTML View / HTTP Redirect]
```

### Key Design Choices & Best Practices
- **Session-Based State Control**: Since standard HTTP is stateless, the controllers access `HttpContext.Session` to store and verify authenticated users (e.g. `GuestId`, `StaffId`, `StaffRole`).
- **Input Preservation**: If form validation fails, the controller returns the original DTO back to the view (`return View(dto)`) to avoid wiping out user-entered values in the form.
- **Asynchronous Execution**: Every database-related action uses C# `async/await` to prevent blocking Kestrel server threads, optimizing the application for high concurrent user loads.

---

## 2. API Controllers

### Purpose & REST Architecture
API Controllers inherit from `Microsoft.AspNetCore.Mvc.ControllerBase` and are decorated with `[ApiController]` and `[Route("api/[controller]")]`. They provide stateless RESTful JSON interfaces for programmatic integration or AJAX requests.

### Routing & REST Conventions
API routes are mapped directly using attribute routing:
- **`GET /api/rooms`**: Retrieve all rooms.
- **`GET /api/rooms/{id}`**: Retrieve a specific room by integer ID.
- **`POST /api/rooms`**: Create a new room resource.
- **`PUT /api/rooms/{id}`**: Update an existing room resource.
- **`DELETE /api/rooms/{id}`**: Delete a room resource.
- **`PATCH /api/rooms/{id}/status`**: Partially update a room status (e.g., set status to Cleaning or Occupied).

### Response Status Codes
The API controllers strictly follow standard HTTP status codes:
- `200 OK`: Successful retrieval or update.
- `201 Created`: Resource successfully created (includes a `Location` header pointing to the new resource details).
- `204 NoContent`: Update or deletion was successful, returning no content.
- `400 BadRequest`: Model validation failed, or IDs did not match.
- `401 Unauthorized`: Authentication credentials (e.g. login credentials) are incorrect.
- `404 NotFound`: The requested resource does not exist.
- `409 Conflict`: Business key violation (e.g., trying to register a guest with an email that is already registered).
- `500 InternalServerError`: Unhandled database exception or server error.

---

## 3. Service Layer

### Purpose & Business Logic Encapsulation
The Service Layer contains the core application business rules and workflows. This layer abstracts away the data persistence details from the controllers.
- **Encapsulates Operations**: Calculations (like total billing amounts), state validations (confirming a reservation is Active before checking in), and operations orchestration are performed here.
- **Data Integrity / Security**: Handles operations such as generating SHA256 hashes of passwords during guest registration and validating logins:
  ```csharp
  private static string HashPassword(string password)
  {
      using var sha256 = SHA256.Create();
      var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
      return Convert.ToBase64String(bytes);
  }
  ```

### Why Use a Service Layer?
Without a Service Layer, business logic leaks into Controllers or Repositories. The Service Layer isolates logic, making components reusable, testable (via mock repositories), and easily maintainable.

---

## 4. Repository Layer

### Purpose & Database Abstraction
The Repository Layer isolates the domain model from EF Core details. It abstracts raw database operations into generic or specific C# interfaces.

### Class Structure & SaveChanges Workflow
- **Generic Repository (`Repository<T>`)**: Implements generic interfaces for basic CRUD:
  ```csharp
  public virtual async Task AddAsync(T entity)
  {
      await _dbSet.AddAsync(entity);
      await _context.SaveChangesAsync();
  }
  ```
  Note: Unlike unit-of-work patterns that batch transactions, each CRUD action here calls `SaveChangesAsync()` immediately, committing data directly.
- **Specific Repositories**: Extend `Repository<T>` to load complex associations. For instance, `IReservationRepository` needs to load both the related Room and Guest records, accomplished via EF Core's `.Include()`:
  ```csharp
  public async Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync()
  {
      return await _dbSet
          .Include(r => r.Guest)
          .Include(r => r.Room)
          .ToListAsync();
  }
  ```

---

## 5. Models vs. DTOs (Data Transfer Objects)

### Definition & Purpose
- **Domain Models (`Models/`)**: Represent the exact schema of the database tables (e.g. `Guest`, `Reservation`, `Billing`). They contain internal entity relationships and EF navigation properties (like `public virtual ICollection<Reservation> Reservations { get; set; }`).
- **DTOs (`DTOs/`)**: Simplified data contracts optimized for network transfer. They contain no database configuration details or virtual navigation properties, protecting internal schemas.

### Data Flow
When querying data:
1. EF Core retrieves database rows and instantiates **Domain Models**.
2. The Repository returns **Domain Models** to the Service.
3. The Service extracts data from the **Domain Model** and maps it into a read-only **Response DTO** (e.g. `RoomResponseDTO`).
4. The Controller receives the **DTO** and serializes it to JSON or renders it in the View.

When writing data:
1. The user submits a **Create DTO** (e.g. `CreateReservationDTO`).
2. The Controller binds and validates the **DTO**.
3. The Service verifies business rules and maps the incoming **DTO** properties into a new **Domain Model**.
4. The Repository saves the **Domain Model** into the database.

---

## 6. Database Layer

### DbContext & Relations Configurations
The database is managed via `HotelDbContext`. Relationships and data constraints are explicitly defined using the EF Core Fluent API inside `OnModelCreating`:
- **One-to-Many Relationships**: One Guest has many Reservations, configured with RESTRICT delete behavior:
  ```csharp
  modelBuilder.Entity<Reservation>()
      .HasOne(res => res.Guest)
      .WithMany(g => g.Reservations)
      .HasForeignKey(res => res.GuestId)
      .OnDelete(DeleteBehavior.Restrict);
  ```
- **One-to-One Relationships**: One StayRecord has one Billing, configured with CASCADE delete behavior:
  ```csharp
  modelBuilder.Entity<Billing>()
      .HasOne(b => b.StayRecord)
      .WithOne(s => s.Billing)
      .HasForeignKey<Billing>(b => b.StayId)
      .OnDelete(DeleteBehavior.Cascade);
  ```
- **Database Constraints**: Defines unique indices (e.g. `Email` on Guest/Staff, `RoomNumber` on Room), decimal number precisions (`PricePerNight`, `TotalAmount`), and text column length restrictions.
