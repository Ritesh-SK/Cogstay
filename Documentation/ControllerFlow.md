# Complete Request Flow - Lifecycle of a request

This document details the step-by-step request flow in the decoupled CogStay solution, showing how a user interaction travels from the front-end to the database and back.

---

## Request Flow Diagram

```text
  USER
   │
   ▼
┌────────────────────────────────────────────────────────┐
│                      CogStay (MVC)                     │
│                                                        │
│  [1] User interacts with browser (e.g. clicks Delete)  │
│  [2] Request maps to MVC View (Razor Template)         │
│  [3] Postback sent to MVC Controller (RoomController)  │
│  [4] Controller calls Extension Method on HttpClient   │
└──────────────────────────┬─────────────────────────────┘
                           │
                           │  HTTP Request (e.g. DELETE /api/rooms/5)
                           ▼
┌────────────────────────────────────────────────────────┐
│                     CogStayApi (API)                   │
│                                                        │
│  [5] Request hits Web API Routing & Middleware        │
│  [6] Dispatched to API Controller (RoomApiController)  │
│  [7] Calls validation & business logic (RoomService)   │
│  [8] Accesses database via Repository (RoomRepository) │
│  [9] Queries DB (HotelDbContext / SQL Server)          │
└──────────────────────────┬─────────────────────────────┘
                           │
                           │  Database Result (SQL Row deletion)
                           ▼
┌────────────────────────────────────────────────────────┐
│                     CogStayApi (API)                   │
│                                                        │
│  [10] Repository returns result to Service             │
│  [11] Service updates business status, returns to API  │
│  [12] API Controller returns HTTP Response (204)       │
└──────────────────────────┬─────────────────────────────┘
                           │
                           │  HTTP Response (JSON or StatusCode)
                           ▼
┌────────────────────────────────────────────────────────┐
│                      CogStay (MVC)                     │
│                                                        │
│  [13] HttpClient reads response, deserializes payload  │
│  [14] MVC Controller catches any errors or sets success│
│  [15] Controller returns View / Redirect               │
│  [16] Razor view renders clean HTML                    │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼
  USER (Sees "Room deleted successfully!")
```

---

## Detailed Example: Deleting a Room (ID: 5)

Below is the detailed traceback of the request lifecycle for a room deletion action:

1. **User Interaction**:
   The staff member clicks the **Delete** button on a room row in the room list page.

2. **MVC View**:
   The HTML form triggers a POST action targeting the MVC URL: `/Room/Delete/5`.

3. **MVC Controller (`RoomController`)**:
   - The framework maps the request to `public async Task<IActionResult> Delete(int id)` in [RoomController.cs](file:///c:/Users/Ritesh%20SK/Desktop/New%20folder/CogStay---Project2/CogStay/Controllers/RoomController.cs).
   - The controller retrieves the named `HttpClient` from the factory.
   - It executes:
     ```csharp
     await _httpClient.DeleteOrThrowAsync($"api/rooms/{id}");
     ```

4. **HttpClient & Extension Helper (`ControllerExtensions`)**:
   - `DeleteOrThrowAsync` translates this to a raw HTTP call: `DELETE https://localhost:5001/api/rooms/5`.
   - Sends the request over the network.

5. **Web API Endpoint (`RoomApiController`)**:
   - The request is routed by ASP.NET Core API routing engine in `CogStayApi` to `public async Task<IActionResult> DeleteRoom(int id)` in `RoomApiController`.
   - The controller executes:
     ```csharp
     var room = await _roomService.GetRoomByIdAsync(id);
     await _roomService.DeleteRoomAsync(id);
     return NoContent(); // Returns HTTP Status 204
     ```

6. **Service Layer (`RoomService`)**:
   - `RoomService` carries out checks (e.g. check if the room status is occupied before deletion).
   - It then requests the repository to execute the delete transaction.

7. **Repository Layer & Database (`RoomRepository`)**:
   - `RoomRepository` accesses the database context `HotelDbContext`.
   - EF Core compiles the removal transaction and performs `DELETE FROM Rooms WHERE RoomId = 5` against the SQL Server Database.
   - The database commits the change.

8. **Response Return (API ➔ MVC)**:
   - The repository returns control to the service, which returns control to `RoomApiController`.
   - `RoomApiController` returns a `NoContent` result (HTTP `204`).

9. **Handling HTTP Response in MVC**:
   - The MVC `HttpClient` receives the `204` response.
   - Since `204` indicates success, the helper method `DeleteOrThrowAsync` returns control without throwing an exception.
   - `RoomController` sets `TempData["Success"] = "Room deleted successfully!"`.
   - It redirects the user to the Index action of the Room controller.

10. **View Rendering**:
    - The browser is redirected to `/Room/Index`.
    - The `RoomController` makes a GET request to `/api/rooms` to retrieve the updated room list.
    - It passes the list to the MVC View `/Views/Room/Index.cshtml`.
    - Razor engine compiles the template into HTML, embedding the "Room deleted successfully!" alert.
    - The HTML is sent back to the browser.
