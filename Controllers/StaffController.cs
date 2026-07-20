using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class StaffController : Controller
{
    private readonly IStaffService _staffService;
    private readonly IRoomService _roomService;
    private readonly IReservationService _reservationService;
    private readonly ICheckInService _checkInService;
    private readonly IHousekeepingService _housekeepingService;
    private readonly IBillingService _billingService;

    public StaffController(
        IStaffService staffService,
        IRoomService roomService,
        IReservationService reservationService,
        ICheckInService checkInService,
        IHousekeepingService housekeepingService,
        IBillingService billingService)
    {
        _staffService = staffService;
        _roomService = roomService;
        _reservationService = reservationService;
        _checkInService = checkInService;
        _housekeepingService = housekeepingService;
        _billingService = billingService;
    }

    [HttpGet]
    public IActionResult Login() => View(new StaffLoginDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(StaffLoginDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var staff = await _staffService.ValidateStaffLoginAsync(dto);
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

    [HttpGet]
    public async Task<IActionResult> Dashboard(string role = "Admin")
    {
        string sessionRole = HttpContext.Session.GetString("StaffRole") ?? role;
        ViewData["Role"] = sessionRole;
        ViewBag.StaffName = HttpContext.Session.GetString("StaffName") ?? "Staff Member";

        var rooms = await _roomService.GetAllRoomsAsync();
        var availableRooms = rooms.Where(r => r.Status == Enums.RoomStatus.Available).ToList();
        var reservations = await _reservationService.GetAllReservationsAsync();
        var activeStays = await _checkInService.GetAllStaysAsync();
        var currentActiveStays = activeStays.Where(s => !s.ActualCheckOut.HasValue).ToList();
        var housekeepingTasks = await _housekeepingService.GetAllTasksAsync();
        var bills = await _billingService.GetAllBillsAsync();
        var pendingBills = bills.Where(b => b.PaymentStatus == Enums.PaymentStatus.Pending).ToList();

        ViewBag.TotalRoomsCount = rooms.Count();
        ViewBag.AvailableRoomsCount = availableRooms.Count();
        ViewBag.ReservationsCount = reservations.Count(r => r.ReservationStatus == Enums.ReservationStatus.Booked);
        ViewBag.ActiveStaysCount = currentActiveStays.Count();
        ViewBag.PendingTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.Pending);
        ViewBag.InProgressTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.InProgress);
        ViewBag.CompletedTasksCount = housekeepingTasks.Count(t => t.TaskStatus == Enums.TaskStatus.Completed);
        ViewBag.PendingPaymentAmount = pendingBills.Sum(b => b.TotalAmount);
        ViewBag.PendingBillsCount = pendingBills.Count();

        ViewBag.ArrivalsList = reservations.Where(r => r.ReservationStatus == Enums.ReservationStatus.Booked).Take(5).ToList();
        ViewBag.HousekeepingTasksList = housekeepingTasks.Where(t => t.TaskStatus != Enums.TaskStatus.Completed).Take(5).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var staffList = await _staffService.GetAllStaffAsync();
        return View(staffList);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateStaffDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _staffService.CreateStaffAsync(dto);
            TempData["Success"] = "Staff member created successfully!";
            return RedirectToAction(nameof(Index));
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
        var staff = await _staffService.GetStaffByIdAsync(id);
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
        if (id != dto.StaffId) return BadRequest();
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _staffService.UpdateStaffAsync(dto);
            TempData["Success"] = "Staff updated successfully!";
            return RedirectToAction(nameof(Index));
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
        var staff = await _staffService.GetStaffByIdAsync(id);
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _staffService.DeleteStaffAsync(id);
            TempData["Success"] = "Staff member deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
