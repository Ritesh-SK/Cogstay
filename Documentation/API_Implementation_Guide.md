# CogStay REST API Implementation Guide

This guide details the architecture, routing, request lifecycle, calling conventions, endpoint mapping, and implementation details of the REST APIs and their integration with the ASP.NET Core MVC controllers.

---

## Project API Architecture

The application is built on a hybrid architecture that combines standard ASP.NET Core MVC (delivering views and managing user sessions) and RESTful API Controllers.

- **MVC Controllers**: Responsible for handling user requests from the web interface, managing cookie-based user session state, and returning HTML pages (CSHTML views) populated with data models.
- **API Controllers**: Expose a set of stateless, decoupled REST API endpoints. They process JSON requests and return data (in DTO format) wrapped in standard HTTP responses (`Ok`, `Created`, `NoContent`, `BadRequest`, `NotFound`, etc.).
- **Integration Layer**: The MVC Controllers consume the API Controllers directly via in-process Dependency Injection. This leverages the REST API layer for all CRUD operations, ensuring unified business logic execution and avoiding code duplication.

---

## API Request Flow

The diagram below represents the lifecycle of a request initiated from the user interface:

```
    User / Client
         │
         ▼
  MVC View (CSHTML)
         │  (Form submission or navigation)
         ▼
   MVC Controller
         │  (Constructs DTO & invokes API Controller action)
         ▼
   API Controller
         │  (Performs model validation & authorization checks)
         ▼
   Service Layer (Business Logic)
         │  (Coordinates transactional logic)
         ▼
Repository Layer (Data Access)
         │  (Executes EF Core commands)
         ▼
     Database (SQL Server)
         │
         ▼ (Entity Record)
Repository Layer (Data Access)
         │
         ▼ (Response DTO)
   Service Layer
         │
         ▼ (ActionResult<T> / IActionResult)
   API Controller
         │
         ▼ (Unpacks data or throws error message)
   MVC Controller
         │
         ▼ (Renders View with Model data / sets TempData)
  MVC View (CSHTML)
         │
         ▼
    User / Client
```

---

## Attribute Routing

In this project, API Controllers utilize **Attribute-Based Routing** to define precise HTTP routes.

### Overview
Unlike conventional convention-based routing mapped in `Program.cs`, Attribute Routing allows developers to decorate controllers and actions with route attributes (like `[Route]`, `[HttpGet]`, `[HttpPost]`, etc.) directly in code. This provides finer control over URI design and parameter constraints.

### Key Annotations
1. **`[ApiController]`**: Indicates that a controller serves REST API responses. It automatically enables features like automatic `400 Bad Request` responses on model validation errors and binding source inference (like `[FromBody]`, `[FromRoute]`).
2. **`[Route("api/...")`**: Placed on the controller class to define the base URI segment (e.g., `[Route("api/rooms")]`).
3. **HTTP Verb Attributes**: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]` define which action handles which HTTP method.
4. **Route Parameters & Constraints**: Variables defined in brackets (e.g., `[HttpGet("{id:int}")]`) capture parameters directly from the URL path and enforce type safety (like `int`).

### Examples
From `RoomApiController.cs`:
```csharp
[ApiController]
[Route("api/rooms")]
public class RoomApiController : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomResponseDTO>> GetRoomById(int id) { ... }

    [HttpPatch("{roomId:int}/status")]
    public async Task<IActionResult> UpdateRoomStatus(int roomId, [FromBody] RoomStatus status) { ... }
}
```

---

## API Endpoint Mapping

Below is the comprehensive mapping of every API endpoint in the system to its corresponding MVC Controller, HTTP Method, Route, Purpose, and DTO types:

| API Controller | MVC Controller | HTTP Method | API Route Path | Purpose | Request DTO / Parameter | Response DTO / Result |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BillingApiController** | `BillingController` | `GET` | `/api/billing` | Retrieve all billing invoices | None | `IEnumerable<BillingResponseDTO>` |
| | `BillingController` | `GET` | `/api/billing/{id}` | Get billing invoice details by ID | `id` (int) | `BillingResponseDTO` |
| | `BillingController` | `GET` | `/api/billing/stay/{stayId}` | Get invoice associated with a guest stay | `stayId` (int) | `BillingResponseDTO` |
| | `BillingController` | `POST` | `/api/billing` | Create a new custom invoice record | `CreateBillDTO` (JSON body) | `BillingResponseDTO` |
| | `BillingController` | `POST` | `/api/billing/generate/stay/{stayId}` | Calculate and generate invoice for a stay | `stayId` (int), `remarks` (query string) | `BillingResponseDTO` |
| | `BillingController` | `POST` | `/api/billing/payment` | Record payment settlement for an invoice | `ProcessPaymentDTO` (JSON body) | `NoContent` |
| | `BillingController` | `DELETE` | `/api/billing/{id}` | Remove a billing record from history | `id` (int) | `NoContent` |
| **CheckInApiController** | `CheckInController`, `StaffController` | `GET` | `/api/stays` | Retrieve list of all stays (active and historical) | None | `IEnumerable<StayRecordResponseDTO>` |
| | `CheckInController` | `GET` | `/api/stays/{id}` | Get stay record by ID | `id` (int) | `StayRecordResponseDTO` |
| | `CheckInController` | `GET` | `/api/stays/reservation/{reservationId}` | Get stay record by reservation ID | `reservationId` (int) | `StayRecordResponseDTO` |
| | `CheckInController` | `POST` | `/api/stays/checkin` | Check in a guest with room and key assignment | `CreateCheckInDTO` (JSON body) | `StayRecordResponseDTO` |
| | `StaffController` | `POST` | `/api/stays/{id}/request-checkout` | Request check-out for an active stay | `id` (int) | `NoContent` |
| | `BillingController` (via Payment) | `POST` | `/api/stays/{id}/complete-checkout` | Complete check-out operations (after payment) | `id` (int) | `NoContent` |
| | `CheckInController` | `DELETE` | `/api/stays/{id}` | Delete a stay record | `id` (int) | `NoContent` |
| **FeedbackApiController** | `FeedbackController` | `GET` | `/api/feedback` | Retrieve all guest feedbacks and ratings | None | `IEnumerable<FeedbackResponseDTO>` |
| | `FeedbackController` | `GET` | `/api/feedback/{id}` | Retrieve specific feedback details by ID | `id` (int) | `FeedbackResponseDTO` |
| | `FeedbackController` | `POST` | `/api/feedback` | Submit a review for a guest stay | `CreateFeedbackDTO` (JSON body) | `FeedbackResponseDTO` |
| | `FeedbackController` | `DELETE` | `/api/feedback/{id}` | Remove feedback submission | `id` (int) | `NoContent` |
| **GuestApiController** | `GuestController`, `ReservationController` | `GET` | `/api/guests` | List all registered hotel guests | None | `IEnumerable<GuestResponseDTO>` |
| | `GuestController` | `GET` | `/api/guests/{id}` | Get guest profile details by ID | `id` (int) | `GuestResponseDTO` |
| | `GuestController` | `GET` | `/api/guests/email/{email}` | Retrieve guest profile by email address | `email` (string) | `GuestResponseDTO` |
| | `GuestController` | `POST` | `/api/guests/register` | Register a new guest account | `CreateGuestDTO` (JSON body) | `GuestResponseDTO` |
| | `GuestController` | `POST` | `/api/guests/login` | Validate guest login credentials | `GuestLoginDTO` (JSON body) | `GuestResponseDTO` |
| | `GuestController` | `PUT` | `/api/guests/{id}` | Update guest profile information | `id` (int), `UpdateGuestDTO` (JSON body) | `NoContent` |
| | `GuestController` | `DELETE` | `/api/guests/{id}` | Delete guest profile record | `id` (int) | `NoContent` |
| **HousekeepingApiController**| `HousekeepingController` | `GET` | `/api/housekeeping` | List all housekeeping duties | None | `IEnumerable<HousekeepingTaskResponseDTO>` |
| | `HousekeepingController` | `GET` | `/api/housekeeping/{id}` | Get specific cleaning task details by ID | `id` (int) | `HousekeepingTaskResponseDTO` |
| | `HousekeepingController` | `GET` | `/api/housekeeping/room/{roomId}`| List cleaning tasks assigned to a room | `roomId` (int) | `IEnumerable<HousekeepingTaskResponseDTO>` |
| | `HousekeepingController` | `POST` | `/api/housekeeping` | Create new operational cleaning assignment | `CreateHousekeepingTaskDTO` (JSON body) | `HousekeepingTaskResponseDTO` |
| | `HousekeepingController` | `PUT` | `/api/housekeeping/status` | Update status of cleaning duty (e.g. In Progress) | `UpdateTaskStatusDTO` (JSON body) | `NoContent` |
| | `HousekeepingController` | `DELETE` | `/api/housekeeping/{id}` | Remove housekeeping duty record | `id` (int) | `NoContent` |
| **ReservationApiController** | `ReservationController`, `GuestController` | `GET` | `/api/reservations` | List all room bookings | None | `IEnumerable<ReservationResponseDTO>` |
| | `ReservationController` | `GET` | `/api/reservations/{id}` | Get details of a room booking | `id` (int) | `ReservationResponseDTO` |
| | `GuestController`, `ReservationController` | `GET` | `/api/reservations/guest/{guestId}`| Retrieve bookings for a specific guest | `guestId` (int) | `IEnumerable<ReservationResponseDTO>` |
| | `GuestController`, `ReservationController` | `POST` | `/api/reservations` | Book a room for guest | `CreateReservationDTO` (JSON body) | `ReservationResponseDTO` |
| | `ReservationController` | `PUT` | `/api/reservations/{id}` | Update reservation details | `id` (int), `UpdateReservationDTO` (JSON body) | `NoContent` |
| | `ReservationController` | `POST` | `/api/reservations/{id}/cancel` | Cancel an active reservation | `id` (int) | `NoContent` |
| | `ReservationController` | `DELETE` | `/api/reservations/{id}` | Delete a reservation record | `id` (int) | `NoContent` |
| **RoomApiController** | `RoomController`, `GuestController`, `HomeController` | `GET` | `/api/rooms` | Retrieve inventory room catalog | None | `IEnumerable<RoomResponseDTO>` |
| | `RoomController`, `GuestController` | `GET` | `/api/rooms/available` | List available rooms | None | `IEnumerable<RoomResponseDTO>` |
| | `RoomController` | `GET` | `/api/rooms/{id}` | Retrieve room details by ID | `id` (int) | `RoomResponseDTO` |
| | `RoomController` | `POST` | `/api/rooms` | Configure and add a new room | `CreateRoomDTO` (JSON body) | `RoomResponseDTO` |
| | `RoomController` | `PUT` | `/api/rooms/{id}` | Edit room details | `id` (int), `UpdateRoomDTO` (JSON body) | `NoContent` |
| | `RoomController` | `PATCH` | `/api/rooms/{roomId}/status` | Update room status directly | `roomId` (int), `RoomStatus` (JSON body) | `NoContent` |
| | `RoomController` | `DELETE` | `/api/rooms/{id}` | Delete a room from inventory | `id` (int) | `NoContent` |
| **StaffApiController** | `StaffController` | `GET` | `/api/staff` | List all staff profiles | None | `IEnumerable<StaffResponseDTO>` |
| | `StaffController` | `GET` | `/api/staff/{id}` | Retrieve specific staff profile by ID | `id` (int) | `StaffResponseDTO` |
| | `StaffController` | `POST` | `/api/staff` | Create a new corporate staff profile | `CreateStaffDTO` (JSON body) | `StaffResponseDTO` |
| | `StaffController` | `PUT` | `/api/staff/{id}` | Edit staff profile details | `id` (int), `UpdateStaffDTO` (JSON body) | `NoContent` |
| | `StaffController` | `POST` | `/api/staff/login` | Validate corporate credentials | `StaffLoginDTO` (JSON body) | `StaffResponseDTO` |
| | `StaffController` | `DELETE` | `/api/staff/{id}` | Terminate and delete staff profile record | `id` (int) | `NoContent` |

---

## API Calling Process

MVC Controllers invoke REST API endpoints directly inside the server process.

### Configuration
API Controllers are registered as services in the Dependency Injection container inside [Program.cs](file:///c:/Users/Ritesh%20SK/Desktop/Cogstay%2011/CogStay---Project2/Program.cs):
```csharp
builder.Services.AddControllersWithViews().AddControllersAsServices();
```
This enables constructor injection of API controllers directly into MVC controllers.

### Request Lifecycle and Unpacking
When an action is triggered, the MVC controller invokes the appropriate API controller method asynchronously. The API controller returns an `ActionResult<T>` or `IActionResult`.

To extract the successful response object or correctly handle error states, a utility helper [ControllerExtensions.cs](file:///c:/Users/Ritesh%20SK/Desktop/Cogstay%2011/CogStay---Project2/Controllers/ControllerExtensions.cs) is used:
- **`Unpack<T>(ActionResult<T> result)`**: Unpacks the `T` payload from an `OkObjectResult` or `CreatedAtActionResult`. If the result represents an error (e.g. `BadRequestObjectResult`, `NotFoundObjectResult`), it parses the error message and throws a standard `Exception`.
- **`Unpack(IActionResult result)`**: Evaluates success results (like `NoContentResult`). If it represents an error result, it extracts the error message and throws an exception.

### Validation Flow and Error Handling
1. **Model Validation**: The MVC framework automatically validates incoming DTO properties against data annotations (e.g., `[Required]`, `[EmailAddress]`).
2. **Error Catching**: MVC actions execute API calls within a `try-catch` block.
3. **Binding to UI**: If the API call fails, `ControllerExtensions.Unpack` throws an exception containing the error message returned by the API (like business validation conflicts). The `catch` block intercepts this exception and registers it via `ModelState.AddModelError(string.Empty, ex.Message)`. This displays the error message on the corresponding View.

Example from `RoomController.cs`:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateRoomDTO dto)
{
    if (!ModelState.IsValid) return View(dto);

    try
    {
        ControllerExtensions.Unpack(await _roomApiController.CreateRoom(dto));
        TempData["Success"] = "Room created successfully!";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(dto);
    }
}
```

---

## CRUD Flow

### Create Operation
1. User enters values in the View form and submits.
2. MVC action validates properties with `ModelState.IsValid`.
3. MVC controller calls `APIController.Create(...)`.
4. API Controller invokes `Service.CreateAsync(...)`.
5. Service verifies business constraints, hashes secrets, and invokes `Repository.AddAsync(...)`.
6. Repository persists data via EF Core `SaveChangesAsync()`.
7. Database updates, returning the seeded auto-increment primary key ID.
8. API Controller returns `CreatedAtAction` containing the created response DTO.
9. MVC Controller unpacks the DTO, sets a success message in `TempData`, and redirects the user to the Index grid view.

### Read Operation
1. User requests a page or clicks "Details".
2. MVC Controller invokes `APIController.GetById(id)`.
3. API Controller reads database through the Service and Repository layer.
4. Repository uses EF Core `FindAsync(id)`.
5. Entity data is mapped into a response DTO and returned as `Ok(dto)`.
6. MVC Controller unpacks the DTO and passes it to the `View(dto)`.
7. Browser renders HTML page with guest stay, room catalog, or billing record details.

### Update Operation
1. User modifies values in the edit page form and submits.
2. MVC action verifies model status.
3. MVC Controller invokes `APIController.Update(id, dto)`.
4. API Controller calls `Service.UpdateAsync(dto)`.
5. Service validates records, updates attributes, and calls `Repository.UpdateAsync(...)`.
6. EF Core generates an `UPDATE` query and runs `SaveChangesAsync()`.
7. API Controller returns `NoContent()` (HTTP 204).
8. MVC Controller unpacks success, assigns a success message, and redirects to list view.

### Delete Operation
1. User clicks "Delete" button inside a table row and confirms.
2. Form submits HTTP POST delete command to the MVC controller action.
3. MVC Controller calls `APIController.Delete(id)`.
4. API Controller reads record, verifies dependencies, and executes `Service.DeleteAsync(id)`.
5. Service commands `Repository.DeleteAsync(id)`.
6. EF Core issues a `DELETE` query, committing changes with `SaveChangesAsync()`.
7. API Controller returns `NoContent()`.
8. MVC Controller unpacks response, issues success alert, and reloads the index view.

---

## Best Practices Used

1. **Separation of Concerns (SoC)**: Controllers handle incoming Web/API requests, the Service Layer implements business processes, and the Repository Layer encapsulates database transactions.
2. **Dependency Injection (DI)**: Interfaces are registered in the DI container in `Program.cs` and resolved at runtime, making code modular and testable.
3. **Data Transfer Objects (DTOs)**: Prevents over-posting vulnerabilities and decouples DB entities from models exposed to the API/UI layers.
4. **Attribute Routing**: Simplifies REST routing configuration, ensuring descriptive URLs, parameterized constraints, and clean route mappings.
5. **Robust Error Handling**: Exception propagation through `Unpack` ensures that business validation failures inside services are gracefully caught and mapped to standard UI validation messages.
6. **Stateless API Design**: API endpoints operate independently of session states, enabling them to be easily extended for JWT token authorization or external consumers.
