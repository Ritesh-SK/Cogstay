using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Enums;
using CogStayMVC.Controllers.Api;

namespace CogStayMVC.Controllers;

public class StaffController : Controller
{
    private readonly StaffApiController _staffApiController;
    private readonly RoomApiController _roomApiController;
    private readonly ReservationApiController _reservationApiController;
    private readonly CheckInApiController _checkInApiController;
    private readonly HousekeepingApiController _housekeepingApiController;
    private readonly BillingApiController _billingApiController;

    public StaffController(
        StaffApiController staffApiController,
        RoomApiController roomApiController,
        ReservationApiController reservationApiController,
        CheckInApiController checkInApiController,
        HousekeepingApiController housekeepingApiController,
        BillingApiController billingApiController)
    {
        _staffApiController = staffApiController;
        _roomApiController = roomApiController;
        _reservationApiController = reservationApiController;
        _checkInApiController = checkInApiController;
        _housekeepingApiController = housekeepingApiController;
        _billingApiController = billingApiController;
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
            var staff = ControllerExtensions.Unpack(await _staffApiController.LoginStaff(dto));
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

        var rooms = ControllerExtensions.Unpack(await _roomApiController.GetAllRooms()) ?? Enumerable.Empty<RoomResponseDTO>();
        var availableRooms = rooms.Where(r => r.Status == RoomStatus.Available).ToList();
        var reservations = ControllerExtensions.Unpack(await _reservationApiController.GetAllReservations()) ?? Enumerable.Empty<ReservationResponseDTO>();
        var activeStays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays()) ?? Enumerable.Empty<StayRecordResponseDTO>();
        var currentActiveStays = activeStays.Where(s => !s.ActualCheckOut.HasValue).ToList();
        var housekeepingTasks = ControllerExtensions.Unpack(await _housekeepingApiController.GetAllTasks()) ?? Enumerable.Empty<HousekeepingTaskResponseDTO>();
        var bills = ControllerExtensions.Unpack(await _billingApiController.GetAllBills()) ?? Enumerable.Empty<BillingResponseDTO>();
        var pendingBills = bills.Where(b => b.PaymentStatus == PaymentStatus.Pending).ToList();
        var staffList = ControllerExtensions.Unpack(await _staffApiController.GetAllStaff()) ?? Enumerable.Empty<StaffResponseDTO>();

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
        var staffList = ControllerExtensions.Unpack(await _staffApiController.GetAllStaff());
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
        var staff = ControllerExtensions.Unpack(await _staffApiController.GetStaffById(id));
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
            ControllerExtensions.Unpack(await _staffApiController.CreateStaff(dto));
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
        var staff = ControllerExtensions.Unpack(await _staffApiController.GetStaffById(id));
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
            ControllerExtensions.Unpack(await _staffApiController.UpdateStaff(id, dto));
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
        var staff = ControllerExtensions.Unpack(await _staffApiController.GetStaffById(id));
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
            ControllerExtensions.Unpack(await _staffApiController.DeleteStaff(id));
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
        var stays = ControllerExtensions.Unpack(await _checkInApiController.GetAllStays());
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
        ControllerExtensions.Unpack(await _checkInApiController.RequestCheckOut(stayId));
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
