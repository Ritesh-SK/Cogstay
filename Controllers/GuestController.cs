using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CogStayMVC.DTOs;
using CogStayMVC.Services.Interfaces;

namespace CogStayMVC.Controllers;

public class GuestController : Controller
{
    private readonly IGuestService _guestService;
    private readonly IRoomService _roomService;
    private readonly IReservationService _reservationService;
    private readonly ICheckInService _checkInService;
    private readonly IBillingService _billingService;

    public GuestController(
        IGuestService guestService,
        IRoomService roomService,
        IReservationService reservationService,
        ICheckInService checkInService,
        IBillingService billingService)
    {
        _guestService = guestService;
        _roomService = roomService;
        _reservationService = reservationService;
        _checkInService = checkInService;
        _billingService = billingService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(GuestLoginDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var guest = await _guestService.ValidateGuestLoginAsync(dto);
        if (guest == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(dto);
        }

        HttpContext.Session.SetInt32("GuestId", guest.GuestId);
        HttpContext.Session.SetString("GuestName", guest.FullName);
        HttpContext.Session.SetString("GuestEmail", guest.Email);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CreateGuestDTO dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            var guest = await _guestService.RegisterGuestAsync(dto);
            HttpContext.Session.SetInt32("GuestId", guest.GuestId);
            HttpContext.Session.SetString("GuestName", guest.FullName);
            HttpContext.Session.SetString("GuestEmail", guest.Email);

            TempData["Success"] = "Account registered successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _guestService.GetGuestByIdAsync(guestId.Value);
        if (guest == null) return RedirectToAction(nameof(Login));

        ViewBag.GuestName = guest.FullName;
        HttpContext.Session.SetString("GuestName", guest.FullName);
        HttpContext.Session.SetString("GuestEmail", guest.Email);

        var reservations = await _reservationService.GetReservationsByGuestAsync(guestId.Value);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableRooms()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewData["Role"] = "Guest";
        var rooms = await _roomService.GetAvailableRoomsAsync();
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> BookRoom(int? roomId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        ViewBag.AvailableRooms = await _roomService.GetAvailableRoomsAsync();
        var dto = new CreateReservationDTO
        {
            GuestId = guestId.Value,
            RoomId = roomId ?? 0,
            CheckInDate = DateTime.Today,
            CheckOutDate = DateTime.Today.AddDays(1)
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookRoom(CreateReservationDTO dto)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));
        dto.GuestId = guestId.Value;

        if (!ModelState.IsValid)
        {
            ViewBag.AvailableRooms = await _roomService.GetAvailableRoomsAsync();
            return View(dto);
        }

        try
        {
            await _reservationService.BookRoomAsync(dto);
            TempData["Success"] = "Room booked successfully!";
            return RedirectToAction(nameof(MyReservations));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.AvailableRooms = await _roomService.GetAvailableRoomsAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var reservations = await _reservationService.GetReservationsByGuestAsync(guestId.Value);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> BookingHistory()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var reservations = await _reservationService.GetReservationsByGuestAsync(guestId.Value);
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> CheckInStatus()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var stays = await _checkInService.GetAllStaysAsync();
        return View(stays);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestCheckOut(int stayId)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        await _checkInService.RequestCheckOutAsync(stayId);
        TempData["Success"] = "Checkout requested. Please proceed to Front Desk for billing & payment.";
        return RedirectToAction(nameof(CheckInStatus));
    }

    [HttpGet]
    public async Task<IActionResult> Billing()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var bills = await _billingService.GetAllBillsAsync();
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));

        var guest = await _guestService.GetGuestByIdAsync(guestId.Value);
        if (guest == null) return NotFound();

        var dto = new UpdateGuestDTO
        {
            GuestId = guest.GuestId,
            FullName = guest.FullName,
            Email = guest.Email,
            PhoneNumber = guest.PhoneNumber,
            Address = guest.Address
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateGuestDTO dto)
    {
        int? guestId = HttpContext.Session.GetInt32("GuestId");
        if (!guestId.HasValue) return RedirectToAction(nameof(Login));
        dto.GuestId = guestId.Value;

        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _guestService.UpdateGuestAsync(dto);
            HttpContext.Session.SetString("GuestName", dto.FullName);
            HttpContext.Session.SetString("GuestEmail", dto.Email);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
