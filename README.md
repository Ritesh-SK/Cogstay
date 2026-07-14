# CogStay - Integrated Hotel Management System

CogStay is a modern, enterprise-grade Hotel Management System (HMS) built with **ASP.NET Core MVC**. It provides a seamless experience for guests and comprehensive digital tools for hotel staff across various departments including Administration, Front Desk, Housekeeping, and Management.

## 🏨 Project Overview

The system is designed to streamline hotel operations, from the initial room booking by a customer to the final checkout and subsequent room maintenance. It features a role-based access control system implemented through MVC Areas.

---

## 📋 System Architecture

LANDING PAGE 1 (Customer Hotel Homepage)
│
├── Customer Login
│   └── Customer Login Page
│       └── Customer Dashboard
│           ├── Available Rooms
│           ├── Book Rooms
│           ├── Booking History
│           ├── Payment
│           ├── Profile Management
│           └── Feedback
│
└── Staff Login
    └── LANDING PAGE 2 (Staff Role Login Page)
        ├── Admin Login
        │   └── Admin Dashboard
        │       ├── Room Management
        │       ├── Reservation Monitoring
        │       ├── Reports
        │       ├── Staff & User Management
        │       └── Settings
        │
        ├── Manager Login
        │   └── Manager Dashboard
        │       ├── Reservations
        │       ├── Occupancy
        │       ├── Revenue
        │       ├── Housekeeping
        │       └── Feedbacks
        │
        ├── Front Desk Login
        │   └── Front Desk Dashboard
        │       ├── Check In
        │       ├── Check Out
        │       ├── Reservations
        │       ├── Housekeeping
        │       └── Guest Operations
        │
        └── Housekeeping Login
            └── Housekeeping Dashboard
                ├── Housekeeping Service Requests
                ├── Laundry Service
                ├── Food Service
                ├── Maintenance Service
                ├── Cleaning Service
                ├── WellnessSpaRequest
                └── Assign Task to Employee

## 🚀 Core Modules

The application is divided into five primary modules, each tailored to specific user roles:

### 1. Admin Module
The central hub for system administration and high-level control.
- **Staff User Management:** Add, edit, or deactivate staff accounts and assign roles.
- **Room Management:** Configure room types, pricing, capacity, and status.
- **Reservation Monitoring:** Real-time oversight of all hotel bookings.
- **System Settings:** Global configuration for hotel policies and system behavior.
- **Reporting:** Access to historical data and system logs.

### 2. Customer Module
A user-friendly interface designed for guests to manage their stay.
- **Room Discovery:** Browse available rooms with detailed specs (King, Deluxe, Penthouse, etc.).
- **Booking Engine:** Securely book rooms for specific dates.
- **Payment Integration:** Handle room charges and view transaction history.
- **Guest Profile:** Manage personal information and preferences.
- **Feedback System:** Submit ratings and comments regarding their stay.

### 3. Front Desk Module
The operational engine for daily guest interactions.
- **Check-In/Out:** Efficiently process guest arrivals and departures.
- **Live Reservations:** View and modify upcoming and current bookings.
- **Housekeeping Coordination:** Communicate directly with the housekeeping team for room readiness.
- **Guest Operations:** Handle special guest requests and billing inquiries.

### 4. Housekeeping Module
Dedicated to maintaining the highest standards of room cleanliness and service.
- **Task Management:** Real-time list of cleaning tasks (Checkout Clean, Stay-Over Clean).
- **Service Requests:** Handle guest requests for Laundry, Food, SPA, and Maintenance.
- **Status Updates:** Update room cleanliness status (Clean/Dirty) to notify the Front Desk.
- **Checklists:** Follow systematic cleaning protocols for different task types.

### 5. Manager Module
Focused on business performance and operational efficiency.
- **Occupancy Tracking:** Monitor current and projected hotel occupancy rates.
- **Revenue Analytics:** Analyze daily, weekly, and monthly revenue reports.
- **Performance Overview:** Review housekeeping efficiency and staff performance.
- **Feedback Moderation:** Review and analyze guest feedback to improve service quality.

---

## 🔄 Key Operational Flows

### A. The Booking & Reservation Flow
1. **Inquiry:** Customer browses `AvailableRooms`.
2. **Booking:** Customer selects a room and provides stay details.
3. **Reservation:** System creates a `Pending` reservation.
4. **Payment:** Customer completes the `Payment` flow; status moves to `Confirmed`.

### B. The Check-In & Stay Flow
1. **Arrival:** Front Desk identifies the reservation and selects `Check-In`.
2. **Activation:** Room status changes from `Available` to `Occupied`.
3. **Guest Experience:** Customer uses the `Dashboard` to request services (Laundry, Food, etc.).
4. **Service Execution:** Housekeeping receives the request, completes the task, and marks it as `Done`.

### C. The Checkout & Turnover Flow
1. **Departure:** Front Desk processes `Check-Out` and ensures `TotalAmount` is paid.
2. **Cleaning Trigger:** System automatically creates a `Checkout Clean` task for Housekeeping.
3. **Room Status:** Room status becomes `Dirty/Maintenance`.
4. **Turnover:** Housekeeper completes the task; room status reverts to `Available` and `Clean`, ready for the next guest.

---

## 🛠 Tech Stack

- **Framework:** ASP.NET Core 8.0 (MVC Architecture)
- **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5
- **Data Layer:** Entity Framework Core (In-memory/LocalDB for development)
- **Architecture:** Area-based modularity for distinct user roles

## 📂 Project Structure

- `/Areas`: Contains the five core modules (Admin, Customer, FrontDesk, Housekeeping, Manager).
- `/Controllers`: Shared controllers (Home, StaffLogin).
- `/Models`: Core data entities (Room, Booking, StaffUser, etc.).
- `/Views`: Shared UI components and layouts.
- `/wwwroot`: Static assets (Images, CSS, JS).
- `/Data`: Database context and sample data seed logic.

---
*Developed as part of the Cognizant Project Series.*
