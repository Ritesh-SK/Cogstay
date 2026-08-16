using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStay.Application.DTOs;
using CogStay.Domain.Enums;
using TaskStatus = CogStay.Domain.Enums.TaskStatus;

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
            var auth = await _httpClient.PostAsJsonOrThrowAsync<AuthResponseDTO, StaffLoginDTO>("api/auth/staff-login", dto);
            if (auth == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials or unauthorized role access.");
                return View(dto);
            }

            HttpContext.Session.SetString("JwtToken", auth.Token);
            HttpContext.Session.SetString("RefreshToken", auth.RefreshToken);
            HttpContext.Session.SetInt32("StaffId", auth.IntegerId);
            HttpContext.Session.SetString("StaffName", auth.FullName);
            HttpContext.Session.SetString("StaffRole", auth.Role);

            return RedirectToAction(nameof(Dashboard), new { role = auth.Role });
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

        var rooms = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<RoomResponseDTO>>("api/rooms", HttpContext) ?? Enumerable.Empty<RoomResponseDTO>();
        var availableRooms = rooms.Where(r => r.Status == RoomStatus.Available).ToList();
        var reservations = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<ReservationResponseDTO>>("api/reservations", HttpContext) ?? Enumerable.Empty<ReservationResponseDTO>();
        var activeStays = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StayRecordResponseDTO>>("api/stays", HttpContext) ?? Enumerable.Empty<StayRecordResponseDTO>();
        var currentActiveStays = activeStays.Where(s => !s.ActualCheckOut.HasValue).ToList();
        var housekeepingTasks = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<HousekeepingTaskResponseDTO>>("api/housekeeping", HttpContext) ?? Enumerable.Empty<HousekeepingTaskResponseDTO>();
        var bills = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<BillingResponseDTO>>("api/billing", HttpContext) ?? Enumerable.Empty<BillingResponseDTO>();
        var pendingBills = bills.Where(b => b.PaymentStatus == PaymentStatus.Pending).ToList();
        
        IEnumerable<StaffResponseDTO> staffList = Enumerable.Empty<StaffResponseDTO>();
        if (sessionRole == "Admin" || sessionRole == "Manager")
        {
            staffList = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StaffResponseDTO>>("api/staff", HttpContext) ?? Enumerable.Empty<StaffResponseDTO>();
        }

        ViewBag.TotalRoomsCount = rooms.Count();
        ViewBag.TotalStaffCount = staffList.Count();
        ViewBag.AvailableRoomsCount = availableRooms.Count();
        ViewBag.ReservationsCount = reservations.Count(r => r.ReservationStatus == ReservationStatus.Confirmed || r.ReservationStatus == ReservationStatus.Pending);
        ViewBag.ActiveStaysCount = currentActiveStays.Count();
        ViewBag.PendingTasksCount = housekeepingTasks.Count(t => t.TaskStatus == TaskStatus.Pending);
        ViewBag.InProgressTasksCount = housekeepingTasks.Count(t => t.TaskStatus == TaskStatus.InProgress);
        ViewBag.CompletedTasksCount = housekeepingTasks.Count(t => t.TaskStatus == TaskStatus.Completed);
        ViewBag.PendingPaymentAmount = pendingBills.Sum(b => b.TotalAmount);
        ViewBag.PendingBillsCount = pendingBills.Count();

        ViewBag.ArrivalsList = reservations.Where(r => r.ReservationStatus == ReservationStatus.Confirmed || r.ReservationStatus == ReservationStatus.Pending).Take(5).ToList();
        ViewBag.HousekeepingTasksList = housekeepingTasks.Where(t => t.TaskStatus != TaskStatus.Completed).Take(5).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Admin" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var staffList = await _httpClient.GetFromJsonOrThrowAsync<IEnumerable<StaffResponseDTO>>("api/staff", HttpContext);
        return View(staffList);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        string? staffRole = HttpContext.Session.GetString("StaffRole");
        if (string.IsNullOrEmpty(staffRole) || (staffRole != "Admin" && staffRole != "Manager"))
        {
            if (string.IsNullOrEmpty(staffRole)) return RedirectToAction("Login");
            return RedirectToAction("Dashboard", new { role = staffRole });
        }

        ViewData["Role"] = staffRole;
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}", HttpContext);
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
            await _httpClient.PostAsJsonOrThrowAsync<StaffResponseDTO, CreateStaffDTO>("api/staff", dto, HttpContext);
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
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}", HttpContext);
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
            await _httpClient.PutAsJsonOrThrowAsync($"api/staff/{id}", dto, HttpContext);
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
        var staff = await _httpClient.GetFromJsonOrThrowAsync<StaffResponseDTO>($"api/staff/{id}", HttpContext);
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
            await _httpClient.DeleteOrThrowAsync($"api/staff/{id}", HttpContext);
            TempData["Success"] = "Staff member deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { role = "Admin" });
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
