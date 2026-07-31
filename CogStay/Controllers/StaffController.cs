using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;

namespace CogStayMVC.Controllers;

public class StaffController : Controller
{
    private readonly HttpClient _httpClient;

    public StaffController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CogStayApi");
    }

    [HttpGet]
    public IActionResult Login() => View(new StaffLoginDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(StaffLoginDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var staff = await _httpClient.PostAsJsonOrThrowAsync<StaffResponseDTO, StaffLoginDTO>("api/staff/login", dto);
            if (staff == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials or unauthorized role access.");
                return View(dto);
            }

            HttpContext.Session.SetInt32("StaffId", staff.StaffId);
            HttpContext.Session.SetString("StaffName", staff.FullName);
            HttpContext.Session.SetString("StaffRole", staff.Role.ToString());

            return RedirectToAction(nameof(Dashboard), new { role = staff.Role.ToString() });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(string role = "Admin")
    {
        int? staffId = HttpContext.Session.GetInt32("StaffId");
        if (!staffId.HasValue) return RedirectToAction(nameof(Login));

        string? sessionRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(sessionRole)) return RedirectToAction(nameof(Login));

        ViewData["Role"] = sessionRole;
        ViewBag.StaffName = HttpContext.Session.GetString("StaffName") ?? "Staff Member";

        var rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms") ?? Enumerable.Empty<RoomResponseDTO>();
        var availableRooms = rooms.Where(r => r.Status == RoomStatus.Available).ToList();
        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations") ?? Enumerable.Empty<ReservationResponseDTO>();
        var activeStays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays") ?? Enumerable.Empty<StayRecordResponseDTO>();
        var currentActiveStays = activeStays.Where(s => !s.ActualCheckOut.HasValue).ToList();
        var housekeepingTasks = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<HousekeepingTaskResponseDTO>>("api/housekeeping") ?? Enumerable.Empty<HousekeepingTaskResponseDTO>();
        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing") ?? Enumerable.Empty<BillingResponseDTO>();
        var pendingBills = bills.Where(b => b.PaymentStatus == PaymentStatus.Pending).ToList();
        var staffList = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StaffResponseDTO>>("api/staff") ?? Enumerable.Empty<StaffResponseDTO>();

        ViewBag.TotalRoomsCount = rooms.Count();
        ViewBag.TotalStaffCount = staffList.Count();
        ViewBag.AvailableRoomsCount = availableRooms.Count();
        ViewBag.ReservationsCount = reservations.Count(r => r.ReservationStatus == ReservationStatus.Booked);
        ViewBag.ActiveStaysCount = currentActiveStays.Count();
        ViewBag.PendingTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.Pending);
        ViewBag.InProgressTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.InProgress);
        ViewBag.CompletedTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.Completed);
        ViewBag.PendingPaymentAmount = pendingBills.Sum(b => b.TotalAmount);
        ViewBag.PendingBillsCount = pendingBills.Count();

        ViewBag.ArrivalsList = reservations.Where(r => r.ReservationStatus == ReservationStatus.Booked).Take(5).ToList();
        ViewBag.HousekeepingTasksList = housekeepingTasks.Where(t => t.TaskStatus != Enums.TaskStatus.Completed).Take(5).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        var staffList = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StaffResponseDTO>>("api/staff");
        return View(staffList);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}");
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpGet]
    public IActionResult Create()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        return View(new CreateStaffDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _httpClient.PostAsJsonOrThrowAsync<StaffResponseDTO, CreateStaffDTO>("api/staff", dto);
            TempData["Success"] = "Staff member created successfully!";
            return RedirectToAction(nameof(Index), new { role = "Admin" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}");
        if (staff == null) return NotFound();

        var dto = new UpdateStaffDTO
        {
            StaffId = staff.StaffId,
            FullName = staff.FullName,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            Role = staff.Role,
            IsActive = staff.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStaffDTO dto)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        if (id != dto.StaffId) return BadRequest();
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _httpClient.PutAsJsonOrThrowAsync($"api/staff/{id}", dto);
            TempData["Success"] = "Staff updated successfully!";
            return RedirectToAction(nameof(Index), new { role = "Admin" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}");
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        try
        {
            await _httpClient.DeleteOrThrowAsync($"api/staff/{id}");
            TempData["Success"] = "Staff member deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> CheckInStatus()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        var stays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays");
        return View(stays);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestCheckOut(int stayId)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || staffRole != "Admin")
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login", "Staff");
            return RedirectToAction("Dashboard", "Staff", new { role = staffRole });
        }

        ViewData["Role"] = "Admin";
        await _httpClient.PostAsJsonOrThrowAsync<object, object>($"api/stays/{stayId}/request-checkout", new { });
        TempData["Success"] = "Checkout requested successfully.";
        return RedirectToAction(nameof(CheckInStatus));
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
