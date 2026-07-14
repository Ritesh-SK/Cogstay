
---

# Hotel / Lodge Management System – Complete Workflow

## Overall Website Structure

Our project have **2 main landing/login entry pages**:

### **Landing Page 1 – Main Hotel Website + Customer/Staff Access**

This is the public-facing hotel homepage.
It includes hotel branding, room highlights, services, offers, and **2 login cards/buttons**:

* **Customer Login**
* **Staff Login**

### **Landing Page 2 – Staff Role Selection/Login Page**

This page is dedicated only to hotel staff.
After clicking **Staff Login** from Landing Page 1, staff will come here and choose one of the **4 staff roles**:

* Admin
* Manager
* Front Desk
* Housekeeping

---

# 1) Full Workflow Diagram / Navigation Flow

```text
LANDING PAGE 1 (Public Hotel Homepage)
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
```

---

# 2) Landing Pages

# **Landing Page 1 – Public Hotel Homepage + Customer/Staff Login**

### Purpose

This is the main homepage of the hotel/lodge website where visitors can view the property, services, rooms, offers, and basic information. It also acts as the first access point by showing **2 cards/buttons: Customer Login and Staff Login**.

### Sections to include

* Navbar
* Hero section with hotel image/banner
* About hotel/lodge
* Featured rooms
* Amenities/services
* Testimonials/reviews
* Contact/footer
* **Two login cards/buttons**

  * Customer Login
  * Staff Login

### 2-line description

**This page acts as the public-facing homepage of the hotel where customers can explore rooms, amenities, and hotel information. It also provides two clear login options so users can continue either as a customer or as a staff member.**

---

# **Landing Page 2 – Staff Role Login Page**

### Purpose

This page is only for staff members. After clicking **Staff Login** on the main homepage, the staff user lands here and selects their role before logging in.

### Staff role cards on this page

* Admin
* Manager
* Front Desk
* Housekeeping

### 2-line description

**This page is the dedicated staff entry portal where employees select their role before accessing their role-based dashboard. It separates internal hotel operations from the public customer-facing part of the website for better organization and security.**

---

---

# 3) Customer Module Workflow

## Customer Flow

```text
Landing Page 1 → Customer Login → Customer Dashboard
                                     ├── Available Rooms
                                     ├── Book Rooms
                                     ├── Booking History
                                     ├── Payment
                                     ├── Profile Management
                                     └── Feedback
```

# Customer Pages with 2-line Description

---

## **1. Customer Login Page**

### Purpose

Used by customers to sign in to their account before booking rooms or viewing previous bookings.

### 2-line description

**This page allows customers to securely log in using their credentials and access their personalized hotel dashboard. It acts as the authentication gateway for room booking, payment, booking history, and profile features.**

---

## **2. Customer Dashboard**

### Purpose

Main overview page after customer login. Shows welcome message, booking summary, quick actions, offers, and recent activity.

### 2-line description

**This page is the central hub for customers after login, showing a summary of current bookings, available services, and quick navigation links. It helps users manage their stay, room reservations, payments, and personal information from one place.**

---

## **3. Available Rooms Page**

### Purpose

Displays all rooms available for booking with filters like room type, price, occupancy, AC/non-AC, etc.

### 2-line description

**This page shows the list of rooms currently available for booking along with room images, pricing, features, and occupancy details. It helps customers compare room options and choose the best stay based on their needs and budget.**

---

## **4. Book Rooms Page**

### Purpose

Allows customer to select room, choose dates, number of guests, and confirm booking.

### 2-line description

**This page is used by customers to complete the room reservation process by selecting dates, room type, guest count, and other stay details. It collects booking information and moves the customer toward payment confirmation.**

---

## **5. Booking History Page**

### Purpose

Shows previous, current, and upcoming bookings with status like confirmed, checked-in, completed, or cancelled.

### 2-line description

**This page displays a complete record of the customer’s previous and current reservations with booking dates, room details, and booking status. It helps users track their stay history and review past transactions or reservations.**

---

## **6. Payment Page**

### Purpose

Used to pay for room bookings and view payment details such as amount, transaction ID, and payment status.

### 2-line description

**This page handles the payment process for room reservations and shows billing details, total amount, and transaction confirmation. It ensures that the customer can complete bookings securely and view the status of their payments.**

---

## **7. Profile Management Page**

### Purpose

Allows customer to view and update profile details such as name, email, phone, address, ID details, etc.

### 2-line description

**This page enables customers to manage their personal information, contact details, and account preferences. It keeps customer records updated and helps the hotel maintain accurate guest information for bookings and communication.**

---

## **8. Feedback Page**

### Purpose

Used by customers to submit reviews, ratings, complaints, or suggestions about their stay and services.

### 2-line description

**This page allows customers to share their hotel experience by submitting ratings, comments, and suggestions. It helps the hotel collect valuable feedback for service improvement and customer satisfaction analysis.**

---

---

# 4) Staff Module Workflow

## Staff Flow

```text
Landing Page 1 → Staff Login → Landing Page 2 (Role Selection)
                                  ├── Admin Login → Admin Module
                                  ├── Manager Login → Manager Module
                                  ├── Front Desk Login → Front Desk Module
                                  └── Housekeeping Login → Housekeeping Module
```

---

# 5) Admin Module

## Admin Flow

```text
Staff Role Page → Admin Login → Admin Dashboard
                                ├── Room Management
                                ├── Reservation Monitoring
                                ├── Reports
                                ├── Staff & User Management
                                └── Settings
```

## **1. Admin Dashboard**

### 2-line description

**This page provides the administrator with a complete overview of hotel operations including bookings, rooms, staff activity, and performance summaries. It serves as the control center for monitoring and managing the entire hotel management system.**

---

## **2. Room Management**

### 2-line description

**This page allows the admin to add, update, delete, and manage room details such as room type, availability, pricing, and facilities. It ensures that the hotel inventory remains accurate and up to date for both staff and customers.**

---

## **3. Reservation Monitoring**

### 2-line description

**This page enables the admin to monitor all room reservations made by customers, including booking status, guest details, and check-in/check-out schedules. It helps in tracking reservation flow and resolving booking conflicts or issues.**

---

## **4. Reports**

### 2-line description

**This page provides business reports in weekly, monthly, quarterly, and yearly formats for bookings, revenue, occupancy, and customer trends. It helps the admin analyze hotel performance and make strategic operational decisions.**

---

## **5. Staff and User Management**

### 2-line description

**This page is used to manage staff accounts and customer user records by adding, editing, deleting, or assigning access roles. It helps maintain secure access control and organized employee/user data across the system.**

---

## **6. Settings**

### 2-line description

**This page allows the admin to configure system-level settings such as hotel information, account settings, role permissions, notification preferences, and security options. It centralizes platform customization and operational control.**

---

---

# 6) Manager Module

## Manager Flow

```text
Staff Role Page → Manager Login → Manager Dashboard
                                  ├── Reservations
                                  ├── Occupancy
                                  ├── Revenue
                                  ├── Housekeeping
                                  └── Feedbacks
```

## **1. Manager Dashboard**

### 2-line description

**This page gives the hotel manager a summarized view of daily hotel operations including reservations, room occupancy, revenue status, and customer service performance. It helps managers quickly monitor the business and coordinate with departments.**

---

## **2. Reservations**

### 2-line description

**This page allows the manager to oversee all active and upcoming reservations, booking changes, cancellations, and room assignments. It supports smooth reservation handling and helps avoid overbooking or operational confusion.**

---

## **3. Occupancy**

### 2-line description

**This page displays room occupancy information such as occupied rooms, vacant rooms, reserved rooms, and upcoming check-ins/check-outs. It helps the manager track room utilization and optimize hotel availability planning.**

---

## **4. Revenue**

### 2-line description

**This page provides revenue insights such as room income, service income, payment trends, and total earnings over selected time periods. It helps the manager evaluate financial performance and identify business growth opportunities.**

---

## **5. Housekeeping**

### 2-line description

**This page allows the manager to review housekeeping task status, pending cleaning requests, service completion updates, and department workload. It helps coordinate room readiness and service efficiency across the hotel.**

---

## **6. Feedbacks**

### 2-line description

**This page displays customer reviews, complaints, and service ratings submitted through the system. It helps the manager identify service issues, improve guest satisfaction, and take corrective actions where needed.**

---

---

# 7) Front Desk Module

## Front Desk Flow

```text
Staff Role Page → Front Desk Login → Front Desk Dashboard
                                     ├── Check In
                                     ├── Check Out
                                     ├── Reservations
                                     ├── Housekeeping
                                     └── Guest Operations
```

## **1. Front Desk Dashboard**

### 2-line description

**This page gives front desk staff a quick overview of daily guest operations including arrivals, departures, active reservations, and pending service requests. It acts as the operational workspace for handling all guest-facing activities efficiently.**

---

## **2. Check-In Page**

### 2-line description

**This page is used to register arriving guests, verify booking details, allocate rooms, and complete the check-in process. It ensures a smooth guest arrival experience and updates room occupancy in real time.**

---

## **3. Check-Out Page**

### 2-line description

**This page is used to process guest departures, finalize billing, confirm room release, and update booking records. It helps front desk staff complete the checkout process efficiently while keeping room availability accurate.**

---

## **4. Reservations Page**

### 2-line description

**This page allows front desk staff to view, create, modify, or confirm guest reservations during direct hotel interactions. It supports walk-in guests, phone bookings, and reservation adjustments handled at the reception desk.**

---

## **5. Housekeeping Coordination Page**

### 2-line description

**This page enables front desk staff to coordinate with housekeeping by sending room cleaning, room readiness, and service status requests. It helps ensure that guest rooms are prepared on time for check-in and post-check-out operations.**

---

## **6. Guest Operations Page**

### 2-line description

**This page is used to manage guest-related service activities such as special requests, room changes, issue reporting, and communication support. It helps the front desk provide a better guest experience by handling day-to-day customer needs effectively.**

---

---

# 8) Housekeeping Module

## Housekeeping Flow

```text
Staff Role Page → Housekeeping Login → Housekeeping Dashboard
                                       ├── Housekeeping Service Requests
                                       ├── Laundry Service
                                       ├── Food Service
                                       ├── Maintenance Service
                                       ├── Cleaning Service
                                       └── Assign Task to Employee
```

## **1. Housekeeping Dashboard**

### 2-line description

**This page gives housekeeping staff an overview of pending service requests, assigned tasks, room cleaning schedules, and task completion updates. It acts as the daily task center for managing operational service work across the hotel.**

---

## **2. Housekeeping Service Requests**

### 2-line description

**This page lists all housekeeping-related requests raised by guests, front desk, or management such as extra bedding, room cleaning, toiletries, and support services. It helps staff respond quickly and track request completion efficiently.**

---

## **3. Laundry Service**

### 2-line description

**This page is used to manage laundry-related guest and hotel requests including pickup, washing status, delivery tracking, and special instructions. It helps housekeeping staff organize laundry operations and maintain service quality.**

---

## **4. Food Service**

### 2-line description

**This page manages in-room food delivery or food-related service requests assigned to the housekeeping/service team. It helps track guest food requests, delivery status, and coordination with kitchen or support staff.**

---

## **5. Maintenance Service**

### 2-line description

**This page is used to report and track room maintenance issues such as electrical faults, plumbing issues, damaged furniture, or appliance problems. It helps the housekeeping team coordinate with maintenance staff and ensure rooms remain operational and guest-ready.**

---

## **6. Cleaning Service**

### 2-line description

**This page handles room cleaning schedules, deep cleaning tasks, checkout room cleanup, and daily housekeeping assignments. It helps staff maintain room hygiene standards and update room cleaning status in the system.**

---

## **7. Assign Task to Employee**

### 2-line description

**This page allows housekeeping supervisors or authorized staff to assign service tasks to individual employees based on room number, request type, and urgency. It helps distribute workload properly and ensures task accountability within the housekeeping team.**

---

# 9) Complete Page Count Summary

# **Landing Pages**

1. Landing Page 1 – Public Hotel Homepage + Customer/Staff Login
2. Landing Page 2 – Staff Role Login Page

# **Customer Pages**

3. Customer Login
4. Customer Dashboard
5. Available Rooms
6. Book Rooms
7. Booking History
8. Payment
9. Profile Management
10. Feedback

# **Admin Pages**

11. Admin Dashboard
12. Room Management
13. Reservation Monitoring
14. Reports
15. Staff & User Management
16. Settings

# **Manager Pages**

17. Manager Dashboard
18. Reservations
19. Occupancy
20. Revenue
21. Housekeeping
22. Feedbacks

# **Front Desk Pages**

23. Front Desk Dashboard
24. Check In
25. Check Out
26. Reservations
27. Housekeeping
28. Guest Operations

# **Housekeeping Pages**

29. Housekeeping Dashboard
30. Housekeeping Service Requests
31. Laundry Service
32. Food Service
33. Maintenance Service
34. Cleaning Service
35. Assign Task to Employee

# **Total Pages = 35**

---

# 10) Recommended Navigation Structure for Frontend

## **Landing Page 1 Navbar**

* Home
* Rooms
* Amenities
* About
* Contact
* Customer Login
* Staff Login

## **Customer Sidebar**

* Dashboard
* Available Rooms
* Book Rooms
* Booking History
* Payment
* Profile Management
* Feedback
* Logout

## **Admin Sidebar**

* Dashboard
* Room Management
* Reservation Monitoring
* Reports
* Staff & User Management
* Settings
* Logout

## **Manager Sidebar**

* Dashboard
* Reservations
* Occupancy
* Revenue
* Housekeeping
* Feedbacks
* Logout

## **Front Desk Sidebar**

* Dashboard
* Check In
* Check Out
* Reservations
* Housekeeping
* Guest Operations
* Logout

## **Housekeeping Sidebar**

* Dashboard
* Service Requests
* Laundry Service
* Food Service
* Maintenance Service
* Cleaning Service
* Assign Task
* Logout

---