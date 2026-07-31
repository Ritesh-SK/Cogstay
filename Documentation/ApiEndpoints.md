# Web API Endpoints Catalog

This document registers and describes all RESTful API endpoints exposed by the `CogStayApi` project.

---

## 1. Rooms API (`api/rooms`)
Exposes operations to manage rooms and inspect availability.
* **Controller**: `RoomApiController`

### Get All Rooms
* **HTTP Method**: `GET`
* **Route**: `/api/rooms`
* **Request Body**: None
* **Response Body**: `IEnumerable<RoomResponseDTO>`
* **Status Codes**: 
  - `200 OK` on success.

### Get Available Rooms
* **HTTP Method**: `GET`
* **Route**: `/api/rooms/available`
* **Request Body**: None
* **Response Body**: `IEnumerable<RoomResponseDTO>`
* **Status Codes**: 
  - `200 OK` on success.

### Get Room By ID
* **HTTP Method**: `GET`
* **Route**: `/api/rooms/{id}`
* **Request Body**: None
* **Response Body**: `RoomResponseDTO` or `{ message: string }`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if room is not found.

### Create Room
* **HTTP Method**: `POST`
* **Route**: `/api/rooms`
* **Request Body**: `CreateRoomDTO`
* **Response Body**: `RoomResponseDTO`
* **Status Codes**: 
  - `210 Created` on success.
  - `400 Bad Request` if payload is invalid.
  - `409 Conflict` if room number already exists.

### Update Room
* **HTTP Method**: `PUT`
* **Route**: `/api/rooms/{id}`
* **Request Body**: `UpdateRoomDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` if IDs mismatch or payload is invalid.
  - `404 Not Found` if room is not found.

### Update Room Status
* **HTTP Method**: `PATCH`
* **Route**: `/api/rooms/{roomId}/status`
* **Request Body**: `RoomStatus` (Enum/JSON value)
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if room is not found.

### Delete Room
* **HTTP Method**: `DELETE`
* **Route**: `/api/rooms/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if room is not found.

---

## 2. Reservations API (`api/reservations`)
Handles lodge bookings, updates, cancellations, and status monitoring.
* **Controller**: `ReservationApiController`

### Get All Reservations
* **HTTP Method**: `GET`
* **Route**: `/api/reservations`
* **Request Body**: None
* **Response Body**: `IEnumerable<ReservationResponseDTO>`
* **Status Codes**: `200 OK`

### Get Reservation By ID
* **HTTP Method**: `GET`
* **Route**: `/api/reservations/{id}`
* **Request Body**: None
* **Response Body**: `ReservationResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Get Reservations By Guest
* **HTTP Method**: `GET`
* **Route**: `/api/reservations/guest/{guestId}`
* **Request Body**: None
* **Response Body**: `IEnumerable<ReservationResponseDTO>`
* **Status Codes**: `200 OK`

### Book Room
* **HTTP Method**: `POST`
* **Route**: `/api/reservations`
* **Request Body**: `CreateReservationDTO`
* **Response Body**: `ReservationResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `400 Bad Request` if invalid dates, room unavailable, or validation fails.

### Update Reservation
* **HTTP Method**: `PUT`
* **Route**: `/api/reservations/{id}`
* **Request Body**: `UpdateReservationDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` on ID mismatch.
  - `404 Not Found` if reservation doesn't exist.

### Cancel Reservation
* **HTTP Method**: `POST`
* **Route**: `/api/reservations/{id}/cancel`
* **Request Body**: Empty (`new { }`)
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if reservation doesn't exist.
  - `400 Bad Request` if reservation cannot be cancelled.

### Delete Reservation
* **HTTP Method**: `DELETE`
* **Route**: `/api/reservations/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 3. Guests API (`api/guests`)
* **Controller**: `GuestApiController`

### Get All Guests
* **HTTP Method**: `GET`
* **Route**: `/api/guests`
* **Request Body**: None
* **Response Body**: `IEnumerable<GuestResponseDTO>`
* **Status Codes**: `200 OK`

### Get Guest By ID
* **HTTP Method**: `GET`
* **Route**: `/api/guests/{id}`
* **Request Body**: None
* **Response Body**: `GuestResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Get Guest By Email
* **HTTP Method**: `GET`
* **Route**: `/api/guests/email/{email}`
* **Request Body**: None
* **Response Body**: `GuestResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `400 Bad Request` if email is empty.
  - `404 Not Found` if not found.

### Register Guest
* **HTTP Method**: `POST`
* **Route**: `/api/guests/register`
* **Request Body**: `CreateGuestDTO`
* **Response Body**: `GuestResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `409 Conflict` if email already registered.

### Validate Guest Login
* **HTTP Method**: `POST`
* **Route**: `/api/guests/login`
* **Request Body**: `GuestLoginDTO`
* **Response Body**: `GuestResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `401 Unauthorized` on invalid credentials.

### Update Guest
* **HTTP Method**: `PUT`
* **Route**: `/api/guests/{id}`
* **Request Body**: `UpdateGuestDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` on ID mismatch.
  - `404 Not Found` if guest doesn't exist.

### Delete Guest
* **HTTP Method**: `DELETE`
* **Route**: `/api/guests/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 4. Check-Ins & Stays API (`api/stays`)
* **Controller**: `CheckInApiController`

### Get All Stays
* **HTTP Method**: `GET`
* **Route**: `/api/stays`
* **Request Body**: None
* **Response Body**: `IEnumerable<StayRecordResponseDTO>`
* **Status Codes**: `200 OK`

### Get Stay By ID
* **HTTP Method**: `GET`
* **Route**: `/api/stays/{id}`
* **Request Body**: None
* **Response Body**: `StayRecordResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Get Stay By Reservation ID
* **HTTP Method**: `GET`
* **Route**: `/api/stays/reservation/{reservationId}`
* **Request Body**: None
* **Response Body**: `StayRecordResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Check In Guest
* **HTTP Method**: `POST`
* **Route**: `/api/stays/checkin`
* **Request Body**: `CreateCheckInDTO`
* **Response Body**: `StayRecordResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `400 Bad Request` if checking in invalid reservation or room is occupied.

### Request Check Out
* **HTTP Method**: `POST`
* **Route**: `/api/stays/{id}/request-checkout`
* **Request Body**: Empty (`new { }`)
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` if stay status cannot change.

### Complete Check Out
* **HTTP Method**: `POST`
* **Route**: `/api/stays/{id}/complete-checkout`
* **Request Body**: Empty (`new { }`)
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` if stay status cannot change.

### Delete Stay
* **HTTP Method**: `DELETE`
* **Route**: `/api/stays/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 5. Billing API (`api/billing`)
* **Controller**: `BillingApiController`

### Get All Bills
* **HTTP Method**: `GET`
* **Route**: `/api/billing`
* **Request Body**: None
* **Response Body**: `IEnumerable<BillingResponseDTO>`
* **Status Codes**: `200 OK`

### Get Bill By ID
* **HTTP Method**: `GET`
* **Route**: `/api/billing/{id}`
* **Request Body**: None
* **Response Body**: `BillingResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Get Bill By Stay ID
* **HTTP Method**: `GET`
* **Route**: `/api/billing/stay/{stayId}`
* **Request Body**: None
* **Response Body**: `BillingResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Create Bill
* **HTTP Method**: `POST`
* **Route**: `/api/billing`
* **Request Body**: `CreateBillDTO`
* **Response Body**: `BillingResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `400 Bad Request` on invalid details.

### Generate Bill For Stay
* **HTTP Method**: `POST`
* **Route**: `/api/billing/generate/stay/{stayId}`
* **Request Body**: Empty (`new { }`)
* **Query Parameters**: `remarks` (optional string)
* **Response Body**: `BillingResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `400 Bad Request` if bill is already generated or stay details are invalid.

### Process Payment
* **HTTP Method**: `POST`
* **Route**: `/api/billing/payment`
* **Request Body**: `ProcessPaymentDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` on invalid payment configuration.

### Delete Bill
* **HTTP Method**: `DELETE`
* **Route**: `/api/billing/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 6. Housekeeping API (`api/housekeeping`)
* **Controller**: `HousekeepingApiController`

### Get All Tasks
* **HTTP Method**: `GET`
* **Route**: `/api/housekeeping`
* **Request Body**: None
* **Response Body**: `IEnumerable<HousekeepingTaskResponseDTO>`
* **Status Codes**: `200 OK`

### Get Task By ID
* **HTTP Method**: `GET`
* **Route**: `/api/housekeeping/{id}`
* **Request Body**: None
* **Response Body**: `HousekeepingTaskResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Get Tasks By Room ID
* **HTTP Method**: `GET`
* **Route**: `/api/housekeeping/room/{roomId}`
* **Request Body**: None
* **Response Body**: `IEnumerable<HousekeepingTaskResponseDTO>`
* **Status Codes**: `200 OK`

### Create Task
* **HTTP Method**: `POST`
* **Route**: `/api/housekeeping`
* **Request Body**: `CreateHousekeepingTaskDTO`
* **Response Body**: `HousekeepingTaskResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.

### Update Task Status
* **HTTP Method**: `PUT`
* **Route**: `/api/housekeeping/status`
* **Request Body**: `UpdateTaskStatusDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` / `404 Not Found` on errors.

### Delete Task
* **HTTP Method**: `DELETE`
* **Route**: `/api/housekeeping/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 7. Staff API (`api/staff`)
* **Controller**: `StaffApiController`

### Get All Staff
* **HTTP Method**: `GET`
* **Route**: `/api/staff`
* **Request Body**: None
* **Response Body**: `IEnumerable<StaffResponseDTO>`
* **Status Codes**: `200 OK`

### Get Staff By ID
* **HTTP Method**: `GET`
* **Route**: `/api/staff/{id}`
* **Request Body**: None
* **Response Body**: `StaffResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Create Staff
* **HTTP Method**: `POST`
* **Route**: `/api/staff`
* **Request Body**: `CreateStaffDTO`
* **Response Body**: `StaffResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.
  - `409 Conflict` if staff email already exists.

### Update Staff
* **HTTP Method**: `PUT`
* **Route**: `/api/staff/{id}`
* **Request Body**: `UpdateStaffDTO`
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `400 Bad Request` on ID mismatch.
  - `404 Not Found` if staff doesn't exist.

### Staff Login
* **HTTP Method**: `POST`
* **Route**: `/api/staff/login`
* **Request Body**: `StaffLoginDTO`
* **Response Body**: `StaffResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `401 Unauthorized` on invalid credentials or role mismatch.

### Delete Staff
* **HTTP Method**: `DELETE`
* **Route**: `/api/staff/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.

---

## 8. Feedback API (`api/feedback`)
* **Controller**: `FeedbackApiController`

### Get All Feedbacks
* **HTTP Method**: `GET`
* **Route**: `/api/feedback`
* **Request Body**: None
* **Response Body**: `IEnumerable<FeedbackResponseDTO>`
* **Status Codes**: `200 OK`

### Get Feedback By ID
* **HTTP Method**: `GET`
* **Route**: `/api/feedback/{id}`
* **Request Body**: None
* **Response Body**: `FeedbackResponseDTO`
* **Status Codes**: 
  - `200 OK` on success.
  - `404 Not Found` if not found.

### Submit Feedback
* **HTTP Method**: `POST`
* **Route**: `/api/feedback`
* **Request Body**: `CreateFeedbackDTO`
* **Response Body**: `FeedbackResponseDTO`
* **Status Codes**: 
  - `201 Created` on success.

### Delete Feedback
* **HTTP Method**: `DELETE`
* **Route**: `/api/feedback/{id}`
* **Request Body**: None
* **Response Body**: None
* **Status Codes**: 
  - `204 NoContent` on success.
  - `404 Not Found` if not found.
